// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

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
                    Console.WriteLine($"{servo.Name}: {servo.TargetValue}");
                    await Task.Delay(3000);
                }
            }
        }

        private async Task Log(string message, bool error)
        {
            Console.WriteLine($"{(error ? "Error: " : " ")}{message}");
            await Task.CompletedTask;
        }

    }
}
