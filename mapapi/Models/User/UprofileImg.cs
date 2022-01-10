using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace mapapi.Models.User
{
    public class UprofileImg
    {
        public int IdUprofileImg { get; set; }
        public int UserId { get; set; }
        public bool IsCurrent { get; set; }
        [JsonIgnore]
        public byte[] OrigImgData { get; set; }
        [JsonIgnore]
        public byte[] ThumbnailImgData { get; set; }
        public DateTime UploadedDate { get; set; }
        public string? Description { get; set; }
        public string ContentType { get; set; }
    }
}
