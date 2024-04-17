// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.InputControllers.XTouchMini;
using Awb.Core.Player;
using Awb.Core.Services;

namespace MidiControllerLiveControl
{
    internal class XTouchMiniMidiControlDemo
    {
        private readonly IActuatorsService _actuatorsService;
        private readonly IAwbClientsService _awbClientsService;
        private readonly IAwbLogger _logger;

        public XTouchMiniMidiControlDemo(IActuatorsService actuatorsService, IAwbClientsService awbClientsService, IAwbLogger logger)
        {
            _actuatorsService = actuatorsService;
            _awbClientsService = awbClientsService;
            _logger = logger;
        }

        public async Task Run()
        {
            var midiController = new XTouchMiniController(_logger, new MockInvoker());
            midiController.ActionReceived += ActionReceived;

            for (int i = 0; i <= 8; i++)
            {
                midiController.SetKnobPosition((byte)i, 64);
            }   

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

        private async void ActionReceived(object? sender, XTouchMiniEventArgs e)
        {
            switch (e.InputType)
            {
                case XTouchMiniEventArgs.InputTypes.KnobRotation:
                    var servoId = $"Servo {e.InputIndex}";
                    var servo = _actuatorsService.Servos.SingleOrDefault(s => s.Id.Equals(servoId));
                    if (servo == null)
                    {
                        await _logger.LogError("Servo with ID '" + servoId + "' not configured");
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

