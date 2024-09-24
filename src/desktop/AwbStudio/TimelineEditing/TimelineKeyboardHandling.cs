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
        private readonly TimelineEditingManipulation _timelineEditingManipulation;
        private readonly TimelineViewContext _timelineViewContext;
        private bool _ctrlKeyPressed; // is the control key on the keyboard pressed?
        private bool _winKeyPressed; // is the windows key on the keyboard pressed?

        public EventHandler? SaveTimelineData { get; set; }

        public TimelineKeyboardHandling(
            TimelineEventHandling timelineEventHandling,
            TimelinePlayer timelinePlayer,
            PlayPosSynchronizer playPosSynchronizer,
            TimelineEditingManipulation timelineEditingManipulation,
            TimelineViewContext timelineViewContext)
        {
            if (timelineEventHandling == null) throw new ArgumentNullException(nameof(timelineEventHandling));
            if (timelinePlayer == null) throw new ArgumentNullException(nameof(timelinePlayer));
            if (playPosSynchronizer == null) throw new ArgumentNullException(nameof(playPosSynchronizer));
            if (timelineEditingManipulation == null) throw new ArgumentNullException(nameof(timelineEditingManipulation));
            if (timelineViewContext == null) throw new ArgumentNullException(nameof(timelineViewContext));


            _timelineEventHandling = timelineEventHandling;
            _timelinePlayer = timelinePlayer;
            _playPosSynchronizer = playPosSynchronizer;
            _timelineEditingManipulation = timelineEditingManipulation;
            _timelineViewContext = timelineViewContext;
        }

        /// <summary>
        /// Keybord input handling
        /// </summary>
        public void KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            const int manualScrollSpeedMs = 125;

            var switchPlayStop = () =>
            {
                if (this._timelineEventHandling != null)
                {
                    if (this._timelinePlayer.PlayState == TimelinePlayer.PlayStates.Playing)
                        this._timelineEventHandling.Stop();
                    else
                        this._timelineEventHandling.Play();
                }
            };

          
            var startMs = _timelineViewContext.SelectionStartMs;
            var endMs = _timelineViewContext.SelectionEndMs;
            var existsSelection = startMs != null && endMs != null;
            if (startMs != null && endMs != null && startMs > endMs) (startMs, endMs) = (endMs, startMs); // swap values

            if (this._ctrlKeyPressed)
            {
                switch (e.Key)
                {
                    case System.Windows.Input.Key.X: // CTRL + X : cut 
                        if (existsSelection)
                        { 
                            _timelineEditingManipulation.CopyNPasteBuffer = _timelineEditingManipulation.Cut(startMs!.Value, endMs!.Value);
                            if (_timelineEditingManipulation.CopyNPasteBuffer != null)
                            {
                                _timelineViewContext.SelectionStartMs = null;
                                _timelineViewContext.SelectionEndMs = null;
                            }
                        }
                        break;

                    case System.Windows.Input.Key.C:// CTRL + C : copy
                        if (existsSelection)
                        {
                            _timelineEditingManipulation.CopyNPasteBuffer = _timelineEditingManipulation.Copy(startMs!.Value, endMs!.Value);
                            if (_timelineEditingManipulation.CopyNPasteBuffer != null)
                            {
                                // copy was successful
                            }
                        }
                        break;

                    case System.Windows.Input.Key.V: // CTRL + V : paste
                        if (_timelineEditingManipulation.CopyNPasteBuffer != null)
                        {
                            var targetStartMs2 = _playPosSynchronizer.PlayPosMsGuaranteedSnapped;
                            if (existsSelection)
                            {
                                // first remove the actual selection
                                _ = _timelineEditingManipulation.Cut(startMs!.Value, endMs!.Value);
                                targetStartMs2 = startMs!.Value;
                            }

                            if (_timelineEditingManipulation.Paste(_timelineEditingManipulation.CopyNPasteBuffer, targetStartMs2))
                            {
                                // paste was successful
                                _playPosSynchronizer.SetNewPlayPos(targetStartMs2);
                            }
                        }
                        break;

                    case System.Windows.Input.Key.I:// CTRL + I : insert 1 second empt
                        var targetStartMs = _playPosSynchronizer.PlayPosMsGuaranteedSnapped;
                        if (existsSelection)
                        {
                            // first remove the actual selection
                           
                            _ = _timelineEditingManipulation.Cut(startMs!.Value, endMs!.Value);
                            targetStartMs = startMs!.Value;
                        }
                        var emptyBuffer = new CopyNPasteBuffer { LengthMs = 1000, TimelinePoints = [] };
                        if (_timelineEditingManipulation.Paste(emptyBuffer, targetStartMs))
                        {
                            // paste was successful
                        }
                        break;

                    case System.Windows.Input.Key.S: // CTRL + S: save actual timeline
                        SaveTimelineData?.Invoke(this, EventArgs.Empty);
                        break;
                }
            }

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

                case System.Windows.Input.Key.Delete: // del to delete the selected points
                    if (existsSelection)
                    {
                        _ = _timelineEditingManipulation.Cut(startMs!.Value, endMs!.Value);
                        _playPosSynchronizer.SetNewPlayPos(startMs!.Value);
                        _timelineViewContext.SelectionStartMs = null;
                        _timelineViewContext.SelectionEndMs = null;
                    }
                    break;
                
                case System.Windows.Input.Key.Space: // start / stop playback
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
