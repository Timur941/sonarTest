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
    public class ReviewsController : Controller
    {
        private readonly GeosocdbContext _context;
        private readonly IConfiguration _configuration;


        public ReviewsController(GeosocdbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Review>>> GetReviews()
        //{
        //    return await _context.Reviews.ToListAsync();
        //}

        /// <summary>
        /// Get reviews for entity using paging. Returns 10 reviews in specified page
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<Review>), 200)]
        [ProducesResponseType(500)]
        [Route("[action]/{typeId}/{entityId}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviewsForEntity(int typeId, long entityId, int page)
        {
            const int reviewsPerPage = 10;
            int offset = (page - 1)* reviewsPerPage;
            var reviews = await _context.Reviews.Where(i => i.TypeId == typeId && i.EntityId == entityId)
                .OrderByDescending(s => s.PostedDate).Skip(offset).Take(reviewsPerPage)
                .ToListAsync();
            foreach (Review review in reviews)
            {
                review.LikesCount = _context.LikeDislikes
                    .Count(i => i.TypeId == 5 && i.EntityId == review.IdReview && i.IsLike == true); 
                review.DislikesCount = _context.LikeDislikes
                    .Count(i => i.TypeId == 5 && i.EntityId == review.IdReview && i.IsLike == false);
            }
            return reviews;
        }

        /// <summary>
        /// Get detailed information about review
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Specified review was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Review), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [HttpGet("{id}")]
        public async Task<ActionResult<Review>> GetReview(long id)
        {
            var review = await _context.Reviews.FirstOrDefaultAsync(i => i.IdReview == id);

            if (review == null)
            {
                return NotFound();
            }
            review.LikesCount = _context.LikeDislikes
                .Count(i => i.TypeId == 5 && i.EntityId == review.IdReview && i.IsLike == true);
            review.DislikesCount = _context.LikeDislikes
                    .Count(i => i.TypeId == 5 && i.EntityId == review.IdReview && i.IsLike == false);

            return review;
        }

        /// <summary>
        /// Edit information about review
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///         "idReview": 1,
        ///         "typeId": 1,
        ///         "entityId": 10,
        ///         "userId": 1,
        ///         "ratingValue": 4,
        ///         "reviewText": "Nice place!",
        ///         "postedDate": "2021-05-20T16:00:00"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Request processed successfully</response>
        /// <response code="400">The request is incorrect (id specified in parameters doen't math the id specified in request body)</response>
        /// <response code="401">Unathorized</response>
        /// <response code="404">Specified review was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Review), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReview(long id, Review review)
        {
            if (id != review.IdReview)
            {
                return BadRequest();
            }

            _context.Entry(review).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReviewExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Add new review
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///         "typeId": 1,
        ///         "entityId": 10,
        ///         "ratingValue": 4,
        ///         "reviewText": "Nice place!",
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Request processed successfully</response>
        /// <response code="400">The request is incorrect</response>
        /// <response code="401">Unathorized</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Review), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Review>> PostReview(Review review)
        {
            int userId = Int32.Parse(User.FindFirst("id").Value);
            if (review.PostedDate == DateTime.Parse("0001-01-01T00:00:00"))
                review.PostedDate = DateTime.UtcNow;
            review.UserId = userId;
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetReview", new { id = review.IdReview }, review);
        }

        /// <summary>
        /// Delete review
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="401">Unathorized</response>
        /// <response code="404">Specified review was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Review), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Review>> DeleteReview(long id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return NotFound();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return review;
        }

        private bool ReviewExists(long id)
        {
            return _context.Reviews.Any(e => e.IdReview == id);
        }
    }
}
