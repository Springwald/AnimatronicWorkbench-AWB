﻿// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.DataPackets;
using Awb.Core.Services;
using System.Text.Json;

namespace Awb.Core.Player
{
    public class ChangesToClientSender
    {
        private Dictionary<uint, DateTime> _notFoundClients = [];

        private readonly IAwbLogger _logger;
        private readonly IActuatorsService _actuatorsService;
        private readonly IAwbClientsService _awbClientsService;

        public ChangesToClientSender(IActuatorsService actuatorsService, IAwbClientsService awbClientsService, IAwbLogger logger)
        {
            _actuatorsService = actuatorsService;
            _awbClientsService = awbClientsService;
            _logger = logger;
        }

        public async Task<bool> SendChangesToClients()
        {
            var dataPacketFactory = new DataPacketFactory();
            var dataPackets = dataPacketFactory.GetDataPackets(_actuatorsService.Servos);

            var success = true;

            foreach (var packet in dataPackets)
            {

                if (packet.IsEmpty) continue;

                var clientID = packet.ClientId;
                var client = _awbClientsService.GetClient(clientID);
                if (client == null)
                {
                    if (_notFoundClients.ContainsKey(clientID) == false)
                    {
                        // not requested yet
                    }
                    else
                    {
                        if ((DateTime.Now - _notFoundClients[clientID]).TotalSeconds < 30)
                        {
                            // reported already and not too long ago
                            continue; // dont show message again
                        }
                    }
                    _notFoundClients[clientID] = DateTime.Now;
                    await _logger.LogErrorAsync($"ClientId '{clientID}' not found!");

                    continue;
                }

                var options = new JsonSerializerOptions()
                {
                    WriteIndented = false,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                var jsonStr = JsonSerializer.Serialize(packet.Content, options);
                var result = await client.Send(payload: jsonStr, debugInfo: jsonStr);
                if (result.Ok)
                {
                    // await _logger.Log($"Sent to client Id '{clientID}'. ({result.DebugInfos})");
                    // await _logger.Log(string.Join(", ", dataPackets.Select(p => string.Join("|", p.Content.StsServos?.Servos?.Select(s => s.Name + ":" + s.TargetValue)))));
                    dataPacketFactory.SetDataPacketDone(packet);
                }
                else
                {
                    await _logger.LogErrorAsync($"Error sending data to client Id '{clientID}': {result.ErrorMessage}");
                    success = false;
                }

            }



            return success;
        }
    }
}
