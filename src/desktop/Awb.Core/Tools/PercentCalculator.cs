// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Tools
{
    public class PercentCalculator(double min, double max)
    {
        public double Min { get; } = min;
        public double Max { get; } = max;

        public double CalculatePercent(double value)
        {
            if (Max.Equals(Min)) return Min;
            if (Min > Max) return 100 - ((value - Max) / (Min - Max) * 100);
            return (value - Min) / (Max - Min) * 100;
        }

        public double CalculateValue(double percent)
        {
            if (Max.Equals(Min)) return Min;
            if (Min > Max) return Max + (100 - percent) / 100 * (Min - Max);
            return Min + percent / 100 * (Max - Min);
        }
    }
}
