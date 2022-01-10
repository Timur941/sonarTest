using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mapapi
{
    public class Route
    {
        public long IdEntity { get; set; }
        public Geometry Way { get; set; }
        public string Title { get; set; }
        public string PreviewDescription { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public float Rating { get; set; }
        public bool Private { get; set; }
        public TimeSpan? Duration { get; set; }
        public decimal? Price { get; set; }
        public string AgeLimit { get; set; }
        public float? Distance { get; set; }
        public int TypeId { get; set; }
        public int UserId { get; set; }
        public float FScore { get; set; }

        public virtual ICollection<RoutePoint> RoutePoints { get; set; }
    }
}
