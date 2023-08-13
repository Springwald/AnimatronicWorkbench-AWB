// AnimatronicWorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Clients
{
    public interface IAwbClient
    {
        public class ReceivedEventArgs : EventArgs
        {
            public byte[] Payload { get; }

            public ReceivedEventArgs(byte[] payload)
            {
                Payload = payload;
            }
        }

        public class SendResult
        {
            public bool Ok { get; }
            public string? ErrorMessage { get; }

            public string? DebugInfos { get; }

            public SendResult(bool ok, string? errorMessage, string? debugInfos)
            {
                Ok = ok;
                ErrorMessage = errorMessage;
                DebugInfos = debugInfos;
            }
        }

        uint ClientId { get; }
        string FriendlyName { get; }

        EventHandler<ReceivedEventArgs> Received { get; }

        Task<bool> Init();
        Task<SendResult> Send(byte[] payload);
    }
}
