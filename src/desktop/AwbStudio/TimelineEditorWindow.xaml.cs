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
        private TimelinePlayer _timelinePlayer;
        protected TimelineData _timelineData;
        private IActuatorsService _actuatorsService;
        private int _lastBankIndex = -1;


        private TimelineControllerPlayViewPos _timelineControllerPlayViewPos = new TimelineControllerPlayViewPos();

        private bool _unsavedChanges;

        private volatile bool _manualUpdatingValues;
        private bool _switchingPages;
        private int _lastActuatorChanged = 1; // prevent double actuator change events to the midi controller
        private bool _ctrlKeyPressed;

        public TimelineEditorWindow(IServiceProvider serviceProvider, ITimelineController[] timelineControllers, IProjectManagerService projectManagerService, IAwbClientsService clientsService, IAwbLogger awbLogger)
        {
            InitializeComponent();

            _serviceProvider = serviceProvider;

            DebugOutputLabel.Content = string.Empty;

            awbLogger.OnLog += (s, args) =>
            {
                MyInvoker.Invoke(new Action(() =>
                {
                    var msg = args;
                    DebugOutputLabel.Content = $"{DateTime.UtcNow.ToShortDateString()}: {msg}\r\n{DebugOutputLabel.Content}";
                }));
            };
            awbLogger.OnError += (s, args) =>
            {
                MyInvoker.Invoke(new Action(() =>
                {
                    var msg = args;
                    DebugOutputLabel.Content = $"{DateTime.UtcNow.ToShortDateString()}: ERR: {msg}\r\n{DebugOutputLabel.Content}";
                }));
            };

            _clientsService = clientsService;
            _projectManagerService = projectManagerService;
            _logger = awbLogger;

            _project = _projectManagerService.ActualProject;
            _fileManager = new FileManagement.TimelineFileManager(_project);
            _timelineControllers = timelineControllers;

            _viewContext = new TimelineViewContext();
            _viewContext.Changed += ViewContext_Changed;

            _playPosSynchronizer = new PlayPosSynchronizer();

            Loaded += TimelineEditorWindow_Loaded;
        }

        private void ViewContext_Changed(object? sender, ViewContextChangedEventArgs e)
        {
            switch (e.ChangeType)
            {
                case ViewContextChangedEventArgs.ChangeTypes.Duration:
                case ViewContextChangedEventArgs.ChangeTypes.PixelPerMs:
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.BankIndex:

                    if (_lastBankIndex != _viewContext.BankIndex && _actuatorsService != null)
                    {
                        _lastBankIndex = _viewContext.BankIndex;
                        MyInvoker.Invoke(new Action(() =>
                        {
                            var bankStartItemNo = _viewContext.BankIndex * _viewContext.ItemsPerBank + 1; // base 1
                            labelBankNo.Content = $"Bank {_viewContext.BankIndex + 1} [{bankStartItemNo}-{Math.Min(_actuatorsService.AllIds.Length, bankStartItemNo + _viewContext.ItemsPerBank - 1)}]";
                        }));
                    }
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.FocusObject:
                    var y = ValuesEditorControl.GetScrollPosForEditorControl(_viewContext.ActualFocusObject);
                    if (y != null)
                        timelineValuesEditorScrollViewer.ScrollToVerticalOffset(y.Value);
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.FocusObjectValue:
                case ViewContextChangedEventArgs.ChangeTypes.Scroll:
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{nameof(e.ChangeType)}:{e.ChangeType}");
            }
        }

        private async void TimelineEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= TimelineEditorWindow_Loaded;

            this.IsEnabled = false;

            _playPosSynchronizer.OnPlayPosChanged += PlayPos_Changed;

            SetupToasts();

            if (_project.TimelinesStates?.Any() == false)
            {
                MessageBox.Show("Project file has no timelineStates defined!");
            }

            this.SizeChanged += TimelineEditorWindow_SizeChanged;
            CalculateSizeAndPixelPerMs();

            _actuatorsService = new ActuatorsService(_project, _clientsService, _logger);

            this._timelineData = CreateNewTimelineData("");

            _timelinePlayer = new TimelinePlayer(timelineData: this._timelineData, playPosSynchronizer: _playPosSynchronizer, actuatorsService: _actuatorsService, awbClientsService: _clientsService, logger: _logger);
            _timelinePlayer.OnPlayStateChanged += OnPlayStateChanged;
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
            _playPosSynchronizer.Dispose();
        }

        private void CalculateSizeAndPixelPerMs()
        {
            this._viewContext.PixelPerMs = this.ActualWidth / msPerScreenWidth;
        }

        private void TimelineEditorWindow_SizeChanged(object sender, SizeChangedEventArgs e) => CalculateSizeAndPixelPerMs();

        private void SetupToasts()
        {
        }

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
                case TimelineControllerEventArgs.EventTypes.PlayPosAbsoluteChanged:
                    switch (_timelinePlayer.PlayState)
                    {
                        case TimelinePlayer.PlayStates.Playing:
                            break;

                        case TimelinePlayer.PlayStates.Nothing:
                            _timelineControllerPlayViewPos.SetPositionFromValueInPercent(e.ValueInPercent);
                            _playPosSynchronizer.SetNewPlayPos(_timelineControllerPlayViewPos.PlayPosAbsoluteMs);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException($"{nameof(_timelinePlayer.PlayState)}:{_timelinePlayer.PlayState.ToString()}");
                    }
                    _lastActuatorChanged = -1;
                    break;

                case TimelineControllerEventArgs.EventTypes.NextBank:
                    SwitchToNextBank();
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

                    var allActuators = _actuatorsService?.AllActuators;
                    if (allActuators == null) return;

                    var actuatorIndex = e.ActuatorIndex_ + _viewContext.BankIndex * _viewContext.ItemsPerBank;
                    if (actuatorIndex >= allActuators.Length) return;

                    var actuator = allActuators[actuatorIndex];
                    var targetPercent = e.ValueInPercent;

                    switch (actuator)
                    {
                        case IServo servo:
                            var servoPoint = _timelineData?.ServoPoints.OfType<ServoPoint>().SingleOrDefault(p => p.ServoId == servo.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMs); // check existing point
                            if (servoPoint == null)
                            {
                                servoPoint = new ServoPoint(servo.Id, targetPercent, _playPosSynchronizer.PlayPosMs);
                                _timelineData?.ServoPoints.Add(servoPoint);
                            }
                            else
                            {
                                servoPoint.ValuePercent = targetPercent;
                            }
                            _timelineData!.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.ServoPointChanged, servoPoint.ServoId);
                            break;

                        case ISoundPlayer soundPlayer:
                            if (_project.Sounds.Length > 0)
                            {
                                var soundIndex = (int)((targetPercent * (_project.Sounds.Length - 1)) / 100);
                                if (soundIndex > 0 && soundIndex < _project.Sounds.Length)
                                {
                                    var sound = _project.Sounds[soundIndex];
                                    var soundPoint = _timelineData?.SoundPoints.OfType<SoundPoint>().SingleOrDefault(p => p.SoundPlayerId == soundPlayer.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMs); // check existing point
                                    if (soundPoint == null)
                                    {
                                        soundPoint = new SoundPoint(timeMs: _playPosSynchronizer.PlayPosMs, soundPlayerId: soundPlayer.Id, title: sound.Title, soundId: sound.Id); ;
                                        _timelineData!.SoundPoints.Add(soundPoint);
                                    }
                                    else
                                    {
                                        soundPoint.SoundId = sound.Id;
                                        soundPoint.Title = sound.Title;
                                    }
                                    _timelineData!.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.SoundPointChanged, soundPoint.SoundPlayerId);

                                }
                            }
                            break;

                        default:
                            throw new ArgumentOutOfRangeException($"{nameof(actuator)}:{actuator} ");
                    }

                    if (_lastActuatorChanged != actuatorIndex)
                    {
                        ShowActuatorValuesOnTimelineInputController();
                        _lastActuatorChanged = actuatorIndex;
                    }
                    break;

                case TimelineControllerEventArgs.EventTypes.ActuatorSetValueToDefault:
                case TimelineControllerEventArgs.EventTypes.ActuatorTogglePoint:
                    _unsavedChanges = true;
                    _lastActuatorChanged = -1;
                    var actuators = _actuatorsService?.AllActuators;
                    if (actuators == null) return;

                    actuatorIndex = e.ActuatorIndex_ + _viewContext.BankIndex * _viewContext.ItemsPerBank;
                    if (actuatorIndex >= actuators.Length) return;

                    actuator = actuators[actuatorIndex];

                    switch (actuator)
                    {
                        case IServo servo:
                            var servoPoint = _timelineData?.ServoPoints.OfType<ServoPoint>().SingleOrDefault(p => p.ServoId == servo.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMs); // check existing point
                            if (servoPoint == null)
                            {
                                // Insert a new servo point
                                var targetValue = e.EventType == TimelineControllerEventArgs.EventTypes.ActuatorSetValueToDefault ? servo.DefaultValue : servo.TargetValue;
                                targetPercent = 100.0 * (targetValue - servo.MinValue) / (servo.MaxValue - servo.MinValue);
                                servo.TargetValue = targetValue;
                                servoPoint = new ServoPoint(servo.Id, targetPercent, _playPosSynchronizer.PlayPosMs);
                                _timelineData?.ServoPoints.Add(servoPoint);
                            }
                            else
                            {

                                if (e.EventType == TimelineControllerEventArgs.EventTypes.ActuatorSetValueToDefault)
                                {
                                    // set target value to default
                                    var targetValue = servo.DefaultValue;
                                    targetPercent = 100.0 * (targetValue - servo.MinValue) / (servo.MaxValue - servo.MinValue);
                                    servoPoint.ValuePercent = targetPercent;
                                    servo.TargetValue = targetValue;
                                }
                                else
                                {
                                    // Remove the existing servo point
                                    _timelineData?.ServoPoints.Remove(servoPoint);
                                }
                            }
                            _timelineData!.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.ServoPointChanged, servoPoint.ServoId);
                            break;
                        case ISoundPlayer soundPlayer:
                            if (_project.Sounds?.Any() == true)
                            {

                                var soundPoint = _timelineData?.SoundPoints.OfType<SoundPoint>().SingleOrDefault(p => p.SoundPlayerId == soundPlayer.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMs); // check existing point
                                if (soundPoint == null)
                                {
                                    // Insert a new sound point
                                    var soundId = soundPlayer.ActualSoundId == 0 ? _project.Sounds.FirstOrDefault()?.Id : soundPlayer.ActualSoundId;
                                    var sound = _project.Sounds.FirstOrDefault(s => s.Id == soundId);
                                    if (sound == null)
                                    {
                                        MessageBox.Show($"Actual sound id{soundPlayer.ActualSoundId} not found");
                                    }
                                    else
                                    {
                                        soundPoint = new SoundPoint(timeMs: _playPosSynchronizer.PlayPosMs, soundPlayerId: soundPlayer.Id, title: sound.Title, soundId: soundPlayer.ActualSoundId);
                                        _timelineData?.SoundPoints.Add(soundPoint);
                                    }
                                }
                                else
                                {
                                    // Remove the existing sound point
                                    _timelineData?.SoundPoints.Remove(soundPoint);
                                }
                                _timelineData!.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.SoundPointChanged, soundPlayer.Id);
                            }
                            break;
                        default:

                            throw new ArgumentOutOfRangeException($"{actuator.Id}/{actuator.Title} is an unhandled actutuator type.");
                    }

                    _manualUpdatingValues = true;
                    if (_manualUpdatingValues) await _timelinePlayer.UpdateActuators();
                    _manualUpdatingValues = false;
                    break;

                case TimelineControllerEventArgs.EventTypes.NextPage:
                    await ScrollPaging(_timelineControllerPlayViewPos.PageWidthMs);
                    _lastActuatorChanged = -1;
                    break;

                case TimelineControllerEventArgs.EventTypes.PreviousPage:
                    await ScrollPaging(_timelineControllerPlayViewPos.PageWidthMs);
                    _lastActuatorChanged = -1;
                    break;

                case TimelineControllerEventArgs.EventTypes.Save:
                    this.SaveTimelineData();
                    _lastActuatorChanged = -1;
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{nameof(e.EventType)}:{e.EventType.ToString()}");
            }
        }

        private void SwitchToNextBank()
        {
            var newBankIndex = _viewContext.BankIndex + 1;
            var maxBankIndex = _actuatorsService.AllIds.Length / _viewContext.ItemsPerBank;
            if (newBankIndex > maxBankIndex) newBankIndex = 0;
            _viewContext.BankIndex = newBankIndex;
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
                MyInvoker.Invoke(new Action(() =>
                {
                    timelineAllValuesScrollViewer.ScrollToHorizontalOffset(newScrollOffset);
                }));
                newPosMs = _playPosSynchronizer.PlayPosMs + scrollSpeedMs;
                _playPosSynchronizer.SetNewPlayPos(newPosMs);
                await Task.Delay(1000 / fps);
            }
            _switchingPages = false;
        }

        private void PlayPos_Changed(object? sender, int newPlayPosMs)
        {
            MyInvoker.Invoke(new Action(() => this.LabelPlayTime.Content = $"{(newPlayPosMs / 1000.0):0.00}s / {_timelinePlayer.PlaybackSpeed:0.0}X"));

            if (!_manualUpdatingValues)
                ShowActuatorValuesOnTimelineInputController();

            _timelineControllerPlayViewPos.SetPlayPosFromTimelineControl(newPlayPosMs);
        }

        private void OnPlayStateChanged(object? sender, PlayStateEventArgs e)
        {
        }

        private void ShowActuatorValuesOnTimelineInputController()
        {
            if (_timelineControllers == null) return;

            var playPosMs = _playPosSynchronizer.PlayPosMs;

            var actuators = _actuatorsService?.AllActuators;
            if (actuators == null) return;

            for (int iActuator = 0; iActuator < actuators.Length; iActuator++)
            {
                var timelineControllerIndex = iActuator - _viewContext.BankIndex * _viewContext.ItemsPerBank;
                if (timelineControllerIndex < 0 || timelineControllerIndex >= _viewContext.ItemsPerBank) continue;

                switch (actuators[iActuator])
                {
                    case IServo servo:
                        foreach (var timelineController in _timelineControllers)
                        {
                            timelineController.SetActuatorValue(index: timelineControllerIndex, valueInPercent: Math.Max(0, Math.Min(100.0, 100.0 * (servo.TargetValue - servo.MinValue * 1.0) / (1.0 * servo.MaxValue - servo.MinValue))));
                            timelineController.ShowPointButtonState(index: timelineControllerIndex, pointExists: _timelineData.ServoPoints.Any(p => p.ServoId == servo.Id && p.TimeMs == playPosMs));
                        }
                        break;
                    case ISoundPlayer soundPlayer:
                        foreach (var timelineController in _timelineControllers)
                        {
                            if (_project.Sounds?.Any() == true)
                            {
                                var soundIndex = -1;
                                for (int iSnd = 0; iSnd < _project.Sounds.Length; iSnd++)
                                {
                                    if (_project.Sounds[iSnd].Id == soundPlayer.ActualSoundId)
                                    {
                                        soundIndex = iSnd;
                                        break;
                                    }
                                }
                                if (soundIndex != -1)
                                {
                                    timelineController.SetActuatorValue(index: timelineControllerIndex, valueInPercent: Math.Max(0, Math.Min(100.0, 100.0 * soundIndex / _project.Sounds.Length)));
                                    timelineController.ShowPointButtonState(index: timelineControllerIndex, pointExists: _timelineData.SoundPoints.Any(p => p.SoundPlayerId == soundPlayer.Id && p.TimeMs == playPosMs));
                                }
                            }
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"{actuators[iActuator].Id}/{actuators[iActuator].Title} is an unhandled actutuator type.");
                }
            }
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

            //var stsServos = _actuatorsService.Servos.Select(s => s as StsScsServo).Where(s => s != null);
            //foreach (var stsServo in stsServos)
            //{
            //    var valuePercent = 100d * (stsServo.DefaultValue - stsServo.MinValue) / (stsServo.MaxValue - stsServo.MinValue);
            //    timelineData.ServoPoints.Add(new ServoPoint(servoId: stsServo.Id, valuePercent: valuePercent, timeMs: 0));
            //}

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

            _playPosSynchronizer.SetNewPlayPos(0);

            if (_timelinePlayer != null)
            {
                await _timelinePlayer.UpdateActuators();
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
            foreach (var timelineController in _timelineControllers)
                timelineController?.SetPlayState(ITimelineController.PlayStates.Playing);
        }

        private async void Stop()
        {
            // snap scrollpos to snap positions 
            _timelinePlayer?.Stop();
            foreach (var timelineController in _timelineControllers)
                timelineController?.SetPlayState(ITimelineController.PlayStates.Editor);
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
