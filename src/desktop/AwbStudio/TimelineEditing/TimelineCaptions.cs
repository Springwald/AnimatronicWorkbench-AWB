// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.Project;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace AwbStudio.TimelineEditing
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
            new SolidColorBrush(Colors.Beige),
            new SolidColorBrush(Colors.Tomato),
            new SolidColorBrush(Colors.SkyBlue),
            new SolidColorBrush(Colors.MintCream),
            new SolidColorBrush(Colors.LightGoldenrodYellow),
            new SolidColorBrush(Colors.LightPink),
            new SolidColorBrush(Colors.LightSteelBlue),
            new SolidColorBrush(Colors.LemonChiffon),
        };

        private Brush _brushBlack = new SolidColorBrush(Colors.Black);

        private int _brushIndex = 0;

        private List<TimelineCaption> _captions = new List<TimelineCaption>();

        public TimelineCaption[] Captions => _captions.ToArray();

        public TimelineCaptions()
        {
        }

        public void AddAktuator(IActuator actuator)
        {
            var clientIdPraefix = actuator.ClientId == 1 ? string.Empty : $"C{actuator.ClientId}-";

            var label = actuator switch
            {
                Mp3PlayerYX5300 mp3 => $"{clientIdPraefix}{(string.IsNullOrWhiteSpace(mp3.Title) ? $"MP3-{mp3.Id}" : mp3.Title)}",
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
            if (inverse)
            {
                _captions.Add(new TimelineCaption(foregroundColor: _brushBlack, id: id, label: label)
                {
                    ObjectIsControllerTuneable = objectIsControllerTuneable,
                    BackgroundColor = _brushes[_brushIndex++],
                });
            }
            else
            {
                _captions.Add(new TimelineCaption(foregroundColor: _brushes[_brushIndex++], id: id, label: label)
                {
                    ObjectIsControllerTuneable = objectIsControllerTuneable,
                    BackgroundColor = null,
                });
            }
            if (_brushIndex >= _brushes.Length) _brushIndex = 0; // start with first color again
        }

    }

}
