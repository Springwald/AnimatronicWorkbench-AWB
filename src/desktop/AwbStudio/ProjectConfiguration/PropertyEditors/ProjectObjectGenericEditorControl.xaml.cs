// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Project;
using Awb.Core.Project.Servos;
using Awb.Core.Services;
using Awb.Core.Tools;
using Awb.Core.Tools.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AwbStudio.ProjectConfiguration.PropertyEditors
{
    public partial class ProjectObjectGenericEditorControl : UserControl
    {
        private class PropertyDetails
        {
            public string Group { get; set; } = string.Empty;
            public string TechnicalName { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public int Order { get; set; } = 999;
            public required PropertyInfo Property { get; internal set; }
        }

        private readonly IAwbClientsService _awbClientsService;

        private List<SinglePropertyEditorControl>? _editors;
        private IProjectObjectListable? _projectObject;
        private AwbProject? _awbProject;
        private IInvoker? _invoker;
        private string[]? _objectsUsingThisObject;

        public EventHandler? OnUpdatedData { get; set; }
        public EventHandler<DeleteObjectEventArgs>? OnDeleteObject { get; set; }

        public string? ActualProblems { get; set; }

        public async Task SetProjectAndObject(IProjectObjectListable projectObject, AwbProject awbProject, IInvoker invoker, string[] objectsUsingThisObject)
        {
            _projectObject = projectObject;
            _awbProject = awbProject;
            _invoker = invoker;
            _objectsUsingThisObject = objectsUsingThisObject;
            await WriteDataToEditor(projectObject);
            UpdateProblems();
        }

        public IProjectObjectListable? ProjectObjectToEdit
        {
            get => _projectObject;
        }

        public ProjectObjectGenericEditorControl(IAwbClientsService awbClientsService)
        {
            _awbClientsService = awbClientsService;
            InitializeComponent();
            this.DataContext = ProjectObjectToEdit;
        }

        private string GroupOrderKey(string groupname) => groupname switch
        {
            "General" => "0_" + groupname,
            "" => "99_" + groupname,
            null => "99_",
            _ => "1_" + groupname
        };

        private async Task WriteDataToEditor(IProjectObjectListable projectObject)
        {
            if (_editors != null) throw new System.Exception("Object to edit is already set.");
            if (_invoker == null) throw new ArgumentNullException(nameof(_invoker), "Invoker cannot be null.");
            if (_objectsUsingThisObject == null) throw new ArgumentNullException(nameof(_objectsUsingThisObject), "Objects using this object cannot be null.");

            var type = projectObject.GetType();
            _editors = [];

            // add special editors for servos to tune and test the values interactively
            ServoConfigBonusEditorControl? servoConfigBonusEditorControl = null;
            if (projectObject is IServoConfig servoConfig)
            {
                servoConfigBonusEditorControl = new ServoConfigBonusEditorControl(_awbClientsService, _invoker)
                {
                    ServoConfig = servoConfig,
                };
            }

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
                    var groupBox = CreateGroupBox(group.GroupName);
                    this.EditorStackPanel.Children.Add(groupBox);
                    groupBox.Content = stackPanel;
                }

                // iterate trough all properties  of the object which have a DataAnnotation for "Name" and add an editor for each
                foreach (var property in group.Properties)
                {
                    var editor = new SinglePropertyEditorControl();
                    //editor.SetPropertyToEditByExpression(() => stsServoConfig.GetType().GetProperty(property.Name));
                    editor.SetPropertyToEditByName(targetObject: projectObject, propertyName: property.TechnicalName, title: property.Title);
                    editor.PropertyChanged += (s, e) =>
                    {
                        LabelObjectTypeTitle.Content = projectObject.TitleDetailed;
                        this.UpdateProblems();
                        this.OnUpdatedData?.Invoke(this, EventArgs.Empty);
                        if (servoConfigBonusEditorControl != null && projectObject is IServoConfig servoConfig)
                        {
                            // update the servo limits in the config bonus editor control
                            servoConfigBonusEditorControl.UpdateProjectLimits();
                        }
                    };

                    // connect the button to get the actual servo position to the editor
                    editor.GetActualServoPositionRequested += async (s, e) =>
                    {
                        if (servoConfigBonusEditorControl != null && projectObject is IServoConfig servoConfig && servoConfig.CanReadServoPosition)
                        {
                            var position = await servoConfigBonusEditorControl.ReadPosFromServo(servoConfig);
                            if (position.HasValue)
                                e.ServoPositionReceivedDelegate(position.Value);
                        }
                    };

                    _editors.Add(editor);
                    stackPanel.Children.Add(editor);
                }
            }

            // if this object supports servo bonus editor, add it to the editor stack panel
            if (servoConfigBonusEditorControl != null)
            {
                var groupBox = CreateGroupBox("Interactive servo settings");
                groupBox.Content = servoConfigBonusEditorControl;
                this.EditorStackPanel.Children.Add(groupBox);
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

            await Task.CompletedTask;
        }

        private GroupBox CreateGroupBox(string title)
        {
            return new GroupBox
            {
                Header = title,
                Margin = new System.Windows.Thickness(left: 5, top: 0, right: 5, bottom: 10),
                Padding = new System.Windows.Thickness(left: 10, top: 10, right: 5, bottom: 10),
                Background = new SolidColorBrush(Color.FromArgb(10,0,0,0))
            };

        }

        private void UpdateProblems()
        {
            if (_projectObject == null || _awbProject == null || _editors == null)
            {
                ActualProblems = null;
                TextProblems.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }

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
            if (_objectsUsingThisObject == null)
                return;

            if (_objectsUsingThisObject.Any())
            {
                MessageBox.Show($"This object is used in the following objects:\r\n{string.Join(",\r\n", _objectsUsingThisObject)}\r\n\r\nPlease remove the object from these usages first.", "Object is used", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_projectObject == null)
            {
                MessageBox.Show("No object to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // throw delete event
            if (MessageBox.Show($"Do you really want to delete the object '{_projectObject.TitleDetailed}'?", "Delete object", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                this.OnDeleteObject?.Invoke(this, new DeleteObjectEventArgs(_projectObject));
            }

        }
    }
}
