// AnimatronicWorkBench core routines
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
                var stsServos = this.GetStsServoChanges(servosByClient.Servos);
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
                            PwmServos = pwmServos
                        }
                    };
                }
            }
        }

        public void SetDataPacketDone(IServo[] servos, ClientDataPacket clientDataPacket)
        {
            foreach (var servoPacketData in clientDataPacket.Content.StsServos.Servos)
            {
                var stsServo = servos.Select(s => s as StsServo).FirstOrDefault(s => s?.Channel == servoPacketData.Channel && s.ClientId == clientDataPacket.ClientId);
                {
                    if (stsServo != null) stsServo.IsDirty = false;
                }
            }
        }

        private StsServosPacketData? GetStsServoChanges(IServo[] allSservos)
        {
            var stsServos = new List<StsServoPacketData>();

            foreach (var servo in allSservos)
            {
                var stsServo = servo as StsServo;
                if (stsServo != null && stsServo.IsDirty)
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

        private AdafruitPwm[]? GetPwmServoChanges(IServo[] allServos)
        {
            return null;
        }


    }
}
