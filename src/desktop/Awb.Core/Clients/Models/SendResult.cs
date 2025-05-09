// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Clients.Models
{
    public class SendResult
    {
        public bool Ok { get; }
        public string? ErrorMessage { get; }
        public string? ResultPayload { get; }
        public string? DebugInfos { get; }

        public SendResult(bool ok, string? errorMessage, string? resultPlayload, string? debugInfos)
        {
            Ok = ok;
            ErrorMessage = errorMessage;
            ResultPayload = resultPlayload;
            DebugInfos = debugInfos;
        }
    }
}
