// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using AwbStudio.StudioSettings;
using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace AwbStudio.UserControls
{
    public partial class About : UserControl
    {
        public static string VersionLabelInfo
        {
            get
            {
                var version = VersionInfo.ReadFromEmbeddedJson();
                return $"Version {version.Version} - {version.VersionReleaseDate:dd.MM.yyyy}";
            }
        }

        public About()
        {
            InitializeComponent();
            Loaded += About_Loaded;
        }

        private void About_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            LabelVersion.Content = VersionLabelInfo;

            /// Check for updates
            StackPanelUpdateAvailable.Visibility = System.Windows.Visibility.Collapsed;
            var thisVersion = VersionInfo.ReadFromEmbeddedJson();
            var latestVersion = VersionInfo.ReadFromGitHub();
            if (latestVersion != null && !thisVersion.Version.Equals(latestVersion.Version))
            {
                StackPanelUpdateAvailable.Visibility = System.Windows.Visibility.Visible;
                LabelNewVersion.Content = $"Version {latestVersion.Version} available since {latestVersion.VersionReleaseDate:dd.MM.yyyy}";
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void DownloadButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            const string downloadUrl = "https://github.com/Springwald/AnimatronicWorkbench-AWB";
            try
            {
                Process.Start(new ProcessStartInfo(downloadUrl) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                // Handle the exception if the process fails to start
                System.Windows.MessageBox.Show($"Failed to open link: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
