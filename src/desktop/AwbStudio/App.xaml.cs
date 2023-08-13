// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using AwbStudio.DependencyInjection;
using AwbStudio.Projects;
using AwbStudio.StudioSettings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Windows;

namespace AwbStudio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider serviceProvider;
        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddILoggerServices();
            services.TryAddSingleton<IAwbStudioSettingsService, AwbStudioSettingsService>();
            services.AddInputControllerServices();
            services.TryAddSingleton<IProjectManagerService, ProjectManagerService>();
            services.TryAddSingleton<DebugWindow>();
            services.TryAddSingleton<ProjectManagementWindow>();
            services.TryAddTransient<TimelineEditorWindow>();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var projectManagementWindow = serviceProvider.GetService<ProjectManagementWindow>();

            if (projectManagementWindow != null)
            {
                projectManagementWindow.Show();
                projectManagementWindow.Closed += (s, args) => Shutdown();
            }
        }

    }
}
