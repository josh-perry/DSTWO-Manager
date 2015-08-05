using System.Collections.Generic;

namespace DSTWO_Manager
{
    public class Plugin
    {
        public string Name { get; set; }
        public List<string> Files { get; set; }
        public string Description { get; set; }
        public int Filesize { get; set; }
    }
}