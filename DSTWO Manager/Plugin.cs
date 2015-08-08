using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Windows;
using Humanizer;

namespace DSTWO_Manager
{
    public class Plugin
    {
        public string Name { get; set; }
        //public List<string> Files { get; set; }
        public string ParentPath { get; set; }

        public string PluginFile { get; set; }
        public string IniFile { get; set; }
        public string BmpFile { get; set; }

        public Uri Icon
        {
            get
            {
                var original = BmpFile;

                if (!File.Exists(original))
                    return null;

                var copy = Path.Combine("icon_cache/", Name + ".bmp");

                if (!Directory.Exists("icon_cache"))
                    Directory.CreateDirectory("icon_cache");

                if (!File.Exists(copy))
                    File.Copy(original, copy);

                return new Uri(Path.GetFullPath(copy));
            }
            set { }
        }

        public long Filesize
        {
            get
            {
                long f = 0;

                foreach (var file in new List<string> { PluginFile, IniFile, BmpFile})
                {
                    try
                    {
                        f += new FileInfo(Path.Combine(ParentPath, file)).Length;
                    }
                    catch
                    {
                        f += 0;
                    }
                }

                return f;
            }
            set { }
        }

        public string HumanFilesize
        {
            get { return Filesize.Bytes().Kilobytes.ToString("#.#KB"); }
            set { }
        }

        public string GetIconFromIni()
        {
            if (IniFile == null)
                return String.Empty;

            var filename = string.Empty;

            foreach (var line in File.ReadAllLines(IniFile))
            {
                if (!line.StartsWith("icon=")) continue;

                var splitLine = line.Split(new [] {"icon="}, StringSplitOptions.RemoveEmptyEntries);
                splitLine = splitLine[0].Split(new [] {@"dstwoplug/"}, StringSplitOptions.RemoveEmptyEntries);

                return Path.Combine(ParentPath, splitLine.Last());
            }

            return filename;
        }

        public void Install(string installDirectory)
        {
            if (PluginFile == null)
            {
                MessageBox.Show($"{Name} has no ini file! TODO: Autogenerate an alternative.");
                return;
            }
            else if (File.Exists(Path.Combine(installDirectory, new FileInfo(PluginFile).Name)))
            {
                MessageBox.Show($"Plugin: {Name} is already installed!");
                return;
            }

            try
            {
                File.Copy(BmpFile, Path.Combine(installDirectory, new FileInfo(BmpFile).Name));
            }
            catch
            {}

            try
            {
                File.Copy(IniFile, Path.Combine(installDirectory, new FileInfo(IniFile).Name));
            }
            catch
            {}

            try
            {
                File.Copy(PluginFile, Path.Combine(installDirectory, new FileInfo(PluginFile).Name));
            }
            catch
            {}
        }

        public void Uninstall()
        {
            foreach (var file in new List<string> { PluginFile, IniFile, BmpFile })
            {
                try
                {
                    File.Delete(Path.Combine(ParentPath, file));
                }
                catch {}
            }
        }
    }
}