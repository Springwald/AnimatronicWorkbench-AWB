﻿// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.Project.Servos;

namespace Awb.Core.DataPackets
{
    public class DataPacketFactory
    {

        public ClientDataPacket? GetDataPacketGetServoPos(IServoConfig servo)
        {
            if (servo is StsFeetechServoConfig stsFeetechServoConfig)
            {
                return new ClientDataPacket(stsFeetechServoConfig.ClientId,
                new DataPacketContent
                {
                    ReadValue = new ReadValueData(typeName: ReadValueData.TypeNames.StsServo, id: stsFeetechServoConfig.Channel.ToString())
                });
            }

            if (servo is ScsFeetechServoConfig scsFeetechServoConfig)
            {
                return new ClientDataPacket(scsFeetechServoConfig.ClientId,
                    new DataPacketContent
                    {
                        ReadValue = new ReadValueData(typeName: ReadValueData.TypeNames.ScsServo, id: scsFeetechServoConfig.Channel.ToString())
                    });
            }

            if (servo is Pca9685PwmServoConfig pwmServoConfig)
            {
                return null; // PWM servos can't send their position
            }

            return null;
        }

        /// <summary>
        /// created a data packet to set the position of a servo
        /// </summary>
        public ClientDataPacket? GetDataPacketSetServoPos(IServoConfig servo, int absolutePos)
        {
            if (servo is StsFeetechServoConfig stsFeetechServoConfig)
            {
                return new ClientDataPacket(stsFeetechServoConfig.ClientId,
                new DataPacketContent
                {
                    StsServos = new StsServosPacketData
                    {
                        Servos = new[]
                         {
                             new StsServoPacketData
                             {
                                 Channel = stsFeetechServoConfig.Channel,
                                 TargetValue = absolutePos,
                                 Name = string.IsNullOrWhiteSpace(stsFeetechServoConfig.Title) ? $"STS{stsFeetechServoConfig.Channel}" : stsFeetechServoConfig.Title,
                                 Speed = stsFeetechServoConfig.Speed.HasValue ? stsFeetechServoConfig.Speed.Value : 0,
                                 Acc = stsFeetechServoConfig.Acceleration.HasValue ? stsFeetechServoConfig.Acceleration.Value : 0,
                             }
                         }
                    },
                });
            }
            if (servo is ScsFeetechServoConfig scsFeetechServoConfig)
            {
                return new ClientDataPacket(scsFeetechServoConfig.ClientId,
                    new DataPacketContent
                    {
                        ScsServos = new StsServosPacketData
                        {
                            Servos = new[]
                            {
                                new StsServoPacketData
                                {
                                    Channel = scsFeetechServoConfig.Channel,
                                    TargetValue = absolutePos,
                                    Name = string.IsNullOrWhiteSpace(scsFeetechServoConfig.Title) ? $"SCS{scsFeetechServoConfig.Channel}" : scsFeetechServoConfig.Title,
                                    Speed = scsFeetechServoConfig.Speed.HasValue ? scsFeetechServoConfig.Speed.Value : 0,
                                }
                            }
                        },
                    });
            }
            if (servo is Pca9685PwmServoConfig pwmServoConfig)
            {
                return new ClientDataPacket(pwmServoConfig.ClientId,
                    new DataPacketContent
                    {
                        Pca9685PwmServos = new Pca9685PwmServosPacketData
                        {
                            Servos = new[]
                            {
                                new Pca9685PwmServoPacketData
                                {
                                    I2cAddress = pwmServoConfig.I2cAdress,
                                    Channel = pwmServoConfig.Channel,
                                    TargetValue = absolutePos,
                                    Name = string.IsNullOrWhiteSpace(pwmServoConfig.Title) ? $"PWM{pwmServoConfig.Channel}" : pwmServoConfig.Title,
                                }
                            }
                        },
                    });
            }

            return null;
        }

        public IEnumerable<ClientDataPacket> GetDataPackets(IServo[] servos)
        {
            // group servos by their clients
            var servosByClients = servos.GroupBy(
                s => s.ClientId,
                s => s,
                (key, g) => new { ClientId = key, Servos = g.ToArray() });

            foreach (var servosByClient in servosByClients)
            {
                var stsServos = this.GetStsServoChanges(servosByClient.Servos, servoType: StsScsServo.StsScsTypes.Sts);
                var scsServos = this.GetStsServoChanges(servosByClient.Servos, servoType: StsScsServo.StsScsTypes.Scs);
                var pwmServos = this.GetPwmServoChanges(servosByClient.Servos);

                if (stsServos != null || pwmServos != null || scsServos != null)
                {
                    yield return new ClientDataPacket(
                        clientId: servosByClient.ClientId,
                        dataPacketContent: new DataPacketContent
                        {
                            DisplayMessage = null,
                            StsServos = stsServos,
                            ScsServos = scsServos,
                            Pca9685PwmServos = pwmServos
                        });
                }
            }
        }

        public void SetDataPacketDone(IServo[] servos, ClientDataPacket clientDataPacket)
        {
            if (clientDataPacket.Content.StsServos?.Servos != null)
            {
                foreach (var servoPacketData in clientDataPacket.Content.StsServos.Servos)
                {
                    var stsServo = servos.Select(s => s as StsScsServo).FirstOrDefault(s => s?.Channel == servoPacketData.Channel && s.ClientId == clientDataPacket.ClientId);
                    {
                        if (stsServo != null) stsServo.IsDirty = false;
                    }
                }
            }
            if (clientDataPacket.Content.Pca9685PwmServos?.Servos != null)
            {
                foreach (var servoPacketData in clientDataPacket.Content.Pca9685PwmServos.Servos)
                {
                    var pwmServo = servos.Select(s => s as Pca9685PwmServo).FirstOrDefault(s => s?.Channel == servoPacketData.Channel && s.ClientId == clientDataPacket.ClientId && s.I2cAdress == servoPacketData.I2cAddress);
                    {
                        if (pwmServo != null) pwmServo.IsDirty = false;
                    }
                }
            }
        }

        private StsServosPacketData? GetStsServoChanges(IServo[] allServos, StsScsServo.StsScsTypes servoType)
        {
            var stsServos = new List<StsServoPacketData>();

            foreach (var servo in allServos)
            {
                var stsServo = servo as StsScsServo;
                if (stsServo != null && stsServo.IsDirty && stsServo.StsScsType == servoType)
                {
                    stsServos.Add(new StsServoPacketData
                    {
                        Channel = stsServo.Channel,
                        TargetValue = servo.TargetValue,
                        Name = string.IsNullOrWhiteSpace(stsServo.Title) ? $"STS{stsServo.Channel}" : stsServo.Title,
                        Speed = stsServo.Speed.HasValue ? stsServo.Speed.Value : 0,
                        Acc = stsServo.Acceleration.HasValue ? stsServo.Acceleration.Value : 0,
                    });
                }
            }

            if (stsServos.Count > 0)
            {
                return new StsServosPacketData
                {
                    Servos = stsServos.ToArray(),
                };
            }
            return null;
        }

        private Pca9685PwmServosPacketData? GetPwmServoChanges(IServo[] allServos)
        {
            var pwmServos = new List<Pca9685PwmServoPacketData>();

            foreach (var servo in allServos)
            {
                var pwmServo = servo as Pca9685PwmServo;
                if (pwmServo != null && pwmServo.IsDirty)
                {
                    pwmServos.Add(new Pca9685PwmServoPacketData
                    {
                        I2cAddress = pwmServo.I2cAdress,
                        Channel = pwmServo.Channel,
                        TargetValue = servo.TargetValue,
                        Name = string.IsNullOrWhiteSpace(pwmServo.Title) ? $"STS{pwmServo.Channel}" : pwmServo.Title,
                    });
                }
            }

            if (pwmServos.Count > 0)
            {
                return new Pca9685PwmServosPacketData
                {
                    Servos = pwmServos.ToArray(),
                };
            }
            return null;
        }


    }
}
