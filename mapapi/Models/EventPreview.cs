using mapapi.Models;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace mapapi
{
    public class EventPreview
    {
        public long IdEntity { get; set; }
        public int TypeId { get; set; }
        public Geometry Way { get; set; }
        public string Title { get; set; }
        public float? Rating { get; set; }
        public virtual Category Category { get; set; }
        [JsonIgnore]
        public virtual CategoryClassifier CategoryClassifier { get; set; }
        public DateTime? Date { get; set; }
        [JsonConverter(typeof(TimeSpanConverter))]
        public TimeSpan? Duration { get; set; }
        public int IconRadius { get; set; }
    }
}
