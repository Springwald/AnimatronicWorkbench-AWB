// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Clients;
using PacketLogistics.ComPorts;

namespace Awb.Core.Services
{
    public interface IAwbClientsService
    {
        Esp32ComPortClient[] ComPortClients { get; }

        event EventHandler? ClientsLoaded;

        Task InitAsync();

        Esp32ComPortClient? GetClient(uint clientId);
    }

    public class AwbClientsService : IAwbClientsService
    {
        private Esp32ComPortClient[]? _clients;
        private readonly IAwbLogger? _logger;

        public event EventHandler? ClientsLoaded;

        public AwbClientsService(IAwbLogger? logger)
        {
            _logger = logger;
        }

        public Esp32ComPortClient[] ComPortClients
        {
            get
            {
                if (_clients == null)
                {
                    _logger?.Log($"Access to clients not available because still searching clients...");
                    return Array.Empty<Esp32ComPortClient>();
                }
                return _clients;
            }
        }

        public async Task InitAsync()
        {
            _logger?.Log($"\r\nSearching clients...");

            var config = new AwbEsp32ComportClientConfig();
            var clientIdScanner = new ClientIdScanner(config);
            clientIdScanner.OnLog += (s, e) => _logger?.Log(e);
            var foundComPortClients = await clientIdScanner.FindAllClientsAsync(useComPortCache: true);
            if (foundComPortClients.Any() == false)
            {
                foundComPortClients = await clientIdScanner.FindAllClientsAsync(useComPortCache: false);
            }

            var clients = foundComPortClients.Select(c => new Esp32ComPortClient(c.ComPortName, c.ClientId)).ToArray();
            var clientTasks = clients.Select(async c =>
            {
                _logger?.Log("Init client " + c.ClientId + "/" + c.FriendlyName);
                var ok = await c.InitAsync();
                if (ok == false) _logger?.LogError($"Can't init client {c.ClientId}/{c.FriendlyName}");
            }).ToArray();

            await Task.WhenAll(clientTasks);

            foreach (var client in clients)
            {
                client.OnError += (s, e) => _logger?.LogError($"Client {client.ClientId}/{client.FriendlyName}: {e}");
            }

            _clients = clients;
            _logger?.Log($"Found {_clients.Length} clients. ({string.Join(", ", _clients.Select(c => c.ClientId))})");

            if (ClientsLoaded != null)
                ClientsLoaded(this, EventArgs.Empty);
        }

        public Esp32ComPortClient? GetClient(uint clientId)
            => ComPortClients.SingleOrDefault(c => c.ClientId.Equals(clientId));
    }
}
