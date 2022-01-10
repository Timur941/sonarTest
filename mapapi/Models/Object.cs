using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Web.Http.Description;
using mapapi.Models;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

#nullable disable

namespace mapapi
{
    //[DataContract(IsReference = true)]
    public partial class Object
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
        public decimal? Price { get; set; }
        public string AgeLimit { get; set; }
        public int TypeId { get; set; }
        public long? PhotoId { get; set; }
        public int UserId { get; set; }
        public float FScore { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }
        public string Vk { get; set; }
        public string Instagram { get; set; }
        public Category Category { get; set; }
        public Photo Photo { get; set; }
        public List<ObjectSchedule> OSchedule { get; set; } //ICollection
    }
}
