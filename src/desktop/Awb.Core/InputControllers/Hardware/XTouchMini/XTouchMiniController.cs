// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.InputControllers.Midi;
using Awb.Core.Services;
using Awb.Core.Tools;

namespace Awb.Core.InputControllers.XTouchMini
{
    public class XTouchMiniController : MidiDevice
    {
        private byte?[] _valuesKnobRotation = new byte?[8];
        private LedState?[] _buttonLedStates = new LedState?[16];

        public EventHandler<XTouchMiniEventArgs>? ActionReceived;
        private readonly IInvoker _invoker;

        public XTouchMiniController(IAwbLogger awbLogger, IInvoker invoker) : base(deviceName: "X-TOUCH MINI", awbLogger: awbLogger)
        {
            _invoker = invoker;
            if (_midiPort == null || !Available) return;
            _midiPort.OnInputEvent += _midiPort_OnInputEvent;
        }

        private void _midiPort_OnInputEvent(object? sender, MidiInputEventArgs e)
        {
            var args = new XTouchMiniEventArgs(e.InputId, e.Value);

            // cache values to prevent sending the same value twice later
            switch (args.InputType)
            {
                case XTouchMiniEventArgs.InputTypes.Unknown:
                    break;
                case XTouchMiniEventArgs.InputTypes.KnobRotation:
                    if (args.InputIndex < 0 || args.InputIndex > 8) return; // Layer B?
                    if (_valuesKnobRotation[args.InputIndex - 1] == (byte)args.Value) return; // no change
                    _valuesKnobRotation[args.InputIndex - 1] = (byte)args.Value;
                    break;
                case XTouchMiniEventArgs.InputTypes.KnobPress:
                    break;
                case XTouchMiniEventArgs.InputTypes.ButtonTopLine:
                    break;
                case XTouchMiniEventArgs.InputTypes.ButtonBottomLine:
                    break;
                case XTouchMiniEventArgs.InputTypes.MainFader:
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(args.InputType) + ":" + args.InputType);
            }

            // This is important! We must not call the event handler in the timer thread, because the event handler should update the UI.
            // so we use the invoker using the hosting wpf application thread instead.
            _invoker.Invoke(() => ActionReceived?.Invoke(this, args));
        }

        /// <summary>
        /// Sets the ring lights for a knob.
        /// </summary>
        /// <param name="knob">1-8</param>
        /// <param name="pos">0-127</param>
        /// <returns></returns>
        public bool SetKnobPosition(byte knob, byte pos)
        {
            if (knob == 0) throw new ArgumentOutOfRangeException("knob must be 1-8; is not 0 based!");
            if (pos == _valuesKnobRotation[knob - 1]) return true;
            return _midiPort?.SendMidiMessage(0xba, knob, pos) == true;
        }

        /// <param name="topLine">top- or bottomline</param>
        /// <param name="button">1-8</param>
        public bool SetButtonLedState(bool topLine, byte button, LedState ledState)
        {
            if (button == 0) throw new ArgumentOutOfRangeException("button must be 1-8; is not 0 based!");
            if (topLine == false) button += 8;
            if (_buttonLedStates[button - 1] == ledState) return true;
            _buttonLedStates[button - 1] = ledState;
            return _midiPort?.SendMidiMessage(0x90, (byte)(button - 1), (byte)ledState) == true;
        }

        ///// <summary>
        ///// Sets the ring lights for a knob.
        ///// </summary>
        ///// <param name="knob">The knob to set the lights for</param>
        ///// <param name="state">The overall state: off, on, or blinking</param>
        ///// <param name="value">The individual value (0 for off, </param>
        //public bool SetButtonState(MidiPort midi, int knob, LedState state, int value)
        //{
        //    //byte midiValue = (state, value) switch
        //    //{
        //    //    (LedState.Off, _) => 0,
        //    //    (_, 0) => 0,
        //    //    (LedState.On, 14) => 27,
        //    //    (LedState.Blinking, 14) => 28,
        //    //    (LedState.On, >= 1 and <= 13) => (byte)value,
        //    //    (LedState.Blinking, >= 1 and <= 13) => (byte)(value + 13),
        //    //    _ => 0
        //    //};
        //    return midi.SendMidiMessage(0xba, (byte)knob, (byte)state);
        //    //return midi.SendMidiMessage(0x90, (byte)knob, (byte)1);

        //    //return midi.SendMidiMessage(0x90, (byte)knob, (byte)8);
        //    //return midi.SendMidiMessage(0x90, (byte)(knob - 1), (byte)state);
        //    //return midi.SendMidiMessage(0xb0, (byte)(knob + 8), midiValue);
        //}
    }
}
