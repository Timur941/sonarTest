using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace mapapi.Models
{
    public class ObjectSchedule
    {
        public long ObjectId { get; set; }
        public short WeekDayNum { get; set; }
        public bool IsWeekend { get; set; }
        [JsonConverter(typeof(TimeSpanConverter))]
        public TimeSpan StartTime { get; set; }
        [JsonConverter(typeof(TimeSpanConverter))]
        public TimeSpan EndTime { get; set; }
    }
}
