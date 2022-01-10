using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace mapapi.Models
{
    public class Chat
    {
        public Guid IdChat { get; set; }
        public DateTime CreationTime { get; set; }
        public string ChatName { get; set; }
        public int? TypeId { get; set; }
        public long? EntityId { get; set; }
        public int Owner { get; set; }
        public bool Personal { get; set; }
        public List<ChatUser> ChatUsers { get; set; }

        [NotMapped]
        public virtual Message LastMessage { get; set; }
        [JsonIgnore]
        [NotMapped]
        public DateTime LastActivity { get; set; }

    }
}
