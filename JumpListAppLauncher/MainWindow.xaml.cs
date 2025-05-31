using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Windows.Graphics;
using WinRT.Interop;

namespace JumpListAppLauncher
{
    public sealed partial class MainWindow : Window
    {
        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int GetDpiForWindow(IntPtr hwnd);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


        public MainWindow() {
            InitializeComponent();
            SetThemeColors(this);

            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            int dpi = GetDpiForWindow(hWnd);
            float scale = dpi / 96.0f;
            AppWindow.ResizeClient(new SizeInt32((int)(450*scale), (int)(620*scale)));

            List<string> cmdlineArgs = Environment.GetCommandLineArgs().ToList();
            if (cmdlineArgs.Count >= 2 && !string.IsNullOrWhiteSpace(cmdlineArgs[1])) {
                if (LaunchProgram(cmdlineArgs[1])){
                    Environment.Exit(0);
                    return;
                }
            }

            MainFrame.Navigate(typeof(MainPage));
        }

        public bool LaunchProgram(string args) {
            var idx1 = args.IndexOf('|');
            var idx2 = args.LastIndexOf('|');
            if (idx1 > 0 && idx2 > 0 && idx2 > idx1){
                string path = args.Substring(0, idx1);
                string arg = args.Substring(idx1+1, idx2-idx1-1);
                string dir = args.Substring(idx2+1);
                return LaunchProgram(path, arg, dir);
            }
            UnminimizeAndForeground();
            return false;
        }

        public bool LaunchProgram(string path, string arg, string dir) {
            var startInfo = new ProcessStartInfo {
                FileName = path,
                Arguments = arg,
                WorkingDirectory = dir,
                UseShellExecute = path.EndsWith(".exe")?false:true,
            };
            try {
                Process.Start(startInfo);
                return true;
            } catch(Exception ex){
                ShowError(ex.Message);
                return false;
            }
        }

        public void ShowError(string msg){
            UnminimizeAndForeground();
            ErrorBar.Message = msg;
            ErrorBar.IsOpen = true;
        }

        public void UnminimizeAndForeground() {
            const int SW_RESTORE = 9;
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            ShowWindow(hWnd, SW_RESTORE);
            SetForegroundWindow(hWnd);
        }

        private void Grid_ActualThemeChanged(FrameworkElement sender, object args) {
            SetThemeColors(this);
        }

        public static void SetThemeColors(Window window) {
            Application app = Application.Current;
            if (app == null) return;
            var fg = (SolidColorBrush)app.Resources["MyForeground"];
            var bg = (SolidColorBrush) app.Resources["MyBackground"];
            var fgInactive = (SolidColorBrush) app.Resources["MyForegroundDisabled"];
            var bgInactive = (SolidColorBrush) app.Resources["MyBackgroundDisabled"];
            var bgHover = (SolidColorBrush) app.Resources["MyHoverBackground"];
            var bgPressed = (SolidColorBrush) app.Resources["MyPressedBackground"];

            IntPtr hWnd = WindowNative.GetWindowHandle(window);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(wndId);
            if (AppWindowTitleBar.IsCustomizationSupported()) {
                AppWindowTitleBar titleBar = appWindow.TitleBar;
                titleBar.ForegroundColor = fg.Color;
                titleBar.BackgroundColor = bg.Color;
                titleBar.InactiveForegroundColor = fgInactive.Color;
                titleBar.InactiveBackgroundColor = bgInactive.Color;

                titleBar.ButtonForegroundColor = fg.Color;
                titleBar.ButtonBackgroundColor = bg.Color;
                titleBar.ButtonInactiveForegroundColor = fgInactive.Color;
                titleBar.ButtonInactiveBackgroundColor = bgInactive.Color;
                titleBar.ButtonHoverForegroundColor = fg.Color;
                titleBar.ButtonHoverBackgroundColor = bgHover.Color;
                titleBar.ButtonPressedForegroundColor = fg.Color;
                titleBar.ButtonPressedBackgroundColor = bgPressed.Color;
            }
            string iconFileName = "Assets/AppLauncher.ico";
            string iconPath = Path.Combine(AppContext.BaseDirectory, iconFileName);
            appWindow.SetIcon(iconPath);
        }
    }
}
