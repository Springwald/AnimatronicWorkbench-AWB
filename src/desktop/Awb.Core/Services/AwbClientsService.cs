// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

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

        /// <summary>
        /// Searches for all available AWB clients on the COM ports.
        /// </summary>
        /// <param name="fastMode">true=use cached COM Port data; false=rescan for COM Ports (slower)</param>
        /// <returns>Number of found clients</returns>
        public Task<int> ScanForClients(bool fastMode);
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
                    _logger?.LogAsync($"Access to clients not available because still searching clients...");
                    return Array.Empty<Esp32ComPortClient>();
                }
                return _clients;
            }
        }

        /// <summary>
        /// Searches for all available AWB clients on the COM ports.
        /// </summary>
        /// <param name="fastMode">true=use cached COM Port data; false=rescan for COM Ports (slower)</param>
        /// <returns>Number of found clients</returns>
        public async Task<int> ScanForClients(bool fastMode)
        {
            _logger?.LogAsync($"\r\nSearching clients (fastMode={fastMode})...");

            var config = new AwbEsp32ComportClientConfig();
            var clientIdScanner = new ClientIdScanner(config);
            clientIdScanner.OnLog += (s, e) => _logger?.LogAsync(e);

            FoundClient[] foundComPortClients = [];

            if (fastMode)
            {
                _logger?.LogAsync("Using cached COM port info...");
                foundComPortClients = await clientIdScanner.FindAllClientsAsync(useComPortCache: true);
            }

            if (foundComPortClients.Any() == false)
            {
                _logger?.LogAsync("No clients found with fastMode (cached COM port info)! Trying live detection...");
                foundComPortClients = await clientIdScanner.FindAllClientsAsync(useComPortCache: false);
            }

            var clients = foundComPortClients.Select(c => new Esp32ComPortClient(c.ComPortName, c.ClientId)).ToArray();
            var clientTasks = clients.Select(async c =>
            {
                _logger?.LogAsync("Init client " + c.ClientId + "/" + c.FriendlyName);
                var ok = await c.InitAsync();
                if (ok == false) _logger?.LogErrorAsync($"Can't init client {c.ClientId}/{c.FriendlyName}");
            }).ToArray();

            await Task.WhenAll(clientTasks);

            foreach (var client in clients)
            {
                client.OnError += (s, e) => _logger?.LogErrorAsync($"Client {client.ClientId}/{client.FriendlyName}: {e}");
            }

            _clients = clients;
            _logger?.LogAsync($"Found {_clients.Length} clients. ({string.Join(", ", _clients.Select(c => c.ClientId))})");

            if (ClientsLoaded != null)
                ClientsLoaded(this, EventArgs.Empty);

            return _clients.Length;
        }

        public async Task InitAsync()
        {
            await this.ScanForClients(fastMode: true);
        }

        public Esp32ComPortClient? GetClient(uint clientId)
            => ComPortClients.SingleOrDefault(c => c.ClientId.Equals(clientId));
    }
}
