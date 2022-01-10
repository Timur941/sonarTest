using mapapi.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace mapapi
{
    public class ChatUser
    {
        public long IdChatUser { get; set; }
        public int UserId { get; set; }
        public Guid ChatId { get; set; }

        [JsonIgnore]
        public virtual Chat Chat { get; set; }
        [JsonIgnore]
        public virtual User User { get; set; }

    }
}
