// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Tools;
using System.Timers;

namespace Awb.Core.Player
{
    public class PlayPosSynchronizer : IDisposable
    {
        /// <summary>
        /// The distance between two snap positions in milliseconds
        /// </summary>
        public const int SnapMs = 125;

        private const int _timerIntervalMs = 100;

        private int _playPosMsRaw;
        private int _lastPlayPosAnnounced;

        /// <summary>
        /// Is the playpos to snap to the next snap position?
        /// </summary>
        private bool _inSnapMode = true;
        private readonly IInvoker _invoker;
        private System.Timers.Timer? _timer;

        /// <summary>
        /// The playpos has changed
        /// </summary>
        public EventHandler<int>? OnPlayPosChanged;

        /// <summary>
        /// the actual play position in  millseconds; snapped or no snapped - depending on "InSnapMode"
        /// </summary>
        public int PlayPosMsAutoSnappedOrUnSnapped { get; private set; }

        /// <summary>
        /// the actual play position in  millseconds; snapped - independing on "InSnapMode"
        /// </summary>
        public int PlayPosMsGuaranteedSnapped => Snap(PlayPosMsAutoSnappedOrUnSnapped);

        /// <summary>
        /// Snap the given millisecond value to the next snap position
        /// </summary>
        public static int Snap(int ms) => (ms / SnapMs) * SnapMs;  

        public PlayPosSynchronizer(IInvoker invoker)
        {
            _invoker = invoker;
            _timer = new System.Timers.Timer();
            _timer.Elapsed += new ElapsedEventHandler(OnTimedEvent!);
            _timer.Interval = _timerIntervalMs; // ms
            _timer.Enabled = true;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (_lastPlayPosAnnounced != PlayPosMsAutoSnappedOrUnSnapped)
            {
                _lastPlayPosAnnounced = PlayPosMsAutoSnappedOrUnSnapped;
                // this is important! We must not call the event handler in the timer thread, because the event handler should update the UI.
                // so we use the invoker using the hosting wpf application thread instead.
                _invoker.Invoke(() => this.OnPlayPosChanged?.Invoke(this, PlayPosMsAutoSnappedOrUnSnapped));
            }
        }


        public TimelinePlayer.PlayStates PlayState
        {
            set
            {
                bool newInSnapMode;
                switch (value)
                {
                    case TimelinePlayer.PlayStates.Nothing:
                        _timer!.Interval = _timerIntervalMs;
                        newInSnapMode = true;
                        break;
                    case TimelinePlayer.PlayStates.Playing:
                        _timer!.Interval = _timerIntervalMs / 5; // higher resolution in playing mode
                        newInSnapMode = false;
                        break;
                    default:
                        throw new NotImplementedException($"{nameof(PlayState)}:{value}");
                }

                if (_inSnapMode != newInSnapMode)
                {
                    _inSnapMode = newInSnapMode;
                    SetNewPlayPos(_playPosMsRaw);
                }
            }
        }

        public void SetNewPlayPos(int playPosMs)
        {
            _playPosMsRaw = playPosMs;
            PlayPosMsAutoSnappedOrUnSnapped = _inSnapMode ? (playPosMs / SnapMs) * SnapMs : playPosMs;
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
        }
    }
}
