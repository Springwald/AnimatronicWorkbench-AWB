// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.InputControllers.Midi;
using Awb.Core.Services;
using Awb.Core.Tools;

namespace Awb.Core.InputControllers.BCF2000
{
    public class Bcf2000Controller : MidiDevice
    {
        private byte?[] _valuesKnobRotation = new byte?[8];
        private LedState?[] _buttonLedStates = new LedState?[16];

        public EventHandler<Bcf2000EventArgs>? ActionReceived;
        private readonly IInvoker? _invoker;

        public Bcf2000Controller(IAwbLogger awbLogger, IInvoker invoker) : base(deviceName: "BCF2000", awbLogger: awbLogger)
        {
            if (_midiPort == null || !Available) return;
            _midiPort.OnInputEvent += _midiPort_OnInputEvent;
            _invoker = invoker;
        }

        private void _midiPort_OnInputEvent(object? sender, MidiInputEventArgs e)
        {
            var args = new Bcf2000EventArgs(e.InputId, e.Value);

            // cache values to prevent sending the same value twice later
            switch (args.InputType)
            {
                case Bcf2000EventArgs.InputTypes.Unknown:
                    break;
                case Bcf2000EventArgs.InputTypes.KnobRotation:
                    break;
                case Bcf2000EventArgs.InputTypes.KnobPress:
                    break;
                case Bcf2000EventArgs.InputTypes.ButtonTopLine:
                    break;
                case Bcf2000EventArgs.InputTypes.ButtonBottomLine:
                    break;
                case Bcf2000EventArgs.InputTypes.Fader:
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(args.InputType) + ":" + args.InputType);
            }
            // This is important! We must not call the event handler in the timer thread, because the event handler should update the UI.
            // so we use the invoker using the hosting wpf application thread instead.
            _invoker.Invoke(() => ActionReceived?.Invoke(this, args));
        }

        /// <summary>
        /// Sets the position of the motor fade
        /// </summary>
        /// <param name="fader">1-8</param>
        /// <param name="pos">0-127</param>
        /// <returns></returns>
        public async Task<bool> SetFaderPosition(byte fader, byte pos)
        {
            if (fader == 0) throw new ArgumentOutOfRangeException("fader must be 1-8; is not 0 based!");
            if (pos == _valuesKnobRotation[fader - 1]) return true;
            if (fader > 3) fader++; // fader 4 is missing, so shift all fader after 3 by one
            return _midiPort?.SendMidiMessage((byte)176, (byte)(fader + 7), pos) == true;
        }

        ///// <param name="topLine">top- or bottomline</param>
        ///// <param name="button">1-8</param>
        //public bool SetButtonLedState(bool topLine, byte button, LedState ledState)
        //{
        //    if (button == 0) throw new ArgumentOutOfRangeException("button must be 1-8; is not 0 based!");
        //    if (topLine == false) button += 8;
        //    if (_buttonLedStates[button - 1] == ledState) return true;
        //    _buttonLedStates[button - 1] = ledState;
        //    return _midiPort?.SendMidiMessage(0x90, (byte)(button-1), (byte)ledState) == true;
        //}


    }
}
