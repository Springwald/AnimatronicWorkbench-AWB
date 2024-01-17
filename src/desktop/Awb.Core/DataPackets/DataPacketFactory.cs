// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;

namespace Awb.Core.DataPackets
{
    public class DataPacketFactory
    {
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

                if (stsServos != null || pwmServos != null)
                {
                    yield return new ClientDataPacket
                    {
                        ClientId = servosByClient.ClientId,
                        Content = new DataPacketContent
                        {
                            DisplayMessage = null,
                            StsServos = stsServos,
                            ScsServos = scsServos,
                            Pca9685PwmServos = pwmServos
                        }
                    };
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
                        Name = string.IsNullOrWhiteSpace(stsServo.Name) ? $"STS{stsServo.Channel}" : stsServo.Name,
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
                        Name = string.IsNullOrWhiteSpace(pwmServo.Name) ? $"STS{pwmServo.Channel}" : pwmServo.Name,
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
