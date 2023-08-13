// AnimatronicWorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.DataPackets
{
    public class ClientDataPacket
    {
        public uint ClientId { get; internal set; }
        public DataPacketContent Content { get; internal set; }

        public bool IsEmpty
        {
            get
            {
                if (Content == null) return true;

                if (Content.DisplayMessage?.Message != null && !string.IsNullOrWhiteSpace(Content.DisplayMessage.Message)) return false;
                if (Content.StsServos?.Servos?.Any() == true) return false;
                if (Content.PwmServos?.Any() == true) return false;
                return true;
            }
        }
    }
}
