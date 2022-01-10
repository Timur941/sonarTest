using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mapapi.Models
{
    public class City
    {
        public int IdCity { get; set; }
        public string Name { get; set; }
        public string UrlName { get; set; }
        public Geometry Way { get; set; }
    }
}
