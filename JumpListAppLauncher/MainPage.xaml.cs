using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.UI.StartScreen;

namespace JumpListAppLauncher
{
    public sealed partial class MainPage : Page
    {
        private const string SaveFileName = "SaveData.json";
        public ObservableCollection<ProgramItem> Programs { get; set; } = new();

        readonly ResourceLoader resourceLoader = new();
        bool showFYI = false;

        public MainPage() {
            InitializeComponent();
            LoadItems();
        }

        //------------------------------------------------------------------------------
        private void Item_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e) {
            ProgramItem? item = ((FrameworkElement)sender).DataContext as ProgramItem;
            if (item != null && !string.IsNullOrEmpty(item.ExecutablePath) && App.MainWindow != null){
                App.MainWindow.LaunchProgram(item.ExecutablePath, item.Arguments, item.WorkingDir);
            }
        }

        private async void EditItem_Click(object sender, RoutedEventArgs e) {
            ProgramItem? item = ((FrameworkElement)sender).DataContext as ProgramItem;
            if (item == null) return;

            int idx = Programs.IndexOf(item);
            if (idx >= 0){
                ContentDialog dialog = new () {
                    XamlRoot = this.Content.XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = resourceLoader.GetString("Dialog_TitleEdit"),
                    PrimaryButtonText = resourceLoader.GetString("Button_OK"),
                    CloseButtonText = resourceLoader.GetString("Button_Cancel"),
                };

                var content = new AddItemContent(item.Name, item.ExecutablePath, item.Arguments, item.WorkingDir);
                dialog.Content = content;
                dialog.RequestedTheme = Enum.Parse<ElementTheme>(App.Current.RequestedTheme.ToString());

                string oldPath = item.ExecutablePath;
                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary) {
                    Programs.RemoveAt(idx);
                    item.Name = content.DisplayName;
                    item.ExecutablePath = content.ExecutablePath;
                    item.Arguments = content.Arguments;
                    item.WorkingDir = content.WorkingDir;
                    if (item.ExecutablePath != oldPath){
                        await item.LoadIcon();
                    }
                    Programs.Insert(idx, item);
                }
            }
        }

        private async void DeleteItem_Click(object sender, RoutedEventArgs e) {
            ProgramItem? item = ((FrameworkElement)sender).DataContext as ProgramItem;
            if (item == null) return;

            ContentDialog dialog = new() {
                XamlRoot = this.Content.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                PrimaryButtonText = resourceLoader.GetString("Button_Remove"),
                CloseButtonText = resourceLoader.GetString("Button_Cancel"),
            };
            if (string.IsNullOrEmpty(item.ExecutablePath)){
                dialog.Title = resourceLoader.GetString("Remove_Separator");
            }else{
                dialog.Title = string.Format(resourceLoader.GetString("Remove_Item"),item.Name);
            }
            dialog.RequestedTheme = Enum.Parse<ElementTheme>(App.Current.RequestedTheme.ToString());

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary) {
                Programs.Remove(item);
            }
        }

        private async void AddItem_Click(object sender, RoutedEventArgs e) {
            ContentDialog dialog = new () {
                XamlRoot = this.Content.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = resourceLoader.GetString("Dialog_TitleAdd"),
                PrimaryButtonText = resourceLoader.GetString("Button_Add"),
                IsPrimaryButtonEnabled = false,
                CloseButtonText = resourceLoader.GetString("Button_Cancel"),
            };
            var content = new AddItemContent();
            dialog.Content = content;
            dialog.RequestedTheme = Enum.Parse<ElementTheme>(App.Current.RequestedTheme.ToString());

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary) {
                var item = new ProgramItem(content.DisplayName, content.ExecutablePath, content.Arguments, content.WorkingDir);
                await item.LoadIcon();
                Programs.Add(item);
                if (Programs.Count > 10 && !showFYI){
                    FYI.IsOpen = true;
                    showFYI = true;
                }
            }
        }

        private void AddSeparator_Click(object sender, RoutedEventArgs e) {
            Programs.Add(new ProgramItem());
        }

        private async void Apply_Click(object sender, RoutedEventArgs e) {
            SaveItems(Programs);
            await UpdateJumpList();
        }


        //------------------------------------------------------------------------------
        private async System.Threading.Tasks.Task UpdateJumpList()
        {
            if (!JumpList.IsSupported()) return;

            JumpList jumpList = await JumpList.LoadCurrentAsync();
            jumpList.Items.Clear();

            foreach (var prog in Programs) {
                JumpListItem item;
                if (string.IsNullOrEmpty(prog.ExecutablePath)){
                    item = JumpListItem.CreateSeparator();
                }else{
                    var arg = "\""+prog.ExecutablePath+"|"+prog.Arguments+"|"+prog.WorkingDir+"\"";
                    item = JumpListItem.CreateWithArguments(arg, prog.Name);
                    string cacheFileName = prog.GetCacheFileName();
                    item.Logo = new Uri("ms-appdata:///local/"+cacheFileName);
                }
                jumpList.Items.Add(item);
            }
            await jumpList.SaveAsync();
        }

        async void LoadItems(){
            var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, SaveFileName);
            bool needUpdate = false;
            if (!File.Exists(path)){
                MakePreset();
                needUpdate = true;
            }

            var json = File.ReadAllText(path);
            var loaded = JsonSerializer.Deserialize(json, ProgramItemContext.Default.ObservableCollectionProgramItem);
            if (loaded != null) {
                List<Task> loading = new();
                foreach (var item in loaded){
                    loading.Add(item.LoadIcon());
                }
                await Task.WhenAll(loading);
                foreach (var item in loaded){
                    Programs.Add(item);
                }
                if (needUpdate) await UpdateJumpList();
            }
        }

        static void SaveItems(ObservableCollection<ProgramItem> data){
            var json = JsonSerializer.Serialize(data, ProgramItemContext.Default.ObservableCollectionProgramItem);
            File.WriteAllText(Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, SaveFileName), json);
        }


        // make usage example
        static void MakePreset(){
            const string url="https://github.com/ueda-san/JumpListAppLauncher";

            ObservableCollection<ProgramItem> oc = new();
            oc.Add(new ProgramItem("Calculator",
                                   @"C:\Windows\System32\calc.exe"));
            oc.Add(new ProgramItem("Windows Update",
                                   @"C:\Windows\System32\control.exe",
                                   @"/name Microsoft.WindowsUpdate"));
            oc.Add(new ProgramItem("Open Web Page (Edge)",
                                   @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                                   url));
            if (File.Exists(@"C:\Program Files\Mozilla Firefox\firefox.exe")){
                oc.Add(new ProgramItem("Open Web Page (Firefox)",
                                       @"C:\Program Files\Mozilla Firefox\firefox.exe",
                                       url));
            }else if (File.Exists(@"C:\Program Files (x86)\Mozilla Firefox\firefox.exe")){
                oc.Add(new ProgramItem("Open Web Page (Firefox)",
                                       @"C:\Program Files (x86)\Mozilla Firefox\firefox.exe",
                                       url));
            }
            if (File.Exists(@"C:\Program Files\Google\Chrome\Application\chrome.exe")){
                oc.Add(new ProgramItem("Open Web Page (Chrome)",
                                       @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                                       url));
            }else if (File.Exists(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe")){
                oc.Add(new ProgramItem("Open Web Page (Chrome)",
                                       @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                                       url));
            }
            oc.Add(new ProgramItem());
            oc.Add(new ProgramItem("Copy text to clipboard",
                                   @"cmd.exe",
                                   @"/c echo Hello World | clip.exe"));
            oc.Add(new ProgramItem("Open app's SaveData Folder",
                                   @"C:\Windows\explorer.exe",
                                   Windows.Storage.ApplicationData.Current.LocalFolder.Path));
            SaveItems(oc);
        }
    }

    //==============================================================================
    public class ProgramItem
    {
        public string Name { get; set; }
        public string ExecutablePath { get; set; }
        public string Arguments { get; set; }
        public string WorkingDir { get; set; }

        public BitmapImage? IconImage = null;

        public ProgramItem() { // for separator
            Name = "";
            ExecutablePath = "";
            Arguments = "";
            WorkingDir = "";
        }

        public ProgramItem(string name, string path, string args="", string dir="") {
            Name = name;
            ExecutablePath = path;
            Arguments = args;
            WorkingDir = dir;
        }

        public string GetCacheFileName() {
            byte[] hashBytes = System.Security.Cryptography.SHA1.HashData(System.Text.Encoding.UTF8.GetBytes(ExecutablePath));
            string hash = Convert.ToHexString(hashBytes);
            return $"IconCache_{hash}.png";
        }

        public async Task LoadIcon(){
            if (string.IsNullOrEmpty(ExecutablePath)) return;
            string cacheFileName = GetCacheFileName();
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            try {
                var cacheFile = await localFolder.GetFileAsync(cacheFileName);
                using var stream = await cacheFile.OpenReadAsync();
                IconImage = new BitmapImage();
                var _ = IconImage.SetSourceAsync(stream);
                return;
            } catch(FileNotFoundException) {
                Debug.WriteLine($"no cache. create for {Name}({ExecutablePath})");
            } catch(Exception ex) {
                Debug.WriteLine($"cache load failed: {ex.Message}");
             }

            Icon? icon = null;
            try {
                icon = Icon.ExtractAssociatedIcon(ExecutablePath);
                if (icon != null) {
                    using var bmp = icon.ToBitmap();
                    using var ms = new MemoryStream();
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    var cacheFile = await localFolder.CreateFileAsync(cacheFileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);
                    using (var fs = await cacheFile.OpenStreamForWriteAsync()) {
                        ms.CopyTo(fs);
                    }
                    ms.Position = 0;
                    IconImage = new BitmapImage();
                    var _ = IconImage.SetSourceAsync(ms.AsRandomAccessStream());
                    return;
                }
            } catch(Exception ex) {
                Debug.WriteLine($"Icon extraction failed: {ex.Message}");
                return;
            } finally {
                icon?.Dispose();
            }
        }
    }

    //==============================================================================
    public partial class ItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Normal { get; set; } = null!;
        public DataTemplate Separator { get; set; } = null!;

        protected override DataTemplate SelectTemplateCore(object obj)
        {
            if (obj is not ProgramItem item) return Separator;
            if (string.IsNullOrEmpty(item.Name)){
                return Separator;
            }else{
                return Normal;
            }
        }
    }

    //==============================================================================
    [JsonSerializable(typeof(ObservableCollection<ProgramItem>))]
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    internal partial class ProgramItemContext : JsonSerializerContext
    {
    }

}
