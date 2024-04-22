// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Services;
using Awb.Core.Tools;
using AwbStudio.DependencyInjection;
using AwbStudio.Projects;
using AwbStudio.StudioSettings;
using AwbStudio.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
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
            services.TryAddSingleton<IInvokerService, WpfAppInvokerService>();
            services.TryAddSingleton<IAwbStudioSettingsService, AwbStudioSettingsService>();
            services.AddPropertyEditorVirtualInputControllerService();
            services.AddInputControllerServices();
            services.TryAddSingleton<IProjectManagerService, ProjectManagerService>();
            services.TryAddTransient<IAwbClientsService, AwbClientsService>();
            services.TryAddTransient<DebugWindow>();
            services.TryAddTransient<ProjectManagementWindow>();
            services.TryAddTransient<ProjectConfigurationWindow>();
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
