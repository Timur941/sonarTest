using mapapi.Models.SocialAction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace mapapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SharingsController : Controller
    {
        private readonly GeosocdbContext _context;
        private readonly IConfiguration _configuration;
        public SharingsController(GeosocdbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sharing>>> GetSharings()
        {
            return await _context.Sharings.ToListAsync();
        }

        /// <summary>
        /// Get detailed information about sharing
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Specified sharing was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Sharing), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [HttpGet("{id}")]
        public async Task<ActionResult<Sharing>> GetSharing(long id)
        {
            var sharing = await _context.Sharings.FirstOrDefaultAsync(i => i.IdSharing == id);

            if (sharing == null)
            {
                return NotFound();
            }

            return sharing;
        }

        /// <summary>
        /// Add new sharing
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///         "typeId": 1,
        ///         "entityId": 10,
        ///         "userId": 1,
        ///         "shareText": "Visited the restaurant yesterday",
        ///         "shareTime": "2021-05-20T16:00:00"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Request processed successfully</response>
        /// <response code="400">The request is incorrect</response>
        /// <response code="401">Unathorized</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Sharing), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Sharing>> PostSharing(Sharing sharing)
        {
            if (sharing.ShareTime == DateTime.Parse("0001-01-01T00:00:00"))
                sharing.ShareTime = DateTime.UtcNow;

            _context.Sharings.Add(sharing);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetShaing", new { id = sharing.IdSharing }, sharing);
        }

        /// <summary>
        /// Delete sharing
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="401">Unathorized</response>
        /// <response code="403">Not allowed to delete this sharing</response>
        /// <response code="404">Specified sharing was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Sharing), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Sharing>> DeleteSharing(long id)
        {
            string userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            int userId = Int32.Parse(User.FindFirst("id").Value);
            var sharing = await _context.Sharings.FindAsync(id);
            if (sharing == null)
            {
                return NotFound();
            }
            if (sharing.UserId == userId || userRole == "admin")
            {
                _context.Sharings.Remove(sharing);
                await _context.SaveChangesAsync();
                return sharing;
            }
            else
                return Forbid();
        }
    }
}
