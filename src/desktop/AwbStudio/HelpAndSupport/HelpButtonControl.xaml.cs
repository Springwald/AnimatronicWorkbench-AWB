// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System.Windows;
using System.Windows.Controls;

namespace AwbStudio.HelpAndSupport
{
    /// <summary>
    /// A button control that opens help documentation in the default web browser.  
    /// </summary>
    public partial class HelpButtonControl : UserControl
    {
        /// <summary>
        /// Represents the available help topics within the application.
        /// </summary>
        /// <remarks>This enumeration is used to categorize help content for different areas of
        /// functionality. For example, <see cref="HelpTopics.Projects"/> corresponds to help content related to
        /// project management.</remarks>
        public enum HelpTopics
        {
            None,
            Projects,
        }

        /// <summary>
        /// Gets or sets the help topic associated with the current context.
        /// </summary>
        public HelpTopics HelpTopic { get; set; }

        public HelpButtonControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Open the help documentation in the default web browser based on the selected HelpTopic.
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var RootPath = "https://daniel.springwald.de/post/AWB-Docs/";

            var topicPath = HelpTopic switch
            {
                HelpTopics.Projects => "AWB-Projects",
                _ => "AWB-Docs" // Fallback to a default topic if none matches
            };

            // Construct the full URL to the help documentation
            var destinationurl = RootPath + topicPath;

            // Open the URL in the default web browser
            var sInfo = new System.Diagnostics.ProcessStartInfo(destinationurl)
            {
                UseShellExecute = true,
            };
            System.Diagnostics.Process.Start(sInfo);
        }


    }
}
