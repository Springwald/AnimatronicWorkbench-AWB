using PacketLogistics.ComPorts;

var config = new ComPortCommandConfig(packetHeader: "AWB");

Console.WriteLine("Scanning for clients...");
var clientIdScanner = new ClientIdScanner(config);
var clients = await clientIdScanner.FindAllClients(useComPortCache: false);

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
    int lost = 0;
    int ok = 0;

    while (Console.KeyAvailable == false)
        foreach (var senderReceiver in senderReceivers)
        {
            var payload = ByteArrayConverter.AsciiStringToBytes($"Hello world {count++}, client {senderReceiver.ClientId}!");
            var result = await senderReceiver.SendPacket(payload);

            if (result.Ok == false)
            {
                lost++;
                ShowStatus(lost, ok);
            }
            else
            {
                ok++;
            }
            if ((lost + ok ) % 100 == 0)
            {
                ShowStatus(lost, ok);
            }
        }
}

static void ShowStatus(int lost, int ok)
{
    Console.WriteLine($"ok:{ok} lost:{lost}");
}