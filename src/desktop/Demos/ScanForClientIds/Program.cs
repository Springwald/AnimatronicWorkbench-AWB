using Awb.Core.DataPackets;
using Awb.Core.Services;
using PacketLogistics.ComPorts;
using System.Text;
using System.Text.Json;

var logger = new AwbLoggerConsole(throwWhenInDebugMode: false);

var config = new ComPortCommandConfig(packetHeader: "AWB");

Console.WriteLine("Scanning for clients...");
var clientIdScanner = new ClientIdScanner(config);
var clients = await clientIdScanner.FindAllClients(useComPortCache: true);
if (clients.Any() == false)
{
    clients = await clientIdScanner.FindAllClients(useComPortCache: false);
}

Console.WriteLine($"Scanning for clients done. Found {clients.Length} clients.");

if (clients.Any())
{
    foreach (var client in clients)
    {
        Console.WriteLine($"{client.ComPortName}/{client.Caption}/{client.DeviceId}: ClientID={client.ClientId}");
    }

    // Now that we have a list of clients, we can send messages to them.
    var senderReceivers = clients.Select(client => new PacketSenderReceiverComPort(client.ComPortName, client.ClientId, config)).ToArray();

    foreach (var senderReceiver in senderReceivers)
    {
        if (await senderReceiver.Connect() == false)
        {
            Console.WriteLine($"Can`t connected to client {senderReceiver.ClientId}");
        }
        else
        {
            senderReceiver.ErrorOccured += (sender, args) => Console.WriteLine($"Error occured: {args.Message}");
        }
    }

    int count = 0;

    while (Console.KeyAvailable == false)
        foreach (var senderReceiver in senderReceivers)
        {
            var contentObj = new DataPacketContent
            {
                DisplayMessage = new DisplayMessage(message: $"Hello client {senderReceiver.ClientId}! " + count++, durationMs: 100)
            };
            var jsonStr = JsonSerializer.Serialize(contentObj);
            var result = await senderReceiver.SendPacket(Encoding.ASCII.GetBytes(jsonStr));
            if (result.Ok == false)
            {
                Console.WriteLine($"Error sending packet to client {senderReceiver.ClientId}: {result.Message}");
            }
            // await Task.Delay(150);
        }

}

static void ShowStatus(int lost, int ok)
{
    Console.WriteLine($"ok:{ok} lost:{lost}");
}