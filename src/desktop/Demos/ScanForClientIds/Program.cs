// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.DataPackets;
using Awb.Core.Services;
using PacketLogistics.ComPorts;
using PacketLogistics.ComPorts.ComportPackets;
using ScanForClientIds;
using System.Text.Json;

var logger = new AwbLoggerConsole(throwWhenInDebugMode: false);
var config = new ComPortCommandConfig(packetIdentifier: "AWB");

Console.WriteLine("Scanning for clients...");
var clientIdScanner = new ClientIdScanner(config);
clientIdScanner.OnLog += async (s, e) => { await logger.LogAsync(e); await Task.CompletedTask; };
var clients = await clientIdScanner.FindAllClientsAsync(useComPortCache: true);
if (clients.Any() == false)
{
    clients = await clientIdScanner.FindAllClientsAsync(useComPortCache: false);
}

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

    int count = 400;
    int waitMs = 50;
    PacketSendResult result;

    while (Console.KeyAvailable == false)
        foreach (var senderReceiver in senderReceivers)
        {
            count++;
            // send real data
            var contentObj = new DataPacketContent
            {
                DisplayMessage = new DisplayMessage(message: $"Hello client {senderReceiver.ClientId}! " + count++, durationMs: 100)
            };
            var jsonStr = JsonSerializer.Serialize(contentObj);
            result = await senderReceiver.SendPacket(payload: jsonStr, payloadType: PayloadTypes.Dummy);
            if (result.Ok == false)
            {
                Console.WriteLine($"Error sending real packet {result.OriginalPacketId} to client {senderReceiver.ClientId}: {result.ReturnPayload}");
            }
            else
            {
                Console.WriteLine($"Sent real packet {result.OriginalPacketId} to client {senderReceiver.ClientId}: {result.ReturnPayload}");
            }
            await Task.Delay(waitMs);
        }

}

