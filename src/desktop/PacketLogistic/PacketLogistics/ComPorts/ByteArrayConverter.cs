// Send and receivce data to/from ESP-32 microcontroller
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("PacketLogisticsTests")]
namespace PacketLogistics.ComPorts
{
    public static class ByteArrayConverter
    {
        const byte splitPoint = 128;

        /// <summary>
        /// to prevent usage of the reserved bytes for packet headers, we split the bytes into 2 parts
        /// </summary>
        public static IEnumerable<byte> SplitByte(byte b) => SplitBytes(new byte[] { b });

        /// <summary>
        /// to prevent usage of the reserved bytes for packet headers, we split the bytes into 2 parts
        /// </summary>
        public static IEnumerable<byte> SplitBytes(byte[] bytes)
        {
            foreach (var b in bytes)
            {
                if (b > splitPoint)
                {
                    yield return splitPoint;
                    yield return (byte)(b - splitPoint);
                }
                else
                {
                    yield return b;
                    yield return 0;
                }
            }
        }

        public static IEnumerable<byte> UnSplitBytes(byte[] bytes)
        {
            for (var i = 0; i < bytes.Length - 1; i += 2)
                yield return (byte)(bytes[i] + bytes[i + 1]);
        }

        public static byte[] UintTo4Bytes(uint value)
        {
            return new byte[4] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value };
        }

        public static uint UintFrom4Bytes(byte[] value)
        {
            if (value.Length != 4) throw new ArgumentOutOfRangeException(paramName: nameof(value), message: "value must be 4 bytes long");
            return (uint)(value[0] << 24 | value[1] << 16 | value[2] << 8 | value[3]);
        }

        public static byte[] AsciiStringToBytes(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return Array.Empty<byte>();
            return Encoding.ASCII.GetBytes(value);
        }

        public static string BytesToAsciiString(byte[] value)
        {
            if (value == null) return string.Empty;
            return Encoding.ASCII.GetString(value);
        }

        public static byte[]? GetNextBytes(byte[] value, int count, ref int pos)
        {
            if (pos + count > value.Length) return null;
            var result = value[pos..(pos + count)];
            pos += count;
            return result;
        }

        public static bool AreEqual(byte[]? value1, byte[]? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            if (value1.Length != value2.Length) return false;
            for (int i = 0; i < value1.Length; i++)
            {
                if (value1[i] != value2[i]) return false;
            }
            return true;

        }
    }
}
