// Send and receivce data to/from ESP-32 microcontroller
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System.Text.Json;

namespace PacketLogistics.PacketPayloadWrapper
{
    internal class PacketWrapper<PayloadTypes> where PayloadTypes : Enum
    {
        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            IgnoreReadOnlyProperties = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            IgnoreReadOnlyFields = true,
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>
        /// Wraps the specified packet into a JSON-serialized envelope.
        /// </summary>
        /// <remarks>The method calculates a checksum for the packet's payload and includes it in the
        /// envelope. The resulting JSON string can be used for transmission</remarks>
        public string WrapDataPacket(uint packetId, PacketEnvelope<PayloadTypes>.PacketTypes packetType, PayloadTypes payLoadType, string? payload)
        {
            var envelope = new PacketEnvelope<PayloadTypes>
            {
                Id = packetId,
                PacketType = packetType,
                PayloadType = payLoadType,
                Payload = payload,
                Checksum = new PayloadChecksumCalculator().CalculateChecksum(payload: payload) // Simple checksum based on the payload
            };

            // serialize the envelope to json
            string json = JsonSerializer.Serialize(envelope, _jsonSerializerOptions);
            return json;
        }

        public UnwrapResult<PayloadTypes> UnwrapPacket(string wrappedPacket)
        {
            PacketEnvelope<PayloadTypes>? packetEnvelope = null;

            try
            {
                // Deserialize the JSON string to a PacketEnvelope object
                packetEnvelope = JsonSerializer.Deserialize<PacketEnvelope<PayloadTypes>>(wrappedPacket, _jsonSerializerOptions);
            }
            catch (Exception ex)
            {
                return new UnwrapResult<PayloadTypes>
                {
                    Ok = false,
                    ErrorMessage = $"Error deserializing packet '{wrappedPacket}': {ex.Message}"
                };
            }

            // Don't allow empty envelopes
            if (packetEnvelope == null)
            {
                return new UnwrapResult<PayloadTypes>
                {
                    Ok = false,
                    ErrorMessage = "Failed to deserialize packet envelope."
                };
            }

            // Validate checksum
            var correctChecksum = new PayloadChecksumCalculator().CalculateChecksum(payload: packetEnvelope.Payload ?? string.Empty);
            if (packetEnvelope.Checksum != correctChecksum)
            {
                return new UnwrapResult<PayloadTypes>
                {
                    Ok = false,
                    ErrorMessage = $"Checksum mismatch: received {packetEnvelope.Checksum} but must be {correctChecksum}"
                };
            }

            // Validate packet type
            if (packetEnvelope.PacketType == null)
            {
                return new UnwrapResult<PayloadTypes>
                {
                    Ok = false,
                    ErrorMessage = "Packet type is null."
                };
            }

            switch (packetEnvelope.PacketType)
            {
                case PacketEnvelope<PayloadTypes>.PacketTypes.NotSet:
                    return new UnwrapResult<PayloadTypes>
                    {
                        Ok = false,
                        ErrorMessage = "Packet type is not set."
                    };

                case PacketEnvelope<PayloadTypes>.PacketTypes.AlivePacket:
                case PacketEnvelope<PayloadTypes>.PacketTypes.ResponsePacket:
                    // These packet types do not contain a payload
                    break;

                case PacketEnvelope<PayloadTypes>.PacketTypes.PayloadPacket:
                    if (!Enum.IsDefined(typeof(PayloadTypes), packetEnvelope.PacketType))
                    {
                        return new UnwrapResult<PayloadTypes>
                        {
                            Ok = false,
                            ErrorMessage = $"Invalid packet type: {packetEnvelope.PacketType}"
                        };
                    }
                    if (string.IsNullOrEmpty(packetEnvelope.Payload))
                    {
                        return new UnwrapResult<PayloadTypes>
                        {
                            Ok = false,
                            ErrorMessage = "Payload is null or empty for a payload packet."
                        };
                    }
                    break;

                default:
                    return new UnwrapResult<PayloadTypes>
                    {
                        Ok = false,
                        ErrorMessage = $"Unknown packet type: {packetEnvelope.PacketType}"
                    };
            }

            return new UnwrapResult<PayloadTypes>
            {
                Ok = true,
                Packet = packetEnvelope,
                ErrorMessage = null
            };
        }

    }
}
