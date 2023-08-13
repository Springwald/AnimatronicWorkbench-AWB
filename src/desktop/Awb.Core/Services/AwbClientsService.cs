// AnimatronicWorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Clients;
using PacketLogistics.ComPorts;

namespace Awb.Core.Services
{
    public interface IAwbClientsService
    {
        Esp32ComPortClient[] ComPortClients { get; }

        Task Init();

        Esp32ComPortClient? GetClient(uint clientId);
    }

    public class AwbClientsService : IAwbClientsService
    {
        private Esp32ComPortClient[]? _clients;
        private readonly IAwbLogger? _logger;

        public AwbClientsService(IAwbLogger? logger)
        {
            _logger = logger;
        }

        public Esp32ComPortClient[] ComPortClients
        {
            get
            {
                if (_clients == null) throw new InvalidOperationException("Not initialized.");
                return _clients;
            }
        }

        public async Task Init()
        {
            _logger?.Log($"Searching clients...");

            var config = new AwbEsp32ComportClientConfig();
            var clientIdScanner = new ClientIdScanner(config);
            var foundComPortClients = await clientIdScanner.FindAllClients(useComPortCache: true);
            if (foundComPortClients.Any() == false)
            {
                foundComPortClients = await clientIdScanner.FindAllClients(useComPortCache: false);
            }
            _clients = foundComPortClients.Select(c => new Esp32ComPortClient(c.ComPortName, c.ClientId)).ToArray();
            foreach (var client in _clients)
            {
                var ok = await client.Init();
                if (ok == false)
                {
                    _logger?.LogError($"Can't init client {client.ClientId}/{client.FriendlyName}");
                }
            }

            _logger?.Log($"Found {_clients.Length} clients. ({string.Join(", ", _clients.Select(c => c.ClientId))})");
        }

        public Esp32ComPortClient? GetClient(uint clientId)
            => ComPortClients.SingleOrDefault(c => c.ClientId.Equals(clientId));
    }
}
