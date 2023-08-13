// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Globalization;

namespace Awb.Core.InputControllers.BCF2000
{
    public class Bcf2000EventArgs : EventArgs
    {
        public enum InputTypes
        {
            Unknown,
            KnobRotation,
            KnobPress,
            ButtonTopLine,
            ButtonBottomLine,
            Fader
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


        public Bcf2000EventArgs(string inputHex, string valueHex)
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
                if (inputHex.EndsWith(("B0")))
                {
                    // Knobs rotation or fader movement
                    var indexHex = inputHex.Substring(0, 2);
                    if (int.TryParse(indexHex, System.Globalization.NumberStyles.HexNumber, null, out int index))
                    {
                        if (index >= 8 && index <= 8 + 8) // Fader
                        {
                            InputType = InputTypes.Fader;
                            InputIndex = CorrectIndex(index - 7);
                            return;
                        }
                        else if (index >= 68 && index <= 68 + 7) // rotation knobs
                        {
                            InputType = InputTypes.KnobRotation;
                            InputIndex = index - 67;
                            return;
                        }
                        //else if (index >= 24 && index <= 24 + 7)
                        //{
                        //    InputType = InputTypes.ButtonTopLine;
                        //    InputIndex = index - 23;
                        //    return;
                        //}
                        else if (index >= 53 && index <= 53 + 7) // knob press
                        {
                            InputType = InputTypes.KnobPress;
                            InputIndex = index - 52;
                            return;
                        }
                        else
                        {
                            //InputType = InputTypes.KnobPress;
                            //InputIndex = index + 1;
                            //return;
                        }
                    }
                }
                /*

                if (inputHex.EndsWith(("BA")))
                {
                    // Knobs or main fader rotation
                    InputIndex = int.TryParse(inputHex.Substring(0, 2), out int index) ? index : -1;
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
                }*/
            }

            switch (inputHex)
            {
                default:
                    InputType = InputTypes.Unknown;
                    InputIndex = -1;
                    break;
            }
        }

        /// <summary>
        /// Corrects the number to the correct value (1-8) because 4 is missing
        /// and 1,2,3,5,6,7,8 are sent instead
        /// </summary>
        /// <param name="rawNumber"></param>
        /// <returns></returns>
        private int CorrectIndex(int rawNumber) => rawNumber > 3 ? rawNumber - 1 : rawNumber;
    }
}