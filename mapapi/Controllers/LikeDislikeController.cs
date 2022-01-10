using mapapi.Models.SocialAction;
using Microsoft.AspNetCore.Authorization;
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
    public class LikeDislikeController : Controller
    {
        private readonly GeosocdbContext _context;
        private readonly IConfiguration _configuration;

        public LikeDislikeController(GeosocdbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Get likes and dislikes for specified entity 
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<LikeDislike>), 200)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        [Route("[action]/{typeId}/{entityId}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDislike>>> GetLikesDislikesForEntity(int typeId, long entityId)
        {
            return await _context.LikeDislikes.Where(i => i.TypeId == typeId && i.EntityId == entityId)
                .ToListAsync();
        }

        /// <summary>
        /// Get detailed information about like or dislike
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Specified likeDislike was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(LikeDislike), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<LikeDislike>> GetLikeDislike(long id)
        {
            var likeOrDislike = await _context.LikeDislikes.FirstOrDefaultAsync(i => i.IdLike == id);

            if (likeOrDislike == null)
            {
                return NotFound();
            }

            return likeOrDislike;
        }

        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutLikeDislike(long id, LikeDislike likeDislike)
        //{
        //    if (id != likeDislike.IdLike)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(likeDislike).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!LikeDislikeExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        /// <summary>
        /// Add like to the entity
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(LikeDislike), 200)]
        [ProducesResponseType(500)]
        [Route("Like/{typeId}/{entityId}")]
        [HttpPost]
        public async Task<ActionResult<LikeDislike>> PostLike(int typeId, long entityId)
        {
            int userId = Int32.Parse(User.FindFirst("id").Value);
            DateTime postedDate = DateTime.UtcNow;

            LikeDislike like = new LikeDislike
            {
                TypeId = typeId,
                EntityId = entityId,
                UserId = userId,
                IsLike = true,
                PostedDate = postedDate
            };
            _context.LikeDislikes.Add(like);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLikeDislike", new { id = like.IdLike }, like);
        }

        /// <summary>
        /// Add dislike to the entity
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(LikeDislike), 200)]
        [ProducesResponseType(500)]
        [Route("Dislike/{typeId}/{entityId}")]
        [HttpPost]
        public async Task<ActionResult<LikeDislike>> PostDislike(int typeId, long entityId)
        {
            int userId = Int32.Parse(User.FindFirst("id").Value);
            DateTime postedDate = DateTime.UtcNow;

            LikeDislike dislike = new LikeDislike
            {
                TypeId = typeId,
                EntityId = entityId,
                UserId = userId,
                IsLike = false,
                PostedDate = postedDate
            };
            _context.LikeDislikes.Add(dislike);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLikeDislike", new { id = dislike.IdLike }, dislike);
        }

        /// <summary>
        /// Delete like or dislike
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="401">Unathorized</response>
        /// <response code="403">Forbidden. Not allowed to delete this entity</response>
        /// <response code="404">Specified like or dislike was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(LikeDislike), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [HttpDelete("{id}")]
        public async Task<ActionResult<LikeDislike>> DeleteLikeDislike(long id)
        {
            int userId = Int32.Parse(User.FindFirst("id").Value);
            if (!LikeDislikeExists(id))
            {
                return NotFound();
            }
            var likeDislike = await _context.LikeDislikes.FindAsync(id);
            if (likeDislike.UserId != userId)
                return Forbid();
            _context.LikeDislikes.Remove(likeDislike);
            await _context.SaveChangesAsync();

            return likeDislike;
        }

        private bool LikeDislikeExists(long id)
        {
            return _context.LikeDislikes.Any(e => e.IdLike == id);
        }
    }
}
