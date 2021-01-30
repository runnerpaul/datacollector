using System;

namespace DataCollector
{
    public class PerformanceData
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public long Difference { get; set; }
        public long StartOffset { get; set; }
        public long EndOffset { get; set; }
        public long StartRate { get; set; }
        public long EndRate { get; set; }
    }
}