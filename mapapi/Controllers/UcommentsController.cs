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
    public class UcommentsController : Controller
    {
        private readonly GeosocdbContext _context;
        private readonly IConfiguration _configuration;
        public UcommentsController(GeosocdbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Get comments for photo or review
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<Ucomment>), 200)]
        [ProducesResponseType(500)]
        [HttpGet]
        [Route("[action]/{typeId}/{entityId}")]
        public async Task<ActionResult<IEnumerable<Ucomment>>> GetCommentsForEntity(int typeId, long entityId)
        {
            return await _context.Ucomments.Where(s => s.TypeId == typeId && s.EntityId == entityId).ToListAsync();
        }

        /// <summary>
        /// Add comment for photo or review
        /// </summary>
        /// <remarks>
        /// "typeId": 5 - for reviews, "typeId": 6 - for photos; parentId used to specify the reply comment.
        /// Sample request:
        ///
        ///     {
        ///         "typeId": 5,
        ///         "entityId": 10,
        ///         "userId": 1,
        ///         "commentText": "Visited the restaurant yesterday",
        ///         "postedDate": "2021-05-20T16:00:00",
        ///         "parentId": null
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Request processed successfully</response>
        /// <response code="400">The request is incorrect</response>
        /// <response code="401">Unathorized</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Ucomment), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Ucomment>> PostComment(Ucomment comment)
        {
            int userId = Int32.Parse(User.FindFirst("id").Value);
            comment.UserId = userId;

            _context.Ucomments.Add(comment);
            await _context.SaveChangesAsync();

            return comment;
        }

        /// <summary>
        /// Delete comment
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="401">Unathorized</response>
        /// <response code="403">Forbidden. Not allowed to delete this comment</response>
        /// <response code="404">Specified comment was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Ucomment), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Ucomment>> DeleteComment(long id)
        {
            string userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            int userId = Int32.Parse(User.FindFirst("id").Value);
            var ucomment = await _context.Ucomments.FindAsync(id);
            if (ucomment == null)
            {
                return NotFound();
            }
            if (ucomment.UserId == userId || userRole == "admin")
            {
                _context.Ucomments.Remove(ucomment);
                await _context.SaveChangesAsync();
                return ucomment;
            }
            else
                return Forbid();
        }
    }
}
