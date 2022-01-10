using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mapapi.Models
{
    public class Message
    {
        public long IdMessage { get; set; }
        public Guid ChatId { get; set; }
        public int UserId { get; set; }
        public string MessageText { get; set; }
        public int? TypeId { get; set; }
        public long? EntityId { get; set; }
        public long? Replied { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
