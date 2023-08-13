// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace AwbStudio.TimelineControls
{
    internal class TimelineCaptions
    {
        private Brush[] _brushes = new[]
{
            new SolidColorBrush(Colors.White),
            new SolidColorBrush(Colors.LightBlue),
            new SolidColorBrush(Colors.LightGreen),
            new SolidColorBrush(Colors.Salmon),
            new SolidColorBrush(Colors.Magenta),
            new SolidColorBrush(Colors.Orange),
            new SolidColorBrush(Colors.Pink),
            new SolidColorBrush(Colors.Yellow),
        };
        private int _brushIndex = 0;

        private List<TimelineCaption> _captions = new List<TimelineCaption>();

        public TimelineCaption[] Captions => _captions.ToArray();

        public TimelineCaptions()
        {
        }

        public void AddServo(string servoId, string label)
        {
            var id = GetIdForServo(servoId);
            AddCaption(new TimelineCaption
            {
                Id = id,
                Label = label
            });
        }

        public TimelineCaption? GetServoCaption(string servoId) 
        {
            var id = GetIdForServo(servoId);
            return _captions.FirstOrDefault(c => c.Id ==id);
        }


        public void AddCaption(TimelineCaption caption)
        {
            caption.Color = _brushes[_brushIndex++];
            if (_brushIndex >= _brushes.Length) _brushIndex = 0; // start with first color again
            _captions.Add(caption);
        }

        private static string GetIdForServo(string servoId) => "Servo-" + servoId;

        
    }

}
