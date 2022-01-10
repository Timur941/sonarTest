using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mapapi.Models.SocialAction
{
    public partial class Ucomment
    {
        public long IdComment { get; set; }
        public int TypeId { get; set; }
        public long EntityId { get; set; }
        public int UserId { get; set; }
        public DateTime PostedDate { get; set; }
        public long? ParentId { get; set; }
        public string CommentText { get; set; }
    }
}
