using mapapi.Models.SocialAction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mapapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FriendshipController : Controller
    {
        private readonly GeosocdbContext _context;

        public FriendshipController(GeosocdbContext context, IConfiguration configuration)
        {
            _context = context;
        }

        /// <summary>
        /// Get friends for user
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="401">Unathorized</response>
        /// <response code="404">Specified user was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<User>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        [Route("[action]/{userId}")]
        [HttpGet]
        public ActionResult<IEnumerable<User>> GetFriends(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user == null)
                return NotFound();
            var receivers = _context.Users
                .Where(user => _context.Friendships
                    .Any(fr => fr.UserFirst == userId && fr.Status == 2 && fr.UserSecond == user.IdUser));
            var users = receivers.Union(_context.Users
                .Where(user => _context.Friendships
                    .Any(fr => fr.UserSecond == userId && fr.Status == 2 && fr.UserFirst == user.IdUser)));
            return users.ToList();
        }

        /// <summary>
        /// Get friendship requests for current user
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="401">Unathorized</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<Friendship>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult<List<FriendshipRequest>>> GetFriendRequests()
        {
            int userId = Int32.Parse(User.FindFirst("id").Value);
            //var friendship = await _context.Friendships.Where(s => s.UserSecond == userId).ToListAsync();
            //var requesterUsers = await _context.Users
            //    .Where(user => _context.Friendships
            //        .Any(fr => fr.UserSecond == userId && fr.Status == 1 && fr.UserFirst == user.IdUser))
            //    .ToListAsync();


            var friendship = await _context.Friendships.Where(s => s.UserSecond == userId && s.Status == 1).Join(_context.Users, // второй набор
                u => u.UserFirst,
                c => c.IdUser,
                (u, c) => new FriendshipRequest
                {
                    IdFriendship = u.IdFriendship,
                    UserId = u.UserFirst,
                    Username = c.Username,
                    Status = u.Status,
                    CreatedTime = u.CreatedTime,
                    UpdatedTime = u.UpdatedTime
                }).ToListAsync();

            return friendship;
            //return await _context.Users
            //    .Where(user => _context.Friendships
            //        .Any(fr => fr.UserSecond == userId && fr.Status == 1 && fr.UserFirst == user.IdUser))
            //    .ToListAsync();
        }

        /// <summary>
        /// Send friendship request to user
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="401">Unathorized</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Friendship), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpPost("{receiverId}")]
        public async Task <ActionResult<Friendship>> FriendshipRequest(int receiverId)
        {
            int userId = Int32.Parse(User.FindFirst("id").Value);
            Friendship friendship = new Friendship
            {
                UserFirst = userId,
                UserSecond = receiverId,
                Status = 1,
                CreatedTime = DateTime.UtcNow,
                UpdatedTime = null
            };
            _context.Friendships.Add(friendship);
            await _context.SaveChangesAsync();
            return friendship;
        }

        /// <summary>
        /// Edit friendship status
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="401">Unathorized</response>
        /// <response code="403">Forbidden. Not allowed to edit this entity</response>
        /// <response code="404">Specified friendship was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Friendship), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [HttpPut("{id}/{status}")]
        public async Task<ActionResult<Friendship>> PutFriendship(int id, short status)
        {
            int userId = Int32.Parse(User.FindFirst("id").Value);
            var friendship = await _context.Friendships.FindAsync(id);
            if(userId == friendship.UserFirst || userId == friendship.UserSecond)
            {
                try
                {
                    friendship.UpdatedTime = DateTime.UtcNow;
                    friendship.Status = status;
                    _context.Entry(friendship).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    if (ex is DbUpdateConcurrencyException || ex is NullReferenceException)
                    {
                        if (!FriendshipExists(id))
                        {
                            return NotFound();
                        }
                    }
                    throw;
                }
                return friendship;
            }
            return Forbid();
        }

        /// <summary>
        /// Delete friendship
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="401">Unathorized</response>
        /// <response code="403">Forbidden. Not allowed to delete this entity</response>
        /// <response code="404">Specified friendship was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Friendship), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Friendship>> DeleteFriendship(int id)
        {
            int userId = Int32.Parse(User.FindFirst("id").Value);
            var friendship = await _context.Friendships.FindAsync(id);
            if(friendship != null)
            {
                if (userId == friendship.UserFirst || userId == friendship.UserSecond)
                {
                   
                    _context.Friendships.Remove(friendship);
                    await _context.SaveChangesAsync();
                    return friendship;
                }
                else
                    return Forbid();
            }
            else
                return NotFound();
        }

        private bool FriendshipExists(int id)
        {
            return _context.Friendships.Any(e => e.IdFriendship == id);
        }
    }
}
