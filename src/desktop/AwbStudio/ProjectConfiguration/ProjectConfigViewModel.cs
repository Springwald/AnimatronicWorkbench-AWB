// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AwbStudio.ProjectConfiguration
{
    public class ProjectConfigViewModel : INotifyPropertyChanged
    {
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler? handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

         private AwbProject? _awbProject;

        public required AwbProject AwbProject
        {
            get => _awbProject! ?? throw new NullReferenceException("AwbProject not set!");
            init
            {
                _awbProject = value ?? throw new ArgumentNullException(nameof(AwbProject));
                this.UpdateProjectObjectLists();
            }
        }
        public bool UnsavedChanges { get; set; }

        #region project objects

        private ObservableCollection<IProjectObjectListable> _scsServos;
        public ObservableCollection<IProjectObjectListable> ScsServos
        {
            get => _scsServos;
            set
            {
                _scsServos = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<IProjectObjectListable> _stsServos;
        public ObservableCollection<IProjectObjectListable> StsServos
        {
            get => _stsServos;
            set
            {
                _stsServos = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<IProjectObjectListable> _pca9685PwmServos;
        public ObservableCollection<IProjectObjectListable> Pca9685PwmServos
        {
            get => _pca9685PwmServos;
            set
            {
                _pca9685PwmServos = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<IProjectObjectListable> _mp3PlayerYX5300;
        public ObservableCollection<IProjectObjectListable> Mp3PlayerYX5300
        {
            get => _mp3PlayerYX5300;
            set
            {
                _mp3PlayerYX5300 = value;
                OnPropertyChanged();
            }
        }

                 private ObservableCollection<IProjectObjectListable> _inputs;
        public ObservableCollection<IProjectObjectListable> Inputs
        {
            get => _inputs;
            set
            {
                _inputs = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<IProjectObjectListable> _timelineStates;
        public ObservableCollection<IProjectObjectListable> TimelineStates
        {
            get => _timelineStates;
            set
            {
                _timelineStates = value;
                OnPropertyChanged();
            }
        }


        #endregion

        public ProjectConfigViewModel()
        {
            _scsServos = new ObservableCollection<IProjectObjectListable>();
            _stsServos = new ObservableCollection<IProjectObjectListable>();
            _pca9685PwmServos = new ObservableCollection<IProjectObjectListable>();
            _mp3PlayerYX5300 = new ObservableCollection<IProjectObjectListable>();
            _inputs = new ObservableCollection<IProjectObjectListable>();
            _timelineStates = new ObservableCollection<IProjectObjectListable>();
        }

        private void UpdateProjectObjectLists()
        {
            this.ScsServos.Clear();
            if (_awbProject == null) return;

            if (_awbProject?.ScsServos != null)
                foreach (var scsServo in _awbProject.ScsServos)
                    this.ScsServos.Add(scsServo);

            if (_awbProject?.StsServos != null)
                foreach (var stsServo in _awbProject.StsServos)
                    this.StsServos.Add(stsServo);

            if (_awbProject?.Pca9685PwmServos != null)
                foreach (var pca9685PwmServo in _awbProject.Pca9685PwmServos)
                    this.Pca9685PwmServos.Add(pca9685PwmServo);

            if (_awbProject?.Mp3PlayersYX5300 != null)
                foreach (var mp3PlayerYX5300 in _awbProject.Mp3PlayersYX5300)
                    this.Mp3PlayerYX5300.Add(mp3PlayerYX5300);

            if (_awbProject?.Inputs != null)
                foreach (var input in _awbProject.Inputs)
                    this.Inputs.Add(input);

            if (_awbProject?.TimelinesStates != null)
                foreach (var timelineState in _awbProject.TimelinesStates)
                    this.TimelineStates.Add(timelineState); 

        }
    }
}
