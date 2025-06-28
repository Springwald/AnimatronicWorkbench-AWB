// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace PacketLogistics.PacketPayloadWrapper
{
    internal class PayloadChecksumCalculator
    {
        internal uint CalculateChecksum(string? payload)
        {
            if (string.IsNullOrEmpty(payload))
                return 0; // Return 0 if the payload is null or empty

            // Calculate a simple checksum by summing the ASCII values of the characters in the payload
            uint checksum = 0;
            foreach (char c in payload)
                checksum += c;

            // Return the checksum as an integer
            return checksum;
        }
    }
}
