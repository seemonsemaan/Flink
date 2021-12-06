using System.Collections.Generic;

namespace Flink.Models
{
    public class IndexResult
    {
        public ICollection<JarFile> JarFiles { get; set; }
        public ICollection<Job> Jobs { get; set; }
    }
}