// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.Configs;
using Awb.Core.InputControllers.TimelineInputControllers;
using Awb.Core.LoadNSave.Export;
using Awb.Core.Player;
using Awb.Core.Services;
using Awb.Core.Timelines;
using AwbStudio.Exports;
using AwbStudio.FileManagement;
using AwbStudio.Projects;
using AwbStudio.TimelineControls;
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
        const int pageSizeMs = 2000; // 2 seconds per page when scrolling

        private readonly IProjectManagerService _projectManagerService;
        private readonly IAwbLogger _logger;
        private readonly AwbProject _project;
        private readonly FileManager _fileManager;
        private readonly ITimelineController? _timelineController;

        private TimelinePlayer _timelinePlayer;
        private IActuatorsService _actuatorsService;
        private IAwbClientsService _clientService;

        private bool _unsavedChanges;
        private volatile bool _manualUpdatingPlayPos;
        private volatile bool _manualUpdatingValues;
        private bool _switchingPages;

        protected TimelineData TimelineData { get; set; }

        public TimelineEditorWindow(IInputControllerService inputControllerService, IProjectManagerService projectManagerService, IAwbLogger awbLogger)
        {
            InitializeComponent();

            _projectManagerService = projectManagerService;
            _logger = awbLogger;

            _project = _projectManagerService.ActualProject;
            _fileManager = new FileManager(_project);

            _timelineController = inputControllerService.TimelineController;

            Loaded += TimelineEditorWindow_Loaded;
        }

        private async void TimelineEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= TimelineEditorWindow_Loaded;
            this.IsEnabled = false;

            if (_project.TimelinesStates?.Any() == false)
            {
                MessageBox.Show("Project file has no timelineStates defined!");
            }

            _clientService = new AwbClientsService(_logger);
            await _clientService.Init();

            _actuatorsService = new ActuatorsService(_project, _clientService, _logger);

            // fill timeline state chooser
            ComboTimelineStates.ItemsSource = _project.TimelinesStates?.Select(ts => ts.Name).ToList();
            TimelineChooser.FileManager = _fileManager;

            this.TimelineData = CreateNewTimelineData("");
            await TimelineDataLoaded();

            _timelinePlayer = new TimelinePlayer(timelineData: this.TimelineData, actuatorsService: _actuatorsService, awbClientsService: _clientService, logger: _logger);

            _timelinePlayer.OnPlayStateChanged += OnPlayStateChanged;

            TimelineViewerControl.TimelineData = TimelineData;
            TimelineViewerControl.TimelinePlayer = _timelinePlayer;
            TimelineViewerControl.ActuatorsService = _actuatorsService;

            TimelineChooser.OnTimelineChosen += TimelineChosenToLoad;

            if (_timelineController != null)
                _timelineController.OnTimelineEvent += TimelineController_OnTimelineEvent;

            Closing += TimelineEditorWindow_Closing;

            this.Topmost = true;
            this.Topmost = false;
            this.IsEnabled = true;
            _unsavedChanges = false;

            await _timelinePlayer.Update();
        }

        private async void TimelineChosenToLoad(object? sender, TimelineNameChosenEventArgs e)
        {
            await this.LoadTimelineData(filename: e.FileName);
        }
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

            if (_timelineController != null)
            {
                _timelineController.SetPlayState(ITimelineController.PlayStates.Editor);
                _timelineController.OnTimelineEvent -= TimelineController_OnTimelineEvent;
            }

            _timelinePlayer.OnPlayStateChanged -= OnPlayStateChanged;
            _timelinePlayer.Dispose();
        }
        private async void TimelineController_OnTimelineEvent(object? sender, TimelineControllerEventArgs e)
        {
            switch (e.EventType)
            {
                case TimelineControllerEventArgs.EventTypes.PlayPosAbsoluteChanged:
                    var viewPos = TimelineViewerControl.ViewPos;
                    var playPosAbsoluteMs = ((int)((viewPos.DisplayMs / 100.0 * e.ValueInPercent) / _timelinePlayer.PlayPosSnapMs) * _timelinePlayer.PlayPosSnapMs);
                    int newPos = _timelinePlayer.PositionMs;
                    newPos = viewPos.ScrollOffsetMs + playPosAbsoluteMs;
                    _manualUpdatingPlayPos = true;
                    await _timelinePlayer.Update(newPositionMs: newPos);
                    _manualUpdatingPlayPos = false;
                    break;

                case TimelineControllerEventArgs.EventTypes.Play:
                    Play();
                    break;

                case TimelineControllerEventArgs.EventTypes.Stop:
                    Stop();
                    break;

                case TimelineControllerEventArgs.EventTypes.Forward:
                    if (_timelinePlayer.PlaybackSpeed < 4) _timelinePlayer.PlaybackSpeed += 0.5;
                    break;

                case TimelineControllerEventArgs.EventTypes.Backwards:
                    if (_timelinePlayer.PlaybackSpeed > 0.5) _timelinePlayer.PlaybackSpeed -= 0.5;
                    break;

                case TimelineControllerEventArgs.EventTypes.ActuatorValueChanged:
                    _unsavedChanges = true;
                    var servos = _actuatorsService?.Servos;
                    if (servos == null) return;
                    if (e.ActuatorIndex >= servos.Length) return;
                    var servo = servos[e.ActuatorIndex];
                    var targetPercent = e.ValueInPercent;
                    var point = TimelineData?.ServoPoints.OfType<ServoPoint>().SingleOrDefault(p => p.ServoId == servo.Id && (int)p.TimeMs == _timelinePlayer.PositionMs); // check existing point
                    if (point == null)
                    {
                        point = new ServoPoint(servo.Id, targetPercent, _timelinePlayer.PositionMs);
                        TimelineData?.ServoPoints.Add(point);
                    }
                    else
                    {
                        point.ValuePercent = targetPercent;
                    }
                    _manualUpdatingValues = true;
                    if (_manualUpdatingValues) await _timelinePlayer.Update();
                    _manualUpdatingValues = false;
                    MyInvoker.Invoke(new Action(() => { TimelineViewerControl.PaintTimeLine(); }));
                    break;

                case TimelineControllerEventArgs.EventTypes.ActuatorTogglePoint:
                    _unsavedChanges = true;
                    servos = _actuatorsService?.Servos;
                    if (servos == null) return;
                    if (e.ActuatorIndex >= servos.Length) return;
                    servo = servos[e.ActuatorIndex];
                    point = TimelineData?.ServoPoints.OfType<ServoPoint>().SingleOrDefault(p => p.ServoId == servo.Id && (int)p.TimeMs == _timelinePlayer.PositionMs); // check existing point
                    if (point == null)
                    {
                        targetPercent = 100.0 * (servo.TargetValue - servo.MinValue) / (servo.MaxValue - servo.MinValue);
                        point = new ServoPoint(servo.Id, targetPercent, _timelinePlayer.PositionMs);
                        TimelineData?.ServoPoints.Add(point);
                    }
                    else
                    {
                        TimelineData?.ServoPoints.Remove(point);
                    }
                    _manualUpdatingValues = true;
                    if (_manualUpdatingValues) await _timelinePlayer.Update();
                    MyInvoker.Invoke(new Action(() => { TimelineViewerControl.PaintTimeLine(); }));
                    _manualUpdatingValues = false;
                    break;

                case TimelineControllerEventArgs.EventTypes.NextPage:
                    await ScrollPaging(pageSizeMs);
                    break;

                case TimelineControllerEventArgs.EventTypes.PreviousPage:
                    await ScrollPaging(-pageSizeMs);
                    break;

                case TimelineControllerEventArgs.EventTypes.Save:

                    this.SaveTimelineData();
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{nameof(e.EventType)}:{e.EventType.ToString()}");
            }
        }

        private async Task ScrollPaging(int howManyMs)
        {
            if (TimelineViewerControl == null) return;
            if (TimelineViewerControl.TimelinePlayer == null) return;
            if (_switchingPages) return;
            int fps = 20;
            int speed = 4; // speed (x seconds per second)
            _switchingPages = true;
            int scrollSpeedMs = (howManyMs > 0 ? 1 : -1) * (1000 / fps) * speed;
            for (int i = 0; i < howManyMs / scrollSpeedMs; i++)
            {
                var newOffset = TimelineViewerControl.ViewPos.ScrollOffsetMs + scrollSpeedMs;
                TimelineViewerControl.ViewPos.ScrollOffsetMs = Math.Max(0, newOffset);
                await TimelineViewerControl.TimelinePlayer.Update(TimelineViewerControl.TimelinePlayer.PositionMs + scrollSpeedMs);
                MyInvoker.Invoke(new Action(() => TimelineViewerControl.PaintTimeLine()));
                await Task.Delay(1000 / fps);
            }
            _switchingPages = false;
        }

        private void OnPlayStateChanged(object? sender, PlayStateEventArgs e)
        {
            if (!_manualUpdatingPlayPos)
                MyInvoker.Invoke(new Action(() => this.LabelPlayTime.Content = $"{(e.PositionMs / 1000.0):0.00}s / {e.PlaybackSpeed:0.0}X"));
            if (!_manualUpdatingValues)
                ShowValuesOnTimelineInputController(e.PositionMs);
        }

        private void ShowValuesOnTimelineInputController(int playPosMs)
        {
            if (_timelineController == null) return;

            var servos = _actuatorsService?.Servos;
            if (servos == null) return;
            for (int i = 0; i < servos.Length; i++)
            {
                var servo = servos[i];
                _timelineController.SetActuatorValue(index: i, valueInPercent: Math.Max(0, Math.Min(100.0, 100.0 * (servo.TargetValue - servo.MinValue * 1.0) / (1.0 * servo.MaxValue - servo.MinValue))));
                _timelineController.ShowPointButtonState(index: i, pointExists: TimelineData.ServoPoints.Any(p => p.ServoId == servo.Id && p.TimeMs == playPosMs));
            }
        }

        private TimelineData CreateNewTimelineData(string title)
        {
            var timelineData = new TimelineData(
                servoPoints: new List<ServoPoint>(),
                soundPoints: new List<SoundPoint>(),
                timelineStateId: _project.TimelinesStates?.FirstOrDefault()?.Id ?? 0)
            {
                Title = title
            };


            var stsServos = _actuatorsService.Servos.Select(s => s as StsServo).Where(s => s != null);
            foreach (var stsServo in stsServos)
            {
                var valuePercent = 100d * (stsServo.DefaultValue - stsServo.MinValue) / (stsServo.MaxValue - stsServo.MinValue);
                timelineData.ServoPoints.Add(new ServoPoint(servoId: stsServo.Id, valuePercent: valuePercent, timeMs: 0));
            }

            return timelineData;
        }
        private async Task TimelineDataLoaded()
        {
            var data = TimelineData;
            if (data == null) return;

            var changesAfterLoading = false;

            this.Title = TimelineData == null ? "No Timeline" : $"Timeline '{TimelineData.Title}'";
            if (_timelinePlayer != null) _timelinePlayer.TimelineData = data;
            TimelineViewerControl.TimelineData = TimelineData;
            TxtActualTimelineName.Text = TimelineData?.Title ?? string.Empty;
            _unsavedChanges = false;

            // fill state choice
            var stateExists = _project.TimelinesStates?.SingleOrDefault(t => t.Id == data.TimelineStateId) != null;
            if (!stateExists)
            {
                MessageBox.Show($"Timeline {data.Title} has timelineStateID {data.TimelineStateId} not listed in actual project.");
                var state = _project.TimelinesStates?.FirstOrDefault();
                if (state != null)
                {
                    MessageBox.Show($"Using state {state.Name}[{state.Id}] instead");
                    data.TimelineStateId = state.Id;
                    changesAfterLoading = true;
                }
            }
            ComboTimelineStates.SelectedIndex = _project.TimelinesStates?.TakeWhile(t => t.Id != data.TimelineStateId).Count() ?? 0;


            if (_timelinePlayer != null)
            {
                await _timelinePlayer.Update(0);
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
            TimelineData = _fileManager.LoadTimelineData(filename);
            await TimelineDataLoaded();
        }

        private bool SaveTimelineData()
        {
            var name = TimelineData.Title;
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Timeline has no title!", "Can't save timeline");
                return false;

            }
            var filename = _fileManager.GetTimelineFilename(name);
            if (_fileManager.SaveTimelineData(filename, TimelineData))
            {
                _unsavedChanges = false;
                MyInvoker.Invoke(new Action(() => { TimelineChooser.Refresh(); }));
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Play()
        {
            _timelinePlayer?.Play();
            _timelineController?.SetPlayState(ITimelineController.PlayStates.Playing);
        }

        private async void Stop()
        {
            // snap scrollpos to snap positions 
            TimelineViewerControl.ViewPos.ScrollOffsetMs = (TimelineViewerControl.ViewPos.ScrollOffsetMs / _timelinePlayer.PlayPosSnapMs) * _timelinePlayer.PlayPosSnapMs;
            if (_timelinePlayer != null)
            {
                _timelinePlayer.Stop();

                // snap playpos to snap positions 
                await _timelinePlayer.Update((_timelinePlayer.PositionMs / _timelinePlayer.PlayPosSnapMs) * _timelinePlayer.PlayPosSnapMs);
            }
            _timelineController?.SetPlayState(ITimelineController.PlayStates.Editor);
            MyInvoker.Invoke(new Action(() => TimelineViewerControl.PaintTimeLine()));
        }


        private void TxtActualTimelineName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (TimelineData == null) return;
            TimelineData.Title = TxtActualTimelineName.Text;
            _unsavedChanges = true;
        }

        private void ComboTimelineStates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TimelineData == null) return;
            TimelineData.TimelineStateId = _project.TimelinesStates[ComboTimelineStates.SelectedIndex].Id;
            _unsavedChanges = true;
        }

        #region Button Events

        private void ButtonSave_Click(object sender, RoutedEventArgs e) => SaveTimelineData();

        private void ButtonPlay_Click(object sender, RoutedEventArgs? e) => Play();

        private void ButtonStop_Click(object sender, RoutedEventArgs? e) => Stop();

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

            TimelineData = CreateNewTimelineData("no title");
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

            var fileManager = new FileManager(_project);
            var timelineFilenames = fileManager.TimelineFilenames;

            foreach (var timelineFilename in timelineFilenames)
            {
                var timelineData = _fileManager.LoadTimelineData(timelineFilename);
                if (timelineData == null)
                {
                    MessageBox.Show("Can't load timeline '" + timelineFilename + "'. Export canceled.");
                    return;
                }
                timelines.Add(timelineData);
            }

            var data = new Esp32ClientExportData()
            {
                TimelineStates = _project.TimelinesStates,
                StsServoConfigs = _project.StsServos,
                TimelineData = timelines.ToArray()
            };
            var exporter = new Esp32DataExporter();
            var result = exporter.GetExportCode(data);

            exportWindow.Show();
            exportWindow.ShowResult(result);
        }


        #endregion Button Events


    }
}
