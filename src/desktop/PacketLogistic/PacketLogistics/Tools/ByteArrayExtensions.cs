// Send and receivce data to/from ESP-32 microcontroller
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace PacketLogistics.Tools
{
    internal static class ByteArrayExtensions
    {
        public static bool EndsWith(this byte[] data, byte[] ending)
        {
            if (ending.Length > data.Length) return false;
            for (int i = 0; i < ending.Length; i++)
            {
                if (data[data.Length - ending.Length + i] != ending[i]) return false;
            }
            return true;
        }

        public static bool Contains(this byte[] data, byte[] ending)
        {
            if (ending.Length > data.Length) return false;
            for (int i = 0; i < data.Length - ending.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < ending.Length; j++)
                {
                    if (data[i + j] != ending[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found) return true;
            }
            return false;
        }

        public static bool EndsWith(this List<byte> data, byte[] ending)
        {
            if (ending.Length > data.Count) return false;
            for (int i = 0; i < ending.Length; i++)
            {
                if (data[data.Count - ending.Length + i] != ending[i]) return false;
            }
            return true;
        }
    }
}
