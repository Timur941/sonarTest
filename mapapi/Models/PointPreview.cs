using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mapapi
{
    public class PointPreview
    {
        public long IdEntity { get; set; }
        public int? TypeId { get; set; }
        public Geometry Way { get; set; }
        public string Title { get; set; }
        //public string PreviewDescription { get; set; }
        //public string Address { get; set; }
        //public decimal? Price { get; set; }
        public float? Rating { get; set; }
    } 
}
