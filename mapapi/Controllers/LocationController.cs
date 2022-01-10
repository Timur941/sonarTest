using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mapapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : Controller
    {
        private readonly GeosocdbContext _context;
        public LocationController(GeosocdbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Checks if point belongs to Ufa city area. lng - x, lat - y coord.
        /// </summary>
        [Route("[action]")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<OkObjectResult> PointInCity(double lng, double lat) //IActionResult 
        {
            var city = await _context.Cities.FindAsync(26); // 26 - Ufa
            Point point = new Point(new Coordinate(lng, lat));
            bool pointBelongs = city.Way.Contains(point);
            return Ok(new
            {
                Belongs = pointBelongs,
                DateTime = DateTime.UtcNow,
                en = city.UrlName,
                ru = city.Name
            });
        }
    }
}
