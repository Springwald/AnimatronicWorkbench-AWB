// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.DataPackets
{
    public class ClientDataPacket
    {
        public uint ClientId { get; }
        public DataPacketContent Content { get; }

        public bool IsEmpty
        {
            get
            {
                if (Content == null) return true;
                if (Content.ReadValue != null) return false;
                if (Content.DisplayMessage?.Message != null && !string.IsNullOrWhiteSpace(Content.DisplayMessage.Message)) return false;
                if (Content.StsServos?.Servos?.Any() == true) return false;
                if (Content.ScsServos?.Servos?.Any() == true) return false;
                if (Content.Pca9685PwmServos?.Servos?.Any() == true) return false;
                return true;
            }
        }

        public ClientDataPacket(uint clientId, DataPacketContent dataPacketContent)
        {
            ClientId = clientId;
            Content = dataPacketContent;
        }
    }
}
