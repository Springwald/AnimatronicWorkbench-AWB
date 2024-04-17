// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Tools
{
    public class PercentCalculator
    {
        public PercentCalculator(double min, double max)
        {
            Min = min;
            Max = max;
        }

        public double Min { get; }
        public double Max { get; }

        public double CalculatePercent(double value)
        {
            if (Min > Max)
            {
                return 100 - ((value - Max) / (Min - Max) * 100);
            }
            return (value - Min) / (Max - Min) * 100;
        }

        public double CalculateValue(double percent)
        {
            if (Min > Max)
            {
                return Max + (100 - percent) / 100 * (Min - Max);
            }
            return Min + percent / 100 * (Max - Min);
        }
    }
}
