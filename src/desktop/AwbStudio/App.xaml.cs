// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

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
        private static ServiceProvider? _serviceProvider;

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
            SetupUnhandledExceptionHandling();
        }

        /// <summary>
        /// Gets registered service.
        /// </summary>
        /// <typeparam name="T">Type of the service to get.</typeparam>
        /// <returns>Instance of the service or <see langword="null"/>.</returns>
        public static T GetService<T>()
            where T : class
        {
            if (_serviceProvider == null) throw new Exception("Service provider not ready.");
            var service = _serviceProvider.GetService(typeof(T)) as T;
            return service!;
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddILoggerServices();
            services.TryAddSingleton<IInvokerService, WpfAppInvokerService>();
            services.TryAddSingleton<IAwbStudioSettingsService, AwbStudioSettingsService>();
            services.AddPropertyEditorVirtualInputControllerService();
            services.AddInputControllerServices();
            services.TryAddSingleton<IProjectManagerService, ProjectManagerService>();

            // add the AwbClientsService as a singleton of IAwbClientsService and call the InitAsync method when initializing the service
            services.AddAwbClientService();

            services.TryAddTransient<AwbClientsWindow>();
            services.TryAddTransient<DebugWindow>();
            services.TryAddTransient<ProjectManagementWindow>();
            services.TryAddTransient<ProjectConfigurationWindow>();
            services.TryAddTransient<TimelineEditorWindow>();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var projectManagementWindow = _serviceProvider!.GetService<ProjectManagementWindow>();
            if (projectManagementWindow != null)
            {
                projectManagementWindow.Show();
                projectManagementWindow.Closed += (s, args) => Shutdown();
            }

            var awbClientWindow = _serviceProvider!.GetService<AwbClientsWindow>();
            if (awbClientWindow != null)
            {
                awbClientWindow.Show();
                // awbClientWindow.Closed += (s, args) => Shutdown();
            }
        }

        private void SetupUnhandledExceptionHandling()
        {
            // Catch exceptions from all threads in the AppDomain.
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                ShowUnhandledException(args.ExceptionObject as Exception ?? new Exception(args.ExceptionObject?.ToString()), "AppDomain.CurrentDomain.UnhandledException", false);

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
        }

        static void ShowUnhandledException(Exception e, string unhandledExceptionType, bool promptUserForShutdown)
        {
            var messageBoxTitle = $"Unexpected Error Occurred: {unhandledExceptionType}";
            var messageBoxMessage = $"The following exception occurred and has been copied into the clipboard:\n\n{e}";
            var messageBoxButtons = MessageBoxButton.OK;

            // copy the exception message to the clipboard
            _errorMessages += "\r\n--------------------------------------------------\r\n" + e.ToString();
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
