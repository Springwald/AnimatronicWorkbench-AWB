using Awb.Core.DataPackets;
using Awb.Core.Services;
using PacketLogistics.ComPorts;
using PacketLogistics.ComPorts.ComportPackets;
using System.Text;
using System.Text.Json;

var logger = new AwbLoggerConsole(throwWhenInDebugMode: false);

var config = new ComPortCommandConfig(packetHeader: "AWB");

Console.WriteLine("Scanning for clients...");
var clientIdScanner = new ClientIdScanner(config);
clientIdScanner.OnLog += async (s, e) => logger.LogAsync(e);
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

    int count = 400;
    int waitMs = 50;
    PacketSendResult result;

    while (Console.KeyAvailable == false)
        foreach (var senderReceiver in senderReceivers)
        {
            count++;
            if (false)
            {
                // send real data
                var contentObj = new DataPacketContent
                {
                    DisplayMessage = new DisplayMessage(message: $"Hello client {senderReceiver.ClientId}! " + count++, durationMs: 100)
                };
                var jsonStr = JsonSerializer.Serialize(contentObj);
                result = await senderReceiver.SendPacket(Encoding.ASCII.GetBytes(jsonStr));
                if (result.Ok == false)
                {
                    Console.WriteLine($"Error sending real packet {result.OriginalPacketId} to client {senderReceiver.ClientId}: {result.Message}");
                }
                else
                {
                    Console.WriteLine($"Sent real packet {result.OriginalPacketId} to client {senderReceiver.ClientId}: {result.Message}");
                }
                await Task.Delay(waitMs);
            }

            if (true)
            {
                // send byte data
                var bytes = new List<byte>();
                for (int i = 0; i < count; i++)
                {
                    bytes.Add((byte)i);
                }
                result = await senderReceiver.SendPacket(new byte[count]);

                //result = await senderReceiver.SendPacket(new byte[] { 123, 34, 83, 84, 83, 34, 58, 123, 34, 83, 101, 114, 118, 111, 115, 34, 58, 91, 123, 34, 67, 104, 34, 58, 54, 44, 34, 84, 86, 97, 108, 34, 58, 50, 48, 52, 56, 44, 34, 78, 97, 109, 101, 34, 58, 34, 72, 101, 97, 100, 32, 114, 111, 116, 97, 116, 101, 34, 125, 44, 123, 34, 67, 104, 34, 58, 55, 44, 34, 84, 86, 97, 108, 34, 58, 50, 49, 50, 56, 44, 34, 78, 97, 109, 101, 34, 58, 34, 78, 101, 99, 107, 32, 114, 105, 103, 104, 116, 34, 125, 44, 123, 34, 67, 104, 34, 58, 56, 44, 34, 84, 86, 97, 108, 34, 58, 50, 48, 54, 52, 44, 34, 78, 97, 109, 101, 34, 58, 34, 78, 101, 99, 107, 32, 108, 101, 102, 116, 34, 125, 44, 123, 34, 67, 104, 34, 58, 57, 44, 34, 84, 86, 97, 108, 34, 58, 53, 48, 50, 44, 34, 78, 97, 109, 101, 34, 58, 34, 65, 114, 109, 32, 114, 105, 103, 104, 116, 34, 125, 44, 123, 34, 67, 104, 34, 58, 49, 49, 44, 34, 84, 86, 97, 108, 34, 58, 51, 53, 52, 57, 44, 34, 78, 97, 109, 101, 34, 58, 34, 65, 114, 109, 32, 108, 101, 102, 116, 34, 125, 93, 125, 44, 34, 83, 67, 83, 34, 58, 123, 34, 83, 101, 114, 118, 111, 115, 34, 58, 91, 123, 34, 67, 104, 34, 58, 49, 44, 34, 84, 86, 97, 108, 34, 58, 53, 49, 50, 44, 34, 78, 97, 109, 101, 34, 58, 34, 69, 121, 101, 115, 32, 117, 112, 34, 125, 44, 123, 34, 67, 104, 34, 58, 50, 44, 34, 84, 86, 97, 108, 34, 58, 53, 49, 50, 44, 34, 78, 97, 109, 101, 34, 58, 34, 69, 121, 101, 115, 32, 108, 111, 119, 34, 125, 44, 123, 34, 67, 104, 34, 58, 51, 44, 34, 84, 86, 97, 108, 34, 58, 53, 55, 50, 44, 34, 78, 97, 109, 101, 34, 58, 34, 77, 111, 117, 116, 104, 34, 125, 44, 123, 34, 67, 104, 34, 58, 52, 44, 34, 84, 86, 97, 108, 34, 58, 49, 51, 55, 44, 34, 78, 97, 109, 101, 34, 58, 34, 69, 97, 114, 32, 114, 105, 103, 104, 116, 34, 125, 44, 123, 34, 67, 104, 34, 58, 53, 44, 34, 84, 86, 97, 108, 34, 58, 56, 51, 48, 44, 34, 78, 97, 109, 101, 34, 58, 34, 69, 97, 114, 32, 108, 101, 102, 116, 34, 125, 44, 123, 34, 67, 104, 34, 58, 49, 48, 44, 34, 84, 86, 97, 108, 34, 58, 53, 52, 56, 44, 34, 78, 97, 109, 101, 34, 58, 34, 65, 114, 109, 32, 108, 111, 119, 101, 114, 32, 114, 105, 103, 104, 116, 34, 125, 44, 123, 34, 67, 104, 34, 58, 49, 50, 44, 34, 84, 86, 97, 108, 34, 58, 51, 52, 54, 44, 34, 78, 97, 109, 101, 34, 58, 34, 65, 114, 109, 32, 108, 111, 119, 101, 114, 32, 108, 101, 102, 116, 34, 125, 93, 125, 125 });
                /*result = await senderReceiver.SendPacket(new byte[] {
                123, 34, 83, 84, 83, 34, 58, 123, 34, 83, 101, 114, 118, 111, 115, 34,
                58, 91, 123, 34, 67, 104, 34, 58, 54, 44, 34, 84, 86, 97, 108, 34, 58, 50, 48, 52, 56, 44, 34,
                78, 97, 109, 101, 34, 58, 34, 72, 101, 97, 100, 32, 114, 111, 116, 97, 116, 101, 34, 125, 44,
                123, 34, 67, 104, 34, 58, 55, 44, 34, 84, 86, 97, 108, 34, 58, 50, 49, 50, 56, 44, 34, 78, 97,
                109, 101, 34, 58, 34, 78, 101, 99, 107, 32, 114, 105, 103, 104, 116, 34, 125, 44, 123, 34, 67,  // ?
                104, 34, 58, 56, 44, 34, 84, 86, 97, 108, 34, 58, 50, 48, 54, 52, 44, 34, 78, 97, 109, 101, 34,
                58, 34, 78, 101, 99, 107, 32, 108, 101, 102, 116, 34, 125, 44, 123, 34, 67, 104, 34, 58, 57, 44,
                34, 84, 86, 97, 108, 34, 58, 53, 48, 50, 44, 34, 78, 97, 109, 101, 34, 58, 34, 65, 114, 109, 32,
                114, 105, 103, 104, 116, 34, 125, 44, 123, 34, 67, 104, 34, 58, 49, 49, 44, 34, 84, 86, 97, 108,
                34, 58, 51, 53, 52, 57, 44, 34, 78, 97, 109, 101, 34, 58, 34, 65, 114, 109, 32, 108, 101, 102, 116,
                34, 125, 93, 125, 44, 34, 83, 67, 83, 34, 58, 123, 34, 83, 101, 114, 118, 111, 115, 34, 58, 91,
                123, 34, 67, 104, 34, 58, 49, 44, 34, 84, 86, 97, 108, 34, 58, 53, 49, 50, 44, 34, 78, 97, 109,
                101, 34, 58, 34, 69, 121, 101, 115, 32, 117, 112, 34, 125, 44, 123, 34, 67, 104, 34, 58, 50,
                44, 34, 84, 86, 97, 108, 34, 58, 53, 49, 50, 44, 34, 78, 97, 109, 101, 34, 58, 34, 69, 121,
                101, 115, 32, 108, 111, 119, 34, 125, 44, 123, 34, 67, 104, 34, 58, 51, 44, 34, 84, 86, 97,
                108, 34, 58, 53, 55, 50, 44, 34, 78, 97, 109, 101, 34, 58, 34, 77, 111, 117, 116, 104, 34,
                125, 44, 123, 34, 67, 104, 34, 58, 52, 44, 34, 84, 86, 97, 108, 34, 58, 49, 51, 55, 44, 34,
                78, 97, 109, 101, 34, 58, 34, 69, 97, 114, 32, 114, 105, 103, 104, 116, 34, 125, 44, 123,
                34, 67, 104, 34, 58, 53, 44, 34, 84, 86, 97, 108, 34, 58, 56, 51, 48, 44, 34, 78, 97, 109,
                101, 34, 58, 34, 69, 97, 114, 32, 108, 101, 102, 116, 34, 125, 44, 123, 34, 67, 104, 34, 58,
                49, 48, 44, 34, 84, 86, 97, 108, 34, 58, 53, 52, 56, 44, 34, 78, 97, 109, 101, 34, 58, 34, 65,
                114, 109, 32, 108, 111, 119, 101, 114, 32, 114, 105, 103, 104, 116, 34, 125, 44, 123, 34, 67,
                104, 34, 58, 49, 50, 44, 34, 84, 86, 97, 108, 34, 58, 51, 52, 54, 44, 34, 78, 97, 109, 101,
                34, 58, 34, 65, 114, 109, 32, 108, 111, 119, 101, 114, 32, 108, 101, 102, 116, 34, 125, 93, 125
            });*/
                if (result.Ok == false)
                {
                    Console.WriteLine($"Error sending demo packet {result.OriginalPacketId} to client {senderReceiver.ClientId}, {count}: {result.Message}");
                    await Task.Delay(1000);
                }
                else
                {
                    Console.WriteLine($"Sent demo packet {result.OriginalPacketId} to client {senderReceiver.ClientId}, {count}: {result.Message}");
                    await Task.Delay(waitMs);
                }

              
            }

        }

}

static void ShowStatus(int lost, int ok)
{
    Console.WriteLine($"ok:{ok} lost:{lost}");
}