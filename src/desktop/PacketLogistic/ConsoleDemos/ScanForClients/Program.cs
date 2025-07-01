// Send and receivce data to/from ESP-32 microcontroller
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using PacketLogistics.ComPorts;

var config = new ComPortCommandConfig(packetIdentifier: "AWB");

Console.WriteLine("Scanning for clients...");
var clientIdScanner = new ClientIdScanner(config);
clientIdScanner.OnLog += (s, e) => Console.WriteLine(e);
var clients = await clientIdScanner.FindAllClientsAsync(useComPortCache: false);

Console.WriteLine($"Scanning for clients done. Found {clients.Length} clients.");
foreach (var client in clients)
{
    Console.WriteLine($"{client.ComPortName}/{client.Caption}/{client.DeviceId}: ClientID={client.ClientId}");
}