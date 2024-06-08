// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using Awb.Core.Project.Servos;
using Awb.Core.Project.Various;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

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

        public bool UnsavedChanges { get; set; } = true;

        #region project objects

        private string _projectFolder;
        public string ProjectFolder
        {

            get => _projectFolder;
            set
            {
                _projectFolder = value;
                OnPropertyChanged();
            }
        }

        private ProjectMetaData? _projectMetaData { get; set; }
        public ProjectMetaData? ProjectMetaData
        {

            get => _projectMetaData;
            set
            {
                _projectMetaData = value;
                OnPropertyChanged();
            }
        }


        private ObservableCollection<IProjectObjectListable> _scsServos = new ObservableCollection<IProjectObjectListable>();
        public ObservableCollection<IProjectObjectListable> ScsServos
        {

            get => _scsServos;
            set
            {
                _scsServos = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<IProjectObjectListable> _stsServos = new ObservableCollection<IProjectObjectListable>();
        public ObservableCollection<IProjectObjectListable> StsServos
        {
            get => _stsServos;
            set
            {
                _stsServos = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<IProjectObjectListable> _pca9685PwmServos = new ObservableCollection<IProjectObjectListable>();
        public ObservableCollection<IProjectObjectListable> Pca9685PwmServos
        {
            get => _pca9685PwmServos;
            set
            {
                _pca9685PwmServos = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<IProjectObjectListable> _mp3PlayerYX5300 = new ObservableCollection<IProjectObjectListable>();
        public ObservableCollection<IProjectObjectListable> Mp3PlayerYX5300
        {
            get => _mp3PlayerYX5300;
            set
            {
                _mp3PlayerYX5300 = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<IProjectObjectListable> _inputs = new ObservableCollection<IProjectObjectListable>();
        public ObservableCollection<IProjectObjectListable> Inputs
        {
            get => _inputs;
            set
            {
                _inputs = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<IProjectObjectListable> _timelineStates = new ObservableCollection<IProjectObjectListable>();
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

        public ProjectConfigViewModel(AwbProject awbProject)
        {
            FillFromProject(awbProject);
        }

        public void WriteToProject(AwbProject awbProject)
        {
            awbProject.ScsServos = this.ScsServos.Cast<ScsFeetechServoConfig>().ToArray();
            awbProject.StsServos = this.StsServos.Cast<StsFeetechServoConfig>().ToArray();
            awbProject.Pca9685PwmServos = this.Pca9685PwmServos.Cast<Pca9685PwmServoConfig>().ToArray();
            awbProject.Mp3PlayersYX5300 = this.Mp3PlayerYX5300.Cast<Mp3PlayerYX5300Config>().ToArray();
            awbProject.Inputs = this.Inputs.Cast<InputConfig>().ToArray();
            awbProject.TimelinesStates = this.TimelineStates.Cast<TimelineState>().ToArray();
        }

        private void FillFromProject(AwbProject awbProject)
        {
            if (awbProject == null) return;

            this.ProjectFolder = awbProject.ProjectFolder;
            this.ProjectMetaData = awbProject.ProjectMetaData;

            if (awbProject?.ScsServos != null)
                foreach (var scsServo in awbProject.ScsServos)
                    this.ScsServos.Add(scsServo);

            if (awbProject?.StsServos != null)
                foreach (var stsServo in awbProject.StsServos)
                    this.StsServos.Add(stsServo);

            if (awbProject?.Pca9685PwmServos != null)
                foreach (var pca9685PwmServo in awbProject.Pca9685PwmServos)
                    this.Pca9685PwmServos.Add(pca9685PwmServo);

            if (awbProject?.Mp3PlayersYX5300 != null)
                foreach (var mp3PlayerYX5300 in awbProject.Mp3PlayersYX5300)
                    this.Mp3PlayerYX5300.Add(mp3PlayerYX5300);

            if (awbProject?.Inputs != null)
                foreach (var input in awbProject.Inputs)
                    this.Inputs.Add(input);

            if (awbProject?.TimelinesStates != null)
                foreach (var timelineState in awbProject.TimelinesStates)
                    this.TimelineStates.Add(timelineState);
        }
    }
}
