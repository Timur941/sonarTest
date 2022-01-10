using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ChatRoomsController : ControllerBase
    {
        private readonly GeosocdbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string connString;

        public ChatRoomsController(GeosocdbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            connString = Environment.GetEnvironmentVariable("dbconnstring");
        }

        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Chat>>> GetChatRooms()
        //{
        //    var chatRooms = await _context.ChatRooms.ToListAsync(); //.Include(c => c.ChatUsers)
        //    foreach (Chat room in chatRooms)
        //    {
        //        var last_message = await _context.Messages.Where(s => s.ChatId == room.IdChat).OrderBy(s => s.IdMessage).LastOrDefaultAsync();
        //        var chatUsers = await _context.ChatUsers.Where(s => s.ChatId == room.IdChat).Include(c => c.User).ToListAsync();
        //        foreach (ChatUser user in chatUsers)
        //        {
        //            room.ChatUsers.Add(user);
        //        }
        //    }
        //    return chatRooms;
        //}

        //[HttpGet("{id}")]
        //public async Task<ActionResult<Chat>> GetChatRoom(long id)
        //{
        //    //var @object = await _context.Objects.FindAsync(id);
        //    var chatRooms = await _context.ChatRooms.FirstOrDefaultAsync(i => i.IdChat == id);

        //    if (chatRooms == null)
        //    {
        //        return NotFound();
        //    }

        //    return chatRooms;
        //}

        //[Authorize]
        //[HttpPost]
        //public async Task<ActionResult<Chat>> PostChatRoom(Chat chatRoom)
        //{
        //    int userId = Int32.Parse(User.FindFirst("id").Value);
        //    chatRoom.Owner = userId;
        //    chatRoom.CreationTime = DateTime.UtcNow;
        //    //@object.UserId = userId;

        //    _context.ChatRooms.Add(chatRoom);
        //    await _context.SaveChangesAsync();

        //    ChatUser chatUser = new ChatUser { UserId = userId, ChatId = chatRoom.IdChat };

        //    _context.ChatUsers.Add(chatUser);
        //    await _context.SaveChangesAsync();

        //    //return await _context.Objects.Include(m => m.Category).Include(m => m.Photo).FirstOrDefaultAsync(i => i.IdEntity == @object.IdEntity);

        //    return CreatedAtAction("GetChatRoom", new { id = chatRoom.IdChat }, chatRoom);
        //}

        /// <summary>
        /// Get all chat rooms for current user
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="401">Unathorized</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<Chat>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [Authorize] //Change id in parameters to current authorized user_id
        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<List<Chat>>> GetUserChatRooms()
        {
            int userId = Int32.Parse(User.FindFirst("id").Value);
            var chatRoomsId = await _context.ChatUsers.Where(s => s.UserId == userId).Select(s => s.ChatId).ToListAsync();
            var userChatRooms = await _context.ChatRooms.Where(s => chatRoomsId.Contains(s.IdChat)).ToListAsync();
            foreach (Chat room in userChatRooms)
            {
                var last_message = await _context.Messages.Where(s => s.ChatId == room.IdChat).OrderBy(s => s.IdMessage).LastOrDefaultAsync();
                if (last_message != null) {
                    room.LastMessage = last_message;
                    room.LastActivity = last_message.CreationTime;
                }
                else
                {
                    room.LastActivity = room.CreationTime;
                }
            }
            userChatRooms.Sort((x, y) => DateTime.Compare(y.LastActivity, x.LastActivity));
            return userChatRooms;
        }

        /// <summary>
        /// Create new personal chat
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="400">The request is incorrect</response>
        /// <response code="401">Unathorized</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Chat), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [Authorize]
        [HttpPost] //
        [Route("[action]")]
        public async Task<ActionResult<Chat>> CreatePersonalChat(PersonalChatModel chatInfo)
        {
            int userId = Int32.Parse(User.FindFirst("id").Value);
            Chat chatRoom = new Chat {
                IdChat = Guid.NewGuid(),
                CreationTime = DateTime.UtcNow,
                ChatName = chatInfo.ChatName,
                Owner = userId,
                Personal = true,
                ChatUsers = new List<ChatUser>()
            };
            
            ChatUser chatUserOwner = new ChatUser { UserId = userId, ChatId = chatRoom.IdChat };
            ChatUser chatUserReceiver = new ChatUser { UserId = chatInfo.Receiver, ChatId = chatRoom.IdChat };
            chatRoom.ChatUsers.Add(chatUserOwner);
            chatRoom.ChatUsers.Add(chatUserReceiver);
            _context.ChatRooms.Add(chatRoom);

            await _context.SaveChangesAsync();

            //ChatUser chatUser = new ChatUser { UserId = userId, ChatId = chatRoom.IdChat };

            //_context.ChatUsers.Add(chatUser);
            //await _context.SaveChangesAsync();

            return chatRoom;

            //return 0;
        }

        /// <summary>
        /// Create new group chat
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="400">The request is incorrect</response>
        /// <response code="401">Unathorized</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Chat), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [Authorize]
        [HttpPost]
        [Route("[action]")]//
        public async Task<ActionResult<Chat>> CreateGroupChat(GroupChatModel chatInfo)
        {
            int userId = Int32.Parse(User.FindFirst("id").Value);
            //chatRoom.Owner = userId;
            Chat chatRoom = new Chat
            {
                IdChat = Guid.NewGuid(),
                CreationTime = DateTime.UtcNow,
                ChatName = chatInfo.ChatName,
                Owner = userId,
                TypeId = chatInfo.TypeId,
                EntityId = chatInfo.EntityId,
                Personal = false,
                ChatUsers = new List<ChatUser>()
            };

            ChatUser chatUserOwner = new ChatUser { UserId = userId, ChatId = chatRoom.IdChat };
            chatRoom.ChatUsers.Add(chatUserOwner);
            _context.ChatRooms.Add(chatRoom);

            await _context.SaveChangesAsync();

            return chatRoom;
        }
    }
}
