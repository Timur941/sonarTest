using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mapapi;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using mapapi.Models.SocialAction;
using NetTopologySuite.Geometries;
using mapapi.Models;

namespace mapapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlacesController : ControllerBase
    {
        private readonly GeosocdbContext _context;

        public PlacesController(GeosocdbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get previews of all places in the specified city
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Specified city was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<PointPreview>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Route("[action]/{city}")]
        [HttpGet]
        public async Task<ActionResult<List<PointPreview>>> GetInCity(string city)
        {
            City cityData = await _context.Cities.FirstOrDefaultAsync(s => s.Name.ToLower() == city.ToLower());
            if (cityData == null)
                return NotFound();
            return await _context.Places.Where(s => cityData.Way.Contains(s.Way)).Select(s => new PointPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating
            }).ToListAsync();
        }

        /// <summary>
        /// Get previews of all places in the specified city by category
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Specified city</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<PointPreview>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Route("[action]/{city}/{categoryId}")]
        [HttpGet]
        public async Task<ActionResult<List<PointPreview>>> GetInCity(string city, int categoryId)
        {
            City cityData = await _context.Cities.FirstOrDefaultAsync(s => s.Name.ToLower() == city.ToLower());
            if (cityData == null)
                return NotFound();
            return await _context.Places.Where(s => cityData.Way.Contains(s.Way) &&
                s.CategoryId == categoryId).Select(s => new PointPreview
                {
                    IdEntity = s.IdEntity,
                    TypeId = s.TypeId,
                    Way = s.Way,
                    Title = s.Title,
                    Rating = s.Rating
                }).ToListAsync();
        }

        /// <summary>
        /// Get previews of all places in current screen area
        /// </summary>
        ///<remarks>lat - y coordinate, lng - x coordinate</remarks>
        /// <response code="200">Request processed successfully</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<PointPreview>), 200)]
        [ProducesResponseType(500)]
        [Route("[action]/coord")]
        [HttpGet]
        public async Task<ActionResult<List<PointPreview>>> GetPreviewByCoord(double lat1, double lng1, double lat2, double lng2)
        {
            Coordinate[] screen_area_coords = new Coordinate[5]
            {
                new Coordinate(lng1, lat1),
                new Coordinate(lng2, lat1),
                new Coordinate(lng2, lat2),
                new Coordinate(lng1, lat2),
                new Coordinate(lng1, lat1)
            };

            LinearRing screen_borders = new LinearRing(screen_area_coords);
            Polygon screen_area = new Polygon(screen_borders) { SRID = 4326 };
            return await _context.Places.Where(s => screen_area.Contains(s.Way)).Select(s => new PointPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating
            }).ToListAsync();
        }

        /// <summary>
        /// Get previews of all places in current screen area by category
        /// </summary>
        ///<remarks>lat - y coordinate, lng - x coordinate</remarks>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Specified category was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<PointPreview>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Route("[action]/{categoryId}/coord")]
        [HttpGet]
        public async Task<ActionResult<List<PointPreview>>> GetPreviewByCoord(int categoryId, double lat1, double lng1, double lat2, double lng2)
        {
            Coordinate[] screen_area_coords = new Coordinate[5]
            {
                new Coordinate(lng1, lat1),
                new Coordinate(lng2, lat1),
                new Coordinate(lng2, lat2),
                new Coordinate(lng1, lat2),
                new Coordinate(lng1, lat1)
            };

            LinearRing screen_borders = new LinearRing(screen_area_coords);
            Polygon screen_area = new Polygon(screen_borders) { SRID = 4326 };
            return await _context.Places.Where(s => screen_area.Contains(s.Way) && s.CategoryId == categoryId).Select(s => new PointPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating
            }).ToListAsync();
        }

        // GET: api/Places
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Place>>> GetPlaces()
        {
            return await _context.Places.Include(m => m.Category.CategoryClassifier).Include(m => m.Photo).ToListAsync();
        }

        /// <summary>
        /// Get previews of all places by name
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<PointPreview>), 200)]
        [ProducesResponseType(500)]
        [Route("[action]/{name}")]
        [HttpGet]
        public async Task<ActionResult<List<PointPreview>>> GetPreviewByName(string name)
        {
            return await _context.Places.Where(i => i.Title.ToLower().Contains(name.ToLower())).Select(s => new PointPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating
            }).ToListAsync();
        }

        // GET: api/Places/5
        /// <summary>
        /// Get detailed information about place
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Specified place was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Place), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [HttpGet("{id}")]
        public async Task<ActionResult<Place>> GetPlace(long id)
        {
            var place = await _context.Places.Include(m => m.Category.CategoryClassifier).Include(m => m.Photo).FirstOrDefaultAsync(i => i.IdEntity == id);

            if (place == null)
            {
                return NotFound();
            }
            var userIdClaim = User.FindFirst("id");
            int? userId = null;
            if (userIdClaim != null)
                userId = Int32.Parse(userIdClaim.Value);
            ViewStatistics statistic_row = new ViewStatistics
            {
                TypeId = place.TypeId,
                EntityId = place.IdEntity,
                VisitedTime = DateTime.UtcNow,
                //Ip = HttpContext.Connection.RemoteIpAddress,
                UserAgent = HttpContext.Request.Headers["User-Agent"],
                UserId = userId
            };
            _context.ViewStatistics.Add(statistic_row);
            await _context.SaveChangesAsync();
            return place;
        }

        // PUT: api/Places/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        /// <summary>
        /// Edit information about place
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///         "idEntity": 1,
        ///         "way": {
        ///         "type": "Point",
        ///         "coordinates": [
        ///             55.96947237053552,
        ///             54.73270569121833
        ///             ]
        ///         },
        ///         "title": "PlaceTitle",
        ///         "previewDescription": "PlacePreviewDesc",
        ///         "description": "PlaceDesc",
        ///         "categoryId": 2,
        ///         "private": false,
        ///         "address": "Уфа",
        ///         "typeId": 2
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Request processed successfully</response>
        /// <response code="400">The request is incorrect (id specified in parameters doen't math the id specified in request body)</response>
        /// <response code="401">Unathorized</response>
        /// <response code="403">Not allowed to edit info about this place</response>
        /// <response code="404">Specified object was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Place), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "admin,user")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPlace(long id, Place place)
        {
            //if (place.UserId == userId || userRole == "admin")
            //{
            //    if (id != place.IdEntity)
            //    {
            //        return BadRequest();
            //    }

            //    _context.Entry(place).State = EntityState.Modified;

            //    try
            //    {
            //        await _context.SaveChangesAsync();
            //    }
            //    catch (DbUpdateConcurrencyException)
            //    {
            //        if (!PlaceExists(id))
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
            string userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            int userId = Int32.Parse(User.FindFirst("id").Value);
            if (id != place.IdEntity)
            {
                return BadRequest(); //"IdEntity or userId in request body is incorrect"
            }
            try
            {
                //route.UserId = existingRoute.UserId; admins cant change owner of the entity
                if (userRole != "admin")
                {
                    var existingPlace = _context.Places.FirstOrDefault(e => e.IdEntity == id);
                    if (existingPlace.UserId != userId || place.UserId != userId) //raises NullReferenceException if entity not found (null)
                    {
                        return Forbid();
                    }
                    _context.Entry(existingPlace).State = EntityState.Detached;
                }
                _context.Entry(place).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateConcurrencyException || ex is NullReferenceException)
                {
                    if (!PlaceExists(id))
                    {
                        return NotFound();
                    }
                }
                throw;
            }
            return NoContent();
        }

        // POST: api/Places
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        /// <summary>
        /// Add new place
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///         "way": {
        ///         "type": "Point",
        ///         "coordinates": [
        ///             55.96947237053552,
        ///             54.73270569121833
        ///             ]
        ///         },
        ///         "title": "PlaceTitle",
        ///         "previewDescription": "PlacePreviewDesc",
        ///         "description": "PlaceDesc",
        ///         "categoryId": 2,
        ///         "private": false,
        ///         "address": "Уфа",
        ///         "typeId": 2
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Request processed successfully</response>
        /// <response code="400">The request is incorrect</response>
        /// <response code="401">Unathorized</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Place), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Place>> PostPlace(Place place)
        {
            int userId = Int32.Parse(User.FindFirst("id").Value);
            place.UserId = userId;

            _context.Places.Add(place);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPlace", new { id = place.IdEntity }, place);
        }

        // DELETE: api/Places/5
        /// <summary>
        /// Delete place
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="401">Unathorized</response>
        /// <response code="403">Not allowed to delete this place</response>
        /// <response code="404">Specified place was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Place), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "admin,user")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Place>> DeletePlace(long id)
        {
            string userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            int userId = Int32.Parse(User.FindFirst("id").Value);
            var place = await _context.Places.FindAsync(id);
            if (place != null)
            {
                if (place.UserId == userId || userRole == "admin")
                {
                    _context.Places.Remove(place);
                    await _context.SaveChangesAsync();

                    return place;
                }
                else
                    return Forbid();  
            }
            else
                return NotFound();
        }

        private bool PlaceExists(long id)
        {
            return _context.Places.Any(e => e.IdEntity == id);
        }
    }
}
