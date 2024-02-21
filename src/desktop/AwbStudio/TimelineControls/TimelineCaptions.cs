// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace AwbStudio.TimelineControls
{
    public class TimelineCaptions
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

        private Brush _brushBlack = new SolidColorBrush(Colors.Black);

        private int _brushIndex = 0;

        private List<TimelineCaption> _captions = new List<TimelineCaption>();

        public TimelineCaption[] Captions => _captions.ToArray();

        public TimelineCaptions()
        {
        }

        public void AddAktuator(IActuator aktuator, string label)
        {
            //var id = GetIdForAktuator(aktuator);
            AddCaption(new TimelineCaption
            {
                Id = aktuator.Id,
                Label = label
            },
            inverse: aktuator switch
            {
                ISoundPlayer soundPlayer => true,
                IServo servo => false,
                _ => false
            });
        }

        public TimelineCaption? GetAktuatorCaption(string aktuatorId)
        {
            return _captions.FirstOrDefault(c => c.Id == aktuatorId);
        }

        public void AddCaption(TimelineCaption caption, bool inverse)
        {
            if (inverse)
            {
                caption.BackgroundColor = _brushes[_brushIndex++];
                caption.ForegroundColor = _brushBlack;
            }
            else
            {
                caption.BackgroundColor = null;
                caption.ForegroundColor = _brushes[_brushIndex++];
            }
            if (_brushIndex >= _brushes.Length) _brushIndex = 0; // start with first color again
            _captions.Add(caption);
        }

    }

}
