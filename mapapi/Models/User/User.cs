using mapapi.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace mapapi
{
    public class User
    {
        public static object Claims { get; internal set; }
        public int IdUser { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        [JsonIgnore]
        public byte[] Password { get; set; }
        [JsonIgnore]
        public byte[] Salt { get; set; }
        //[JsonIgnore]
        public string Role { get; set; }
        //[JsonIgnore]
        public float ActivityCoin { get; set; }
        public string Fullname { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? BirthDate { get; set; }
        [JsonIgnore]
        public ICollection<ChatConnection> Connections { get; set; }
        [JsonIgnore]
        public List<RefreshToken> RefreshTokens { get; set; }

    }
}
