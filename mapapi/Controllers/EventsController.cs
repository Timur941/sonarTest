using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.Extensions.Configuration;
using NpgsqlTypes;
using NetTopologySuite.Geometries;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using mapapi.Models.SocialAction;
using mapapi.Models;

namespace mapapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly GeosocdbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string connString;

        private static int GetIconRadius(float fscore)
        {
            const int maxRadius = 95;
            const int minRadius = 40;
            const int maxFScore = 100;
            const int minFScore = 10;
            return (int)(fscore > maxFScore ? 100 :
                        (fscore < minFScore ? 0 :
                            Math.Round(((fscore - minFScore) / (maxFScore - minFScore)) * (maxRadius - minRadius) + minRadius)));
        }
        public EventsController(GeosocdbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            connString = Environment.GetEnvironmentVariable("dbconnstring");
        }

        /// <summary>
        /// For duration more than 23:59 use format: "d.hh:mm:ss" (days.hours:minutes:seconds)
        /// </summary>
        // GET: api/Events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventPreview>>> GetEvents()
        {
            var events = await _context.Events.Select(s => new EventPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating,
                Category = s.Category,
                CategoryClassifier = s.Category.CategoryClassifier,
                Date = s.Date,
                Duration = s.Duration,
                IconRadius = GetIconRadius(s.FScore)
            }).ToListAsync();
            return events;
            //return await _context.Events.Include(m => m.Category).ToListAsync();
        }

        /// <summary>
        /// Get previews of all events in the specified city
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Specified city was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<EventPreview>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Route("[action]/{city}")]
        [HttpGet]
        public async Task<ActionResult<List<EventPreview>>> GetInCity(string city)
        {
            City cityData = await _context.Cities.FirstOrDefaultAsync(s => s.Name.ToLower() == city.ToLower());
            if (cityData == null)
                return NotFound();
            return await _context.Events.Where(s => cityData.Way.Contains(s.Way)).Select(s => new EventPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating,
                Category = s.Category,
                CategoryClassifier = s.Category.CategoryClassifier,
                Date = s.Date,
                Duration = s.Duration,
                IconRadius = GetIconRadius(s.FScore)
            }).ToListAsync();
        }

        /// <summary>
        /// Get previews of all events in the specified city by category
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Specified city was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<EventPreview>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Route("[action]/{city}/{categoryId}")]
        [HttpGet]
        public async Task<ActionResult<List<EventPreview>>> GetInCity(string city, int categoryId)
        {
            City cityData = await _context.Cities.FirstOrDefaultAsync(s => s.Name.ToLower() == city.ToLower());
            if (cityData == null)
                return NotFound();
            return await _context.Events.Where(s => cityData.Way.Contains(s.Way) &&
                s.CategoryId == categoryId).Select(s => new EventPreview
                {
                    IdEntity = s.IdEntity,
                    TypeId = s.TypeId,
                    Way = s.Way,
                    Title = s.Title,
                    Rating = s.Rating,
                    Category = s.Category,
                    CategoryClassifier = s.Category.CategoryClassifier,
                    Date = s.Date,
                    Duration = s.Duration,
                    IconRadius = GetIconRadius(s.FScore)
                }).ToListAsync();
        }

        /// <summary>
        /// Get previews of all events for specified entity
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<EventPreview>), 200)]
        [ProducesResponseType(500)]
        [Route("[action]/{typeId}/{entityId}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventPreview>>> GetEventsForEntity(int typeId, long entityId)
        {
            var events = await _context.Events
                .Where(i => i.AssociatedEntityType == typeId && i.AssociatedEntityId == entityId)
                .Select(s => new EventPreview
                {
                    IdEntity = s.IdEntity,
                    TypeId = s.TypeId,
                    Way = s.Way,
                    Title = s.Title,
                    Rating = s.Rating,
                    Category = s.Category,
                    CategoryClassifier = s.Category.CategoryClassifier,
                    Date = s.Date,
                    Duration = s.Duration,
                    IconRadius = GetIconRadius(s.FScore)
                }).ToListAsync();
            return events;
        }

        /// <summary>
        /// Get previews of all events by category classifier
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<EventPreview>), 200)]
        [ProducesResponseType(500)]
        [Route("[action]/{classifierId}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventPreview>>> GetEventsByClassifier(int classifierId)
        {
            var events = await _context.Events
                .Where(c => c.Category.CategoryClassifierId == classifierId)
                .Select(s => new EventPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating,
                Category = s.Category,
                CategoryClassifier = s.Category.CategoryClassifier,
                Date = s.Date,
                Duration = s.Duration
            }).ToListAsync();
            return events;
            //return await _context.Events.Include(m => m.Category).ToListAsync();
        }

        /// <summary>
        /// Get events in current screen area
        /// </summary>
        //[Route("[action]/coord")]
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<PointPreview>>> GetPreviewByCoord(double lat1, double lng1, double lat2, double lng2)
        //{
        //    List<PointPreview> points = new List<PointPreview>();

        //    await using var conn = new NpgsqlConnection(connString);
        //    await conn.OpenAsync();
        //    //conn.TypeMapper.UseNetTopologySuite();

        //    string query = "SELECT id_entity, type_id, title, way, rating FROM event "
        //        + "WHERE ST_Contains(ST_MakeEnvelope(@lat1,@lng1,@lat2,@lng2,4326), way)";

        //    await using (var cmd = new NpgsqlCommand(query, conn))
        //    {
        //        cmd.Parameters.Add("@lat1", NpgsqlDbType.Real); cmd.Parameters.Add("@lng1", NpgsqlDbType.Real);
        //        cmd.Parameters.Add("@lat2", NpgsqlDbType.Real); cmd.Parameters.Add("@lng2", NpgsqlDbType.Real);
        //        cmd.Parameters["@lat1"].Value = lat1; cmd.Parameters["@lng1"].Value = lng1;
        //        cmd.Parameters["@lat2"].Value = lat2; cmd.Parameters["@lng2"].Value = lng2;
        //        await using (var reader = await cmd.ExecuteReaderAsync())
        //            while (await reader.ReadAsync())
        //                //points.Add(new PointPreview() { IdEntity = Int64.Parse(reader[0].ToString()), TypeId = Int32.Parse(reader[1].ToString()), Title = reader[2].ToString(), Way = reader[3] });
        //                points.Add(new PointPreview() { IdEntity = reader.GetInt64(0), TypeId = reader.GetInt32(1), Title = reader.GetString(2), Way = (Geometry)reader[3], Rating = reader.GetFloat(4) });
        //    }
        //    conn.Dispose();
        //    conn.Close();
        //    return points;
        //}

        /// <summary>
        /// Get previews of all events in current screen area
        /// </summary>
        ///<remarks>lat - y coordinate, lng - x coordinate</remarks>
        /// <response code="200">Request processed successfully</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<EventPreview>), 200)]
        [ProducesResponseType(500)]
        [Route("[action]/coord")]
        [HttpGet]
        public async Task<ActionResult<List<EventPreview>>> GetPreviewByCoord(double lat1, double lng1, double lat2, double lng2)
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
            return await _context.Events.Where(s => screen_area.Contains(s.Way)).Select(s => new EventPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating,
                Category = s.Category,
                CategoryClassifier = s.Category.CategoryClassifier,
                Date = s.Date,
                Duration = s.Duration,
                IconRadius = GetIconRadius(s.FScore)
            }).ToListAsync();
        }

        /// <summary>
        /// Get previews of all events in current screen area by category
        /// </summary>
        ///<remarks>lat - y coordinate, lng - x coordinate</remarks>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Specified category was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<EventPreview>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Route("[action]/{categoryId}/coord")]
        [HttpGet]
        public async Task<ActionResult<List<EventPreview>>> GetPreviewByCoord(int categoryId, double lat1, double lng1, double lat2, double lng2)
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
            return await _context.Events.Where(s => screen_area.Contains(s.Way) && s.CategoryId == existingCatId.IdCategory).Select(s => new EventPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating,
                Category = s.Category,
                CategoryClassifier = s.Category.CategoryClassifier,
                Date = s.Date,
                Duration = s.Duration,
                IconRadius = GetIconRadius(s.FScore)
            }).ToListAsync();
        }

        /// <summary>
        /// Get previews of all events by name
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<EventPreview>), 200)]
        [ProducesResponseType(500)]
        [Route("[action]/{name}")]
        [HttpGet]
        public async Task<ActionResult<List<EventPreview>>> GetPreviewByName(string name)
        {
            return await _context.Events.Where(i => i.Title.ToLower().Contains(name.ToLower())).Select(s => new EventPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating,
                Category = s.Category,
                CategoryClassifier = s.Category.CategoryClassifier,
                Date = s.Date,
                Duration = s.Duration,
                IconRadius = GetIconRadius(s.FScore)
            }).ToListAsync();
        }

        // GET: api/Events/5
        /// <summary>
        /// Get detailed information about event
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Specified event was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Event), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(long id)
        {
            //var @event = await _context.Events.FindAsync(id);
            var @event = await _context.Events.Include(m => m.Category.CategoryClassifier).FirstOrDefaultAsync(i => i.IdEntity == id);
            if (@event == null)
            {
                return NotFound();
            }
            var userIdClaim = User.FindFirst("id");
            int? userId = null;
            if (userIdClaim != null)
                userId = Int32.Parse(userIdClaim.Value);
            ViewStatistics statistic_row = new ViewStatistics
            {
                TypeId = @event.TypeId,
                EntityId = @event.IdEntity,
                VisitedTime = DateTime.UtcNow,
                //Ip = HttpContext.Connection.RemoteIpAddress,
                UserAgent = HttpContext.Request.Headers["User-Agent"],
                UserId = userId
            };
            _context.ViewStatistics.Add(statistic_row);
            await _context.SaveChangesAsync();
            return @event;
        }

        // PUT: api/Events/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        /// <summary>
        /// Edit information about event
        /// </summary>
        /// <remarks>
        /// For duration more than 23:59 use format: "d.hh:mm:ss" (days.hours:minutes:seconds).
        /// Sample request:
        ///
        ///     {
        ///         "idEntity": 1,
        ///         "associatedEntityType": null,
        ///         "associatedEntityId": null,
        ///         "title": "EvenetTitle",
        ///         "previewDescription": "EventSDescription",
        ///         "description": "EventDescription",
        ///         "categoryId": 4,
        ///         "private": false,
        ///         "date": "2021-05-20T16:00:00",
        ///         "duration": "01:00:00",
        ///         "price": 0,
        ///         "typeId": 3,
        ///         "way": {
        ///             "type": "Point",
        ///             "coordinates": [
        ///                 56.0565661,
        ///                 54.8185402
        ///             ]
        ///         }
        ///     }
        /// </remarks>
        /// <response code="200">Request processed successfully</response>
        /// <response code="400">The request is incorrect (id specified in parameters doen't math the id specified in request body)</response>
        /// <response code="401">Unathorized</response>
        /// <response code="403">Not allowed to edit info about this object</response>
        /// <response code="404">Specified object was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Event), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "admin,user")]
        [HttpPut("{id}")]
        public async Task<ActionResult<Event>> PutEvent(long id, Event @event)
        {
            string userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            int userId = Int32.Parse(User.FindFirst("id").Value);
            if (id != @event.IdEntity)
            {
                return BadRequest(); //"IdEntity or userId in request body is incorrect"
            }
            try
            {
                if (userRole != "admin")
                {
                    var existingEvent = _context.Events.FirstOrDefault(e => e.IdEntity == id);
                    if (existingEvent.UserId != userId || @event.UserId != userId) //raises NullReferenceException if entity not found (null)
                    {
                        return Forbid();
                    }
                    _context.Entry(existingEvent).State = EntityState.Detached;
                }
                _context.Entry(@event).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateConcurrencyException || ex is NullReferenceException)
                {
                    if (!EventExists(id))
                    {
                        return NotFound();
                    }
                }
                throw;
            }

            return @event;
        }

        // POST: api/Events
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        /// <summary>
        /// Add new event
        /// </summary>
        /// <remarks>
        /// For duration more than 23:59 use format: "d.hh:mm:ss" (days.hours:minutes:seconds).
        /// Sample request:
        ///
        ///     {
        ///         "associatedEntityType": null,
        ///         "associatedEntityId": null,
        ///         "title": "EvenetTitle",
        ///         "previewDescription": "EventSDescription",
        ///         "description": "EventDescription",
        ///         "categoryId": 4,
        ///         "private": false,
        ///         "date": "2021-05-20T16:00:00",
        ///         "duration": "01:00:00",
        ///         "price": 0,
        ///         "typeId": 3,
        ///         "way": {
        ///             "type": "Point",
        ///             "coordinates": [
        ///                 56.0565661,
        ///                 54.8185402
        ///             ]
        ///         }
        ///     }
        /// </remarks>
        /// <response code="200">Request processed successfully</response>
        /// <response code="400">The request is incorrect</response>
        /// <response code="401">Unathorized</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Event), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Event>> PostEvent(Event @event)
        {
            int userId = Int32.Parse(User.FindFirst("id").Value);
            @event.UserId = userId;

            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEvent", new { id = @event.IdEntity }, @event);
        }

        // DELETE: api/Events/5
        //[Authorize(Roles = "admin,user")]
        /// <summary>
        /// Delete information about event
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="401">Unathorized</response>
        /// <response code="403">Not allowed to delete this event</response>
        /// <response code="404">Specified object was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Event), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "admin,user")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Event>> DeleteEvent(long id)
        {
            string userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            int userId = Int32.Parse(User.FindFirst("id").Value);
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                if (@event.UserId == userId || userRole == "admin")
                {
                    _context.Events.Remove(@event);
                    await _context.SaveChangesAsync();

                    return @event;
                }
                else
                    return Forbid(); // OR UNATHORIZED ?
            }
            
            else 
                return NotFound();
        }

        private bool EventExists(long id)
        {
            return _context.Events.Any(e => e.IdEntity == id);
        }
    }
}
