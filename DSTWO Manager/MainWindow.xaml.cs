using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

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

        public MainWindow()
        {
            InitializeComponent();

            Connect(@"G:\");
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
                if (!file.EndsWith(".ini")) continue;

                var plug = new Plugin
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    Description = "aah"
                };

                InstalledPlugins.Add(plug);
            }

            InstalledPluginsDataGrid.ItemsSource = null;
            InstalledPluginsDataGrid.ItemsSource = InstalledPlugins;
            return InstalledPlugins;
        }
    }
}
