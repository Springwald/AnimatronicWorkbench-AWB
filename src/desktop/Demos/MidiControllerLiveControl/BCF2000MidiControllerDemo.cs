// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.InputControllers.BCF2000;
using Awb.Core.Player;
using Awb.Core.Services;

namespace MidiControllerLiveControl
{
    public class BCF2000MidiControllerDemo
    {
        private readonly IActuatorsService _actuatorsService;
        private readonly IAwbClientsService _awbClientsService;
        private readonly IAwbLogger _logger;

        public BCF2000MidiControllerDemo(IActuatorsService actuatorsService, IAwbClientsService awbClientsService, IAwbLogger logger)
        {
            _actuatorsService = actuatorsService;
            _awbClientsService = awbClientsService;
            _logger = logger;
        }

        public async Task Run()
        {
            var midiController = new Bcf2000Controller(_logger, new MockInvoker());
            midiController.ActionReceived += ActionReceived;

            //for (int i = 1; i <= 8; i++)
            //{
            //    await midiController.SetKnobPosition((byte)i, 127);
            //    await Task.Delay(100);
            //}

            //await Task.Delay(500);

            //for (int i = 1; i <= 8; i++)
            //{
            //    await midiController.SetKnobPosition((byte)i, 32);
            //    await Task.Delay(100);
            //}


            var sender = new ChangesToClientSender(_actuatorsService, _awbClientsService, _logger);

            var servos = _actuatorsService.Servos;
            foreach (var servo in servos)
            {
                servo.IsDirty = true;
            }

            while (!Console.KeyAvailable)
            {
                var ok = await sender.SendChangesToClients();
                await Task.Delay(100);
            }

            midiController.Dispose();

            //var mouthLow = 0;
            //var mouthLowSpeed = 100;
            //var mouthLowServo = _actuatorsService.Servos.SingleOrDefault(s => s.Id.Equals("ML"));

            //if (mouthLowServo == null) { throw new Exception("mouthLowServo not found"); };

            //var sender = new ChangesToClientSender(_actuatorsService, _awbClientsService, _logger);


            //while (!Console.KeyAvailable)
            //    for (int i = -300; i < 300; i += 300)
            //    {
            //        mouthLowServo.TargetValue = 2048 + i;
            //        var ok = await sender.SendChangesToClients();
            //        await Task.Delay(300);
            //    }

            //while (!Console.KeyAvailable)
            //{
            //    //mouthLow += mouthLowSpeed;
            //    //if (mouthLow > mouthLowServo.MaxValue)
            //    //{
            //    //    mouthLow = mouthLowServo.MaxValue;
            //    //    mouthLowSpeed = -mouthLowSpeed;
            //    //}
            //    //if (mouthLow < mouthLowServo.MinValue)
            //    //{
            //    //    mouthLow = mouthLowServo.MinValue;
            //    //    mouthLowSpeed = -mouthLowSpeed;
            //    //}
            //    //mouthLowServo.TargetValue = mouthLow;

            //    var ok = await sender.SendChangesToClients();
            //   await Task.Delay(10);
            //}
        }

        private async void ActionReceived(object? sender, Bcf2000EventArgs e)
        {
            switch (e.InputType)
            {
                case Bcf2000EventArgs.InputTypes.KnobRotation:
                    var servoId = $"Servo {e.InputIndex}";
                    var servo = _actuatorsService.Servos.SingleOrDefault(s => s.Id.Equals(servoId));
                    if (servo == null)
                    {
                        await _logger.LogErrorAsync($"Servo with ID '{servoId}' not configured");
                    }
                    else
                    {
                        var servoRange = servo.MaxValue - servo.MinValue;
                        servo.TargetValue = (int)(servo.MinValue + servoRange * (e.Value / 127.0));
                    }
                    break;
            }
            await Log($"Action received! {e.InputType}({e.InputIndex}): {e.Value}  [{e.InputHex}]", error: false);
        }

        private async Task Log(string message, bool error)
        {
            Console.WriteLine($"{(error ? "Error: " : " ")}{message}");
            await Task.CompletedTask;
        }

    }
}
