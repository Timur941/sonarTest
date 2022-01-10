using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

#nullable disable

namespace mapapi.Models.SocialAction
{
    public partial class Review
    {
        public long IdReview { get; set; }
        public int TypeId { get; set; }
        public long EntityId { get; set; }
        public int UserId { get; set; }
        public int RatingValue { get; set; }
        public string ReviewText { get; set; }
        public DateTime PostedDate { get; set; }
        [NotMapped]
        public int LikesCount { get; set; }
        [NotMapped]
        public int DislikesCount { get; set; }
        public float FScore { get; set; }

    }
}
