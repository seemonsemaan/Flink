using System.Collections;
using System.Collections.Generic;

namespace Flink.Models
{
    public class JarsResult
    {
        public string address { get; set; }
        public ICollection<JarFile> files { get; set; }
    }
}