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
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace AwbStudio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static string _errorMessages = string.Empty;

        private ServiceProvider serviceProvider;
        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
            SetupUnhandledExceptionHandling();
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

        private void SetupUnhandledExceptionHandling()
        {
            // Catch exceptions from all threads in the AppDomain.
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                ShowUnhandledException(args.ExceptionObject as Exception, "AppDomain.CurrentDomain.UnhandledException", false);

            // Catch exceptions from each AppDomain that uses a task scheduler for async operations.
            TaskScheduler.UnobservedTaskException += (sender, args) =>
                ShowUnhandledException(args.Exception, "TaskScheduler.UnobservedTaskException", false);

            // Catch exceptions from a single specific UI dispatcher thread.
            Dispatcher.UnhandledException += (sender, args) =>
            {
                // If we are debugging, let Visual Studio handle the exception and take us to the code that threw it.
                if (!Debugger.IsAttached)
                {
                    args.Handled = true;
                    ShowUnhandledException(args.Exception, "Dispatcher.UnhandledException", true);
                }
            };

            // Catch exceptions from the main UI dispatcher thread.
            // Typically we only need to catch this OR the Dispatcher.UnhandledException.
            // Handling both can result in the exception getting handled twice.
            //Application.Current.DispatcherUnhandledException += (sender, args) =>
            //{
            //	// If we are debugging, let Visual Studio handle the exception and take us to the code that threw it.
            //	if (!Debugger.IsAttached)
            //	{
            //		args.Handled = true;
            //		ShowUnhandledException(args.Exception, "Application.Current.DispatcherUnhandledException", true);
            //	}
            //};
        }

        void ShowUnhandledException(Exception e, string unhandledExceptionType, bool promptUserForShutdown)
        {
            var messageBoxTitle = $"Unexpected Error Occurred: {unhandledExceptionType}";
            var messageBoxMessage = $"The following exception occurred and has been copied into the clipboard:\n\n{e}";
            var messageBoxButtons = MessageBoxButton.OK;

            // copy the exception message to the clipboard
            _errorMessages+= "\r\n--------------------------------------------------\r\n" +  e.ToString(); 
            Clipboard.SetText(_errorMessages);

            if (promptUserForShutdown)
            {
                messageBoxMessage += "\n\nNormally the app would die now. Should we let it die?";
                messageBoxButtons = MessageBoxButton.YesNo;
            }

            // Let the user decide if the app should die or not (if applicable).
            if (MessageBox.Show(messageBoxMessage, messageBoxTitle, messageBoxButtons) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

    }
}
