// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PacketLogisticsTests")]
namespace PacketLogistics.ComPorts
{


    public interface IComPortCommandConfig
    {
        string PacketHeader { get; }
        byte[] PacketHeaderBytes { get; }
        byte[] CommandBytes { get; }
        byte SearchForClientByte { get; }
    }

    public class ComPortCommandConfig : IComPortCommandConfig
    {
        /// <summary>
        /// the raw application header for all packets, must have 3 chars
        /// </summary>
        public string PacketHeader { get; }

        /// <summary>
        /// the application header for all packets, has 9 bytes (3 start bytes, 3 header chars, 3 end bytes)   
        /// </summary>
        public byte[] PacketHeaderBytes { get; }

        public byte[] CommandBytes { get; }

        public byte SearchForClientByte { get; }

        public ComPortCommandConfig(string packetHeader, byte headerStartByte = (byte)250, byte headerEndByte = (byte)251, byte searchForClientByte = (byte)252)
        {
            if (packetHeader?.Length != 3) throw new System.ArgumentOutOfRangeException(nameof(packetHeader), "must have 3 chars");

            SearchForClientByte = searchForClientByte;

            var headerInBytes = ByteArrayConverter.AsciiStringToBytes(packetHeader);

            if (headerInBytes.Contains(searchForClientByte)) throw new System.ArgumentOutOfRangeException(nameof(packetHeader), "must not contain search-for-client byte");
            if (headerInBytes.Contains(headerStartByte)) throw new System.ArgumentOutOfRangeException(nameof(packetHeader), "must not contain start byte");
            if (headerInBytes.Contains(headerEndByte)) throw new System.ArgumentOutOfRangeException(nameof(packetHeader), "must not contain end byte");

            this.PacketHeader = packetHeader;
            this.PacketHeaderBytes = new[] {
                headerStartByte, headerStartByte, headerStartByte,
                headerInBytes[0], headerInBytes[1], headerInBytes[2],
                headerEndByte, headerEndByte, headerEndByte
            };
            this.CommandBytes = new[]
            {
                headerStartByte, headerEndByte, searchForClientByte,
            };
        }
    }
}
