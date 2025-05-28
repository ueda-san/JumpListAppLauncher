using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.Activation;
using LaunchActivatedEventArgs = Windows.ApplicationModel.Activation.LaunchActivatedEventArgs;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace JumpListAppLauncher
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static MainWindow? MainWindow;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            var mainInstance = AppInstance.FindOrRegisterForKey("main");
            if (!mainInstance.IsCurrent) {
                // Redirect the activation (and args) to the "main" instance, and exit.
                var activatedEventArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
                await mainInstance.RedirectActivationToAsync(activatedEventArgs);
                System.Diagnostics.Process.GetCurrentProcess().Kill();
                return;
            }

            mainInstance.Activated += OnAppActivated;

            MainWindow = new();
            MainWindow.Activate();
        }

        private void OnAppActivated(object? sender, AppActivationArguments e){
            if (e.Kind == ExtendedActivationKind.Launch) {
                var launchArgs = e.Data as LaunchActivatedEventArgs;
                if (launchArgs != null && MainWindow != null){
                    MainWindow.DispatcherQueue.TryEnqueue(() => {
                        if (MainWindow.LaunchProgram(launchArgs.Arguments.Trim('"'))){
                            return;
                        }
                    });
                }
            }
        }
    }
}
