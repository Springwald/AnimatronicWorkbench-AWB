// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace PacketLogistics.ComPorts
{
    public static class ByteArrayExtensions
    {
        public static int IndexOf(this byte[] sequence, byte[] pattern)
        {
            var patternLength = pattern.Length;
            var matchCount = 0;

            for (var i = 0; i < sequence.Length; i++)
            {
                if (sequence[i] == pattern[matchCount])
                {
                    matchCount++;
                    if (matchCount == patternLength)
                    {
                        return i - patternLength + 1;
                    }
                }
                else
                {
                    matchCount = 0;
                }
            }

            return -1;
        }
    }
}
