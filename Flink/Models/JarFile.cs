using System.Collections;
using System.Collections.Generic;

namespace Flink.Models
{
    public class JarFile
    {
        public string id { get; set; }
        public string name { get; set; }
        public long uploaded { get; set; }
        public ICollection<JarEntry> entry { get; set; }
    }
}