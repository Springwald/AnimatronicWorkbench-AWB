﻿// Communicate between different devices on dotnet or arduino via COM port or Wifi
// https://github.com/Springwald/PacketLogistics
//
// (C) 2024 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using PacketLogistics.ComPorts.ComportPackets;
using PacketLogistics.ComPorts.Serialization;
using PacketLogistics.Tools;
using System.Diagnostics;
using System.IO.Ports;

namespace PacketLogistics.ComPorts
{
    public class ClientIdScanner
    {
        private readonly IComPortCommandConfig _comPortCommandConfig;

        public event EventHandler<string>? OnLog;

        public ClientIdScanner(IComPortCommandConfig comPortCommandConfig)
        {
            _comPortCommandConfig = comPortCommandConfig;
        }

        public async Task<FoundClient[]> FindAllClientsAsync(bool useComPortCache)
        {
            var clients = new List<FoundClient>();
            var portInfoManager = new ComPortInfoManager();
            if (!useComPortCache) portInfoManager.ClearCache();
            var ports = portInfoManager.PortInfos;

            var portTasks = ports.Select(async port =>
            {
                if (port != null)
                {
                    var clientId = await DetectClientIdOnPortAsync(port.ComPort);
                    if (clientId != null && clientId > 0)
                    {
                        clients.Add(new FoundClient(
                            clientId: clientId.Value,
                            comPortName: port.ComPort,
                            caption: port.Caption,
                            deviceId: port.DeviceId));
                    }
                }
            }).ToArray();
            await Task.WhenAll(portTasks);

            return clients.ToArray();
        }

        private async Task<uint?> DetectClientIdOnPortAsync(string portName)
        {
            TimeSpan timeout = TimeSpan.FromSeconds(1);

            OnLog?.Invoke(this, $"ClientIdScanner: Scanning port {portName}");
            var serialPort = new Esp32SerialPort(portName);
            uint? found = null;
            try
            {
                OnLog?.Invoke(this, $"ClientIdScanner open port {portName}");
                serialPort.Open();
                if (serialPort.IsOpen == false)
                {
                    OnLog?.Invoke(this, $"ClientIdScanner port {portName} could not be opened");
                    return null;
                }
                OnLog?.Invoke(this, $"ClientIdScanner port {portName} is open");
                var waitUntil = DateTime.UtcNow + timeout;

                var receiveBuffer = new List<byte>();

                serialPort.WriteTimeout = 500;
                serialPort.Write(new[] { _comPortCommandConfig.SearchForClientByte }, offset: 0, count: 1);

                int bytesReceived = 0;

                while (DateTime.UtcNow < waitUntil && found == null)
                {
                    if (serialPort.BytesToRead > 0)
                    {
                        var bt = (byte)serialPort.ReadByte();
                        bytesReceived++;
                        receiveBuffer.Add(bt);

                        if (receiveBuffer.EndsWith(_comPortCommandConfig.PacketHeaderBytes))
                        {
                            var packetContent = receiveBuffer.Take(receiveBuffer.Count - _comPortCommandConfig.PacketHeaderBytes.Length).ToArray();

                            if (packetContent.Length > 0)
                            {

                                OnLog?.Invoke(this, $"ClientIdScanner port {portName}: received packet.");

                                // deserialize packet
                                var serializer = new PacketSerializer(_comPortCommandConfig);
                                var packet = serializer.GetPacketFromByteArray(packetContent, errorMsg: out string? errorMsg);
                                if (packet == null)
                                {
                                    if (errorMsg != null)
                                    {
                                        OnLog?.Invoke(this, $"ClientIdScanner port {portName}: Error deserializing packet: {errorMsg}");
                                    }
                                }
                                else
                                {
                                    switch (packet.PacketType)
                                    {
                                        case PacketBase.PacketTypes.AlifePacket:
                                            var alifePacket = (AlifePacket)packet;
                                            found = alifePacket?.ClientId;
                                            OnLog?.Invoke(this, $"ClientIdScanner port {portName}: AlifePacket 👍!");
                                            break;

                                        case PacketBase.PacketTypes.DataPacket:
                                            var dataPacket = (DataPacket)packet;
                                            found = dataPacket?.SenderId;
                                            OnLog?.Invoke(this, $"ClientIdScanner port {portName}: DataPacket 🤔...");
                                            break;

                                        case PacketBase.PacketTypes.ResponsePacket:
                                            var responsePacket = (ResponsePacket)packet; OnLog?.Invoke(this, $"ClientIdScanner port {portName}: ResponsePacket🤔...");
                                            break;

                                        default:
                                            OnLog?.Invoke(this, $"ClientIdScanner port {portName}: Unknown packet type '{packet.PacketType}' 😬");
                                            if (Debugger.IsAttached)
                                                throw new ArgumentOutOfRangeException($"ClientIdScanner port {portName}: Unknown packet type '{packet.PacketType}'");
                                            break;
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
                OnLog?.Invoke(this, $"ClientIdScanner port {portName}: received {bytesReceived} bytes.");
            }
            catch (Exception e)
            {
                OnLog?.Invoke(this, $"ClientIdScanner port {portName}: Error {e.Message}");
            }
            finally
            {
                if (serialPort.IsOpen) serialPort.Close();
                OnLog?.Invoke(this, $"ClientIdScanner closing port {portName}");
            }
            return found;
        }
    }
}
