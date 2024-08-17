// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AwbStudio.TimelineEditing
{
    internal class TimelineEditorWindowContext
    {
        private AwbProject _project;

        public TimelineEditorWindowContext(AwbProject project)
        {
            if (project.TimelinesStates?.Any() == false)
                MessageBox.Show("Project file has no timelineStates defined!");

            _project = project;
        }
    }
}
