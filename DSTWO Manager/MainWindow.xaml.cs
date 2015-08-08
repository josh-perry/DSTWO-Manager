using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using Humanizer;
using static System.String;

namespace DSTWO_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string ConnectedDrive;
        public string DstwoPlugDirectory;
        //public List<Plugin> InstalledPlugins;
        public ObservableCollection<Plugin> InstalledPlugins = new ObservableCollection<Plugin>();
        public ObservableCollection<Plugin> UninstalledPlugins = new ObservableCollection<Plugin>();
        public bool IsConnected { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            GetRemovableDrives();
            
            GetRepoPlugins();
        }

        public void GetRemovableDrives()
        {
            var drives = DriveInfo.GetDrives();

            DriveComboBox.Items.Clear();

            foreach (var d in drives)
            {
                if (d.DriveType == DriveType.Removable)
                {
                    DriveComboBox.Items.Add(d);
                }
            }
        }

        public bool Connect(string driveLetter)
        {
            ConnectedDrive = driveLetter;
            DstwoPlugDirectory = Path.Combine(driveLetter, "_dstwoplug");

            if (!Directory.Exists(DstwoPlugDirectory))
            {
                MessageBox.Show("Couldn't find the _dstwoplug directory on root!");
                return false;
            }

            // Clear it just in case
            InstalledPlugins = new ObservableCollection<Plugin>();

            // Get installed plugins
            //InstalledPlugins = GetInstalledPlugins();

            UpdateFreeSpaceBar();
            GetPluginsFromDsTwoPlug(DstwoPlugDirectory);

            return true;
        }

        private void UpdateFreeSpaceBar()
        {
            var driveInfo = new DriveInfo(ConnectedDrive);
            AvailableSpaceProgressBar.Maximum = 100;
            AvailableSpaceProgressBar.Value = 100 - (((float)driveInfo.AvailableFreeSpace / driveInfo.TotalSize) * 100);

            var freeGB = driveInfo.AvailableFreeSpace.Bytes().Gigabytes.ToString("#.#GB");
            var totalGB = driveInfo.TotalSize.Bytes().Gigabytes.ToString("#.#GB");
            AvailableSpaceTextBlock.Text = $"Free space ({freeGB}/{totalGB})";
        }

        public ObservableCollection<Plugin> GetInstalledPlugins()
        {
            foreach (var file in Directory.GetFiles(DstwoPlugDirectory))
            {
                var plugin_file = "";

                if (file.EndsWith(".plg") || file.EndsWith(".nds"))
                    plugin_file = file;
                else
                    continue;
                
                var plug = new Plugin
                {
                    Name = plugin_file,
                    ParentPath = DstwoPlugDirectory,
                    PluginFile = plugin_file
                };

                plugin_file = Path.Combine(DstwoPlugDirectory, Path.GetFileNameWithoutExtension(plugin_file));

                if (File.Exists(plugin_file + ".ini"))
                    plug.IniFile = plugin_file + ".ini";

                plug.BmpFile = plug.GetIconFromIni();

                if (plug.BmpFile == string.Empty)
                    if (File.Exists(plugin_file + ".bmp"))
                        plug.BmpFile = plugin_file + ".bmp";

                InstalledPlugins.Add(plug);
            }

            InstalledPluginsDataGrid.ItemsSource = null;
            InstalledPluginsDataGrid.ItemsSource = InstalledPlugins;
            return InstalledPlugins;
        }

        private void GetRepoPlugins()
        {
            UninstalledPlugins = new ObservableCollection<Plugin>();

            // Local
            // TODO: Seperate boxes instead of splitting by ;?
            AddPlugins(LocalPluginSources.Text.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries));

            // Remote

            //
            InstallPluginsDataGrid.ItemsSource = null;
            InstallPluginsDataGrid.ItemsSource = UninstalledPlugins;
        }

        private void AddPlugins(string[] repoList, bool installed = false)
        {
            foreach (var repo in repoList)
            {
                if (!Directory.Exists(repo))
                    continue;

                foreach (var dir in Directory.GetDirectories(repo))
                {
                    GetPluginsFromDirectory(dir);
                }
            }
        }

        private void GetPluginsFromDsTwoPlug(string dir)
        {
            foreach (var file in new DirectoryInfo(dir).GetFiles())
            {
                var plugin = new Plugin();

                if (!file.Name.EndsWith(".ini", true, CultureInfo.CurrentCulture))
                {
                    continue;
                }

                plugin.IniFile = file.FullName;

                var f = Path.GetFileNameWithoutExtension(file.Name);
                f = Path.Combine(DstwoPlugDirectory, f);

                if (File.Exists(f + ".bmp"))
                    plugin.BmpFile = f + ".bmp";

                if (File.Exists(f + ".plg"))
                    plugin.PluginFile = f + ".plg";
                else if (File.Exists(f + ".nds"))
                    plugin.PluginFile = f + ".nds";

                plugin.Name = Path.GetFileNameWithoutExtension(file.Name);
                plugin.ParentPath = dir;

                InstalledPlugins.Add(plugin);
            }

            InstalledPluginsDataGrid.ItemsSource = null;
            InstalledPluginsDataGrid.ItemsSource = InstalledPlugins;
        }

        private void GetPluginsFromDirectory(string dir)
        {
            var plugin = new Plugin();

            foreach (var file in new DirectoryInfo(dir).GetFiles())
            {
                if (file.Name.EndsWith(".plg", true, CultureInfo.CurrentCulture) ||
                    file.Name.EndsWith(".nds", true, CultureInfo.CurrentCulture))
                {
                    plugin.PluginFile = file.FullName;
                }

                if (file.Name.EndsWith(".ini", true, CultureInfo.CurrentCulture))
                {
                    plugin.IniFile = file.FullName;
                }

                if (file.Name.EndsWith(".bmp", true, CultureInfo.CurrentCulture))
                {
                    plugin.BmpFile = file.FullName;
                }
            }

            if (plugin.BmpFile == string.Empty)
                plugin.BmpFile = plugin.GetIconFromIni();

            plugin.Name = new DirectoryInfo(dir).Name;
            plugin.ParentPath = dir;

            UninstalledPlugins.Add(plugin);
        }

        private void ConnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TabControl.IsEnabled = Connect(DriveComboBox.Text);
            }
            catch (Exception ex)
            {
                TabControl.IsEnabled = false;
                MessageBox.Show(ex.Message);
                return;
            }

            TabControl.IsEnabled = true;
            UpdateFreeSpaceBar();
        }

        private void InstallButton_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (Plugin plugin in InstallPluginsDataGrid.SelectedItems)
            {
                plugin.Install(DstwoPlugDirectory);

                var p = plugin;
                p.ParentPath = DstwoPlugDirectory;
                if (p.BmpFile != null) p.BmpFile = Path.Combine(DstwoPlugDirectory, Path.GetFileName(p.BmpFile));
                if (p.IniFile != null) p.IniFile = Path.Combine(DstwoPlugDirectory, Path.GetFileName(p.IniFile));
                if (p.PluginFile != null) p.PluginFile = Path.Combine(DstwoPlugDirectory, Path.GetFileName(p.PluginFile));
                InstalledPlugins.Add(p);
            }
        }

        private void UninstallButton_OnClick(object sender, RoutedEventArgs e)
        {
            var l = new List<Plugin>();
            foreach (Plugin x in InstalledPluginsDataGrid.SelectedItems)
                l.Add(x);
            
            InstalledPluginsDataGrid.ItemsSource = null;
            
            foreach (Plugin plugin in l)
            {
                plugin.Uninstall();
                InstalledPlugins.Remove(plugin);
            }

            InstalledPluginsDataGrid.ItemsSource = InstalledPlugins;
        }

        private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
        {
            GetRemovableDrives();
        }

        private void BlogButton_OnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            Process.Start(new ProcessStartInfo("http://literallyjosh.com/"));
            routedEventArgs.Handled = true;
        }

        private void GitHubButton_Clicked(object sender, RoutedEventArgs routedEventArgs)
        {
            Process.Start(new ProcessStartInfo("https://github.com/josh-perry/DSTWO-Manager"));
            routedEventArgs.Handled = true;
        }
    }
}
