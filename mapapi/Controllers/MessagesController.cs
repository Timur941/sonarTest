using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using mapapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace mapapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly GeosocdbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string connString;

        public MessagesController(GeosocdbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            connString = Environment.GetEnvironmentVariable("dbconnstring");
        }

        [HttpGet]
        [Route("[action]/{chatId}")]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessages(Guid chatId)
        {
            return await _context.Messages.Where(s => s.ChatId == chatId).ToListAsync();
        }

        //[HttpGet("{id}")]
        //public async Task<ActionResult<Message>> GetMessage(long id)
        //{
        //    var message = await _context.Messages.FirstOrDefaultAsync(i => i.IdMessage == id);

        //    if (message == null)
        //    {
        //        return NotFound();
        //    }

        //    return message;
        //}

        //[Authorize]
        //[Route("[action]/{id}")]
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Message>>> GetChatMessages(long id)
        //{
        //    var messages = _context.Messages.Where(s => s.ChatId == id);
        //    // Way to change attribute of fields for several rows
        //    //var messages = _context.Messages.Where(s => s.ChatId == id).AsEnumerable()
        //    //    .Select(c => {
        //    //    c.IsRead = true;
        //    //    return c;
        //    //});
        //    //--------------------------------------------------------
        //    //Fragment to change IsRead attribute while getting list of messages for specific Chat
        //    foreach (Message message in messages)
        //    {
        //        message.IsRead = true;
        //        _context.Entry(message).State = EntityState.Modified;
        //    }
        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        return StatusCode(500);
        //    }
        //    //--------------------------------------------------------
        //    return await _context.Messages.Where(s => s.ChatId == id).ToListAsync();
        //}

        //[Authorize]
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutMessage(long id, Message message)
        //{
        //    int userId = Int32.Parse(User.FindFirst("id").Value);
        //    if (message.UserId == userId)
        //    {
        //        if (id != message.IdMessage)
        //        {
        //            return BadRequest();
        //        }

        //        _context.Entry(message).State = EntityState.Modified;

        //        try
        //        {
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!MessageExists(id))
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
        //        return Forbid();
        //}

        //[Authorize]
        //[HttpPost]
        //public async Task<ActionResult<Message>> PostMessage(Message message)
        //{
        //    //int userId = Int32.Parse(User.FindFirst("id").Value);
        //    //@object.UserId = userId;
        //    message.CreationTime = DateTime.UtcNow;

        //    _context.Messages.Add(message);
        //    await _context.SaveChangesAsync();

        //    //return await _context.Objects.Include(m => m.Category).Include(m => m.Photo).FirstOrDefaultAsync(i => i.IdEntity == @object.IdEntity);

        //    return CreatedAtAction("GetMessage", new { id = message.IdMessage }, message);
        //}
        //private bool MessageExists(long id)
        //{
        //    return _context.Messages.Any(e => e.IdMessage == id);
        //}
    }
}
