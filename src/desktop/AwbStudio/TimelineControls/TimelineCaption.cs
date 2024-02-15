// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Windows.Media;

namespace AwbStudio.TimelineControls
{
    internal class TimelineCaption
    {
        public string Label { get; set; }
        public Brush ForegroundColor { get; set; }
        public Brush? BackgroundColor { get; set; }
        public string Id { get; set; }
    }
}
