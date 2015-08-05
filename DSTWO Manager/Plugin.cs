using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Humanizer;

namespace DSTWO_Manager
{
    public class Plugin
    {
        public string Name { get; set; }
        public List<string> Files { get; set; }
        public string Path { get; set; }

        public Uri Icon
        {
            get
            {
                return new Uri(System.IO.Path.Combine(Path, Name + ".bmp"));
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
                        f += new FileInfo(System.IO.Path.Combine(Path, file)).Length;
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
    }
}