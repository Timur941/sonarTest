using System;
using System.Collections.Generic;
using mapapi.Models;
using NetTopologySuite.Geometries;

#nullable disable

namespace mapapi
{
    public partial class Place
    {
        public long IdEntity { get; set; }
        public Geometry Way { get; set; }
        public string Title { get; set; }
        public string PreviewDescription { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public float Rating { get; set; }
        public bool Private { get; set; }
        public string Address { get; set; }
        public int TypeId { get; set; }
        public long? PhotoId { get; set; }
        public int UserId { get; set; }
        public float FScore { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }
        public string Vk { get; set; }
        public string Instagram { get; set; }

        public virtual Category Category { get; set; }
        public virtual Photo Photo { get; set; }
    }
}
