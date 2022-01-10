using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

#nullable disable

namespace mapapi.Models
{
    public class Photo
    {
        public long IdPhoto{ get; set; }
        public int? TypeId { get; set; }
        public long? EntityId { get; set; }
        public int UserId { get; set; }
        [JsonIgnore]
        public byte[] OrigImgData { get; set; }
        [JsonIgnore]
        public byte[] ThumbnailImgData { get; set; }
        public string? Title { get; set; }
        public DateTime PostedDate { get; set; }
        public string? Description { get; set; }
        public string ContentType { get; set; }
        public float FScore { get; set; }
    }
}
