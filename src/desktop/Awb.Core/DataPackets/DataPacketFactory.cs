// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2026 Daniel Springwald      -     Bochum, Germany
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
            switch (servo)
            {
                case StsFeetechServoConfigServoMode stsFeetechServoConfigServoMode:
                    return new ClientDataPacket(stsFeetechServoConfigServoMode.ClientId,
                           new DataPacketContent
                           {
                               ReadValue = new ReadValueData(typeName: ReadValueData.TypeNames.StsServo, id: stsFeetechServoConfigServoMode.Channel.ToString())
                           },
                           affectedAcctuatorsToRemoveDirtyFlag: []);

                case ScsFeetechServoConfig scsFeetechServoConfig:
                    return new ClientDataPacket(scsFeetechServoConfig.ClientId,
                        new DataPacketContent
                        {
                            ReadValue = new ReadValueData(typeName: ReadValueData.TypeNames.ScsServo, id: scsFeetechServoConfig.Channel.ToString())
                        },
                        affectedAcctuatorsToRemoveDirtyFlag: []);

                case StsFeetechServoWheelModeConfig stsFeetechServoConfigWheelMode:
                    return null; // Wheel mode servos can't send their position

                case Pca9685PwmServoConfig pwmServoConfig:
                    return null; // PWM servos can't send their position

                default:
                    throw new NotImplementedException($"Unhandled servo type '{servo.GetType().Name}'");
            }
        }

        /// <summary>
        /// created a data packet to set the position of a servo.
        /// The Dirty flag of the servo is not set by this method, so the caller has to set it manually.
        /// </summary>
        public ClientDataPacket? GetDataPacketSetSingleServoPos(IServoConfig servo, int absolutePos)
        {
            switch (servo)
            {
                case StsFeetechServoConfigServoMode stsFeetechServoConfigServoMode:
                    return new ClientDataPacket(stsFeetechServoConfigServoMode.ClientId,
                           new DataPacketContent
                           {
                               StsServos = new StsServosPacketData
                               {
                                   Servos = new[]
                                    {
                                         new StsServoPacketData
                                         {
                                             Channel = stsFeetechServoConfigServoMode.Channel,
                                             WheelMode = false,
                                             TargetValue = absolutePos,
                                             Name = string.IsNullOrWhiteSpace(stsFeetechServoConfigServoMode.Title) ? $"STS{stsFeetechServoConfigServoMode.Channel}" : stsFeetechServoConfigServoMode.Title,
                                             Speed = stsFeetechServoConfigServoMode.Speed.HasValue ? stsFeetechServoConfigServoMode.Speed.Value : 0,
                                             Acc = stsFeetechServoConfigServoMode.Acceleration.HasValue ? stsFeetechServoConfigServoMode.Acceleration.Value : 0,
                                         }
                                    }
                               },
                           }, affectedAcctuatorsToRemoveDirtyFlag: []);

                case StsFeetechServoWheelModeConfig stsFeetechServoConfigWheelMode:
                    return new ClientDataPacket(stsFeetechServoConfigWheelMode.ClientId,
                           new DataPacketContent
                           {
                               StsServos = new StsServosPacketData
                               {
                                   Servos = new[]
                                    {
                                         new StsServoPacketData
                                         {
                                             Channel = stsFeetechServoConfigWheelMode.Channel,
                                             WheelMode = true,
                                             TargetValue = absolutePos,
                                             Name = string.IsNullOrWhiteSpace(stsFeetechServoConfigWheelMode.Title) ? $"STSWHEEL{stsFeetechServoConfigWheelMode.Channel}" : stsFeetechServoConfigWheelMode.Title,
                                             Speed = absolutePos, // speed is not needed here. It will be sent as TargetValue, because wheel mode is used
                                             Acc = stsFeetechServoConfigWheelMode.Acceleration.HasValue ? stsFeetechServoConfigWheelMode.Acceleration.Value : 0,
                                         }
                                    }
                               },
                           }, affectedAcctuatorsToRemoveDirtyFlag: []);

                case ScsFeetechServoConfig scsFeetechServoConfig:
                    return new ClientDataPacket(scsFeetechServoConfig.ClientId,
                    new DataPacketContent
                    {
                        ScsServos = new StsServosPacketData
                        {
                            Servos =
                            [
                                new StsServoPacketData
                                {
                                    Channel = scsFeetechServoConfig.Channel,
                                    WheelMode = false,
                                    TargetValue = absolutePos,
                                    Name = string.IsNullOrWhiteSpace(scsFeetechServoConfig.Title) ? $"SCS{scsFeetechServoConfig.Channel}" : scsFeetechServoConfig.Title,
                                    Speed = scsFeetechServoConfig.Speed.HasValue ? scsFeetechServoConfig.Speed.Value : 0,
                                }
                            ]
                        },
                    }, affectedAcctuatorsToRemoveDirtyFlag: []);

                case Pca9685PwmServoConfig pwmServoConfig:
                    return new ClientDataPacket(pwmServoConfig.ClientId,
                   new DataPacketContent
                   {
                       Pca9685PwmServos = new Pca9685PwmServosPacketData
                       {
                           Servos =
                           [
                               new Pca9685PwmServoPacketData
                                {
                                    I2cAddress = pwmServoConfig.I2cAdress,
                                    Channel = pwmServoConfig.Channel,
                                    TargetValue = absolutePos,
                                    Name = string.IsNullOrWhiteSpace(pwmServoConfig.Title) ? $"PWM{pwmServoConfig.Channel}" : pwmServoConfig.Title,
                                }
                           ]
                       },
                   }, affectedAcctuatorsToRemoveDirtyFlag: []);
                default:
                    throw new NotImplementedException($"Unhandled servo type '{servo.GetType().Name}'");
            }
        }

        public IEnumerable<ClientDataPacket> GetDataPackets(IServo[] servos)
        {
            // group servos by their clients
            var servosByClients = servos.GroupBy(
                s => s.ClientId,
                s => s,
                (key, g) => new { ClientId = key, Servos = g.ToArray() });

            var collectAffectedAcctuatorsToUnsetDirty = new List<IActuator>();

            foreach (var servosByClient in servosByClients)
            {
                var stsServos = this.GetStsScsServoChanges(servosByClient.Servos, servoType: StsScsServo.StsScsTypes.Sts, collectAffectedAcctuatorsToUnsetDirty);
                var scsServos = this.GetStsScsServoChanges(servosByClient.Servos, servoType: StsScsServo.StsScsTypes.Scs, collectAffectedAcctuatorsToUnsetDirty);
                var pwmServos = this.GetPwmServoChanges(servosByClient.Servos, collectAffectedAcctuatorsToUnsetDirty);

                // mix sts and wheel mode servos together, because they are both STS servos, but with different modes. The client will handle them accordingly.
                var stsWheelModeServos = this.GetStsScsServoChanges(servosByClient.Servos, servoType: StsScsServo.StsScsTypes.StsWheelMode, collectAffectedAcctuatorsToUnsetDirty);
                if (stsWheelModeServos != null)
                {
                    if (stsServos == null)
                        stsServos = new StsServosPacketData { Servos = [] };
                    stsServos.Servos = stsServos.Servos!.Concat(stsWheelModeServos.Servos!).ToArray();
                }

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
                        },
                        affectedAcctuatorsToRemoveDirtyFlag: collectAffectedAcctuatorsToUnsetDirty.ToArray());
                }
            }
        }

        /// <summary>
        /// If the data packet was sent to the client, this method is called to unset the dirty flag of the affected actuators.
        /// </summary>
        /// <param name="clientDataPacket"></param>
        public void SetDataPacketDone(ClientDataPacket clientDataPacket)
        {
            foreach (var affectedActuator in clientDataPacket.AffectedAcctuatorsToRemoveDirtyFlag)
                affectedActuator.IsDirty = false;
        }

        private StsServosPacketData? GetStsScsServoChanges(IServo[] allServos, StsScsServo.StsScsTypes servoType, List<IActuator> collectAffectedAcctuatorsToUnsetDirty)
        {
            var stsScsServosPackets = new List<StsServoPacketData>();

            foreach (var servo in allServos)
            {
                if (servo is StsScsServo stsServo && stsServo.IsDirty && stsServo.StsScsType == servoType)
                {
                    collectAffectedAcctuatorsToUnsetDirty.Add(stsServo);
                    stsScsServosPackets.Add(new StsServoPacketData
                    {
                        Channel = stsServo.Channel,
                        TargetValue = servo.TargetValue,
                        WheelMode = stsServo.WheelMode,
                        Name = string.IsNullOrWhiteSpace(stsServo.Title) ? $"STS{stsServo.Channel}" : stsServo.Title,
                        Speed = stsServo.Speed.HasValue ? stsServo.Speed.Value : 0,
                        Acc = stsServo.Acceleration.HasValue ? stsServo.Acceleration.Value : 0,
                    });
                }
            }

            if (stsScsServosPackets.Any())
                return new StsServosPacketData
                {
                    Servos = stsScsServosPackets.ToArray(),
                };

            return null;
        }

        private Pca9685PwmServosPacketData? GetPwmServoChanges(IServo[] allServos, List<IActuator> collectAffectedAcctuatorsToUnsetDirty)
        {
            var pwmServoDataPackets = new List<Pca9685PwmServoPacketData>();

            foreach (var servo in allServos)
                if (servo is Pca9685PwmServo pwmServo && pwmServo.IsDirty)
                {
                    collectAffectedAcctuatorsToUnsetDirty.Add(pwmServo);
                    pwmServoDataPackets.Add(new Pca9685PwmServoPacketData
                    {
                        I2cAddress = pwmServo.I2cAdress,
                        Channel = pwmServo.Channel,
                        TargetValue = servo.TargetValue,
                        Name = string.IsNullOrWhiteSpace(pwmServo.Title) ? $"STS{pwmServo.Channel}" : pwmServo.Title,
                    });
                }

            if (pwmServoDataPackets.Count > 0)
                return new Pca9685PwmServosPacketData
                {
                    Servos = pwmServoDataPackets.ToArray(),
                };

            return null;
        }
    }
}
