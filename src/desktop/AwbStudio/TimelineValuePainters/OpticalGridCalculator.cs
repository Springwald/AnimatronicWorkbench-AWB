// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System.Windows.Controls;

namespace AwbStudio.TimelineValuePainters
{
    internal class OpticalGridCalculator(Grid paintContol)
    {
        private readonly Grid _paintControl = paintContol;

        public const double PaintMarginTopBottom = 10;
        public const double PaintValueMarginLeftRight = 2;
        public const double PaintGridMarginLeftRight = 10;

        public double PaintGridAreaLeft => PaintGridMarginLeftRight - _paintControl.Margin.Left;
        public double PaintGridAreaWidth => _paintControl.ActualWidth - PaintGridAreaLeft * 2;
        public double PaintAreaTop => PaintMarginTopBottom;
        public double PaintAreaHeight => _paintControl.ActualHeight - PaintMarginTopBottom * 2;

        public double GetYForPercentValue(double percent)
        {
            double diagramHeight = PaintAreaHeight;

            var topMargin = _paintControl.Margin.Bottom;

            return _paintControl.ActualHeight - topMargin - (PaintAreaTop + percent / 100.00 * diagramHeight);  // pos 0 is at the top, so we have to invert the y axis
        }
    }
}
