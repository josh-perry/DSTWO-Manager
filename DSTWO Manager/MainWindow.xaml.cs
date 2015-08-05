﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Navigation;

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
                foreach (var dir in Directory.GetDirectories(repo))
                {
                    var ini = "";
                    long filesize = 0;
                    var directoryInfo = new DirectoryInfo(dir);

                    foreach (var file in directoryInfo.GetFiles())
                    {
                        filesize += file.Length;

                        if (!file.Name.EndsWith(".ini", true, CultureInfo.CurrentCulture)) continue;

                        ini = file.FullName;
                    }

                    var plugin = new Plugin();
                    plugin.Name = Path.GetFileNameWithoutExtension(ini);
                    plugin.Description = dir;
                    plugin.Filesize = filesize;

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
                return false;

            // Clear it just in case
            InstalledPlugins = new ObservableCollection<Plugin>();

            // Get installed plugins
            InstalledPlugins = GetInstalledPlugins();

            return true;
        }

        public ObservableCollection<Plugin> GetInstalledPlugins()
        {
            foreach (var file in Directory.GetFiles(DstwoPlugDirectory))
            {
                if (!file.EndsWith(".ini", true, CultureInfo.CurrentCulture)) continue;

                var plug = new Plugin
                {
                    Name = Path.GetFileNameWithoutExtension(file)
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
                TabControl.IsEnabled = false;
                MessageBox.Show(ex.Message);
            }
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.ToString()));
            e.Handled = true;
        }
    }
}
