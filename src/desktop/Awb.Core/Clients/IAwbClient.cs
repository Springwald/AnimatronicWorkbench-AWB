// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Clients.Models;

namespace Awb.Core.Clients
{
    public interface IAwbClient
    {
        /// <summary>
        /// the unique id of the AWB hardware client
        /// </summary>
        uint ClientId { get; }

        /// <summary>
        /// a friendly name for the client, to show it in lists or other UI elements
        /// </summary>
        string FriendlyName { get; }

        /// <summary>
        /// when has the last error occurred
        /// </summary>
        DateTime? LastErrorUtc { get; }

        /// <summary>
        /// a payload was received from the client
        /// </summary>
        EventHandler<ReceivedEventArgs>? Received { get; set; }

        /// <summary>
        /// a payload was sent to the client (for debugging purposes)
        /// </summary>
        EventHandler<string>? PacketSending { get; set; }

        /// <summary>
        /// the client has send an error message or the communication to the client failed
        /// </summary>
        EventHandler<string>? OnError { get; set; }

        /// <summary>
        /// initialize the client
        /// </summary>
        Task<bool> InitAsync();

        /// <summary>
        /// send a payload to the client
        /// </summary>
        Task<SendResult> Send(byte[] payload, string? debugInfos);
    }
}
