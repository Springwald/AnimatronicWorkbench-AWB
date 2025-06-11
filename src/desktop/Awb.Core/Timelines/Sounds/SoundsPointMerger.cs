// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Services;
using Awb.Core.Sounds;

namespace Awb.Core.Timelines.Sounds
{
    public class SoundsPointMerger : IMerger
    {
        private readonly Sound[] _projectSounds;
        private readonly IAwbLogger _awbLogger;

        public IEnumerable<TimelinePoint> Merge(IEnumerable<TimelinePoint> rawPoints, int recursionDepth = 0)
        {
            foreach (var point in rawPoints)
            {
                if (point is SoundPoint soundPoint)
                {
                    var servoPoints = this.GenerateServoPoints(soundPoint);
                    foreach (var servoPoint in servoPoints)
                        yield return servoPoint;
                }
                else
                    yield return point;
            }
        }


        public SoundsPointMerger(Sound[] projectSounds, IAwbLogger awbLogger)
        {
            if (projectSounds == null)
                throw new ArgumentNullException(nameof(projectSounds), "Project sounds cannot be null or empty.");

            _projectSounds = projectSounds;
            _awbLogger = awbLogger;
        }


        private IEnumerable<ServoPoint> GenerateServoPoints(SoundPoint soundPoint)
        {
            if (soundPoint.MovementServoId == null) yield break;

            if (_projectSounds == null || _projectSounds.Length == 0)
            {
                _awbLogger.LogErrorAsync("No project sounds available to generate servo points from SoundPoint.").Wait();
                yield break;
            }

            var sound = _projectSounds.FirstOrDefault(s => s.Id == soundPoint.SoundId);
            if (sound == null)
            {
                _awbLogger.LogErrorAsync($"Sound with ID {soundPoint.SoundId} not found for SoundPoint at {soundPoint.TimeMs}ms.").Wait();
                yield break;
            }

            // Start Point
            yield return new ServoPoint(soundPoint.MovementServoId, soundPoint.MovementInverted ? 100: 0, soundPoint.TimeMs) { IsNestedTimelinePoint = true };

            bool useMaxValue = true; // user max or average value for servo movement?

            double maxValue = 0;
            double sampleSum = 0; // value to be collected for servo movement
            double lastValue = 0;
            int samples = 0;
            var samplesPerFreqcy = (Sound.SamplesPerSecond / 1000.0) * soundPoint.MovementFrequencyMs; // how many samples per frequency
            int timePosMs = soundPoint.TimeMs;
            
            foreach (var sample in sound.Samples)
            {
                maxValue = Math.Max(maxValue, sample); // find the max value in the samples
                sampleSum += sample;
                if (++samples >= samplesPerFreqcy)
                {
                    timePosMs += soundPoint.MovementFrequencyMs; // move to next frequency point
                    var collectedValue = (useMaxValue ? maxValue : (sampleSum / samples)) / 255.0 * 100; // rescale to percentage, taking min and max into account
                    //if (Math.Abs(collectedValue - lastValue) > 0.1) // only create a point if the value changed
                    {
                        if (soundPoint.MovementInverted)
                            collectedValue = 100 - collectedValue; // invert value if needed
                        yield return new ServoPoint(soundPoint.MovementServoId, collectedValue, timePosMs) { IsNestedTimelinePoint = true };
                        lastValue = collectedValue; // remember last value
                    }
                    maxValue = 0;
                    samples = 0;
                    collectedValue = 0;
                    sampleSum = 0;
                }
            }

            // End Point
            yield return new ServoPoint(soundPoint.MovementServoId, soundPoint.MovementInverted ? 100 : 0,  timePosMs+1) { IsNestedTimelinePoint = true };
        }


    }
}
