using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mapapi.Models.User
{
    public partial class RefreshToken
    {
        public string TokenStr { get; set; }
        public int UserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Expires { get; set; }
        public DateTime? Revoked { get; set; }
        public string ReplacedBy { get; set; }
    }
}
