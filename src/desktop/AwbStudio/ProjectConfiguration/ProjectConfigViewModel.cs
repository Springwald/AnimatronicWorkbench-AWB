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
        protected void OnPropertyChanged(
       [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
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

        public ProjectConfigViewModel()
        {
            _scsServos = new ObservableCollection<IProjectObjectListable>();
        }

        private void UpdateProjectObjectLists()
        {
            this.ScsServos.Clear();
            if (_awbProject == null) return;

            if (_awbProject?.ScsServos != null)
                foreach (var scsServo in _awbProject.ScsServos)
                    this.ScsServos.Add(scsServo);
        }
    }
}
