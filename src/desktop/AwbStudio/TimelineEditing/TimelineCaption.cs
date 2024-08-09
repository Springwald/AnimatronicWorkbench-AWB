// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Windows.Media;

namespace AwbStudio.TimelineEditing
{
    public class TimelineCaption
    {
        private string _label;

        public string Label => ControllerChannel.HasValue ? $"[{ControllerChannel}] {_label.Trim()}" : _label.Trim();
        public Brush ForegroundColor { get; }
        public Brush? BackgroundColor { get; set; }
        public string Id { get; set; }
        public bool ObjectIsControllerTuneable { get; set; }
        public int? ControllerChannel { get; set; }

        public TimelineCaption(Brush foregroundColor, string id, string label)
        {
            ForegroundColor = foregroundColor;
            Id = id;
            _label = label;
        }
    }
}
