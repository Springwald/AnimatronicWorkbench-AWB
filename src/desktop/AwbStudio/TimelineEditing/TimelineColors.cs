// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System;
using System.Globalization;
using System.Windows.Media;

namespace AwbStudio.TimelineEditing
{
    public class TimelineColors
    {
        /// <summary>
        /// Is dark mode enabled?
        /// </summary>
        public bool DarkMode { get; }

        /// <summary>
        /// Gets an array of brushes used to render the actuators.
        /// </summary>
        public SolidColorBrush[] TimelineBrushes { get; }

        public Brush CaptionBackgroundBrush(bool inverted) {
            return DarkMode
                ? (Brush)(inverted ? Brushes.White : Brushes.Black)
                : (Brush)(inverted ? Brushes.Black : Brushes.White);
        }

        public TimelineColors()
        {
            this.DarkMode = (App.Current as AwbStudio.App)!.DarkMode;
            this.TimelineBrushes = this.DarkMode ?
            [
                // Colors for DarkMode
                new SolidColorBrush(Colors.White),
                new SolidColorBrush(Colors.LightBlue),
                new SolidColorBrush(Colors.LightGreen),
                new SolidColorBrush(Colors.Salmon),
                new SolidColorBrush(Colors.Magenta),
                new SolidColorBrush(Colors.Orange),
                new SolidColorBrush(Colors.Pink),
                new SolidColorBrush(Colors.Yellow),
                new SolidColorBrush(Colors.Beige),
                new SolidColorBrush(Colors.Tomato),
                new SolidColorBrush(Colors.SkyBlue),
                new SolidColorBrush(Colors.MintCream),
                new SolidColorBrush(Colors.LightGoldenrodYellow),
                new SolidColorBrush(Colors.LightPink),
                new SolidColorBrush(Colors.LightSteelBlue),
                new SolidColorBrush(Colors.LemonChiffon),
            ] :
            [
                // Colors for LightMode
                new SolidColorBrush(Colors.Black),
                new SolidColorBrush(Colors.Blue),
                new SolidColorBrush(Colors.Green),
                new SolidColorBrush(Colors.Red),
                new SolidColorBrush(Colors.Magenta),
                new SolidColorBrush(Colors.Orange),
                new SolidColorBrush(Colors.Violet),
                new SolidColorBrush(Colors.DarkOrchid),
                new SolidColorBrush(Colors.Aquamarine),
                new SolidColorBrush(Colors.Tomato),
                new SolidColorBrush(Colors.SkyBlue),
                new SolidColorBrush(Colors.MintCream),
                new SolidColorBrush(Colors.LightGoldenrodYellow),
                new SolidColorBrush(Colors.LightPink),
                new SolidColorBrush(Colors.LightSteelBlue),
                new SolidColorBrush(Colors.LemonChiffon),
            ];
        }

        public SolidColorBrush GetContrastBrush(SolidColorBrush brush)
        {
            if (Math.Abs(brush.Color.R - brush.Color.G) + Math.Abs(brush.Color.G - brush.Color.B) < 128)
            {
                // less contrast between R, G and B, so we use white or black
                return new SolidColorBrush(brush.Color.R > 128 ? Colors.Black : Colors.White);
            }

            var invertedColor = new SolidColorBrush(
                  Color.FromRgb(
                      (byte)(255 - brush.Color.R),
                      (byte)(255 - brush.Color.G),
                      (byte)(255 - brush.Color.B)));

            return invertedColor;
        }
    }
}
