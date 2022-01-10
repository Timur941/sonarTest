using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mapapi;
using NetTopologySuite.Geometries;
using Npgsql;
using System.Data.Common;
using NpgsqlTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using mapapi.Models.SocialAction;
using System.Net;
using mapapi.Models;

namespace mapapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ObjectsController : ControllerBase
    {
        private readonly GeosocdbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string connString;


        public ObjectsController(GeosocdbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            connString = Environment.GetEnvironmentVariable("dbconnstring");
        }

        // GET: api/Objects
        //[Authorize(Policy = "ValidAccessToken")]
        //[Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Object>>> GetObjects()
        {
            return await _context.Objects.Include(m => m.Category).Include(m => m.Photo).Include(m => m.OSchedule).ToListAsync();  //.Include(m => m.Category)
        }

        /// <summary>
        /// Get previews of all objects in the specified city
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
            return await _context.Objects.Where(s => cityData.Way.Contains(s.Way)).Select(s => new PointPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating
            }).ToListAsync();
        }

        /// <summary>
        /// Get previews of all objects in the specified city by category
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
            return await _context.Objects.Where(s => cityData.Way.Contains(s.Way) && 
                s.CategoryId == existingCatId.IdCategory).Select(s => new PointPreview
                {
                    IdEntity = s.IdEntity,
                    TypeId = s.TypeId,
                    Way = s.Way,
                    Title = s.Title,
                    Rating = s.Rating
                }).ToListAsync();
        }

        //[Route("[action]/coord")]
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<PointPreview>>> GetPreviewByCoord(double lat1, double lng1, double lat2, double lng2)
        //{
        //    List<PointPreview> points = new List<PointPreview>();

        //    await using var conn = new NpgsqlConnection(connString);
        //    await conn.OpenAsync();
        //    //conn.TypeMapper.UseNetTopologySuite();

        //    string query = "SELECT id_entity, type_id, title, way, rating FROM object "
        //        + "WHERE ST_Contains(ST_MakeEnvelope(@lat1,@lng1,@lat2,@lng2,4326), way)";

        //    await using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
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
        /// Get previews of all objects in current screen area
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
            return await _context.Objects.Where(s => screen_area.Contains(s.Way)).Select(s => new PointPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating
            }).ToListAsync();
        }

        /// <summary>
        /// Get previews of all objects in current screen area by category
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
            return await _context.Objects.Where(s => screen_area.Contains(s.Way) && s.CategoryId == existingCatId.IdCategory).Select(s => new PointPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating
            }).ToListAsync();
        }

        /// <summary>
        /// Get previews of all objects by name
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<PointPreview>), 200)]
        [ProducesResponseType(500)]
        [Route("[action]/{name}")]
        [HttpGet]
        public async Task<ActionResult<List<PointPreview>>> GetPreviewByName(string name)
        {
            return await _context.Objects.Where(i => i.Title.ToLower().Contains(name.ToLower())).Select(s => new PointPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating
            }).ToListAsync();
        }

        /// <summary>
        /// Get previews of all objects by category classifier
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Category classifier was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<PointPreview>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Route("[action]/{classifierId}")]
        [HttpGet]
        public async Task<ActionResult<List<PointPreview>>> GetPreviewByClassifier(int classifierId)
        {
            var classifier = await _context.CategoryClassifiers.FindAsync(classifierId);
            if (classifier == null)
                return NotFound();
            return await _context.Objects
                .Where(i => i.Category.CategoryClassifierId == classifier.IdCategoryClassifier)
                .Select(s => new PointPreview
                {
                    IdEntity = s.IdEntity,
                    TypeId = s.TypeId,
                    Way = s.Way,
                    Title = s.Title,
                    Rating = s.Rating
                }).ToListAsync();
        }

        /// <summary>
        /// Get detailed information about object
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Specified object was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Object), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [HttpGet("{id}")]
        public async Task<ActionResult<Object>> GetObject(long id)
        {
            //var @object = await _context.Objects.FindAsync(id);
            var @object = await _context.Objects.Include(m => m.Category).Include(m => m.Photo).FirstOrDefaultAsync(i => i.IdEntity == id);

            if (@object == null)
            {
                return NotFound();
            }
            var userIdClaim = User.FindFirst("id");
            int? userId = null;
            if (userIdClaim != null)
                userId = Int32.Parse(userIdClaim.Value);
            ViewStatistics statistic_row = new ViewStatistics {
                TypeId = @object.TypeId,
                EntityId = @object.IdEntity,
                VisitedTime = DateTime.UtcNow,
                //Ip = HttpContext.Connection.RemoteIpAddress,
                UserAgent = HttpContext.Request.Headers["User-Agent"],
                UserId = userId
            };
            _context.ViewStatistics.Add(statistic_row);
            await _context.SaveChangesAsync();
            return @object;
        }

        // PUT: api/Objects/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        /// <summary>
        /// Edit information about object
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
        ///         "title": "Байрам",
        ///         "previewDescription": "Продуктовый магазин",
        ///         "description": "Продуктовый магазин байрам",
        ///         "categoryId": 2,
        ///         "private": false,
        ///         "address": "Уфа",
        ///         "price": 0,
        ///         "ageLimit": "0",
        ///         "typeId": 1
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Request processed successfully</response>
        /// <response code="400">The request is incorrect (id specified in parameters doen't math the id specified in request body)</response>
        /// <response code="401">Unathorized</response>
        /// <response code="403">Not allowed to edit info about this object</response>
        /// <response code="404">Specified object was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "admin,user")]
        [HttpPut("{id}")]
        public async Task<ActionResult<Object>> PutObject(long id, Object @object)
        {
            string userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            int userId = Int32.Parse(User.FindFirst("id").Value);
            if (id != @object.IdEntity)
            {
                return BadRequest();
            }
            try
            {
                //@object.UserId = existingRoute.UserId; admins cant change owner of the entity
                if (userRole != "admin")
                {
                    var existingObject = _context.Objects.FirstOrDefault(e => e.IdEntity == id);
                    if (existingObject.UserId != userId || @object.UserId != userId) //raises NullReferenceException if entity not found (null)
                    {
                        return Forbid();
                    }
                    _context.Entry(existingObject).State = EntityState.Detached;
                }
                _context.Entry(@object).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateConcurrencyException || ex is NullReferenceException)
                {
                    if (!ObjectExists(id))
                    {
                        return NotFound();
                    }
                }
                throw;
            }
            return @object; //NoContent()
        }

        // POST: api/Objects
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        /// <summary>
        /// Add new object
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
        ///         "title": "Байрам",
        ///         "previewDescription": "Продуктовый магазин",
        ///         "description": "Продуктовый магазин байрам",
        ///         "categoryId": 2,
        ///         "private": false,
        ///         "address": "Уфа",
        ///         "price": 0,
        ///         "ageLimit": "0",
        ///         "typeId": 1
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Request processed successfully</response>
        /// <response code="400">The request is incorrect</response>
        /// <response code="401">Unathorized</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "admin,user")]
        [HttpPost]
        public async Task<ActionResult<Object>> PostObject(Object @object)
        {
            int userId = Int32.Parse(User.FindFirst("id").Value);
            @object.UserId = userId;
            
            _context.Objects.Add(@object);
            await _context.SaveChangesAsync();

            //return await _context.Objects.Include(m => m.Category).Include(m => m.Photo).FirstOrDefaultAsync(i => i.IdEntity == @object.IdEntity);

            return CreatedAtAction("GetObject", new { id = @object.IdEntity }, @object);
        }

        // DELETE: api/Objects/5
        /// <summary>
        /// Delete object
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="401">Unathorized</response>
        /// <response code="403">Not allowed to delete this object</response>
        /// <response code="404">Specified object was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Object), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "admin,user")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Object>> DeleteObject(long id)
        {
            string userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            int userId = Int32.Parse(User.FindFirst("id").Value);
            var @object = await _context.Objects.FindAsync(id);
            if (@object != null)
            {
                if (@object.UserId == userId || userRole == "admin")
                {
                    _context.Objects.Remove(@object);
                    await _context.SaveChangesAsync();
                    return @object;
                }
                else
                {
                    return Forbid(); // OR UNATHORIZED ?
                }
            }
            else
            {
                return NotFound();
            }
            
            //var @object = await _context.Objects.FindAsync(id);
            //if (@object == null)
            //{
            //    return NotFound();
            //}

            //_context.Objects.Remove(@object);
            //await _context.SaveChangesAsync();

            //return @object;
        }

        private bool ObjectExists(long id)
        {
            return _context.Objects.Any(e => e.IdEntity == id);
        }
    }
}
