﻿// Send and receivce data to/from ESP-32 microcontroller
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using PacketLogistics.Tools;

namespace PacketLogistics.ComPorts
{
    public class ClientIdScanner
    {
        private readonly IComPortCommandConfig _comPortCommandConfig;


        public delegate void ScanningProgressMessageHandler(string message);

        private enum DummyPayloadTypes
        {
            Dummy = 1
        }

        public event EventHandler<string>? OnLog;

        public ClientIdScanner(IComPortCommandConfig comPortCommandConfig)
        {
            _comPortCommandConfig = comPortCommandConfig;
        }

        public async Task<FoundClient[]> FindAllClientsAsync(bool useComPortCache, ScanningProgressMessageHandler scanningProgressMessageHandler)
        {

            scanningProgressMessageHandler("ClientIdScanner: Starting to scan for clients...");
            scanningProgressMessageHandler("Use ComPortCache: " + useComPortCache);

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
                        scanningProgressMessageHandler($"ClientIdScanner: Found client with ID {clientId} on port {port.ComPort}");
                    }
                    else
                    {
                        scanningProgressMessageHandler($"ClientIdScanner: No client found on port {port.ComPort}");
                    }
                }
            }).ToArray();

            var parallel = true;
            if (parallel)
            {
                // run all tasks in parallel
                await Task.WhenAll(portTasks);
            }
            else
            {
                // run tasks sequentially
                foreach (var task in portTasks)
                    await task;
            }

            scanningProgressMessageHandler($"ClientIdScanner: Scanning completed. Found {clients.Count} clients.");

            return clients.ToArray();
        }

        private async Task<uint?> DetectClientIdOnPortAsync(string portName)
        {
            TimeSpan timeout = TimeSpan.FromMilliseconds(500);

            OnLog?.Invoke(this, $"ClientIdScanner: Scanning port {portName}");

            var serialPort = new Esp32SerialPort(portName);
            serialPort.WriteTimeout = (int)timeout.TotalMilliseconds;
            serialPort.ReadTimeout = (int)timeout.TotalMilliseconds;

            OnLog?.Invoke(this, $"ClientIdScanner open port {portName}");

            try
            {
                serialPort.Open();
            }
            catch (Exception ex)
            {
                OnLog?.Invoke(this, $"{portName} could not be opened: {ex.Message}");
                return null;
            }

            if (serialPort.IsOpen == false)
            {
                OnLog?.Invoke(this, $"ClientIdScanner port {portName} could not be opened");
                return null;
            }

            OnLog?.Invoke(this, $"ClientIdScanner port {portName} is open");
            var waitUntil = DateTime.UtcNow + timeout;

            var packetReceived = false;
            uint clientId = 0;

            try
            {
                serialPort.Write([255], 0, 1); // send a dummy byte 255 to wake up the device to receive a alive packet
            }
            catch (Exception ex)
            {
                OnLog?.Invoke(this, $"ClientIdScanner port {portName} could not send data: {ex.Message}");
                serialPort.Close();
                return null;
            }

            var packetReceiver = new PacketReceiver<DummyPayloadTypes>(
                serialPort,
                _comPortCommandConfig,
                packetReceivedDelegate: (packet) =>
                {
                    // This delegate is intentionally left empty, as we are not processing packets here.
                    // We just want to receive packets to detect the client ID.
                    switch (packet.PacketType)
                    {
                        case PacketPayloadWrapper.PacketEnvelope<DummyPayloadTypes>.PacketTypes.NotSet:
                            OnLog?.Invoke(this, $"ClientIdScanner port {portName}: NoSet packet received, this should not happen.");
                            break;

                        case PacketPayloadWrapper.PacketEnvelope<DummyPayloadTypes>.PacketTypes.AlivePacket:
                            OnLog?.Invoke(this, $"ClientIdScanner port {portName}: AlifePacket 👍!");
                            break;

                        case PacketPayloadWrapper.PacketEnvelope<DummyPayloadTypes>.PacketTypes.ResponsePacket:
                            OnLog?.Invoke(this, $"ClientIdScanner port {portName}: ResponsePacket🤔...");
                            break;

                        case PacketPayloadWrapper.PacketEnvelope<DummyPayloadTypes>.PacketTypes.PayloadPacket:
                            OnLog?.Invoke(this, $"ClientIdScanner port {portName}: DataPacket 🤔...");
                            break;

                        default:
                            OnLog?.Invoke(this, $"ClientIdScanner port {portName}: Unknown packet type '{packet.PacketType}'");
                            break;
                    }

                    packetReceived = true;
                    clientId = packet.ClientId;
                },
                errorDelegate: (errorMsg) =>
                {
                    OnLog?.Invoke(this, $"ClientIdScanner port {portName}: Error receiving packet: {errorMsg}");
                }
            );


            while (DateTime.UtcNow < waitUntil && packetReceived == false)
            {
                await packetReceiver.TryReceivePacket();
                await Task.Delay(10);
            }

            OnLog?.Invoke(this, $"ClientIdScanner closing port {portName}");
            serialPort.Close();

            if (packetReceived)
            {
                OnLog?.Invoke(this, $"ClientIdScanner port {portName}: ClientId {clientId} detected.");
                return clientId;
            }

            OnLog?.Invoke(this, $"ClientIdScanner port {portName}: No response received within timeout.");
            return null;
        }
    }
}
