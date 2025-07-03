// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.Project.Various;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Media;

namespace AwbStudio.TimelineEditing
{
    public class TimelineCaptions
    {
        private int _brushIndex = 0;
        private List<TimelineCaption> _captions = new List<TimelineCaption>();
        private readonly TimelineColors _timelineBrushes;

        public TimelineCaption[] Captions => _captions.ToArray();

        public TimelineCaptions()
        {
            _timelineBrushes = new TimelineColors();
        }

        public void AddAktuator(IActuator actuator)
        {
            var clientIdPraefix = actuator.ClientId == 1 ? string.Empty : $"C{actuator.ClientId}-";

            var label = actuator switch
            {
                Mp3PlayerYX5300 mp3 => $"{clientIdPraefix}{(string.IsNullOrWhiteSpace(mp3.Title) ? $"MP3-{mp3.Id}" : mp3.Title)}",
                Mp3PlayerDfPlayerMiniConfig mp3 => $"{clientIdPraefix}{(string.IsNullOrWhiteSpace(mp3.Title) ? $"MP3-{mp3.Id}" : mp3.Title)}",
                Pca9685PwmServo servo => $"{clientIdPraefix}PWM{servo.Channel} {servo.Title ?? string.Empty}",
                StsScsServo stsScs => $"{clientIdPraefix}{stsScs.StsScsType.ToString()}{stsScs.Channel} {stsScs.Title ?? string.Empty}",
                _ => $"{clientIdPraefix}{actuator.Id} {actuator.Title ?? string.Empty}"
            };

            AddCaption(
                id: actuator.Id,
                label: label,
                objectIsControllerTuneable: actuator.IsControllerTuneable,
                inverse: actuator switch
                {
                    ISoundPlayer soundPlayer => true,
                    IServo servo => false,
                    _ => false
                });
        }

        public TimelineCaption GetAktuatorCaption(string aktuatorId)
        {
            if (aktuatorId == NestedTimelinesFakeObject.Singleton.Id) // special case for nested timelines (no real actuator
                return new TimelineCaption(
                    foregroundColor: new SolidColorBrush(Colors.White),
                   id: NestedTimelinesFakeObject.Singleton.Id,
                    label: NestedTimelinesFakeObject.Singleton.Title);

            var caption = _captions.FirstOrDefault(c => c.Id == aktuatorId);
            if (caption == null)
            {
                caption = new TimelineCaption(
                    foregroundColor: new SolidColorBrush(Colors.White),
                    id: aktuatorId,
                    label: aktuatorId);
                _captions.Add(caption);
            }
            return caption;
        }

        public void AddCaption(string id, string label, bool objectIsControllerTuneable, bool inverse)
        {
            var brushes = _timelineBrushes.TimelineBrushes;

            if (inverse)
            {
                var brush = brushes[_brushIndex++]!;
                var contrastBrush = _timelineBrushes.GetContrastBrush(brush);

                _captions.Add(new TimelineCaption(foregroundColor: contrastBrush, id: id, label: label)
                {
                    ObjectIsControllerTuneable = objectIsControllerTuneable,
                    BackgroundColor = brush,
                });
            }
            else
            {
                _captions.Add(new TimelineCaption(foregroundColor: brushes[_brushIndex++], id: id, label: label)
                {
                    ObjectIsControllerTuneable = objectIsControllerTuneable,
                    BackgroundColor = _timelineBrushes.CaptionBackgroundBrush,
                });
            }
            if (_brushIndex >= brushes.Length) _brushIndex = 0; // start with first color again
        }

    }
}
