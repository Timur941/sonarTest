using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mapapi
{
    public class RoutePoint
    {
        public long IdRoutePoint { get; set; }
        public long RouteId { get; set; }
        public Geometry Way { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        //public virtual Route Route { get; set; }
    }
}
