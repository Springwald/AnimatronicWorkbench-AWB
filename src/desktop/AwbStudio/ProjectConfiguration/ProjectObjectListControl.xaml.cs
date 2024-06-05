// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace AwbStudio.ProjectConfiguration
{
    /// <summary>
    /// Interaction logic for ProjectObjectListControl.xaml
    /// </summary>
    public partial class ProjectObjectListControl : UserControl
    {
        public ObservableCollection<IProjectObjectListable> ProjectObjects
        {
            get {
                return (ObservableCollection<IProjectObjectListable>)GetValue(ProjectObjectsProperty); 
            }
            set {
                SetValue(ProjectObjectsProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for ProjectObjects.  This enables animation, styling, binding, etc...
       // public static readonly DependencyProperty ProjectObjectsProperty =
       //     DependencyProperty.Register("ProjectObjects", typeof(ObservableCollection<IProjectObjectListable>), typeof(ProjectObjectListControl), new PropertyMetadata(null));


        /*public int ProjectObjects
        {
            get { return (int)GetValue(ProjectObjectsProperty); }
            set { SetValue(ProjectObjectsProperty, value); }
        }*/

        

        public static readonly DependencyProperty ProjectObjectsProperty =
    DependencyProperty.Register(
        nameof(ProjectObjects),
        typeof(ObservableCollection<IProjectObjectListable>),
        typeof(ProjectObjectListControl),
        new PropertyMetadata(null)
        //new PropertyMetadata("DEFAULT")
        );
        

        /*

       

        // Using a DependencyProperty as the backing store for ProjectObjects.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProjectObjectsProperty =
            DependencyProperty.Register("ProjectObjects", typeof(int), typeof(ownerclass), new PropertyMetadata(0));





        public static readonly DependencyProperty ProjectObjectsProperty = DependencyProperty.Register(
    "ProjectObjects", typeof(ObservableCollection<IProjectObjectListable>), typeof(ProjectObjectListControl));
        
        public ObservableCollection<IProjectObjectListable> ProjectObjects
        {
            get => (ObservableCollection<IProjectObjectListable>)GetValue(ProjectObjectsProperty);
            set
            {
                SetValue(ProjectObjectsProperty, value);
                ListProjectObjects.ItemsSource = value;
            }
        }
        */

        public ProjectObjectListControl()
        {
            //// fill the list ProjectObjects with some example data of type StsServoConfig
            //ProjectObjects = new ObservableCollection<IProjectObjectListable>
            //{
            //    new StsServoConfig("Servo1", "Servo 1", 1, 1),
            //    new StsServoConfig("Servo2", "Servo 2", 1, 2),
            //    new StsServoConfig("Servo3", "Servo 3", 1, 3),
            //    new StsServoConfig("Servo4", "Servo 4", 1, 4),
            //    new StsServoConfig("Servo5", "Servo 5", 1, 5),
            //    new StsServoConfig("Servo6", "Servo 6", 1, 6),
            //    new StsServoConfig("Servo7", "Servo 7", 1, 7),
            //    new StsServoConfig("Servo8", "Servo 8", 1, 8),
            //    new StsServoConfig("Servo9", "Servo 9", 1, 9),
            //    new StsServoConfig("Servo10", "Servo 10", 1, 10),
            //    new StsServoConfig("Servo11", "Servo 11", 1, 11),
            //    new StsServoConfig("Servo11", "Servo 12", 1, 12),
            //    new StsServoConfig("Servo11", "Servo 13", 1, 13),
            //    new StsServoConfig("Servo11", "Servo 14", 1, 14),
            //    new StsServoConfig("Servo11", "Servo 15", 1, 15),
            //};

            InitializeComponent();



        }
    }
}
