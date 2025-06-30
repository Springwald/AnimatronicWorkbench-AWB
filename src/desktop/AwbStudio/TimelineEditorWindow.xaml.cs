// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.InputControllers.TimelineInputControllers;
using Awb.Core.Player;
using Awb.Core.Project;
using Awb.Core.Project.Various;
using Awb.Core.Services;
using Awb.Core.Timelines;
using Awb.Core.Timelines.NestedTimelines;
using Awb.Core.Timelines.Sounds;
using Awb.Core.Tools;
using AwbStudio.Exports;
using AwbStudio.Projects;
using AwbStudio.TimelineEditing;
using AwbStudio.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AwbStudio
{
    public partial class TimelineEditorWindow : Window
    {
        const int msPerScreenWidth = 10 * 1000; // todo: zoom in/out

        private readonly IAwbLogger _awbLogger;
        private readonly PlayPosSynchronizer _playPosSynchronizer;
        private readonly IAwbClientsService _clientsService;
        private readonly IProjectManagerService _projectManagerService;
        private readonly ITimelineMetaDataService _timelineMetaDataService;
        private readonly AwbProject _project;
        private readonly ITimelineDataService _timelineDataService;
        private readonly ITimelineController[] _timelineControllers;
        private readonly TimelineViewContext _viewContext;
        private readonly IInvokerService _invokerService;
        private IActuatorsService _actuatorsService;
        private TimelineControllerPlayViewPos _timelineControllerPlayViewPos = new TimelineControllerPlayViewPos();
        private TimelineEventHandling? _timelineEventHandling;
        private TimelinePlayer _timelinePlayer;
        protected TimelineData? _timelineData;
        private readonly CopyNPasteBufferHolder _copyNPasteBufferHolder = new CopyNPasteBufferHolder();

        private int _lastBankIndex = -1;
        private bool _unsavedChanges;
        private bool _switchingPages;

        private bool _loading;
        private Brush _buttonForegroundColorBackup;
        private bool _isZooming;
        private TimelineKeyboardHandling? _timelineKeyboardHandling;

        public TimelineEditorWindow(
            ITimelineController[] timelineControllers,
            IProjectManagerService projectManagerService,
            IAwbClientsService clientsService,
            ITimelineMetaDataService timelineMetaDataService,
            IInvokerService invokerService,
            IAwbLogger awbLogger)
        {
            InitializeComponent();

            _invokerService = invokerService;
            _clientsService = clientsService;
            _projectManagerService = projectManagerService;
            _timelineMetaDataService = timelineMetaDataService;

            _awbLogger = awbLogger;
            awbLogger.OnLog += (s, args) =>
            {
                //WpfAppInvoker.Invoke(new Action(() =>
                //{
                //    var msg = args;
                //    DebugOutputLabel.Content = $"{DateTime.UtcNow.ToShortDateString()}: {msg}\r\n{DebugOutputLabel.Content}";
                //}), System.Windows.Threading.DispatcherPriority.Background);
            };
            awbLogger.OnError += (s, args) =>
            {
                //WpfAppInvoker.Invoke(new Action(() =>
                //{
                //    var msg = args;
                //    DebugOutputLabel.Content = $"{DateTime.UtcNow.ToShortDateString()}: ERR: {msg}\r\n{DebugOutputLabel.Content}";
                //}), System.Windows.Threading.DispatcherPriority.Background);
            };

            _buttonForegroundColorBackup = ButtonSave.Foreground;

            _project = _projectManagerService.ActualProject ?? throw new Exception("Actual project is null?!?");
            _timelineDataService = _project.TimelineDataService;
            _timelineControllers = timelineControllers;

            _viewContext = new TimelineViewContext();
            _viewContext.Changed += ViewContext_Changed;

            _playPosSynchronizer = new PlayPosSynchronizer(_invokerService.GetInvoker());
            _playPosSynchronizer.OnPlayPosChanged += PlayPos_Changed;

            _actuatorsService = new ActuatorsService(_project, _clientsService, _awbLogger);
            _timelinePlayer = new TimelinePlayer(
                playPosSynchronizer: _playPosSynchronizer,
                actuatorsService: _actuatorsService,
                timelineDataService: _timelineDataService,
                awbClientsService: _clientsService,
                invokerService: _invokerService,
                logger: _awbLogger);
            _timelinePlayer.PlaySoundOnDesktop = PlaySoundOnDesktop;
            _timelinePlayer.StopSoundOnDesktop = () => SoundPlayer?.StopSound(); // stop sound on desktop

            Loaded += TimelineEditorWindow_Loaded;
        }

        private void PlaySoundOnDesktop(int soundId, TimeSpan startTime)
        {
            SoundPlayer.PlaySound(this, new SoundPlayEventArgs(soundId, startTime));
        }

        private async void TimelineEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= TimelineEditorWindow_Loaded;

            if (_project.TimelinesStates?.Any() == false)
                MessageBox.Show("Project file has no timelineStates defined!");

            this.IsEnabled = false;
            this.SizeChanged += TimelineEditorWindow_SizeChanged;
            CalculateSizeAndPixelPerMs();

            AllInOnePreviewControl.Timelineplayer = _timelinePlayer;
            SoundPlayer.Sounds = _project.Sounds;

            TimelineChooser.OnTimelineChosen += TimelineChosenToLoad;

            foreach (var timelineController in _timelineControllers)
                timelineController.OnTimelineEvent += TimelineController_OnTimelineEvent;

            // fill timeline state chooser
            var states = _project.TimelinesStates?.Select(ts => $"[{ts.Id}] {GetTimelineStateName(ts)}").ToList();
            ComboTimelineStates.ItemsSource = states;
            ComboTimelineNextStateOnce.ItemsSource = states == null ? states : new[] { "" }.Concat(states).ToList(); // the next-tate-once can be empty

            TimelineChooser.ProjectTitle = _project.ProjectMetaData.ProjectTitle;
            TimelineChooser.FileManager = _timelineDataService;

            Closing += TimelineEditorWindow_Closing;
            KeyDown += TimelineEditorWindow_KeyDown;
            KeyUp += TimelineEditorWindow_KeyUp;

            // zoomslider not working well - disable for now
            StackPanelZoom.Visibility = Visibility.Collapsed;

            var timelineCaptions = new TimelineCaptions();
            TimelineCaptionsViewer.Init(_viewContext, timelineCaptions, _actuatorsService);
            ValuesEditorControl.Init(_viewContext, timelineCaptions, _playPosSynchronizer, _actuatorsService, _timelineDataService.TimelineMetaDataService, _project.TimelineDataService, _awbLogger, _project.Sounds, SoundPlayer);
            AllInOnePreviewControl.Init(_viewContext, timelineCaptions, _playPosSynchronizer, _actuatorsService, _project.TimelineDataService, _timelineMetaDataService, _awbLogger, _project.Sounds);

            // bring to front
            this.IsEnabled = true;
            this.Topmost = true;
            await Task.Delay(100);
            this.Topmost = false;

            // Load and play the first timeline if existing
            var timelineId = _project.TimelineDataService.TimelineIds.FirstOrDefault();
            var newTimelineData = timelineId != null
                // load the first timeline
                ? _project.TimelineDataService.GetTimelineData(timelineId) ?? throw new Exception($"Timeline ID '{timelineId}' exists but timeline cant be loaded?!?")
                // create a new timeline
                : CreateNewTimelineData("");
            await SetupChangedTimelineData(newTimelineData);

            _unsavedChanges = false;

            Unloaded += TimelineEditorWindow_Unloaded;
        }

        private void TimelineEditorWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_timelineEventHandling != null)
            {
                _timelineEventHandling.Dispose();
            }
            if (_timelinePlayer != null)
            {
                _timelinePlayer.Dispose();
            }
            KeyDown -= TimelineEditorWindow_KeyDown;
            KeyUp -= TimelineEditorWindow_KeyUp;
            _playPosSynchronizer.Dispose();
        }

        private async Task SetupChangedTimelineData(TimelineData? timelineData)
        {
            _loading = true;


            if (timelineData == null && _timelineData == null) return; // no change

            if (_timelineKeyboardHandling != null)
            {
                // Need to disconnect the old timeline keyboard handling
                _timelineKeyboardHandling.SaveTimelineData -= OnSaveTimelineDataKeyboardEvent;
                _timelineKeyboardHandling = null;
            }

            if (_timelineEventHandling != null)
            {
                // Need to disconnect the old timeline event handling
                _timelineEventHandling.Dispose();
                _timelineEventHandling = null;
            }

            if (_timelineData != null)
            {
                // need to disconnect the old timeline data
                _timelineData.OnContentChanged -= TimelineData_OnContentChanged;
            }

            _timelineData = timelineData;
            if (timelineData == null) return; // No new timeline data to connect to

            timelineData.OnContentChanged += TimelineData_OnContentChanged;

            var changesAfterLoading = false;

            _timelinePlayer.SetTimelineData(timelineData, _project.Sounds, _actuatorsService.Servos);
            ValuesEditorControl.TimelineDataLoaded(timelineData);
            TimelineCaptionsViewer.TimelineDataLoaded(timelineData);
            AllInOnePreviewControl.TimelineDataLoaded(timelineData);
            TxtActualTimelineName.Text = timelineData?.Title ?? string.Empty;
            ActualTimelineGrid.IsEnabled = timelineData != null;

            _unsavedChanges = false;

            // fill state choice
            changesAfterLoading = SetupTimelineStateChoice(ComboTimelineStates, timelineData, changesAfterLoading);
            changesAfterLoading = SetupTimelineNextStateOnceChoice(ComboTimelineNextStateOnce, timelineData, changesAfterLoading);

            _playPosSynchronizer.SetNewPlayPos(0);

            await _timelinePlayer.RequestActuatorUpdate();
            _timelineEventHandling = new TimelineEventHandling(
                timelineData: timelineData!,
                timelineControllerPlayViewPos: _timelineControllerPlayViewPos,
                copyNPasteBufferHolder: _copyNPasteBufferHolder,
                actuatorsService: _actuatorsService,
                timelinePlayer: _timelinePlayer,
                timelineControllers: _timelineControllers,
                viewContext: _viewContext,
                playPosSynchronizer: _playPosSynchronizer,
                awbLogger: _awbLogger);

            this._timelineKeyboardHandling = new TimelineKeyboardHandling(_timelineEventHandling!, _timelinePlayer, _playPosSynchronizer, _timelineEventHandling.TimelineEditingManipulation, _viewContext);
            this._timelineKeyboardHandling.SaveTimelineData += OnSaveTimelineDataKeyboardEvent;

            FocusObjectPropertyEditorControl.Init(_viewContext, timelineData, _timelineEventHandling, _playPosSynchronizer, _timelineDataService, _project.Sounds, _actuatorsService.Servos, SoundPlayer);

            _viewContext.ActualFocusObject = null;

            ShowTitle(timelineData);
            ValuesEditorControl.ZoomVerticalHeightPerValueEditor = ZoomSlider.Value;

            _loading = false;
            _unsavedChanges = changesAfterLoading;
        }

        private void TimelineData_OnContentChanged(object? sender, TimelineDataChangedEventArgs e)
        {
            if (_timelineData != null) _timelineMetaDataService.ClearCache(_timelineData.Id);
            _unsavedChanges = true;
        }

        private void ViewContext_Changed(object? sender, ViewContextChangedEventArgs e)
        {
            switch (e.ChangeType)
            {
                case ViewContextChangedEventArgs.ChangeTypes.Duration:
                case ViewContextChangedEventArgs.ChangeTypes.PixelPerMs:
                case ViewContextChangedEventArgs.ChangeTypes.Scroll:
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.BankIndex:

                    if (_lastBankIndex != _viewContext.BankIndex && _actuatorsService != null)
                    {
                        _lastBankIndex = _viewContext.BankIndex;
                        var bankStartItemNo = _viewContext.BankIndex * _viewContext.ItemsPerBank + 1; // base 1
                        labelBankNo.Content = $"Bank {_viewContext.BankIndex + 1} [{bankStartItemNo}-{Math.Min(_actuatorsService.AllIds.Length, bankStartItemNo + _viewContext.ItemsPerBank - 1)}]";
                    }
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.FocusObject:
                    var y = ValuesEditorControl.GetScrollPosForEditorControl(_viewContext.ActualFocusObject);
                    if (y != null)
                    {
                        if (y < timelineValuesEditorScrollViewer.VerticalOffset || y + ValuesEditorControl.ZoomVerticalHeightPerValueEditor > timelineValuesEditorScrollViewer.ActualHeight - timelineValuesEditorScrollViewer.VerticalOffset)
                            timelineValuesEditorScrollViewer.ScrollToVerticalOffset(y.Value); // scroll the view to the focus object
                    }
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.FocusObjectValue:
                    _unsavedChanges = true;
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.Selection:
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{nameof(e.ChangeType)}:{e.ChangeType}");
            }
        }

        private void CalculateSizeAndPixelPerMs() => this._viewContext.PixelPerMs = this.ActualWidth / msPerScreenWidth;
        private void TimelineEditorWindow_SizeChanged(object sender, SizeChangedEventArgs e) => CalculateSizeAndPixelPerMs();
        private async void TimelineChosenToLoad(object? sender, TimelineNameChosenEventArgs e) => await this.LoadTimelineData(timelineId: e.TimelineId);
        private string GetTimelineStateName(TimelineState ts) => ts.Export ? ts.Title : $"{ts.Title} (no export)";

        private void TimelineEditorWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_unsavedChanges == true)
            {
                var choice = MessageBox.Show("Save changes to the current timeline?", "Unsaved changes!", MessageBoxButton.YesNoCancel);
                switch (choice)
                {
                    case MessageBoxResult.Yes:
                        if (SaveTimelineData() == false) return;
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        return;
                    default:
                        throw new ArgumentOutOfRangeException($"{nameof(choice)}:{choice}");
                }
            }
            Closing -= TimelineEditorWindow_Closing;

            foreach (var timelineController in _timelineControllers)
            {
                timelineController.SetPlayStateAsync(ITimelineController.PlayStates.Editor);
                timelineController.OnTimelineEvent -= TimelineController_OnTimelineEvent;
            }
        }

        private async void TimelineController_OnTimelineEvent(object? sender, TimelineControllerEventArgs e)
        {
            if (_timelineData == null) return;

            switch (e.EventType)
            {
                case TimelineControllerEventArgs.EventTypes.ActuatorValueChanged:
                case TimelineControllerEventArgs.EventTypes.ActuatorSetValueToDefault:
                case TimelineControllerEventArgs.EventTypes.ActuatorTogglePoint:
                    _unsavedChanges = true;
                    break;

                case TimelineControllerEventArgs.EventTypes.NextPage:
                    await ScrollPaging(_timelineControllerPlayViewPos.PageWidthMs);
                    break;

                case TimelineControllerEventArgs.EventTypes.PreviousPage:
                    await ScrollPaging(_timelineControllerPlayViewPos.PageWidthMs);
                    break;

                case TimelineControllerEventArgs.EventTypes.Save:
                    this.SaveTimelineData();
                    break;
            }
        }

        private void PlayPos_Changed(object? sender, int newPlayPosMs)
        {
            const int detectionZonePx = 30;

            this.LabelPlayTime.Content = $"{(newPlayPosMs / 1000.0):0.00}s / {_timelinePlayer.PlaybackSpeed:0.0}X";

            // check if the play position is out of the view
            var newPlayPosMsInPixel = _viewContext.GetXPos(newPlayPosMs);
            if (newPlayPosMsInPixel < _viewContext.ScrollPositionPx + detectionZonePx || newPlayPosMsInPixel > _viewContext.ScrollPositionPx + timelineAllValuesScrollViewer.ActualWidth - detectionZonePx)
            {
                var newScrollX = newPlayPosMsInPixel - timelineAllValuesScrollViewer.ActualWidth / 2;
                _viewContext.ScrollPositionPx = newScrollX;
                timelineAllValuesScrollViewer.ScrollToHorizontalOffset(newScrollX);
            }
        }

        private async Task ScrollPaging(int howManyMs)
        {
            if (ValuesEditorControl == null) return;
            if (_switchingPages) return;
            int fps = 20;
            int speed = 4; // speed (x seconds per second)
            _switchingPages = true;
            int newPosMs = 0;
            int scrollSpeedMs = (howManyMs > 0 ? 1 : -1) * (1000 / fps) * speed;
            for (int i = 0; i < howManyMs / scrollSpeedMs; i++)
            {
                var newScrollOffset = timelineAllValuesScrollViewer.HorizontalOffset + _viewContext.DurationMs / scrollSpeedMs;
                //MyInvoker.Invoke(new Action(() =>
                {
                    timelineAllValuesScrollViewer.ScrollToHorizontalOffset(newScrollOffset);
                }//));
                newPosMs = _playPosSynchronizer.PlayPosMsAutoSnappedOrUnSnapped + scrollSpeedMs;
                _playPosSynchronizer.SetNewPlayPos(newPosMs);
                await Task.Delay(1000 / fps);
            }
            _switchingPages = false;
        }

        private TimelineData CreateNewTimelineData(string title)
        {
            var timelineData = new TimelineData(
                id: Guid.NewGuid().ToString(),
                servoPoints: new List<ServoPoint>(),
                soundPoints: new List<SoundPoint>(),
                nestedTimelinePoints: new List<NestedTimelinePoint>(),
                timelineStateId: _project.TimelinesStates?.FirstOrDefault()?.Id ?? 0,
                nextTimelineStateIdOnce: null)
            {
                Title = title
            };

            return timelineData;
        }

        private void ShowTitle(TimelineData? timelineData)
        {
            this.Title = timelineData == null ? "No Timeline" : $"Timeline '{timelineData.Title}'";
        }

        private bool SetupTimelineStateChoice(ComboBox comboBox, TimelineData? data, bool changesAfterLoading)
        {
            if (data == null) return changesAfterLoading;
            var stateExists = _project.TimelinesStates?.SingleOrDefault(t => t.Id == data.TimelineStateId) != null;
            if (!stateExists)
            {
                MessageBox.Show($"Timeline {data.Title} has timelineStateID {data.TimelineStateId} not listed in actual project.");
                var state = _project.TimelinesStates?.FirstOrDefault();
                if (state != null)
                {
                    MessageBox.Show($"Using state {state.Title}[{state.Id}] instead");
                    data.TimelineStateId = state.Id;
                    changesAfterLoading = true;
                }
            }
            comboBox.SelectedIndex = _project.TimelinesStates?.TakeWhile(t => t.Id != data.TimelineStateId).Count() ?? 0;
            return changesAfterLoading;
        }

        private bool SetupTimelineNextStateOnceChoice(ComboBox comboBox, TimelineData? data, bool changesAfterLoading)
        {
            if (data == null) return changesAfterLoading;

            if (data.NextTimelineStateIdOnce == null)
            {
                comboBox.SelectedIndex = 0;
                return changesAfterLoading;
            }

            var stateExists = _project.TimelinesStates?.SingleOrDefault(t => t.Id == data.NextTimelineStateIdOnce) != null;
            if (!stateExists)
            {
                MessageBox.Show($"Timeline {data.Title} has TimelineNextStateOnceChoice {data.NextTimelineStateIdOnce} not listed in actual project.");
                MessageBox.Show($"Using state empty choice instead");
                data.NextTimelineStateIdOnce = null;
                changesAfterLoading = true;
            }
            comboBox.SelectedIndex = _project.TimelinesStates?.TakeWhile(t => t.Id != data.NextTimelineStateIdOnce).Count() + 1 ?? 0;
            return changesAfterLoading;
        }

        private async Task LoadTimelineData(string timelineId)
        {
            if (_timelineDataService == null)
            {
                MessageBox.Show("No filemanager is set!");
                return;
            }
            if (_unsavedChanges == true)
            {
                var choice = MessageBox.Show("Save changes to the current timeline?", "Unsaved changes!", MessageBoxButton.YesNoCancel);
                switch (choice)
                {
                    case MessageBoxResult.Yes:
                        if (SaveTimelineData() == false) return;
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                    default:
                        throw new ArgumentOutOfRangeException($"{nameof(choice)}:{choice}");
                }
            }
            var timelineData = _timelineDataService.GetTimelineData(timelineId);
            if (timelineData == null)
            {
                MessageBox.Show($"Timeline with id {timelineId} not found in filemanager.");
                return;
            }
            await SetupChangedTimelineData(timelineData);
        }

        private bool SaveTimelineData()
        {
            if (_timelineData == null)
            {
                MessageBox.Show("No timeline data to save!");
                return false;
            }

            var id = _timelineData.Id;
            if (string.IsNullOrWhiteSpace(id))
            {
                MessageBox.Show("Timeline has no id!", "Can't save timeline");
                return false;
            }

            if (_timelineDataService.SaveTimelineData(_timelineData))
            {
                _unsavedChanges = false;
                _invokerService.GetInvoker().Invoke(new Action(async () =>
                {
                    ButtonSave.Foreground = System.Windows.Media.Brushes.Red;
                    TimelineChooser.Refresh();
                    await Task.Delay(1000);
                    ButtonSave.Foreground = _buttonForegroundColorBackup;
                }));
                return true;
            }
            else
            {
                return false;
            }
        }

        private void TxtActualTimelineName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_timelineData == null) return;
            _timelineData.Title = TxtActualTimelineName.Text;
            ShowTitle(_timelineData);
            _unsavedChanges = true;
        }

        private void ComboTimelineStates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loading) return; // ignore the first call (when loading the timeline data
            if (_timelineData == null) return;
            if (_project == null) return;
            _timelineData.TimelineStateId = _project.TimelinesStates[ComboTimelineStates.SelectedIndex].Id;
            _unsavedChanges = true;
        }

        private void ComboTimelineNextStateOnce_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loading) return; // ignore the first call (when loading the timeline data
            if (_timelineData == null) return;
            if (_project == null) return;
            if (ComboTimelineNextStateOnce.SelectedIndex == 0)
            {  // no next state selected
                _timelineData.NextTimelineStateIdOnce = null;
                _unsavedChanges = true;
                return;
            }
            _timelineData.NextTimelineStateIdOnce = _project.TimelinesStates[ComboTimelineNextStateOnce.SelectedIndex - 1].Id; // the first is empty, so we have to subtract 1
            _unsavedChanges = true;
        }

        /// <summary>
        /// Redirects the keyboard input to the timeline keyboard handling
        /// </summary>
        private void TimelineEditorWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) => _timelineKeyboardHandling?.KeyDown(sender, e);

        /// <summary>
        /// Redirects the keyboard input to the timeline keyboard handling
        /// </summary>
        private void TimelineEditorWindow_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) => _timelineKeyboardHandling?.KeyUp(sender, e);


        /// <summary>
        /// reacts on the save timeline data event from the timeline keyboard handling
        /// </summary>
        private void OnSaveTimelineDataKeyboardEvent(object? sender, EventArgs e) => this.SaveTimelineData();

        #region Button Events

        private void ButtonSave_Click(object sender, RoutedEventArgs e) => SaveTimelineData();

        private void ButtonPlay_Click(object sender, RoutedEventArgs? e) => _timelineEventHandling?.Play();

        private void ButtonStop_Click(object sender, RoutedEventArgs? e) => _timelineEventHandling?.Stop();

        private async void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            if (_unsavedChanges == true)
            {
                var choice = MessageBox.Show("Save changes to the current timeline?", "Unsaved changes!", MessageBoxButton.YesNoCancel);
                switch (choice)
                {
                    case MessageBoxResult.Yes:
                        if (SaveTimelineData() == false) return;
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                    default:
                        throw new ArgumentOutOfRangeException($"{nameof(choice)}:{choice}");
                }
            }

            var timelineData = CreateNewTimelineData("no title");
            await SetupChangedTimelineData(timelineData);
        }

        private void ButtonExportEsp32_Click(object sender, RoutedEventArgs e)
        {
            if (_unsavedChanges == true)
            {
                var choice = MessageBox.Show("Save changes to the current timeline?", "Unsaved changes!", MessageBoxButton.YesNoCancel);
                switch (choice)
                {
                    case MessageBoxResult.Yes:
                        if (SaveTimelineData() == false) return;
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                    default:
                        throw new ArgumentOutOfRangeException($"{nameof(choice)}:{choice}");
                }
            }

            var exportWindow = new ExportToClientsWindow(_projectManagerService, _actuatorsService, _awbLogger);
            exportWindow.Show();
        }


        #endregion Button Events

        private void timelineScrollValueChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_viewContext != null)
                _viewContext.ScrollPositionPx = e.HorizontalOffset;

            if (!timelineAllValuesScrollViewer.HorizontalOffset.Equals(e.HorizontalOffset))
                timelineAllValuesScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);

            if (!timelineValuesEditorScrollViewer.HorizontalOffset.Equals(e.HorizontalOffset))
                timelineValuesEditorScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isZooming) return;

            _isZooming = true;

            if (ValuesEditorControl != null)
            {
                ValuesEditorControl.ZoomVerticalHeightPerValueEditor = e.NewValue;
            }

            _isZooming = false;
        }
    }
}
