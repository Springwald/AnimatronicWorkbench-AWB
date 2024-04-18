// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.InputControllers.TimelineInputControllers;
using Awb.Core.LoadNSave.Export;
using Awb.Core.Player;
using Awb.Core.Project;
using Awb.Core.Services;
using Awb.Core.Timelines;
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

namespace AwbStudio
{
    public partial class TimelineEditorWindow : Window
    {
        const int msPerScreenWidth = 10 * 1000; // todo: zoom in/out

        private readonly PlayPosSynchronizer _playPosSynchronizer;
        private readonly IAwbClientsService _clientsService;
        private readonly IProjectManagerService _projectManagerService;
        private readonly IAwbLogger _logger;
        private readonly AwbProject _project;
        private readonly FileManagement.TimelineFileManager _fileManager;
        private readonly ITimelineController[] _timelineControllers;
        private readonly TimelineViewContext _viewContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly IInvokerService _invokerService;
        private TimelinePlayer? _timelinePlayer;
        protected TimelineData? _timelineData;
        private IActuatorsService _actuatorsService;
        private int _lastBankIndex = -1;
        private TimelineControllerPlayViewPos _timelineControllerPlayViewPos = new TimelineControllerPlayViewPos();
        private TimelineEventHandling? _timelineEventHandling;
        private bool _unsavedChanges;
        private bool _switchingPages;
        private bool _ctrlKeyPressed;

        public TimelineEditorWindow(IServiceProvider serviceProvider, ITimelineController[] timelineControllers, IProjectManagerService projectManagerService, IAwbClientsService clientsService, IInvokerService invokerService, IAwbLogger awbLogger)
        {
            InitializeComponent();

            _serviceProvider = serviceProvider;
            _invokerService = invokerService;

            DebugOutputLabel.Content = string.Empty;

            awbLogger.OnLog += (s, args) =>
            {
                WpfAppInvoker.Invoke(new Action(() =>
                {
                    var msg = args;
                    DebugOutputLabel.Content = $"{DateTime.UtcNow.ToShortDateString()}: {msg}\r\n{DebugOutputLabel.Content}";
                }), System.Windows.Threading.DispatcherPriority.Background);
            };
            awbLogger.OnError += (s, args) =>
            {
                WpfAppInvoker.Invoke(new Action(() =>
                {
                    var msg = args;
                    DebugOutputLabel.Content = $"{DateTime.UtcNow.ToShortDateString()}: ERR: {msg}\r\n{DebugOutputLabel.Content}";
                }), System.Windows.Threading.DispatcherPriority.Background);
            };

            _clientsService = clientsService;
            _projectManagerService = projectManagerService;
            _logger = awbLogger;

            _project = _projectManagerService.ActualProject;
            _fileManager = new FileManagement.TimelineFileManager(_project);
            _timelineControllers = timelineControllers;

            _viewContext = new TimelineViewContext();
            _viewContext.Changed += ViewContext_Changed;

            _playPosSynchronizer = new PlayPosSynchronizer(_invokerService.GetInvoker());
            _playPosSynchronizer.OnPlayPosChanged += PlayPos_Changed;

            Loaded += TimelineEditorWindow_Loaded;
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
                        //MyInvoker.Invoke(new Action(() =>
                        {
                            var bankStartItemNo = _viewContext.BankIndex * _viewContext.ItemsPerBank + 1; // base 1
                            labelBankNo.Content = $"Bank {_viewContext.BankIndex + 1} [{bankStartItemNo}-{Math.Min(_actuatorsService.AllIds.Length, bankStartItemNo + _viewContext.ItemsPerBank - 1)}]";
                        }//));
                    }
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.FocusObject:
                    var y = ValuesEditorControl.GetScrollPosForEditorControl(_viewContext.ActualFocusObject);
                    if (y != null)
                    {
                        if (y < timelineValuesEditorScrollViewer.VerticalOffset || y + ValuesEditorControl.ZoomVerticalHeightPerValueEditor > timelineValuesEditorScrollViewer.ActualHeight - timelineValuesEditorScrollViewer.VerticalOffset )
                            timelineValuesEditorScrollViewer.ScrollToVerticalOffset(y.Value); // scroll the view to the focus object
                    }
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.FocusObjectValue:
                    _unsavedChanges = true;
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{nameof(e.ChangeType)}:{e.ChangeType}");
            }
        }

        private async void TimelineEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= TimelineEditorWindow_Loaded;

            this.IsEnabled = false;

            if (_project.TimelinesStates?.Any() == false)
            {
                MessageBox.Show("Project file has no timelineStates defined!");
            }

            this.SizeChanged += TimelineEditorWindow_SizeChanged;
            CalculateSizeAndPixelPerMs();

            _actuatorsService = new ActuatorsService(_project, _clientsService, _logger);

            this._timelineData = CreateNewTimelineData("");

            _timelinePlayer = new TimelinePlayer(timelineData: this._timelineData,  playPosSynchronizer: _playPosSynchronizer, actuatorsService: _actuatorsService, awbClientsService: _clientsService, invokerService: _invokerService,  logger: _logger);
            _timelinePlayer.OnPlayStateChanged +=       OnPlayStateChanged;
            _timelinePlayer.OnPlaySound += SoundPlayer.SoundToPlay;

            var timelineCaptions = new TimelineCaptions();
            TimelineCaptionsViewer.Init(_viewContext, timelineCaptions, _playPosSynchronizer, _actuatorsService);

            ValuesEditorControl.Init(_viewContext, timelineCaptions, _playPosSynchronizer, _actuatorsService);
            ValuesEditorControl.Timelineplayer = _timelinePlayer;

            AllInOnePreviewControl.Init(_viewContext, timelineCaptions, _playPosSynchronizer, _actuatorsService);
            AllInOnePreviewControl.Timelineplayer = _timelinePlayer;

            FocusObjectPropertyEditorControl.Init(_serviceProvider, _viewContext, _playPosSynchronizer);

            SoundPlayer.Sounds = _project.Sounds;

            await TimelineDataLoaded();

            TimelineChooser.OnTimelineChosen += TimelineChosenToLoad;

            foreach (var timelineController in _timelineControllers)
            {
                timelineController.OnTimelineEvent += TimelineController_OnTimelineEvent;
            }

            // fill timeline state chooser
            ComboTimelineStates.ItemsSource = _project.TimelinesStates?.Select(ts => GetTimelineStateName(ts)).ToList();
            TimelineChooser.FileManager = _fileManager;

            Closing += TimelineEditorWindow_Closing;
            KeyDown += TimelineEditorWindow_KeyDown;

            // bring to front
            this.IsEnabled = true;
            this.Topmost = true;
            await Task.Delay(100);
            this.Topmost = false;

            _unsavedChanges = false;
            ValuesEditorControl.TimelineDataLoaded(_timelineData);

            Unloaded += TimelineEditorWindow_Unloaded;
        }

       

        private void TimelineEditorWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_timelineEventHandling != null) _timelineEventHandling.Dispose();
            _playPosSynchronizer.Dispose();
        }

        private void CalculateSizeAndPixelPerMs()
        {
            this._viewContext.PixelPerMs = this.ActualWidth / msPerScreenWidth;
        }

        private void TimelineEditorWindow_SizeChanged(object sender, SizeChangedEventArgs e) => CalculateSizeAndPixelPerMs();

        private void TimelineEditorWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.S:
                    if (this._ctrlKeyPressed)
                    {
                        SaveTimelineData();
                    }
                    break;

                case System.Windows.Input.Key.LeftCtrl:
                case System.Windows.Input.Key.RightCtrl:
                    this._ctrlKeyPressed = e.IsDown;
                    break;
            }
        }

        private string GetTimelineStateName(TimelineState ts)
            => ts.Export ? ts.Title : $"{ts.Title} (no export)";

        private async void TimelineChosenToLoad(object? sender, TimelineNameChosenEventArgs e) =>   await this.LoadTimelineData(filename: e.FileName);

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
                timelineController.SetPlayState(ITimelineController.PlayStates.Editor);
                timelineController.OnTimelineEvent -= TimelineController_OnTimelineEvent;
            }

            _timelinePlayer.OnPlayStateChanged -= OnPlayStateChanged;
            _timelinePlayer.Dispose();
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

        private void OnPlayStateChanged(object? sender, PlayStateEventArgs e)
        {
        }

        private void PlayPos_Changed(object? sender, int newPlayPosMs)
        {
            this.LabelPlayTime.Content = $"{(newPlayPosMs / 1000.0):0.00}s / {_timelinePlayer?.PlaybackSpeed:0.0}X";
        }

        private async Task ScrollPaging(int howManyMs)
        {
            if (ValuesEditorControl == null) return;
            if (ValuesEditorControl.Timelineplayer == null) return;
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
                newPosMs = _playPosSynchronizer.PlayPosMs + scrollSpeedMs;
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
                timelineStateId: _project.TimelinesStates?.FirstOrDefault()?.Id ?? 0)
            {
                Title = title
            };

            return timelineData;
        }

        private async Task TimelineDataLoaded()
        {
            var data = _timelineData;
            if (data == null) return;

            var changesAfterLoading = false;

            this.Title = _timelineData == null ? "No Timeline" : $"Timeline '{_timelineData.Title}'";
            if (_timelinePlayer != null) _timelinePlayer.TimelineData = data;
            ValuesEditorControl.TimelineDataLoaded(data);
            TimelineCaptionsViewer.TimelineDataLoaded(data);
            AllInOnePreviewControl.TimelineDataLoaded(data);
            TxtActualTimelineName.Text = _timelineData?.Title ?? string.Empty;
            _unsavedChanges = false;

            // fill state choice
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
            ComboTimelineStates.SelectedIndex = _project.TimelinesStates?.TakeWhile(t => t.Id != data.TimelineStateId).Count() ?? 0;

            if (_timelineEventHandling != null)
            {
                _timelineEventHandling.Dispose();
                _timelineEventHandling = null;
            }

            _playPosSynchronizer.SetNewPlayPos(0);

            if (_timelinePlayer != null)
            {
                await _timelinePlayer.UpdateActuators();
                _timelineEventHandling = new TimelineEventHandling(
                    timelineData: data,
                    timelineControllerPlayViewPos: _timelineControllerPlayViewPos,
                    actuatorsService: _actuatorsService,
                    timelinePlayer: _timelinePlayer,
                    timelineControllers: _timelineControllers,
                    viewContext: _viewContext,
                    playPosSynchronizer: _playPosSynchronizer);
            }

            _unsavedChanges = changesAfterLoading;
        }

        private async Task LoadTimelineData(string filename)
        {
            if (_fileManager == null)
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
            _timelineData = _fileManager.LoadTimelineData(filename);
            await TimelineDataLoaded();
        }

        private bool SaveTimelineData()
        {
            var id = _timelineData.Id;
            if (string.IsNullOrWhiteSpace(id))
            {
                MessageBox.Show("Timeline has no id!", "Can't save timeline");
                return false;

            }
            var filename = _fileManager.GetTimelineFilenameById(id);
            if (_fileManager.SaveTimelineData(_timelineData))
            {
                _unsavedChanges = false;
                //MyInvoker.Invoke(new Action(() => { 
                    TimelineChooser.Refresh(); 
                //}));
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
            _unsavedChanges = true;
        }

        private void ComboTimelineStates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_timelineData == null) return;
            if (_project == null) return;
            _timelineData.TimelineStateId = _project.TimelinesStates[ComboTimelineStates.SelectedIndex].Id;
            _unsavedChanges = true;
        }

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

            _timelineData = CreateNewTimelineData("no title");
            await TimelineDataLoaded();
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

            var exportWindow = new ExportToClientCodeWindow(_projectManagerService);

            var timelines = new List<TimelineData>();

            var fileManager = new FileManagement.TimelineFileManager(_project);
            var timelineFilenames = fileManager.TimelineFilenames;

            foreach (var timelineFilename in timelineFilenames)
            {
                var timelineData = _fileManager.LoadTimelineData(timelineFilename);
                if (timelineData == null)
                {
                    MessageBox.Show($"Can't load timeline '{timelineFilename}'. Export canceled.");
                    return;
                }
                timelines.Add(timelineData);
            }

            var data = new Esp32ClientExportData()
            {
                ProjectName = _project.Title,
                TimelineStates = _project.TimelinesStates,
                StsServoConfigs = _project.StsServos,
                ScsServoConfigs = _project.ScsServos,
                Pca9685PwmServoConfigs = _project.Pca9685PwmServos,
                Mp3PlayerYX5300Configs = _project.Mp3PlayersYX5300,
                InputConfigs = _project.Inputs,
                TimelineData = timelines.ToArray()
            };
            var exporter = new Esp32DataExporter();
            var result = exporter.GetExportCode(data);

            exportWindow.Show();
            exportWindow.ShowResult(result);
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
    }
}
