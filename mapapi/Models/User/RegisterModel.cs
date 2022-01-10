using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace mapapi
{
    public class RegisterModel
    {
        [Required]
        public string Username { get; set; }
        public string Email { get; set; }
        public string Fullname { get; set; }
        [Required]
        public DateTime? BirthDate { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
