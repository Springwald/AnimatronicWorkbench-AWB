// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project.Various;
using System.Windows.Controls;

namespace AwbStudio.ProjectConfiguration.PropertyEditors
{
    /// <summary>
    /// Interaction logic for ProjectMetaDataEditorControl.xaml
    /// </summary>
    public partial class ProjectMetaDataEditorControl : UserControl
    {
        public ProjectMetaDataEditorControl()
        {
            InitializeComponent();
        }

        public ProjectMetaData ProjectMetaData { get; internal set; }
    }
}
