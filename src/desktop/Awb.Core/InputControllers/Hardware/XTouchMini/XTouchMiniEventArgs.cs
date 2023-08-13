// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Globalization;

namespace Awb.Core.InputControllers.XTouchMini
{
    public class XTouchMiniEventArgs : EventArgs
    {
        public enum InputTypes
        {
            Unknown,
            KnobRotation,
            KnobPress,
            ButtonTopLine,
            ButtonBottomLine,
            MainFader
        }

        public string InputHex { get; }
        public InputTypes InputType { get; }

        /// <summary>
        /// Number of the input (1-8)
        /// </summary>
        public int InputIndex { get; }

        /// <summary>
        /// Value of the input (0-127)
        /// </summary>
        public int Value { get; }

        public XTouchMiniEventArgs(string inputHex, string valueHex)
        {
            this.InputHex = inputHex;

            if (int.TryParse(valueHex, System.Globalization.NumberStyles.HexNumber, null, out int value))
            {
                this.Value = value;
            }
            else
            {
                this.Value = 0;
            }

            if (inputHex.Length == 4)
            {
                if (inputHex.EndsWith(("BA")))
                {
                    // Knobs or main fader rotation
                    InputIndex = int.TryParse(inputHex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out int index) ? index : -1;
                    InputType = InputIndex == 9 ? InputTypes.MainFader : InputTypes.KnobRotation;
                    return;
                }
                if (inputHex.EndsWith("9A") || inputHex.EndsWith("8A")) // Knob press or  Button down / up
                {
                    var indexHex = inputHex.Substring(0, 2);
                    if (int.TryParse(indexHex, NumberStyles.HexNumber, null, out int index)) 
                    {
                        if (index > 15)
                        {
                            InputType = InputTypes.ButtonBottomLine;
                            InputIndex = index - 15;

                        } else if (index > 7)
                        {
                            InputType = InputTypes.ButtonTopLine;
                            InputIndex = index - 7;

                        }else
                        {
                            InputType = InputTypes.KnobPress;
                            InputIndex = index+1;
                        }
                    } else {
                        InputIndex = -1;
                    }
                    return;
                }
            }

            switch (inputHex)
            {
                default:
                    InputType = InputTypes.Unknown;
                    InputIndex = -1;
                    break;
            }
        }
    }
}