﻿// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Player;
using Awb.Core.Services;

namespace ResetAllServos
{
    internal class StsServoCenter
    {
        private readonly IActuatorsService _actuatorsService;
        private readonly IAwbClientsService _awbClientsService;
        private readonly IAwbLogger _logger;

        public StsServoCenter(IActuatorsService actuatorsService, IAwbClientsService awbClientsService, IAwbLogger logger)
        {
            _actuatorsService = actuatorsService;
            _awbClientsService = awbClientsService;
            _logger = logger;
        }

        public async Task Run()
        {
            foreach (var servo in _actuatorsService.Servos)
            {
                var sender = new ChangesToClientSender(_actuatorsService, _awbClientsService, _logger);
                foreach (var pos in new[] { servo.MinValue, servo.MaxValue, servo.DefaultValue })
                {
                    servo.TargetValue = pos;
                    var ok = await sender.SendChangesToClients();
                    Console.WriteLine($"{servo.Title}: {servo.TargetValue}");
                    await Task.Delay(3000);
                }
            }
        }
    }
}
