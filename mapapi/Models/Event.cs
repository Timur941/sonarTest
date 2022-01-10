using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using mapapi.Models;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;



#nullable disable

namespace mapapi
{
    public partial class Event
    {
        public long IdEntity { get; set; }
        public int? AssociatedEntityType { get; set; }
        public long? AssociatedEntityId { get; set; }
        public string Title { get; set; }
        public string PreviewDescription { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public float Rating { get; set; } //?
        public bool Private { get; set; }
        public DateTime Date { get; set; }
        [JsonConverter(typeof(TimeSpanConverter))]
        public TimeSpan? Duration { get; set; }
        public decimal? Price { get; set; }
        public int TypeId { get; set; }
        public long? PhotoId { get; set; }
        public int UserId { get; set; }
        public float FScore { get; set; }
        public Geometry Way { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }
        public string Vk { get; set; }
        public string Instagram { get; set; }
        public virtual Category Category { get; set; }
    }
}
