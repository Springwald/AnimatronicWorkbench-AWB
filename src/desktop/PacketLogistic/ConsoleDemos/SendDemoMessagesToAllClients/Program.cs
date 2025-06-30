// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using PacketLogistics.ComPorts;
using SendDemoMessagesToAllClients;

Console.WriteLine("Scanning for clients...");
var config = new ComPortCommandConfig(packetIdentifier: "AWB");
var clientIdScanner = new ClientIdScanner(config);
var clients = await clientIdScanner.FindAllClientsAsync(useComPortCache: false);

Console.WriteLine($"Scanning for clients done. Found {clients.Length} clients.");

if (clients.Any())
{
    foreach (var client in clients)
    {
        Console.WriteLine($"{client.ComPortName}/{client.Caption}/{client.DeviceId}: ClientID={client.ClientId}");
    }

    // Now that we have a list of clients, we can send messages to them.
    var senderReceivers = clients.Select(client => new PacketSenderReceiverComPort<PayloadTypes>(client.ComPortName, client.ClientId, config)).ToArray();

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
            var payload = $"Hello world {count++}, client {senderReceiver.ClientId}!";
            var result = await senderReceiver.SendPacket(payloadType: PayloadTypes.Dummy, payload: payload);

            if (result.Ok == false)
            {
                lost++;
                ShowStatus(lost, ok);
            }
            else
            {
                ok++;
            }
            if ((lost + ok) % 100 == 0)
            {
                ShowStatus(lost, ok);
            }
        }
}

static void ShowStatus(int lost, int ok)
{
    Console.WriteLine($"ok:{ok} lost:{lost}");
}