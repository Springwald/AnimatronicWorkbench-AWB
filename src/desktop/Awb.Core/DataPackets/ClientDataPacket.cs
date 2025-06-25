// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Actuators;

namespace Awb.Core.DataPackets
{
    public class ClientDataPacket
    {
        public uint ClientId { get; }
        public DataPacketContent Content { get; }

        /// <summary>
        /// Which actuators are affected by this data packet and can be set from "isDirty" to not "isDirty" after the packet has been processed?
        /// </summary>
        public IActuator[] AffectedAcctuatorsToRemoveDirtyFlag { get; }

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

        public ClientDataPacket(uint clientId, DataPacketContent dataPacketContent, IActuator[] affectedAcctuatorsToRemoveDirtyFlag)
        {
            ClientId = clientId;
            Content = dataPacketContent;
            AffectedAcctuatorsToRemoveDirtyFlag = affectedAcctuatorsToRemoveDirtyFlag ?? Array.Empty<IActuator>();
        }
    }
}
