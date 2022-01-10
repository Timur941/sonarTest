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
    public class FavouritesController : Controller
    {
        private readonly GeosocdbContext _context;
        private readonly IConfiguration _configuration;

        public FavouritesController(GeosocdbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Get list of favourites for current user
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="401">Unathorized</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<Favourite>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Favourite>>> GetFavouritesForUser()
        {
            int userId = Int32.Parse(User.FindFirst("id").Value);
            return await _context.Favourites.Where(i => i.UserId == userId)
                .ToListAsync();
        }

        /// <summary>
        /// Add entity to favourite
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///         "typeId": 1,
        ///         "entityId": 10,
        ///         "addedTime": "2021-05-20T16:00:00",
        ///         "notifications": true
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Request processed successfully</response>
        /// <response code="401">Unathorized</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Favourite), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpPost]
        public async Task<ActionResult<Favourite>> PostFavourite(Favourite favourite)
        {
            int userId = Int32.Parse(User.FindFirst("id").Value);
            favourite.UserId = userId;
            if (favourite.AddedTime == DateTime.Parse("0001-01-01T00:00:00"))
                favourite.AddedTime = DateTime.UtcNow;
            _context.Favourites.Add(favourite);
            await _context.SaveChangesAsync();

            //return CreatedAtAction("GetLikeDislike", new { id = likeDislike.IdLike }, likeDislike);
            return favourite;
        }

        /// <summary>
        /// Delete entity from list of user's favourite
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="401">Unathorized</response>
        /// <response code="404">Specified favourite was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Favourite), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Favourite>> DeleteFavourite(long id)
        {
            var favourite = await _context.Favourites.FindAsync(id);
            if (favourite == null)
            {
                return NotFound();
            }

            _context.Favourites.Remove(favourite);
            await _context.SaveChangesAsync();

            return favourite;
        }

        private bool FavouriteExists(long id)
        {
            return _context.Favourites.Any(e => e.IdFavourite == id);
        }
    }
}
