// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System.Windows.Media;

namespace AwbStudio.TimelineEditing
{
    public class TimelineCaption(Brush foregroundColor, string id, string label)
    {
        private string _label = label;

        public string Id { get; set; } = id;
        public Brush ForegroundColor { get; } = foregroundColor;

        public string Label => ControllerChannel.HasValue ? $"[{ControllerChannel}] {_label.Trim()}" : _label.Trim();
        public Brush? BackgroundColor { get; set; }
        public bool ObjectIsControllerTuneable { get; set; }
        public int? ControllerChannel { get; set; }
    }
}
