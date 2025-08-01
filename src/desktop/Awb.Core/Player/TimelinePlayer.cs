﻿// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.Project.Various;
using Awb.Core.Services;
using Awb.Core.Sounds;
using Awb.Core.Timelines;
using Awb.Core.Timelines.NestedTimelines;
using Awb.Core.Timelines.Sounds;
using Awb.Core.Tools;

namespace Awb.Core.Player
{
    /// <summary>
    /// Playback of a timeline including sending value changes to the actuators 
    /// </summary>
    //todo: no concept for ramping behaviour yet: Value changes are submittet to the actuatator imidiatelly
    public class TimelinePlayer : IDisposable
    {
        const bool throttlePackets = false;

        public enum PlayStates
        {
            Nothing,
            Playing
        }

        private PlayStates _playstateBackingField = PlayStates.Nothing;

        public PlayStates PlayState
        {
            get => _playstateBackingField;
            set
            {
                _playstateBackingField = value;
                PlayPosSynchronizer.PlayState = value;
            }
        }

        private const int _updatePlayPeriodMs = 50;
        private const int _updateActuatorsPeriodMs = 100;

        private TimelineData? _timelineData;
        private volatile bool _timerFiring;
        private volatile bool _updating;
        private volatile bool _actuatorUpdateRequested;
        private int _playPosMsOnLastUpdate;
        private Timer? _playTimer;
        private readonly Timer? _actuatorUpdateTimer;
        private DateTime? _lastPlayUpdate;

        // Delegate to play sounds
        public delegate void PlaySoundInDesktopDelegate(int soundId, TimeSpan start);
        public PlaySoundInDesktopDelegate? PlaySoundOnDesktop;
        public delegate void StopSoundInDesktopDelegate();
        public StopSoundInDesktopDelegate? StopSoundOnDesktop;

        private readonly IActuatorsService _actuatorsService;
        private readonly ITimelineDataService _timelineDataService;
        private readonly IAwbLogger _logger;
        private readonly ChangesToClientSender _sender;
        private readonly IInvoker _myInvoker;
        public EventHandler<PlayStateEventArgs>? OnPlayStateChanged;
        private volatile TimelinePoint[]? _allPointsMerged;
        private Sound[]? _projectSounds;
        private IServo[]? _projectServos;
        public readonly PlayPosSynchronizer PlayPosSynchronizer;


        /// <summary>
        /// The timeline to play
        /// </summary>
        public TimelineData? TimelineData => _timelineData;

        /// <summary>
        /// 1 = normal Speed
        /// </summary>
        public double PlaybackSpeed { get; set; } = 1;


        /// <remarks>
        /// After construction the player the "Play" method should be called with zero TimeSpan
        /// to set the actuators to the initial position. Because of the async character of Play 
        /// this is not dont automatically.
        /// </remarks>
        public TimelinePlayer(PlayPosSynchronizer playPosSynchronizer, IActuatorsService actuatorsService, ITimelineDataService timelineDataService, IAwbClientsService awbClientsService, IInvokerService invokerService, IAwbLogger logger)
        {
            _logger = logger;
            _actuatorsService = actuatorsService;
            _timelineDataService = timelineDataService;
            _sender = new ChangesToClientSender(actuatorsService, awbClientsService, _logger);
            _myInvoker = invokerService.GetInvoker();

            PlayPosSynchronizer = playPosSynchronizer;
            PlayPosSynchronizer.OnPlayPosChanged += async (sender, playPosMs) => await this.RequestActuatorUpdate();

            // Set up the actuator update timer
            if (throttlePackets == false)
            {
                _actuatorUpdateTimer = new Timer(ActuatorUpdateTimerCallback);
                _actuatorUpdateTimer.Change(dueTime: _updateActuatorsPeriodMs, period: _updateActuatorsPeriodMs);
            }
        }

        public void SetTimelineData(TimelineData timelineData, Sound[] projectSounds, IServo[] projectServos)
        {
            _allPointsMerged = null;
            _projectSounds = projectSounds;
            _projectServos = projectServos;
            if (_timelineData != null)
                _timelineData.OnContentChanged -= TimelineContentChanged;
            _timelineData = timelineData;
            _timelineData.OnContentChanged += TimelineContentChanged;
        }

        private void TimelineContentChanged(object? sender, TimelineDataChangedEventArgs e)
        {
            _allPointsMerged = null;
        }

        public async void Play()
        {
            if (_playTimer == null)
            {
                _playTimer = new Timer(PlayTimerCallback);
                _playTimer.Change(dueTime: _updatePlayPeriodMs, period: _updatePlayPeriodMs);
            }
            PlayState = PlayStates.Playing;
            PlayPosSynchronizer.PlayState = PlayState;
            await Task.CompletedTask;
        }

        public async void Stop()
        {
            PlayState = PlayStates.Nothing;
            PlayPosSynchronizer.PlayState = PlayState;

            StopSoundOnDesktop!();

            await Task.CompletedTask;
        }


        public async Task RequestActuatorUpdate()
        {
            if (throttlePackets)
            {
#pragma warning disable CS0162 // Unreachable code detected
                _actuatorUpdateRequested = true;
#pragma warning restore CS0162 // Unreachable code detected
                await Task.CompletedTask;

            }
            else
            {
                await UpdateActuatorsInternal();
            }
        }

        private async void ActuatorUpdateTimerCallback(object? state)
        {
            if (!_actuatorUpdateRequested) return;
            if (await UpdateActuatorsInternal())
            {
                _actuatorUpdateRequested = false;
            }
        }

        /// <summary>
        /// Move the actual play position to the given new position.
        /// Needed changes on the actuators will be communicated to the servos etc..
        /// </summary>
        private async Task<bool> UpdateActuatorsInternal()
        {
            if (_updating)
            {
                return false;
            }

            var playPos = PlayPosSynchronizer.PlayPosMsAutoSnappedOrUnSnapped;

            if (TimelineData == null)
            {
                await _logger.LogErrorAsync($"{nameof(UpdateActuatorsInternal)}: TimelineData is null. Cannot update actuators.");
                return false;
            }
            if (_projectServos == null || _projectSounds == null)
            {
                await _logger.LogErrorAsync($"{nameof(UpdateActuatorsInternal)}: ProjectServos or ProjectSounds is null. Cannot update actuators.");
                return false;
            }

            if (_allPointsMerged == null)
            {
                var allPoints = TimelineData.AllPoints.ToArray();
                _allPointsMerged = new EverythingMerger(_timelineDataService, projectSounds: _projectSounds, projectServos: _projectServos, awbLogger: _logger).Merge(allPoints).ToArray();
            }

            _updating = true;

            var start = DateTime.UtcNow;

            // Play Servos
            var servoPoints = _allPointsMerged.OfType<ServoPoint>().ToArray();
            var servoTargetObjectIds = servoPoints.Select(p => p.AbwObjectId).Distinct().ToArray();
            foreach (var servoTargetObjectId in servoTargetObjectIds)
            {
                var point1 = servoPoints.Where(p => p.AbwObjectId == servoTargetObjectId && p.TimeMs <= playPos).OrderByDescending(p => p.TimeMs).FirstOrDefault();
                var point2 = servoPoints.Where(p => p.AbwObjectId == servoTargetObjectId && p.TimeMs >= playPos).OrderBy(p => p.TimeMs).FirstOrDefault();

                if (point1 == null && point2 == null) continue; // no points found for this object before or after the actual position

                if (point1 == null) continue; // no points found for this object before or after the actual position

                point2 ??= point1;

                var pointDistanceMs = point2!.TimeMs - point1!.TimeMs;
                double targetValuePercent = 0;
                if (pointDistanceMs == 0)
                {
                    targetValuePercent = point1.ValuePercent;
                }
                else
                {
                    var posBetweenPoints = (playPos - point1.TimeMs * 1.0) / pointDistanceMs;
                    targetValuePercent = point1.ValuePercent + (point2.ValuePercent - point1.ValuePercent) * posBetweenPoints;
                }

                var targetServo = _actuatorsService.Servos.SingleOrDefault(o => o.Id.Equals(servoTargetObjectId));
                if (targetServo == null)
                {
                    await _logger.LogErrorAsync($"{nameof(UpdateActuatorsInternal)}: Targets servo object with id {servoTargetObjectId} not found.");
                }
                else
                {
                    var targetValue = (int)(targetServo.MinValue + targetValuePercent / 100.0 * (targetServo.MaxValue - targetServo.MinValue));
                    if (!targetValue.Equals(targetServo.TargetValue))
                    {
                        targetServo.TargetValue = targetValue;
                        targetServo.IsDirty = true;
                    }
                }
            }

            // Play Sounds
            var soundPoints = _allPointsMerged.OfType<SoundPoint>().ToArray();
            var soundTargetObjectIds = soundPoints.Select(p => p.AbwObjectId).Distinct().ToArray();

            foreach (var soundTargetObjectId in soundTargetObjectIds)
            {
                var targetSoundPlayer = _actuatorsService.SoundPlayers.SingleOrDefault(o => o.Id.Equals(soundTargetObjectId));
                if (targetSoundPlayer == null)
                {
                    await _logger.LogErrorAsync($"{nameof(UpdateActuatorsInternal)}: Target soundplayer object with id {soundTargetObjectId} not found.");
                    continue;
                }

                switch (PlayState)
                {
                    case PlayStates.Nothing:
                        // take exactly the point at the actual position
                        var soundPoint = soundPoints.GetPoint(playPos, soundTargetObjectId) as SoundPoint;
                        if (soundPoint == null)
                        {
                            if (targetSoundPlayer.ActualSoundId != null)
                            {
                                targetSoundPlayer.SetActualSoundId(null, TimeSpan.Zero);
                                targetSoundPlayer.SetActuatorMovementBySound([]);
                                targetSoundPlayer.IsDirty = true;
                            }
                        }
                        else
                        {
                            if (soundPoint.SoundId != targetSoundPlayer.ActualSoundId)
                            {
                                targetSoundPlayer.SetActualSoundId(soundPoint.SoundId, TimeSpan.Zero);
                                targetSoundPlayer.IsDirty = true;
                            }
                            if (ActuatorMovementBySound.AreEqual(soundPoint.ActuatorMovementsBySound, targetSoundPlayer.ActuatorMovementsBySound) == false)
                            {
                                targetSoundPlayer.SetActuatorMovementBySound(soundPoint.ActuatorMovementsBySound);
                                targetSoundPlayer.IsDirty = true;
                            }
                        }

                        break;
                    case PlayStates.Playing:
                        // take a point between the last and the actual position
                        //soundPoint = soundPoints.GetPointsBetween<SoundPoint>(_playPosMsOnLastUpdate, playPos, soundTargetObjectId).FirstOrDefault();
                        var playSoundArgs = GetSoundPlayerCommandForTimelinePos(soundPoints, soundTargetObjectId, playPos);
                        if (playSoundArgs != null)
                        {
                            var startTime = playSoundArgs.StartTime ?? TimeSpan.Zero;
                            targetSoundPlayer.SetActualSoundId(playSoundArgs.SoundId, startTime: startTime);
                            targetSoundPlayer.IsDirty = true;
                            if (PlaySoundOnDesktop != null) PlaySoundOnDesktop(playSoundArgs.SoundId, start: startTime);
                        }
                        else
                        {
                            targetSoundPlayer.SetActualSoundId(null, TimeSpan.Zero);
                            targetSoundPlayer.IsDirty = true;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"{nameof(PlayState)}:{PlayState}");
                }
            }


            // Update Nested timelines 
            NestedTimelinePoint? nestedTimelinePoint = null;

            switch (PlayState)
            {
                case PlayStates.Nothing:
                    // take exactly the point at the actual position
                    nestedTimelinePoint = TimelineData?.NestedTimelinePoints.GetPoint<NestedTimelinePoint>(playPos, NestedTimelinesFakeObject.Singleton.Id);
                    break;
                case PlayStates.Playing:
                    // take a point between the last and the actual position
                    nestedTimelinePoint = TimelineData?.NestedTimelinePoints.GetPointsBetween<NestedTimelinePoint>(_playPosMsOnLastUpdate, playPos, NestedTimelinesFakeObject.Singleton.Id).FirstOrDefault();
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"{nameof(PlayState)}:{PlayState}");
            }

            if (nestedTimelinePoint == null)
            {
                NestedTimelinesFakeObject.Singleton.ActualNestedTimelineId = null;
                NestedTimelinesFakeObject.Singleton.IsDirty = true;
            }
            else if (nestedTimelinePoint.TimelineId != NestedTimelinesFakeObject.Singleton.ActualNestedTimelineId)
            {
                NestedTimelinesFakeObject.Singleton.ActualNestedTimelineId = nestedTimelinePoint.TimelineId;
                NestedTimelinesFakeObject.Singleton.IsDirty = true;
            }


            _playPosMsOnLastUpdate = playPos;

            var ok = await _sender.SendChangesToClients();
            _updating = false;
            return ok;
        }

        private SoundPlayEventArgs? GetSoundPlayerCommandForTimelinePos(SoundPoint[] soundPoints, string soundTargetObjectId, int timeMs)
        {
            if (soundPoints == null || soundTargetObjectId == null) return null;
            foreach (var soundPoint in soundPoints.OrderBy(s => s.TimeMs))
            {
                if (soundPoint.AbwObjectId == soundTargetObjectId)
                {
                    var durationMs = GetDurationMsForSoundId(soundPoint.SoundId, soundPoint.AbwObjectId);
                    if (timeMs >= soundPoint.TimeMs && timeMs <= soundPoint.TimeMs + durationMs)
                    {
                        return new SoundPlayEventArgs(soundPoint.SoundId, startTime: TimeSpan.FromMilliseconds(timeMs - soundPoint.TimeMs));
                    }
                }
            }
            return null;
        }

        private int GetDurationMsForSoundId(int soundId, string soundPlayerId)
        {
            var sound = _projectSounds?.FirstOrDefault(s => s.Id == soundId);
            if (sound != null) return sound.DurationMs;
            return 500; // default duration if no sound is found
        }

        private void PlayTimerCallback(object? state)
        {
            if (_timerFiring) return;

            if (_lastPlayUpdate == null)
            {
                _lastPlayUpdate = DateTime.UtcNow;
                return;
            }

            _myInvoker.Invoke(() =>
            {
                _timerFiring = true;

                TimeSpan diff = (DateTime.UtcNow - _lastPlayUpdate.Value);
                _lastPlayUpdate = DateTime.UtcNow;

                if (PlayState == PlayStates.Playing)
                {
                    var newPos = 0;

                    if (_projectSounds == null)
                    {
                        _logger.LogErrorAsync("ProjectSounds is null. Cannot update play position.").Wait();
                        _timerFiring = false;
                        return;
                    }
                    if (TimelineData == null)
                    {
                        _logger.LogErrorAsync("TimelineData is null. Cannot update play position.").Wait();
                        _timerFiring = false;
                        return;
                    }
                    var timelineDurationMs = TimelineData.GetDurationMs(projectSounds: _projectSounds, timelineDataService: _timelineDataService);
                    if (PlayPosSynchronizer.PlayPosMsAutoSnappedOrUnSnapped >= timelineDurationMs)
                    {
                        newPos = 0;
                    }
                    else
                    {
                        newPos = PlayPosSynchronizer.PlayPosMsAutoSnappedOrUnSnapped + (int)(diff.TotalMilliseconds * PlaybackSpeed);
                        if (newPos > timelineDurationMs) newPos = timelineDurationMs;

                    }
                    PlayPosSynchronizer.SetNewPlayPos(newPos);
                }

                _timerFiring = false;
            });
        }

        public async void Dispose()
        {
            _actuatorUpdateTimer?.Dispose();

            if (_playTimer != null)
                _playTimer.Dispose();

            var servos = _actuatorsService.Servos;
            foreach (var servo in servos)
            {
                servo.TurnOff();
            }
            for (int tries = 0; tries < 10; tries++)
            {
                var ok = await _sender.SendChangesToClients();
                await Task.Delay(100);
                if (ok) break;
                await Task.Delay(500);
            }
            if (_timelineData != null)
            {
                _timelineData.OnContentChanged -= TimelineContentChanged;
            }

        }
    }
}
