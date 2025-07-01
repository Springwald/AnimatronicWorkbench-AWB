// Send and receivce data to/from ESP-32 microcontroller
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("PacketLogisticsTests")]
namespace PacketLogistics.ComPorts
{
    public interface IComPortCommandConfig
    {
        string PacketHeader { get; }
        string PacketFooter { get; }
        byte[] PacketHeaderAsBytes { get; }
        byte[] PacketFooterAsBytes { get; }
        Encoding Encoding { get; }
    }

    public class ComPortCommandConfig : IComPortCommandConfig
    {
        public string PacketHeader { get; init; }
        public string PacketFooter { get; init; }
        public byte[] PacketHeaderAsBytes { get; init; }
        public byte[] PacketFooterAsBytes { get; init; }
        public Encoding Encoding { get; init; }

        public ComPortCommandConfig(string packetIdentifier = "pct") : this(encoding: Encoding.ASCII, packetIdentifier: packetIdentifier)
        { }

        public ComPortCommandConfig(Encoding encoding, string packetIdentifier = "pct")
        {
            if (string.IsNullOrWhiteSpace(packetIdentifier))
                throw new System.ArgumentNullException(nameof(packetIdentifier), "must not be null or empty");

            if (packetIdentifier.IndexOf("<") != -1)
                throw new System.ArgumentOutOfRangeException(nameof(packetIdentifier), "must not contain '<' character");

            if (packetIdentifier.IndexOf(">") != -1)
                throw new System.ArgumentOutOfRangeException(nameof(packetIdentifier), "must not contain '>' character");

            PacketHeader = $"<{packetIdentifier}>";
            PacketFooter = $"</{packetIdentifier}>";
            PacketHeaderAsBytes = ByteArrayConverter.AsciiStringToBytes(PacketHeader);
            PacketFooterAsBytes = ByteArrayConverter.AsciiStringToBytes(PacketFooter);
            Encoding = encoding;
        }
    }
}
