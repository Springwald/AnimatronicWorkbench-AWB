// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Player;
using Awb.Core.Services;
using Awb.Core.Sounds;
using Awb.Core.Timelines;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AwbStudio.TimelineControls
{
    public partial class TimelineViewer : UserControl
    {
        private const int ItemsPerBank = 8;

        private int _actualBankIndex = 0;
        private bool _wasPlaying = false;
        private readonly Brush _gridLineBrush = new SolidColorBrush(Color.FromRgb(60, 60, 100));
        private TimelineData? _timelineData;

        private TimelinePlayer? _timelinePlayer;
        private TimelineViewPos _viewPos;
        private MediaPlayer? _mediaPlayer;

        private ITimelineControl[] _subTimelineViewerControls;

        private void PlayerStateChanged(object? sender, PlayStateEventArgs e)
        {
            ViewPos.PosSelectorManualMs = e.PositionMs - ViewPos.ScrollOffsetMs;
            var scrollingChanged = false;

            switch (e.PlayState)
            {
                case TimelinePlayer.PlayStates.Nothing:
                    if (_wasPlaying)
                    {
                        scrollingChanged = SyncScrollOffsetToNewPlayPos(e.PositionMs, snapToGrid: true);
                        _wasPlaying = false;
                    }
                    break;

                case TimelinePlayer.PlayStates.Playing:
                    scrollingChanged = SyncScrollOffsetToNewPlayPos(e.PositionMs, snapToGrid: false);
                    _wasPlaying = true;
                    break;

                default: throw new ArgumentOutOfRangeException($"{nameof(e.PlayState)}:{e.PlayState.ToString()}");
            }

            if (scrollingChanged)
            {
                MyInvoker.Invoke(new Action(() => this.PaintTimeLine()));
            }
            else
            {
                MyInvoker.Invoke(new Action(() => this.PaintPlayPos(_timelineData)));
            }
        }

        /// <summary>
        /// The actual view and scroll position of the timeline
        /// </summary>
        public TimelineViewPos ViewPos
        {
            set
            {
                if (value != null)
                {
                    if (_viewPos != null) throw new Exception("ViewPos.Changed is already set");
                    _viewPos = value;
                    _viewPos.Changed += this.OnViewPosChanged;
                    foreach (var subTimelineViewerControl in _subTimelineViewerControls)
                        subTimelineViewerControl.ViewPos = value;
                    this.OnViewPosChanged(this, EventArgs.Empty);
                }
            }
            get => _viewPos;
        }

        /// <summary>
        /// The timeline data to be displayed
        /// </summary>
        public TimelineData? TimelineData
        {
            get { return _timelineData; }
            set
            {
                _timelineData = value;
                foreach (var subTimelineViewerControl in _subTimelineViewerControls)
                    subTimelineViewerControl.TimelineData = value;
                SyncScrollOffsetToNewPlayPos(0, snapToGrid: true);
                PaintTimeLine();
                PaintPlayPos(_timelineData);
            }
        }

        /// <summary>
        /// The service to get the actuators
        /// </summary>
        public IActuatorsService? ActuatorsService
        {
            set
            {
                _actuatorsService = value;
                foreach (var subTimelineViewerControl in _subTimelineViewerControls)
                    subTimelineViewerControl.ActuatorsService = value;
            }
        }

        /// <summary>
        /// The timeline player to control the timeline playback
        /// </summary>
        public TimelinePlayer? Timelineplayer
        {
            get => _timelinePlayer;
            set
            {
                _timelinePlayer = value;
                if (_timelinePlayer != null)
                {
                    _timelinePlayer.OnPlayStateChanged += this.PlayerStateChanged;
                    _timelinePlayer.OnPlaySound += this.SoundToPlay; 
                }
            }
        }

        public Sound[]? Sounds { get; internal set; }



        /// <summary>
        /// set the scroll offset in a way, that the manual playPos is always at the position chosen in the hardware controller 
        /// </summary>
        /// <returns>true, when changed</returns>
        public bool SyncScrollOffsetToNewPlayPos(int newPlayPosMs, bool snapToGrid)
        {
            if (this._timelinePlayer != null)
            {
                if (snapToGrid)
                {
                    this.ViewPos.ScrollOffsetMs = ((newPlayPosMs - this.ViewPos.PosSelectorManualMs) / TimelinePlayer.PlayPosSnapMs) * TimelinePlayer.PlayPosSnapMs;
                }
                else
                {
                    this.ViewPos.ScrollOffsetMs = newPlayPosMs - this.ViewPos.PosSelectorManualMs;
                }
                return true;
            }
            return false;
        }

        public TimelineViewer()
        {
            InitializeComponent();

            //MyViewModel vm = new MyViewModel();
            Loaded += TimelineViewer_Loaded;
            SizeChanged += TimelineViewer_SizeChanged;

            // set up the sub-timeline-viewers
            _subTimelineViewerControls = new ITimelineControl[] { CaptionsViewer, ServoValueViewer, SoundValueViewer };

            _mediaPlayer = new MediaPlayer();

            // connect timeline-caption- calculator to all sub-timeline-viewers
            var timelineCaptions = new TimelineCaptions();
            foreach (var subTimelineViewerControl in _subTimelineViewerControls)
                subTimelineViewerControl.TimelineCaptions = timelineCaptions;
        }

        public void PaintTimeLine()
        {
            if (ViewPos == null) return;

            const double _paintMarginTopBottom = 30;
            double height = GridTimeline.ActualHeight;
            double width = GridTimeline.ActualWidth;

            if (height < 100 || width < 100) return;

            //r servoIds = _timelineData?.ServoPoints?.OfType<ServoPoint>().Select(p => p.ServoId).Distinct().ToArray() ?? Array.Empty<string>();

            double diagramHeight = height - _paintMarginTopBottom * 2;

            // update the data optical grid lines
            OpticalGrid.Children.Clear();
            var duration = ViewPos.DisplayMs;
            const int STEP = 1000;
            for (int msRaw = 0; msRaw < duration + ViewPos.DisplayMs; msRaw += STEP)
            {
                int ms = msRaw - ViewPos.ScrollOffsetMs;
                var x = ms * width / duration;
                if (x > 0 && x < width)
                {
                    OpticalGrid.Children.Add(new Line { X1 = x, X2 = x, Y1 = _paintMarginTopBottom, Y2 = height - _paintMarginTopBottom, Stroke = _gridLineBrush });
                    OpticalGrid.Children.Add(new Label { Content = ((ms + ViewPos.ScrollOffsetMs) / STEP).ToString(), BorderThickness = new Thickness(left: x, top: height - 30, right: 0, bottom: 0) });
                }
            }

            ServoValueViewer.PaintServoValues();
            SoundValueViewer.PaintSoundValues();

            // update the play position
            PaintPlayPos(_timelineData);
        }

        private int _lastBankIndex = -1;
        private IActuatorsService? _actuatorsService;

        private void OnViewPosChanged(object? sender, EventArgs e)
        {
            if (_timelineData == null) return;

            if (_lastBankIndex != ViewPos.BankIndex && _actuatorsService != null)
            {
                _lastBankIndex = ViewPos.BankIndex;
                MyInvoker.Invoke(new Action(() =>
                {
                    var bankStartItemNo = ViewPos.BankIndex * ViewPos.ItemsPerBank + 1; // base 1
                    labelBankNo.Content = $"Bank {ViewPos.BankIndex + 1} [{bankStartItemNo}-{Math.Min(_actuatorsService.AllIds.Length, bankStartItemNo + ViewPos.ItemsPerBank - 1)}]";
                }));
            }

            // update the scrollbar
            MyInvoker.Invoke(() =>
            {
                PaintTimeLine();  // update the timeline
            });
        }

        private void SoundToPlay(object? sender, SoundPlayEventArgs e)
        {
            if (_mediaPlayer == null) return;
            if (Sounds == null) return;
            if (e.SoundIndex > 0 && e.SoundIndex < Sounds.Length)
            {
                var sound = Sounds[e.SoundIndex];
                MyInvoker.Invoke(new Action(() =>
                {
                    _mediaPlayer.Stop();
                    _mediaPlayer.Open(new Uri(sound.Mp3Filename));
                    _mediaPlayer.Play();
                }));
            }
        }

        private void TimelineViewer_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= TimelineViewer_Loaded;
            this.PaintTimeLine();
            Unloaded += TimelineViewer_Unloaded;
        }

        private void TimelineViewer_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= TimelineViewer_Unloaded;
            if (_timelinePlayer != null) _timelinePlayer.OnPlayStateChanged -= this.PlayerStateChanged;
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Stop();
                _mediaPlayer = null;
            }
        }

        private void TimelineViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PaintTimeLine();
            PaintPlayPos(_timelineData);
        }

        private void PaintPlayPos(TimelineData? timeline)
        {
            if (ViewPos == null) return;

            // draw the manual midi controller play position as triangle at the bottom
            var x = ((double)(ViewPos.PosSelectorManualMs) / ViewPos.DisplayMs) * this.ActualWidth;
            ManualPlayPosAbsolute.Margin = new Thickness(x - ManualPlayPosAbsolute.ActualWidth / 2, 0, 0, 0);

            // draw the play position as a vertical line
            var playPosMs = ViewPos.PosSelectorManualMs + ViewPos.ScrollOffsetMs;
            if (timeline == null || playPosMs < 0 || playPosMs > ViewPos.ScrollOffsetMs + ViewPos.DisplayMs)
            {
                PlayPosLine.Visibility = Visibility.Hidden;
            }
            else
            {
                x = this.ViewPos.GetXPos(ms: playPosMs, controlWidth: this.ActualWidth);
                PlayPosLine.X1 = x;
                PlayPosLine.X2 = x;
                PlayPosLine.Y1 = 0;
                PlayPosLine.Y2 = this.ActualHeight;
                PlayPosLine.Visibility = Visibility.Visible;
            }
        }
    }

}