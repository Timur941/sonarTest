using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using mapapi.Models;
using mapapi.Models.SocialAction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace mapapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatUsersController : ControllerBase
    {
        private readonly GeosocdbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string connString;

        public ChatUsersController(GeosocdbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            connString = Environment.GetEnvironmentVariable("dbconnstring");
        }

        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<ChatUser>>> GetChatUsers()
        //{
        //    return await _context.ChatUsers.Include(c => c.Chat).Include(c => c.User).ToListAsync();
        //}

        [HttpGet]
        [Route("[action]/{chatId}")]
        public async Task<ActionResult<IEnumerable<ChatUser>>> GetUsersInChat(Guid chatId)
        {
            return await _context.ChatUsers.Where(i => i.ChatId == chatId).ToListAsync();
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ChatUser>> AddChatUser(ChatUser chatUser)
        {
            int requestedUserId = Int32.Parse(User.FindFirst("id").Value);
            var chatRoom = await _context.ChatRooms.FindAsync(chatUser.ChatId);
            var addedUser = await _context.Users.FindAsync(chatUser.UserId);
            if (chatRoom == null || addedUser == null)
                return NotFound();
            if (requestedUserId == chatRoom.Owner && !chatRoom.Personal)
            {
                _context.ChatUsers.Add(chatUser);
                await _context.SaveChangesAsync();

                return chatUser;
            }
            else
            {
                return Forbid();
            }
        }
        //[Authorize]
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutChatUser(long id, ChatUser chatUser)
        //{
        //    int userId = Int32.Parse(User.FindFirst("id").Value);
        //    long ownerId = _context.ChatRooms.FirstOrDefault(c => c.IdChat == (_context.ChatUsers.FirstOrDefault(s => s.UserId == )))
        //    if (chatUser.UserId == userId)
        //    {
        //        if (id != chatUser.IdChatUser)
        //        {
        //            return BadRequest();
        //        }

        //        _context.Entry(chatUser).State = EntityState.Modified;

        //        try
        //        {
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!ChatUserExists(id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }

        //        return NoContent();
        //    }
        //    else
        //        return Forbid("No permission");
        //}

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ChatUser>> DeleteChatUser(long id)
        {
            int userId = Int32.Parse(User.FindFirst("id").Value);
            var chatUser = await _context.ChatUsers.FindAsync(id);
            int owner = (await _context.ChatRooms.FindAsync(chatUser.ChatId)).Owner;
            if (chatUser != null)
            {
                if (userId == owner)
                {
                    _context.ChatUsers.Remove(chatUser);
                    await _context.SaveChangesAsync();
                    return chatUser;
                }
                else
                {
                    return Forbid();
                }
            }
            else
            {
                return NotFound();
            }
        }

        //private bool ChatUserExists(long id)
        //{
        //    return _context.ChatUsers.Any(e => e.IdChatUser == id);
        //}
    }
}
