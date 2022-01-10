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

namespace mapapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PointPreviewsController : ControllerBase
    {
        private readonly GeosocdbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string connString;

        public PointPreviewsController(GeosocdbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            connString = Environment.GetEnvironmentVariable("dbconnstring");
        }

        //GET: api/PointPreviews
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PointPreview>>> GetPointPreviews()
        {

            var places = _context.Places.Select(s => new PointPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating
            });

            var objects = _context.Objects.Select(s => new PointPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating
            });

            var events = _context.Events.Select(s => new PointPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating
            });

            var routes = _context.Route.Select(s => new PointPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating
            });

            return await routes.Union(events.Union(objects.Union(places))).ToListAsync();
        }

        [Route("[action]/{name}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PointPreview>>> GetPreviewByName(string name)
        {
            //var @object = await _context.Objects.FindAsync(id);
            //var objects = _context.Objects.Where(i => i.Title.ToLower().Contains(name.ToLower())).ToList();
            //var events = _context.Events.Where(i => i.Title.ToLower().Contains(name.ToLower())).ToList();

            var places = _context.Places.Where(i => i.Title.ToLower().Contains(name.ToLower())).Select(s => new PointPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating
            });
            var objects = _context.Objects.Where(i => i.Title.ToLower().Contains(name.ToLower())).Select(s => new PointPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating
            });
            var events = _context.Events.Where(i => i.Title.ToLower().Contains(name.ToLower())).Select(s => new PointPreview
            {
                IdEntity = s.IdEntity,
                TypeId = s.TypeId,
                Way = s.Way,
                Title = s.Title,
                Rating = s.Rating
            });

            return await events.Union(objects.Union(places)).ToListAsync();
            //return await _context.Objects.Include(m => m.Category).Where(i => i.Title.Contains(name)).ToListAsync();
            //var objects = _context.Objects.Include(m => m.Category).Where(i => i.Title.Contains(name)).ToList();
            //List<PointPreview> points = new List<PointPreview>();
            //foreach (var obj in objects)
            //{
            //    points.Add()
            //}

            //if (@object == null)
            //{
            //    return NotFound();
            //}

            //return @object;
        }
    }
}
