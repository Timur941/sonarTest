using mapapi.Models;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;

#nullable disable

namespace mapapi
{
    //[JsonObject(IsReference = true)]
    public partial class Category
    {
        //public Category()
        //{
        //    Events = new HashSet<Event>();
        //    Objects = new HashSet<Object>();
        //    Places = new HashSet<Place>();
        //}

        public int IdCategory { get; set; }
        public string CategoryName { get; set; }
        public int TypeId { get; set; }
        public int? CategoryClassifierId { get; set; }

        public virtual CategoryClassifier CategoryClassifier { get; set; }

        //public virtual CategoryClassifier CategoryClassifier { get; set; }

        //public virtual ICollection<Event> Events { get; set; }
        //public virtual ICollection<Object> Objects { get; set; }
        //public virtual ICollection<Place> Places { get; set; }
    }
}
