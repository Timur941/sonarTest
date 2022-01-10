using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mapapi.Models.SocialAction
{
    public class LikeDislike
    {
        public long IdLike { get; set; }
        public int TypeId { get; set; }
        public long EntityId { get; set; }
        public int UserId { get; set; }
        public bool IsLike { get; set; }
        public DateTime PostedDate { get; set; }
    }
}
