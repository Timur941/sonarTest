using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mapapi.Models.SocialAction
{
    public class Favourite
    {
        public long IdFavourite { get; set; }
        public int TypeId { get; set; }
        public long EntityId { get; set; }
        public int UserId { get; set; }
        public DateTime AddedTime { get; set; }
        public bool Notifications { get; set; }

    }
}
