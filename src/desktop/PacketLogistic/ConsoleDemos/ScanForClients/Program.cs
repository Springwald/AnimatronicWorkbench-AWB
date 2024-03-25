using PacketLogistics.ComPorts;

var config = new ComPortCommandConfig(packetHeader: "AWB");

Console.WriteLine("Scanning for clients...");
var clientIdScanner = new ClientIdScanner(config);
clientIdScanner.OnLog += (s, e) => Console.WriteLine(e);
var clients = await clientIdScanner.FindAllClientsAsync(useComPortCache: false);

Console.WriteLine($"Scanning for clients done. Found {clients.Length} clients.");
foreach (var client in clients)
{
    Console.WriteLine($"{client.ComPortName}/{client.Caption}/{client.DeviceId}: ClientID={client.ClientId}");
}