﻿// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Services;
using Awb.Core.Timelines;
using System.Diagnostics;

namespace Awb.Core.Player
{
    /// <summary>
    /// Playback of a timeline including sending value changes to the actuators 
    /// </summary>
    //todo: no concept for ramping behaviour yet: Value changes are submittet to the actuatator imidiatelly
    public class TimelinePlayer : IDisposable
    {
        public const int PlayPosSnapMs = 250;

        public enum PlayStates
        {
            Nothing,
            Playing
        }

        public  PlayStates PlayState { get; private set; } = PlayStates.Nothing;

        private volatile bool _timerFiring;
        private volatile bool _updating;

        private int _updatePlayPeriodMs = 50;
        private Timer? _playTimer;
        private DateTime? _lastPlayUpdate;

        private readonly IActuatorsService _actuatorsService;
        private readonly IAwbLogger _logger;
        private readonly ChangesToClientSender _sender;

        public EventHandler<PlayStateEventArgs> OnPlayStateChanged;

        /// <summary>
        /// The timeline to play
        /// </summary>
        public TimelineData TimelineData { get; set; }

        /// <summary>
        /// 1 = normal Speed
        /// </summary>
        public double PlaybackSpeed { get; set; } = 1;

        /// <summary>
        /// The actual play position
        /// </summary>
        public int PositionMs { get; private set; }

        /// <remarks>
        /// After construction the player the "Play" method should be called with zero TimeSpan
        /// to set the actuators to the initial position. Because of the async character of Play 
        /// this is not dont automatically.
        /// </remarks>
        public TimelinePlayer(TimelineData timelineData, IActuatorsService actuatorsService, IAwbClientsService awbClientsService, IAwbLogger logger)
        {
            if (timelineData == null) throw new ArgumentNullException(nameof(timelineData));

            _logger = logger;
            _actuatorsService = actuatorsService;
            _sender = new ChangesToClientSender(actuatorsService, awbClientsService, _logger);
            TimelineData = timelineData;
        }

        public async void Play()
        {
            if (_playTimer == null)
            {
                _playTimer = new Timer(PlayTimerCallback);
                _playTimer.Change(dueTime: _updatePlayPeriodMs, period: _updatePlayPeriodMs);
            }
            PlayState = PlayStates.Playing;
            await Task.CompletedTask;
        }


        public async void Stop()
        {
            PlayState = PlayStates.Nothing;
            await Task.CompletedTask;
        }

        public async Task Update()
        {
            await Update(PositionMs);
        }

        /// <summary>
        /// Move the actual play position to the given new position.
        /// Needed changes on the actuators will be communicated to the servos etc..
        /// </summary>
        /// <param name="newPositionMs">the new position of the timeline</param>
        public async Task Update(int newPositionMs)
        {
            if (_updating) return;

            _updating = true;

            var start = DateTime.UtcNow;

            // Play Servos
            var servoTargetObjectIds = TimelineData.ServoPoints.Select(p => p.TargetObjectId).Distinct().ToArray();
            foreach (var servoTargetObjectId in servoTargetObjectIds)
            {
                var point1 = TimelineData.ServoPoints.Where(p => p.TargetObjectId == servoTargetObjectId && p.TimeMs <= newPositionMs).OrderByDescending(p => p.TimeMs).FirstOrDefault();
                var point2 = TimelineData.ServoPoints.Where(p => p.TargetObjectId == servoTargetObjectId && p.TimeMs >= newPositionMs).OrderBy(p => p.TimeMs).FirstOrDefault();

                if (point1 == null && point2 == null) continue; // no points found for this object before or after the actual position
                point1 ??= point2;
                point2 ??= point1;

                var pointDistanceMs = point2.TimeMs - point1.TimeMs;
                double targetValuePercent = 0;
                if (pointDistanceMs == 0)
                {
                    targetValuePercent = point1.ValuePercent;
                }
                else
                {
                    var posBetweenPoints = (newPositionMs - point1.TimeMs * 1.0) / pointDistanceMs;
                    targetValuePercent = point1.ValuePercent + (point2.ValuePercent - point1.ValuePercent) * posBetweenPoints;
                }

                var targetServo = _actuatorsService.Servos.SingleOrDefault(o => o.Id.Equals(servoTargetObjectId));
                if (targetServo == null)
                {
                    await _logger.LogError($"{nameof(Update)}: Target object with id {servoTargetObjectId} not found.");
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
            var soundTargetObjectIds = TimelineData.SoundPoints.Select(p => p.TargetObjectId).Distinct().ToArray();
            foreach (var soundTargetObjectId in soundTargetObjectIds)
            {
                // find a point exactly on the position
                var targetPoint = TimelineData.SoundPoints.Where(p => p.TargetObjectId == soundTargetObjectId && p.TimeMs <= newPositionMs).OrderByDescending(p => p.TimeMs).FirstOrDefault();
                if (targetPoint == null) continue; // no points found for this object before the actual position
                throw new NotImplementedException();
            }

            PositionMs = newPositionMs;

            if (OnPlayStateChanged != null) OnPlayStateChanged.Invoke(this, new PlayStateEventArgs
            {
                PlayState = PlayState,
                PlaybackSpeed = PlaybackSpeed,
                PositionMs = newPositionMs,
            });

            var ok = await _sender.SendChangesToClients();
            _updating = false;
        }


        private async void PlayTimerCallback(object? state)
        {
            if (_timerFiring) return;

            if (_lastPlayUpdate == null)
            {
                _lastPlayUpdate = DateTime.UtcNow;
                return;
            }

            _timerFiring = true;

            TimeSpan diff = (DateTime.UtcNow - _lastPlayUpdate.Value);
            _lastPlayUpdate = DateTime.UtcNow;

            if (PlayState == PlayStates.Playing)
            {
                if (PositionMs >= TimelineData.DurationMs)
                {
                    await this.Update(newPositionMs: 0);
                }
                else
                {

                    var newPos = PositionMs + (int)(diff.TotalMilliseconds * PlaybackSpeed);
                    if (newPos > TimelineData.DurationMs) newPos = TimelineData.DurationMs;
                    await this.Update(newPositionMs: newPos);
                }
            }

            _timerFiring = false;
        }

        public async void Dispose()
        {
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
            
        }
    }
}
