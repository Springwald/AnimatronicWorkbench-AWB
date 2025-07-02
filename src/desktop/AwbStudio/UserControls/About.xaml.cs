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
        private readonly IAwbStudioSettingsService _awbStudioSettingsService;

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
            _awbStudioSettingsService = App.GetService<IAwbStudioSettingsService>()
                ?? throw new InvalidOperationException("AwbStudioSettingsService is not registered in the service provider.");
            InitializeComponent();
            Loaded += About_Loaded;
        }

        private void About_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            LabelVersion.Content = VersionLabelInfo;

            // check if dark mode is enabled
            CheckboxDarkMode.IsChecked = _awbStudioSettingsService.StudioSettings.DarkMode;
            (App.Current as AwbStudio.App)!.DarkMode = _awbStudioSettingsService.StudioSettings.DarkMode;

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

        /// <summary>
        /// Changes the dark mode setting when the checkbox is checked or unchecked.
        /// </summary>
        /// <param name="e"></param>
        private async void CheckboxDarkMode_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            var isChecked = CheckboxDarkMode.IsChecked ?? false;
            
            _awbStudioSettingsService.StudioSettings.DarkMode = isChecked;
            await _awbStudioSettingsService.SaveSettingsAsync();

            (App.Current as AwbStudio.App)!.DarkMode = isChecked;
        }
    }
}
