using System.Collections.Generic;
using System.IO;
using Humanizer;

namespace DSTWO_Manager
{
    public class Plugin
    {
        public string Name { get; set; }
        public List<string> Files { get; set; }
        public string Path { get; set; }

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