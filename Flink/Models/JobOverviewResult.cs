using System.Collections.Generic;

namespace Flink.Models
{
    public class JobOverviewResult
    {
        public ICollection<Job> jobs { get; set; }
    }
}