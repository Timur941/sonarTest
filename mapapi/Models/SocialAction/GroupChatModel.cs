using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mapapi.Models.SocialAction
{
    public class GroupChatModel
    {
        public string ChatName { get; set; }
        //public int Owner { get; set; }
        public int? TypeId { get; set; }
        public long? EntityId { get; set; }
    }
}
