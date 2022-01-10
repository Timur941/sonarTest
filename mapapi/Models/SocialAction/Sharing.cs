using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mapapi.Models.SocialAction
{
	public class Sharing
    {
		public long IdSharing { get; set; }
		public int TypeId { get; set; }
		public long EntityId { get; set; }
		public int UserId { get; set; }
		public DateTime ShareTime { get; set; }
		public string? ShareText { get; set; }
    }
}
