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
using AwbStudio.FileManagement;
using AwbStudio.Projects;
using AwbStudio.TimelineControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AwbStudio
{
    public partial class TimelineEditorWindow : Window
    {
        private class TimelineControllerPlayViewPos
        {
            /// <summary>
            /// The range in which the PlayPos can be moved by the controller
            /// </summary>
            public int PageWidthMs { get; set; } = 10 * 1000; // 10 seconds

            /// <summary>
            /// The PlayPos in ms, which represents the start of the ScrollPageWidthMs
            /// </summary>
            public int OriginMs { get; set; }

            /// <summary>
            ///  a value between 0 and PageWidthMs, which represents the position of the PlayPos in the PageWidthMs
            /// </summary>
            public int PlayPosRelativeToOriginMs { get; set; }

            /// <summary>
            ///  The absolute position of the PlayPos set by in ms
            /// </summary>
            public int PlayPosAbsoluteMs => OriginMs + PlayPosRelativeToOriginMs;

            public void SetPlayPosFromTimelineControl(int playPosMs)
            {
                OriginMs = playPosMs - PlayPosRelativeToOriginMs;
            }

            public void SetFromValueInPercent(double valueInPercent)
            {
                PlayPosRelativeToOriginMs = (int)(PageWidthMs * valueInPercent / 100.0);
            }
        }

        const int msPerScreenWidth = 20 * 1000; // todo: zoom in/out

        private readonly PlayPosSynchronizer _playPosSynchronizer ;
        private readonly IAwbClientsService _clientsService;
        private readonly IProjectManagerService _projectManagerService;
        private readonly IAwbLogger _logger;
        private readonly AwbProject _project;
        private readonly FileManager _fileManager;
        private readonly ITimelineController[] _timelineControllers;
        private readonly TimelineViewContext _viewContext;
        private TimelinePlayer _timelinePlayer;
        private IActuatorsService _actuatorsService;

        private TimelineViewContext _timeLineViewContext;

        private TimelineControllerPlayViewPos _timelineControllerPlayViewPos = new TimelineControllerPlayViewPos();

        private bool _unsavedChanges;

        public ValueTuningWindow ValueTuningWin { get; private set; }

        private volatile bool _manualUpdatingValues;
        private bool _switchingPages;
        private int _lastActuatorChanged = 1; // prevent double actuator change events to the midi controller
        private bool _ctrlKeyPressed;

        protected TimelineData TimelineData { get; set; }

        public TimelineEditorWindow(ITimelineController[] timelineControllers, IProjectManagerService projectManagerService, IAwbClientsService clientsService, IAwbLogger awbLogger)
        {
            InitializeComponent();

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
            _fileManager = new FileManager(_project);
            _timelineControllers = timelineControllers;

            _viewContext = new TimelineViewContext();

            _playPosSynchronizer = new PlayPosSynchronizer();

            Loaded += TimelineEditorWindow_Loaded;
        }

      

        private void AwbLogger_OnLog(object? sender, string e)
        {
            throw new NotImplementedException();
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

            this.TimelineData = CreateNewTimelineData("");

            _timelinePlayer = new TimelinePlayer(timelineData: this.TimelineData, playPosSynchronizer: _playPosSynchronizer, actuatorsService: _actuatorsService, awbClientsService: _clientsService, logger: _logger);
            _timelinePlayer.OnPlayStateChanged += OnPlayStateChanged;
            _timelinePlayer.OnPlaySound += SoundPlayer.SoundToPlay;

            var timelineCaptions = new TimelineCaptions();
            TimelineViewerControl.Init(_viewContext, timelineCaptions, _playPosSynchronizer, _actuatorsService);
            TimelineViewerControl.TimelineDataLoaded(TimelineData);
            TimelineViewerControl.Timelineplayer = _timelinePlayer;
            SoundPlayer.Sounds = _project.Sounds;

            await TimelineDataLoaded();

            TimelineChooser.OnTimelineChosen += TimelineChosenToLoad;

            foreach (var timelineController in _timelineControllers)
            {
                timelineController.ActualActuatorNames = _actuatorsService.Servos.Select(s => s.Name).ToArray();
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
            => ts.Export ? ts.Name : $"{ts.Name} (no export)";

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
            switch (e.EventType)
            {
                case TimelineControllerEventArgs.EventTypes.PlayPosAbsoluteChanged:
                    switch (_timelinePlayer.PlayState)
                    {
                        case TimelinePlayer.PlayStates.Playing:
                            break;

                        case TimelinePlayer.PlayStates.Nothing:
                            _timelineControllerPlayViewPos.SetFromValueInPercent(e.ValueInPercent);
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
                            var servoPoint = TimelineData?.ServoPoints.OfType<ServoPoint>().SingleOrDefault(p => p.ServoId == servo.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMs); // check existing point
                            if (servoPoint == null)
                            {
                                servoPoint = new ServoPoint(servo.Id, targetPercent, _playPosSynchronizer.PlayPosMs);
                                TimelineData?.ServoPoints.Add(servoPoint);
                            }
                            else
                            {
                                servoPoint.ValuePercent = targetPercent;
                            }
                            break;

                        case ISoundPlayer soundPlayer:
                            if (_project.Sounds.Length > 0)
                            {
                                var soundIndex = (int)((targetPercent * (_project.Sounds.Length - 1)) / 100);
                                if (soundIndex > 0 && soundIndex < _project.Sounds.Length)
                                {
                                    var sound = _project.Sounds[soundIndex];
                                    var soundPoint = TimelineData?.SoundPoints.OfType<SoundPoint>().SingleOrDefault(p => p.SoundPlayerId == soundPlayer.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMs); // check existing point
                                    if (soundPoint == null)
                                    {
                                        soundPoint = new SoundPoint(timeMs: _playPosSynchronizer.PlayPosMs, soundPlayerId: soundPlayer.Id, title: sound.Title, soundId: sound.Id); ;
                                        TimelineData?.SoundPoints.Add(soundPoint);
                                    }
                                    else
                                    {
                                        soundPoint.SoundId = sound.Id;
                                        soundPoint.Title = sound.Title;
                                    }
                                }
                            }
                            break;

                        default:
                            throw new ArgumentOutOfRangeException($"{nameof(actuator)}:{actuator} ");
                    }

                    MyInvoker.Invoke(new Action(() => { TimelineViewerControl.PaintTimeLine(); }));
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
                            var servoPoint = TimelineData?.ServoPoints.OfType<ServoPoint>().SingleOrDefault(p => p.ServoId == servo.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMs); // check existing point
                            if (servoPoint == null)
                            {
                                // Insert a new servo point
                                var targetValue = e.EventType == TimelineControllerEventArgs.EventTypes.ActuatorSetValueToDefault ? servo.DefaultValue : servo.TargetValue;
                                targetPercent = 100.0 * (targetValue - servo.MinValue) / (servo.MaxValue - servo.MinValue);
                                servo.TargetValue = targetValue;
                                servoPoint = new ServoPoint(servo.Id, targetPercent, _playPosSynchronizer.PlayPosMs);
                                TimelineData?.ServoPoints.Add(servoPoint);
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
                                    TimelineData?.ServoPoints.Remove(servoPoint);
                                }
                            }
                            break;
                        case ISoundPlayer soundPlayer:
                            if (_project.Sounds?.Any() == true)
                            {

                                var soundPoint = TimelineData?.SoundPoints.OfType<SoundPoint>().SingleOrDefault(p => p.SoundPlayerId == soundPlayer.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMs); // check existing point
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
                                        TimelineData?.SoundPoints.Add(soundPoint);
                                    }
                                }
                                else
                                {
                                    // Remove the existing sound point
                                    TimelineData?.SoundPoints.Remove(soundPoint);
                                }
                            }
                            break;
                        default:

                            throw new ArgumentOutOfRangeException($"{actuator.Id}/{actuator.Name} is an unhandled actutuator type.");
                    }

                    _manualUpdatingValues = true;
                    if (_manualUpdatingValues) await _timelinePlayer.UpdateActuators();
                    MyInvoker.Invoke(new Action(() => { TimelineViewerControl.PaintTimeLine(); }));
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
            if (TimelineViewerControl == null) return;
            if (TimelineViewerControl.Timelineplayer == null) return;
            if (_switchingPages) return;
            int fps = 20;
            int speed = 4; // speed (x seconds per second)
            _switchingPages = true;
            int newPosMs = 0;
            int scrollSpeedMs = (howManyMs > 0 ? 1 : -1) * (1000 / fps) * speed;
            for (int i = 0; i < howManyMs / scrollSpeedMs; i++)
            {
                var newScrollOffset = timelineScrollViewer.HorizontalOffset + _viewContext.DurationMs / scrollSpeedMs;
                MyInvoker.Invoke(new Action(() =>
                {
                    timelineScrollViewer.ScrollToHorizontalOffset(newScrollOffset);
                }));
                newPosMs = _playPosSynchronizer.PlayPosMs + scrollSpeedMs;
                _playPosSynchronizer.SetNewPlayPos(newPosMs);
                MyInvoker.Invoke(new Action(() => TimelineViewerControl.PaintTimeLine()));
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
                            timelineController.ShowPointButtonState(index: timelineControllerIndex, pointExists: TimelineData.ServoPoints.Any(p => p.ServoId == servo.Id && p.TimeMs == playPosMs));
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
                                    timelineController.ShowPointButtonState(index: timelineControllerIndex, pointExists: TimelineData.SoundPoints.Any(p => p.SoundPlayerId == soundPlayer.Id && p.TimeMs == playPosMs));
                                }
                            }
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"{actuators[iActuator].Id}/{actuators[iActuator].Name} is an unhandled actutuator type.");
                }
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
            var data = TimelineData;
            if (data == null) return;

            var changesAfterLoading = false;

            this.Title = TimelineData == null ? "No Timeline" : $"Timeline '{TimelineData.Title}'";
            if (_timelinePlayer != null) _timelinePlayer.TimelineData = data;
            TimelineViewerControl.TimelineDataLoaded(data);
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
            _playPosSynchronizer.InSnapMode = false;
            _timelinePlayer?.Play();
            foreach (var timelineController in _timelineControllers)
                timelineController?.SetPlayState(ITimelineController.PlayStates.Playing);
        }

        private async void Stop()
        {
            // snap scrollpos to snap positions 
            _timelinePlayer?.Stop();
            _playPosSynchronizer.InSnapMode = true;
            foreach (var timelineController in _timelineControllers)
                timelineController?.SetPlayState(ITimelineController.PlayStates.Editor);

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
            if (_project == null) return;
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

        #region mouse events

        double _mouseX = 0;

        private async void timelineViewerControl_MouseDown(object sender, MouseButtonEventArgs e) =>
            await SetPlayPosByMouse(_mouseX);

        private async void TimelineViewerControl_MouseMove(object sender, MouseEventArgs e)
        {
            _mouseX = e.GetPosition(TimelineViewerControl).X;
            if (e.LeftButton == MouseButtonState.Pressed)
                await SetPlayPosByMouse(_mouseX);
        }

        private async Task SetPlayPosByMouse(double mouseX)
        {
            var newPlayPosMs = (int)((timelineScrollViewer.HorizontalOffset + mouseX) + PlayPosSynchronizer.SnapMs / 2);
           _playPosSynchronizer.SetNewPlayPos(newPlayPosMs);
        }

        #endregion


    }
}
