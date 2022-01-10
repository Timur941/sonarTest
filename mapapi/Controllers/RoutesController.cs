using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mapapi;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using mapapi.Models.SocialAction;
using mapapi.Models;
using NetTopologySuite.Geometries;

namespace mapapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoutesController : ControllerBase
    {
        private readonly GeosocdbContext _context;

        public RoutesController(GeosocdbContext context)
        {
            _context = context;
        }

        // GET: api/Routes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Route>>> GetRoute()
        {
            return await _context.Route.Include(m => m.RoutePoints).ToListAsync();
        }

        /// <summary>
        /// Get previews of all routes in the specified city
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
            return await _context.Route.Where(s => cityData.Way.Contains(s.Way)).Select(s => new PointPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating
            }).ToListAsync();
        }

        /// <summary>
        /// Get previews of all routes in the specified city by category
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Specified city or category was not found</response>
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
            var existingCatId = await _context.Categories.FindAsync(categoryId);
            if (existingCatId == null)
                return NotFound();
            return await _context.Route.Where(s => cityData.Way.Contains(s.Way) &&
                s.CategoryId == existingCatId.IdCategory).Select(s => new PointPreview
                {
                    IdEntity = s.IdEntity,
                    TypeId = s.TypeId,
                    Way = s.Way,
                    Title = s.Title,
                    Rating = s.Rating
                }).ToListAsync();
        }

        /// <summary>
        /// Get previews of all routes in current screen area
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
            return await _context.Route.Where(s => screen_area.Contains(s.Way)).Select(s => new PointPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating
            }).ToListAsync();
        }

        /// <summary>
        /// Get previews of all routes in current screen area by category
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
            var existingCatId = await _context.Categories.FindAsync(categoryId);
            if (existingCatId == null)
                return NotFound();
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
            return await _context.Route.Where(s => screen_area.Contains(s.Way) && s.CategoryId == existingCatId.IdCategory).Select(s => new PointPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating
            }).ToListAsync();
        }

        /// <summary>
        /// Get previews of all routes by name
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<PointPreview>), 200)]
        [ProducesResponseType(500)]
        [Route("[action]/{name}")]
        [HttpGet]
        public async Task<ActionResult<List<PointPreview>>> GetPreviewByName(string name)
        {
            return await _context.Route.Where(i => i.Title.ToLower().Contains(name.ToLower())).Select(s => new PointPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating
            }).ToListAsync();
        }

        // GET: api/Routes/5
        /// <summary>
        /// Get detailed information about route
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Specified route was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Object), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [HttpGet("{id}")]
        public async Task<ActionResult<Route>> GetRoute(long id)
        {
            var route = await _context.Route.FindAsync(id);

            if (route == null)
            {
                return NotFound();
            }
            var userIdClaim = User.FindFirst("id");
            int? userId = null;
            if (userIdClaim != null)
                userId = Int32.Parse(userIdClaim.Value);
            ViewStatistics statistic_row = new ViewStatistics
            {
                TypeId = route.TypeId,
                EntityId = route.IdEntity,
                VisitedTime = DateTime.UtcNow,
                //Ip = HttpContext.Connection.RemoteIpAddress,
                UserAgent = HttpContext.Request.Headers["User-Agent"],
                UserId = userId
            };
            _context.ViewStatistics.Add(statistic_row);
            await _context.SaveChangesAsync();
            return route;
        }

        // PUT: api/Routes/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        /// <summary>
        /// Edit information about route
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///         "idEntity": 1,
        ///         "way": {
        ///             "type": "LineString",
        ///             "coordinates": [
        ///                 [
        ///                     55.999587,
        ///                     54.717465
        ///                 ],
        ///                 [
        ///                     55.999156,
        ///                     54.717106
        ///                 ]
        ///             ]
        ///         },
        ///         "title": "Велодорожка",
        ///         "previewDescription": "Велодорожка",
        ///         "description": "Велодорожка",
        ///         "categoryId": 2,
        ///         "private": false,
        ///         "price": 0,
        ///         "distance": null,
        ///         "ageLimit": "0",
        ///         "typeId": 1
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Request processed successfully</response>
        /// <response code="400">The request is incorrect (id specified in parameters doen't math the id specified in request body)</response>
        /// <response code="401">Unathorized</response>
        /// <response code="403">Not allowed to edit info about this route</response>
        /// <response code="404">Specified route was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Route), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "admin,user")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoute(long id, Route route)
        {
            string userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            int userId = Int32.Parse(User.FindFirst("id").Value);
            if (id != route.IdEntity)
            {
                return BadRequest(); //"IdEntity or userId in request body is incorrect"
            }
            try
            {
                //route.UserId = existingRoute.UserId; admins cant change owner of the entity
                if (userRole != "admin")
                {
                    var existingRoute = _context.Route.FirstOrDefault(e => e.IdEntity == id);
                    if (existingRoute.UserId != userId || route.UserId != userId) //raises NullReferenceException if entity not found (null)
                    {
                        return Forbid();
                    }
                    _context.Entry(existingRoute).State = EntityState.Detached;
                }
                _context.Entry(route).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateConcurrencyException || ex is NullReferenceException)
                {
                    if (!RouteExists(id))
                    {
                        return NotFound();
                    }
                }
                throw;
            }
            return NoContent();
        }

        // POST: api/Routes
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        /// <summary>
        /// Add new route
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///         "way": {
        ///             "type": "LineString",
        ///             "coordinates": [
        ///                 [
        ///                     55.999587,
        ///                     54.717465
        ///                 ],
        ///                 [
        ///                     55.999156,
        ///                     54.717106
        ///                 ]
        ///             ]
        ///         },
        ///         "title": "Велодорожка",
        ///         "previewDescription": "Велодорожка",
        ///         "description": "Велодорожка",
        ///         "categoryId": 2,
        ///         "private": false,
        ///         "price": 0,
        ///         "distance": null,
        ///         "ageLimit": "0",
        ///         "typeId": 1
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Request processed successfully</response>
        /// <response code="400">The request is incorrect</response>
        /// <response code="401">Unathorized</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Route), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Route>> PostRoute(Route route)
        {
            int userId = Int32.Parse(User.FindFirst("id").Value);
            route.UserId = userId;
            _context.Route.Add(route);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRoute", new { id = route.IdEntity }, route);
        }

        // DELETE: api/Routes/5
        /// <summary>
        /// Delete route
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="401">Unathorized</response>
        /// <response code="403">Not allowed to delete this route</response>
        /// <response code="404">Specified object was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Route), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "admin,user")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Route>> DeleteRoute(long id)
        {
            string userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            int userId = Int32.Parse(User.FindFirst("id").Value);
            var route = await _context.Route.FindAsync(id);
            if (route != null)
            {
                if (@route.UserId == userId || userRole == "admin")
                {
                    _context.Route.Remove(route);
                    await _context.SaveChangesAsync();

                    return route;
                }
                else
                {
                    return Forbid(); // OR UNATHORIZED ?
                }
            }
            else
                return NotFound();
        }

        private bool RouteExists(long id)
        {
            return _context.Route.Any(e => e.IdEntity == id);
        }
    }
}
