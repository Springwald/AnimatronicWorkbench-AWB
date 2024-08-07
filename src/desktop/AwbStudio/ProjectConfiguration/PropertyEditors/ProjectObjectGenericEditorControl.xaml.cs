// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using Awb.Core.Tools.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace AwbStudio.ProjectConfiguration.PropertyEditors
{
    /// <summary>
    /// Interaction logic for ScsServoEditorControl.xaml
    /// </summary>
    public partial class ProjectObjectGenericEditorControl : UserControl
    {
        public class DeleteObjectEventArgs : EventArgs
        {
            public IProjectObjectListable ObjectToDelete { get; private set; }

            public DeleteObjectEventArgs(IProjectObjectListable objectToDelete)
            {
                ObjectToDelete = objectToDelete;
            }
        }

        private class PropertyDetails
        {
            public string Group { get; set; } = string.Empty;
            public string TechnicalName { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public int Order { get; set; } = 999;
            public PropertyInfo Property { get; internal set; }
        }

        private List<SinglePropertyEditorControl> _editors;
        private IProjectObjectListable _projectObject;
        private AwbProject _awbProject;
        private string[] _objectsUsingThisObject;

        public EventHandler OnUpdatedData { get;  set; }
        public EventHandler<DeleteObjectEventArgs> OnDeleteObject { get;  set; }

        public string? ActualProblems { get;  set; }

        public void SetProjectAndObject(IProjectObjectListable projectObject, AwbProject awbProject, string[] objectsUsingThisObject)
        {
            _projectObject = projectObject;
            _awbProject = awbProject;
            _objectsUsingThisObject = objectsUsingThisObject;
            WriteDataToEditor(projectObject);
            UpdateProblems();
        }
        
        public IProjectObjectListable ProjectObjectToEdit
        {
            get => _projectObject;
        }

        public ProjectObjectGenericEditorControl()
        {
            InitializeComponent();
            this.DataContext = ProjectObjectToEdit;
        }

        private string GroupOrderKey(string groupname) => groupname switch { 
            "General" => "0_" +  groupname, 
            "" => "99_" + groupname, 
            null => "99_",
            _ => "1_" + groupname };

        private void WriteDataToEditor(IProjectObjectListable stsServoConfig)
        {
            if (_editors != null) throw new System.Exception("Object to edit is already set.");

            var type = stsServoConfig.GetType();
            _editors = new List<SinglePropertyEditorControl>();

            // Group properties
            var propertyGroups = type.GetProperties().Select(p =>
                new { Property = p, Title = p.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? p.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? null })
                .Where(p => p.Title != null)
                .Select(p => new PropertyDetails()
                {
                    TechnicalName = p.Property.Name,
                    Title = p.Title!,
                    Group = p.Property.GetCustomAttribute<DisplayAttribute>()?.GetGroupName() ?? "General",
                    Order = p.Property.GetCustomAttribute<DisplayAttribute>()?.GetOrder() ?? 999,
                    Property = p.Property
                }).GroupBy(p => p.Group, p => p, (key, g) => new { GroupName = key, Properties = g.OrderBy(p => p.Order).ThenBy(p => p.Title).Select(p => p).ToArray() }
                ).OrderBy(g => GroupOrderKey(g.GroupName)).ToArray();

            foreach (var group in propertyGroups)
            {
                StackPanel? stackPanel = new StackPanel();
                if (string.IsNullOrEmpty(group.GroupName))
                {
                    this.EditorStackPanel.Children.Add(stackPanel);
                }
                else
                {
                    var groupControl = new GroupBox
                    {
                        Header = group.GroupName,
                        Margin = new System.Windows.Thickness(left: 5, top: 0, right: 5, bottom: 10),
                        Padding = new System.Windows.Thickness(left: 10, top: 10, right: 5, bottom: 10),
                        Background = System.Windows.Media.Brushes.Black,
                    };
                    this.EditorStackPanel.Children.Add(groupControl);
                    groupControl.Content = stackPanel;
                }

                // iterate trough all properties  of the object which have a DataAnnotation for "Name" and add an editor for each
                foreach (var property in group.Properties)
                {
                    var editor = new SinglePropertyEditorControl();
                    //editor.SetPropertyToEditByExpression(() => stsServoConfig.GetType().GetProperty(property.Name));
                    editor.SetPropertyToEditByName(target: stsServoConfig, propertyName: property.TechnicalName, title: property.Title);
                    editor.PropertyChanged += (s, e) =>
                    {
                        LabelObjectTypeTitle.Content = stsServoConfig.TitleDetailed;
                        this.UpdateProblems();
                        this.OnUpdatedData?.Invoke(this, EventArgs.Empty);
                    };
                    _editors.Add(editor);
                    stackPanel.Children.Add(editor);
                }
            }

            // find objects using this object
            if (_objectsUsingThisObject.Any())
            {
                TextUsageIn.Text = $"Used {_objectsUsingThisObject.Length} times (mouse over for details)"; 
                TextUsageIn.ToolTip = string.Join(",\r\n", _objectsUsingThisObject);
                TextUsageIn.Visibility = System.Windows.Visibility.Visible;
            }
            else
                TextUsageIn.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void UpdateProblems()
        {
            var problemsText = new StringBuilder();

            const bool showAlsoNativeErrors = false; // true = usually results in duplicate error entries
            if (showAlsoNativeErrors)
            {
#pragma warning disable CS0162 // Unreachable code detected
                var nativeErrors = ObjectValidator.ValidateObjectGetErrors(_projectObject);

                foreach (var error in nativeErrors)
                    problemsText.AppendLine("Native error:" + error.PlaintTextDescription);
#pragma warning restore CS0162 // Unreachable code detected
            }

            foreach (var problem in _projectObject.GetContentProblems(_awbProject))
                problemsText.AppendLine(problem.PlaintTextDescription);

            foreach (var editor in _editors)
            {
                var errors = editor.ErrorMessagesJoined;
                if (!string.IsNullOrWhiteSpace(errors))
                    problemsText.AppendLine($"{editor.PropertyTitle}:{errors}");
            }

            if (problemsText.Length == 0)
            {
                ActualProblems = null;
                TextProblems.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                ActualProblems = problemsText.Length == 0 ? null : problemsText.ToString();
                TextProblems.Text = ActualProblems ?? string.Empty;
                TextProblems.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void ButtonDelete_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_objectsUsingThisObject.Any())
            {
                MessageBox.Show($"This object is used in the following objects:\r\n{string.Join(",\r\n", _objectsUsingThisObject)}\r\n\r\nPlease remove the object from these usages first.", "Object is used", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // throw delete event
            if (MessageBox.Show($"Do you really want to delete the object '{_projectObject.TitleDetailed}'?", "Delete object", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                this.OnDeleteObject?.Invoke(this,new DeleteObjectEventArgs(_projectObject));
            }

        }
    }
}
