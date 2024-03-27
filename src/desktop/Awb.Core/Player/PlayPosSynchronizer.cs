// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Timers;

namespace Awb.Core.Player
{
    public class PlayPosSynchronizer: IDisposable
    {
        /// <summary>
        /// The distance between two snap positions in milliseconds
        /// </summary>
        public const int SnapMs = 125;

        private int _playPosMsRaw;
        private int _lastPlayPosAnnounced;
        private bool _inSnapMode = true;
        private System.Timers.Timer _timer;

        /// <summary>
        /// The playpos has changed
        /// </summary>
        public EventHandler<int>? OnPlayPosChanged;

        /// <summary>
        /// the actual play position in  millseconds; snapped or no snapped - depending on "InSnapMode"
        /// </summary>
        public int PlayPosMs { get; private set; }

        public PlayPosSynchronizer()
        {
            _timer = new System.Timers.Timer();
            _timer.Elapsed += new ElapsedEventHandler(OnTimedEvent!);
            _timer.Interval = 100; // ms
            _timer.Enabled = true;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (_lastPlayPosAnnounced != PlayPosMs)
            {
                _lastPlayPosAnnounced = PlayPosMs;
                this.OnPlayPosChanged?.Invoke(this, PlayPosMs);
            }
        }

        /// <summary>
        /// Is the playpos to snap to the next snap position?
        /// </summary>
        public bool InSnapMode
        {
            set
            {
                if (_inSnapMode != value)
                {
                    _inSnapMode = value;
                    SetNewPlayPos(_playPosMsRaw);
                }
            }
        }

        public void SetNewPlayPos(int playPosMs)
        {
            _playPosMsRaw = playPosMs;
            PlayPosMs = _inSnapMode ? (playPosMs / SnapMs) * SnapMs : playPosMs;
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
