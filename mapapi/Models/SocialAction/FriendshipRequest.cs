using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mapapi.Models.SocialAction
{
    public class FriendshipRequest
    {
        public int IdFriendship { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public short Status { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
