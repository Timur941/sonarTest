using mapapi.Hubs;
using mapapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace mapapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : Controller
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [Route("send")]
        [Authorize] //path looks like this: https://localhost:44379/api/chat/send
        [HttpPost]
        public IActionResult SendRequest([FromBody] string message)
        {
            string username = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType).Value;
            _hubContext.Clients.All.SendAsync("Receive", message, username);
            return Ok();
        }


        //[Authorize]
        //[HttpPost]
        //public IActionResult JoinRequest([FromBody] string receiver)
        //{
        //    string username = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType).Value;
        //    _hubContext.Clients.All.SendAsync("Receive", message, username);
        //    return Ok();
        //}
    }
}
