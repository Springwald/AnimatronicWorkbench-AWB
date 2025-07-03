// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System;
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


        /// <summary>
        /// horizontal grid line primary
        /// </summary>
        public Brush GridLineHorizontalBrushPrimary { get; }

        /// <summary>
        /// horizontal grid line secondary
        /// </summary>
        public Brush GridLineHorizontalBrushSecondary { get; }

        /// <summary>
        /// vertical grid line primary
        /// </summary>
        public Brush GridLineVertical1000msBrush { get; }

        /// <summary>
        /// vertical grid line secondary
        /// </summary>
        public Brush GridLineVertical500msBrush { get; }


        public Brush CaptionBackgroundBrush { get; }

        public TimelineColors()
        {
            DarkMode = (App.Current as AwbStudio.App)!.DarkMode;

            CaptionBackgroundBrush = DarkMode ? Brushes.Black : Brushes.White;

            var horizontalGridColor = DarkMode ? Colors.DeepSkyBlue : Colors.Blue;
            GridLineHorizontalBrushPrimary = new SolidColorBrush(horizontalGridColor) { Opacity = 0.5 };
            GridLineHorizontalBrushSecondary = new SolidColorBrush(horizontalGridColor) { Opacity = 0.25 };

            var verticalGridColor = DarkMode ? Colors.White: Colors.Black;
            GridLineVertical1000msBrush = new SolidColorBrush(verticalGridColor) { Opacity = 0.3 };
            GridLineVertical500msBrush = new SolidColorBrush(verticalGridColor) { Opacity = 0.15 };

            TimelineBrushes = DarkMode ?
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
