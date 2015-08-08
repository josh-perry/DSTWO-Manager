﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using Humanizer;

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

        private void GetRepoPlugins()
        {
            UninstalledPlugins = new ObservableCollection<Plugin>();

            // Local
            // TODO: Seperate boxes instead of splitting by ;?
            foreach (var repo in LocalPluginSources.Text.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!Directory.Exists(repo))
                    continue;

                foreach (var dir in Directory.GetDirectories(repo))
                {
                    var ini = "";
                    var directoryInfo = new DirectoryInfo(dir);
                    var files = new List<string>();

                    foreach (var file in directoryInfo.GetFiles())
                    {
                        files.Add(file.Name);

                        if (!file.Name.EndsWith(".ini", true, CultureInfo.CurrentCulture)) continue;

                        ini = file.FullName;
                    }

                    var plugin = new Plugin();
                    plugin.Name = Path.GetFileNameWithoutExtension(ini);
                    plugin.Path = dir;
                    plugin.Files = files;

                    UninstalledPlugins.Add(plugin);
                }
            }

            // Remote
            
            //
            InstallPluginsDataGrid.ItemsSource = null;
            InstallPluginsDataGrid.ItemsSource = UninstalledPlugins;
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
            InstalledPlugins = GetInstalledPlugins();

            UpdateFreeSpaceBar();

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
                if (!file.EndsWith(".ini", true, CultureInfo.CurrentCulture)) continue;

                var f = Path.GetFileNameWithoutExtension(file);
                var filelist = new List<string> {f + ".ini", f + ".bmp", f + ".nds"};

                var plug = new Plugin
                {
                    Name = f,
                    Path = DstwoPlugDirectory,
                    Files = filelist // TODO: De-un-hardcode this maybe?
                };

                InstalledPlugins.Add(plug);
            }

            InstalledPluginsDataGrid.ItemsSource = null;
            InstalledPluginsDataGrid.ItemsSource = InstalledPlugins;
            return InstalledPlugins;
        }

        private void ConnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TabControl.IsEnabled = Connect(DriveComboBox.Text);
            }
            catch (Exception ex)
            {
                //TabControl.IsEnabled = false;
                //MessageBox.Show(ex.Message);
            }

            TabControl.IsEnabled = true;
            UpdateFreeSpaceBar();
        }

        private void InstallButton_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (Plugin plugin in InstallPluginsDataGrid.SelectedItems)
            {
                var scan_fail = false;
                foreach (var file in plugin.Files)
                {
                    if (File.Exists(Path.Combine(DstwoPlugDirectory, file)))
                    {
                        scan_fail = true;
                        MessageBox.Show($"Plugin: {plugin.Name} is already installed!");
                        break;
                    }
                }

                if (scan_fail)
                    continue;

                foreach (var file in plugin.Files)
                {
                    File.Copy(Path.Combine(plugin.Path, file), Path.Combine(DstwoPlugDirectory, file));
                }

                var p = plugin;
                p.Path = DstwoPlugDirectory;
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
