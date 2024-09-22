// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Player;
using System;

namespace AwbStudio.TimelineEditing
{
    internal class TimelineKeyboardHandling
    {
        private readonly TimelineEventHandling _timelineEventHandling;
        private readonly TimelinePlayer _timelinePlayer;
        private readonly PlayPosSynchronizer _playPosSynchronizer;
        private readonly TimelineViewContext _timelineViewContext;
        private bool _ctrlKeyPressed; // is the control key on the keyboard pressed?
        private bool _winKeyPressed; // is the windows key on the keyboard pressed?

        public EventHandler? SaveTimelineData { get; set; }

        public TimelineKeyboardHandling(
            TimelineEventHandling timelineEventHandling, 
            TimelinePlayer timelinePlayer, 
            PlayPosSynchronizer playPosSynchronizer,
            TimelineViewContext timelineViewContext)
        {
            if (timelineEventHandling == null) throw new ArgumentNullException(nameof(timelineEventHandling));
            if (timelinePlayer == null) throw new ArgumentNullException(nameof(timelinePlayer));
            if (playPosSynchronizer == null) throw new ArgumentNullException(nameof(playPosSynchronizer));
            if (timelineViewContext == null) throw new ArgumentNullException(nameof(timelineViewContext));

            _timelineEventHandling = timelineEventHandling;
            _timelinePlayer = timelinePlayer;
            _playPosSynchronizer = playPosSynchronizer;
            _timelineViewContext = timelineViewContext;
        }

        /// <summary>
        /// Keybord input handling
        /// </summary>
        public void KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            const int manualScrollSpeedMs = 125;

            var switchPlayStop = () => {
                if (this._timelineEventHandling != null)
                {
                    if (this._timelinePlayer.PlayState == TimelinePlayer.PlayStates.Playing)
                        this._timelineEventHandling.Stop();
                    else
                        this._timelineEventHandling.Play();
                }
            };

            switch (e.Key)
            {
                // remember special key states
                case System.Windows.Input.Key.LeftCtrl:
                case System.Windows.Input.Key.RightCtrl:
                    this._ctrlKeyPressed = e.IsDown;
                    break;

                case System.Windows.Input.Key.LWin:
                case System.Windows.Input.Key.RWin:
                    this._winKeyPressed = e.IsDown;
                    break;

                // save actual timeline
                case System.Windows.Input.Key.S:
                    if (this._ctrlKeyPressed) SaveTimelineData?.Invoke(this, EventArgs.Empty);
                    break;

                // start / stop playback
                case System.Windows.Input.Key.Space:
                    switchPlayStop();
                    break;

                case System.Windows.Input.Key.System:
                    if (this._winKeyPressed) switchPlayStop(); // support for e.g. shuttle express controller
                    break;

                // playpos navigation support for e.g. shuttle express controller
                case System.Windows.Input.Key.F12: // scroll playpos forward,  support for e.g. shuttle express controller
                    if (_winKeyPressed && _playPosSynchronizer.PlayPosMsGuaranteedSnapped <= _timelineViewContext.DurationMs - manualScrollSpeedMs)
                        _playPosSynchronizer.SetNewPlayPos(_playPosSynchronizer.PlayPosMsAutoSnappedOrUnSnapped + manualScrollSpeedMs);
                    break;
                case System.Windows.Input.Key.F11: // scroll playpos backwards
                    if (_winKeyPressed && _playPosSynchronizer.PlayPosMsGuaranteedSnapped >= manualScrollSpeedMs)
                        _playPosSynchronizer.SetNewPlayPos(_playPosSynchronizer.PlayPosMsAutoSnappedOrUnSnapped - manualScrollSpeedMs);
                    break;

            }
        }
    }
}
