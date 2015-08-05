using System.Collections.Generic;
using Humanizer;

namespace DSTWO_Manager
{
    public class Plugin
    {
        public string Name { get; set; }
        public List<string> Files { get; set; }
        public string Description { get; set; }
        public long Filesize { get; set; }

        public string HumanFilesize
        {
            get { return Filesize.Bytes().Kilobytes.ToString("#.#KB"); }
            set { }
        }
    }
}