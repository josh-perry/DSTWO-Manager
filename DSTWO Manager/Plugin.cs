using System;
using System.Collections.Generic;
using System.IO;
using Humanizer;

namespace DSTWO_Manager
{
    public class Plugin
    {
        public string Name { get; set; }
        public List<string> Files { get; set; }
        public string ParentPath { get; set; }

        public Uri Icon
        {
            get
            {
                var p = Path.Combine("icon_cache/", Name + ".bmp");

                if (!Directory.Exists("icon_cache"))
                    Directory.CreateDirectory("icon_cache");

                if (!File.Exists(p))
                    File.Copy(Path.Combine(ParentPath, Name + ".bmp"), p);

                return new Uri(Path.GetFullPath(p));
            }
            set { }
        }

        public long Filesize
        {
            get
            {
                long f = 0;

                foreach (var file in Files)
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

        public void Uninstall()
        {
            foreach (var file in Files)
            {
                File.Delete(Path.Combine(ParentPath, file));
            }
        }
    }
}