using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace JumpListAppLauncher
{
    public sealed partial class AddItemContent : Page
    {
        public string DisplayName { get { return ItemName.Text; } }
        public string ExecutablePath { get { return ItemPath.Text.Replace("\"",""); } }
        public string Arguments { get { return ItemArgs.Text; } }
        public string WorkingDir { get { return ItemDir.Text; } }

        public AddItemContent() {
            InitializeComponent();
        }

        public AddItemContent(string name, string path, string args="", string dir="") {
            InitializeComponent();
            ItemName.Text = name;
            ItemPath.Text = path;
            ItemArgs.Text = args;
            ItemDir.Text = dir;
        }

        private async void FilePicker_Click(object sender, RoutedEventArgs e) {
            var senderButton = sender as Button;
            if (senderButton == null) return;
            senderButton.IsEnabled = false;

            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            var window = App.MainWindow;
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.FileTypeFilter.Add(".exe");
            var file = await openPicker.PickSingleFileAsync();
            if (file != null) {
                ItemPath.Text = file.Path;
            }

            senderButton.IsEnabled = true;
        }

        private void ItemPath_DragOver(object sender, DragEventArgs e) {
            if (e.DataView.Contains(StandardDataFormats.StorageItems)){
                e.AcceptedOperation = DataPackageOperation.Copy;
            }else{
                e.AcceptedOperation = DataPackageOperation.None;
            }
        }
        private async void ItemPath_Drop(object sender, DragEventArgs e) {
            if (e.DataView.Contains(StandardDataFormats.StorageItems)) {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0) {
                    if (items[0] is StorageFile file) {
                        ItemPath.Text = file.Path;
                    }
                }
            }
        }

        private async void DirPicker_Click(object sender, RoutedEventArgs e) {
            var senderButton = sender as Button;
            if (senderButton == null) return;
            senderButton.IsEnabled = false;

            var openPicker = new Windows.Storage.Pickers.FolderPicker();
            var window = App.MainWindow;
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.FileTypeFilter.Add("*");
            var file = await openPicker.PickSingleFolderAsync();
            if (file != null) {
                ItemDir.Text = file.Path;
            }

            senderButton.IsEnabled = true;
        }

        private void ItemDir_DragOver(object sender, DragEventArgs e) {
            if (e.DataView.Contains(StandardDataFormats.StorageItems)){
                e.AcceptedOperation = DataPackageOperation.Copy;
            }else{
                e.AcceptedOperation = DataPackageOperation.None;
            }
        }
        private async void ItemDir_Drop(object sender, DragEventArgs e) {
            if (e.DataView.Contains(StandardDataFormats.StorageItems)) {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0) {
                    if (items[0] is StorageFolder folder) {
                        ItemDir.Text = folder.Path;
                    }
                }
            }
        }

        private void ItemText_Validate(object sender, TextChangedEventArgs e) {
            ContentDialog dialog = (ContentDialog)Parent;
            if (dialog == null) return;
            ItemPath.Text = ItemPath.Text.Trim('"');

            if (string.IsNullOrWhiteSpace(ItemName.Text)||
                string.IsNullOrWhiteSpace(ItemPath.Text)||
                !File.Exists(ItemPath.Text)){
                dialog.IsPrimaryButtonEnabled = false;
            }else{
                dialog.IsPrimaryButtonEnabled = true;
            }
        }

    }
}
