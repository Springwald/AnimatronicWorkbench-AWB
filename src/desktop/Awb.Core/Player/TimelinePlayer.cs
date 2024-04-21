// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Services;
using Awb.Core.Timelines;
using Awb.Core.Tools;

namespace Awb.Core.Player
{
    /// <summary>
    /// Playback of a timeline including sending value changes to the actuators 
    /// </summary>
    //todo: no concept for ramping behaviour yet: Value changes are submittet to the actuatator imidiatelly
    public class TimelinePlayer : IDisposable
    {

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

        private volatile bool _timerFiring;
        private volatile bool _updating;
        private int _playPosMsOnLastUpdate;

        private int _updatePlayPeriodMs = 50;
        private Timer? _playTimer;
        private DateTime? _lastPlayUpdate;

        private readonly IActuatorsService _actuatorsService;
        private readonly IAwbLogger _logger;

        private readonly ChangesToClientSender _sender;
        private readonly IInvoker _myInvoker;
        public EventHandler<PlayStateEventArgs>? OnPlayStateChanged;
        public EventHandler<SoundPlayEventArgs>? OnPlaySound;


        public readonly PlayPosSynchronizer PlayPosSynchronizer;

        /// <summary>
        /// The timeline to play
        /// </summary>
        public TimelineData TimelineData { get; set; }

        /// <summary>
        /// 1 = normal Speed
        /// </summary>
        public double PlaybackSpeed { get; set; } = 1;

        /// <remarks>
        /// After construction the player the "Play" method should be called with zero TimeSpan
        /// to set the actuators to the initial position. Because of the async character of Play 
        /// this is not dont automatically.
        /// </remarks>
        public TimelinePlayer(TimelineData timelineData, PlayPosSynchronizer playPosSynchronizer, IActuatorsService actuatorsService, IAwbClientsService awbClientsService, IInvokerService invokerService, IAwbLogger logger)
        {
            if (timelineData == null) throw new ArgumentNullException(nameof(timelineData));

            _logger = logger;
            _actuatorsService = actuatorsService;
            _sender = new ChangesToClientSender(actuatorsService, awbClientsService, _logger);
            _myInvoker = invokerService.GetInvoker();

            TimelineData = timelineData;

            PlayPosSynchronizer = playPosSynchronizer;
            PlayPosSynchronizer.OnPlayPosChanged += async (sender, playPosMs) => await this.UpdateActuators();
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
            await Task.CompletedTask;
        }


        /// <summary>
        /// Move the actual play position to the given new position.
        /// Needed changes on the actuators will be communicated to the servos etc..
        /// </summary>
        public async Task UpdateActuators()
        {
            if (_updating)
            {
                return;
            }

            var playPos = PlayPosSynchronizer.PlayPosMsAutoSnappedOrUnSnapped;

            _updating = true;

            var start = DateTime.UtcNow;

            // Play Servos
            var servoTargetObjectIds = TimelineData.ServoPoints.Select(p => p.AbwObjectId).Distinct().ToArray();
            foreach (var servoTargetObjectId in servoTargetObjectIds)
            {
                var point1 = TimelineData.ServoPoints.Where(p => p.AbwObjectId == servoTargetObjectId && p.TimeMs <= playPos).OrderByDescending(p => p.TimeMs).FirstOrDefault();
                var point2 = TimelineData.ServoPoints.Where(p => p.AbwObjectId == servoTargetObjectId && p.TimeMs >= playPos).OrderBy(p => p.TimeMs).FirstOrDefault();

                if (point1 == null && point2 == null) continue; // no points found for this object before or after the actual position
                point1 ??= point2;
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
                    await _logger.LogError($"{nameof(UpdateActuators)}: Targets servo object with id {servoTargetObjectId} not found.");
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
            var soundTargetObjectIds = TimelineData.SoundPoints.Select(p => p.AbwObjectId).Distinct().ToArray();
            foreach (var soundTargetObjectId in soundTargetObjectIds)
            {
                var lower = Math.Min(_playPosMsOnLastUpdate, playPos);
                var higher = Math.Max(_playPosMsOnLastUpdate, playPos);
                var pointsWithMatchingMs = TimelineData.SoundPoints.Where(p => p.AbwObjectId == soundTargetObjectId && p.TimeMs >= lower && p.TimeMs <= higher);
                var targetPoint = pointsWithMatchingMs.OrderBy(p => Math.Abs(p.TimeMs - playPos)).FirstOrDefault();
                if (targetPoint == null) continue; // no points found for this at the actual position

                var targetSoundPlayerId = targetPoint.SoundPlayerId;
                var targetSoundPlayer = _actuatorsService.SoundPlayers.SingleOrDefault(o => o.Id.Equals(targetSoundPlayerId));
                if (targetSoundPlayer == null)
                {
                    await _logger.LogError($"{nameof(UpdateActuators)}: Target soundplayer object with id {targetSoundPlayerId} not found.");
                }
                else
                {
                    if (targetPoint.SoundId != targetSoundPlayer.ActualSoundId)
                    {
                        targetSoundPlayer.PlaySound(targetPoint.SoundId);
                        targetSoundPlayer.IsDirty = true;
                    }
                }
                if (OnPlaySound != null) OnPlaySound.Invoke(this, new SoundPlayEventArgs(targetPoint.SoundId));
            }

            _playPosMsOnLastUpdate = playPos;

            var ok = await _sender.SendChangesToClients();
            _updating = false;

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
                    if (PlayPosSynchronizer.PlayPosMsAutoSnappedOrUnSnapped >= TimelineData.DurationMs)
                    {
                        newPos = 0;
                    }
                    else
                    {
                        newPos = PlayPosSynchronizer.PlayPosMsAutoSnappedOrUnSnapped + (int)(diff.TotalMilliseconds * PlaybackSpeed);
                        if (newPos > TimelineData.DurationMs) newPos = TimelineData.DurationMs;

                    }
                    PlayPosSynchronizer.SetNewPlayPos(newPos);
                }

                _timerFiring = false;
            });
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
