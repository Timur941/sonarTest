using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace mapapi.Models.SocialAction
{
    public class ViewStatistics
    {
        public long IdView { get; set; }
        public int TypeId { get; set; }
        public long EntityId { get; set; }
        public DateTime VisitedTime { get; set; }
        //[JsonIgnore]
        [JsonConverter(typeof(IPAddressConverter))]
        public IPAddress Ip { get; set; }
        public string UserAgent { get; set; }
        public int? UserId { get; set; }
    }
}
