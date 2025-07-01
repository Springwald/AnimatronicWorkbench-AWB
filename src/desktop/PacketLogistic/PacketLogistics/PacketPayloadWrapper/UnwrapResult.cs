// Send and receivce data to/from ESP-32 microcontroller
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace PacketLogistics.PacketPayloadWrapper
{
    internal class UnwrapResult<PayloadTypes> where PayloadTypes : Enum
    {
        /// <summary>
        /// The unwrapped packet.
        /// </summary>
        public PacketEnvelope<PayloadTypes>? Packet { get; internal set; }

        /// <summary>
        /// If the unwrapping failed, this contains an error message.
        /// </summary>
        public string? ErrorMessage { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the operation completed successfully.
        /// If this is false, <see cref="ErrorMessage"/> will contain a description of the error.
        /// </summary>
        public bool Ok { get; internal set; }

    }
}
