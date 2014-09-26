using System;

namespace CodeMetricsLoader.Data
{
    public class Date
    {
        public int DateId { get; set; }
        public DateTime DateTime { get; set; }
        public int Year { get; set; }
        public string YearString { get; set; }
        public int Month { get; set; }
        public string MonthString { get; set; }
        public int MonthOfYear { get; set; }
        public int WeekOfYear { get; set; }
        public DateTime DateNoTime { get; set; }
        public int DayOfYear { get; set; }
        public int DayOfMonth { get; set; }
        public int DayOfWeek { get; set; }
    }
}
