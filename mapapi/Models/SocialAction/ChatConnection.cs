using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mapapi
{
    public class ChatConnection
    {
        public string IdConnection { get; set; }
        public int UserId { get; set; }
        public string UserAgent { get; set; }
        public bool Connected { get; set; }
    }
}
