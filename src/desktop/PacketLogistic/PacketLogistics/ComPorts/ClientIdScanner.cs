﻿// Communicate between different devices on dotnet or arduino via COM port or Wifi
// https://github.com/Springwald/PacketLogistics
//
// (C) 2023 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using PacketLogistics.ComPorts.ComportPackets;
using PacketLogistics.ComPorts.Serialization;
using PacketLogistics.Tools;
using System.Diagnostics;

namespace PacketLogistics.ComPorts
{
    public class ClientIdScanner
    {
        private readonly IComPortCommandConfig _comPortCommandConfig;
        public ClientIdScanner(IComPortCommandConfig comPortCommandConfig)
        {
            _comPortCommandConfig = comPortCommandConfig;
        }

        public async Task<string?> FindComPortName(int clientId)
        {
            var portInfoManager = new ComPortInfoManager();
            var ports = portInfoManager.PortInfos;
            foreach (var port in ports)
            {
                var foundId = await DetectClientIdOnPort(port.ComPort);
                if (foundId == clientId) return port.ComPort;
            }
            return null;
        }
        public async Task<FoundClient[]> FindAllClients(bool useComPortCache)
        {
            var clients = new List<FoundClient>();
            var portInfoManager = new ComPortInfoManager();
            if (!useComPortCache) portInfoManager.ClearCache();
            var ports = portInfoManager.PortInfos;
            foreach (var port in ports)
            {
                if (port != null)
                {
                    var clientId = await DetectClientIdOnPort(port.ComPort);
                    if (clientId != null)
                    {
                        clients.Add(new FoundClient(
                            clientId: clientId.Value,
                            comPortName: port.ComPort,
                            caption: port.Caption,
                            deviceId: port.DeviceId));
                    }
                }
            }
            return clients.ToArray();
        }

        private async Task<uint?> DetectClientIdOnPort(string portName)
        {
            TimeSpan timeout = TimeSpan.FromSeconds(10);

            Debug.WriteLine($"ClientIdScanner: Scanning port {portName}"); ;
            var serialPort = new Esp32SerialPort(portName);
            uint? found = null;
            try
            {
                serialPort.Open();
                var waitUntil = DateTime.UtcNow + timeout;

                var receiveBuffer = new List<byte>();
                bool firstPacketReceived = false;

                while (DateTime.UtcNow < waitUntil && found == null)
                {
                    if (serialPort.BytesToRead > 0)
                    {
                        receiveBuffer.Add((byte)serialPort.ReadByte());

                        if (receiveBuffer.EndsWith(_comPortCommandConfig.PacketHeaderBytes))
                        {
                            var packetContent = receiveBuffer.Take(receiveBuffer.Count - _comPortCommandConfig.PacketHeaderBytes.Length).ToArray();

                            if (packetContent.Length > 0)
                            {

                                if (firstPacketReceived == false)
                                {
                                    // Skip first packet, because it is catched in the middle of the transmission
                                    firstPacketReceived = true;
                                }
                                else
                                {

                                    // deserialize packet
                                    var serializer = new PacketSerializer(_comPortCommandConfig);
                                    var packet = serializer.GetPacketFromByteArray(packetContent, errorMsg: out string? errorMsg);
                                    if (packet == null)
                                    {
                                        if (errorMsg != null)
                                        {
                                            Console.WriteLine($"ClientIdScanner: Error deserializing packet: {errorMsg}");
                                        }
                                    }
                                    else
                                    {
                                        switch (packet.PacketType)
                                        {
                                            case PacketBase.PacketTypes.AlifePacket:
                                                var alifePacket = (AlifePacket)packet;
                                                found = alifePacket?.ClientId;
                                                break;

                                            case PacketBase.PacketTypes.DataPacket:
                                                var dataPacket = (DataPacket)packet;
                                                found = dataPacket?.SenderId;
                                                break;

                                            case PacketBase.PacketTypes.ResponsePacket:
                                                break;

                                            default:
                                                throw new ArgumentOutOfRangeException($"Unknown packet type '{packet.PacketType}'");
                                        }
                                    }
                                }
                            }
                            receiveBuffer.Clear();
                        }
                    }
                    else
                    {
                        await Task.Delay(10);
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                if (serialPort.IsOpen) serialPort.Close();
            }
            return found;
        }
    }
}
