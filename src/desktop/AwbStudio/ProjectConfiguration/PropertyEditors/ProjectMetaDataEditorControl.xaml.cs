// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Project.Various;
using System.Windows.Controls;

namespace AwbStudio.ProjectConfiguration.PropertyEditors
{
    public partial class ProjectMetaDataEditorControl : UserControl
    {
        public ProjectMetaDataEditorControl()
        {
            InitializeComponent();
        }

        public ProjectMetaData ProjectMetaData { get; internal set; }
    }
}
