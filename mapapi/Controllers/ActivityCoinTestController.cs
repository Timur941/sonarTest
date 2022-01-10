using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mapapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityCoinTestController : Controller
    {
        private readonly GeosocdbContext _context;
        private readonly IConfiguration _configuration;

        public ActivityCoinTestController(GeosocdbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        //[HttpPatch]
        //public IActionResult CountActivityCoin(int objectId)
        //{
        //    var @object = _context.Objects.FirstOrDefault(s => s.IdEntity == objectId);
        //    var visitorsId = _context.ViewStatistics
        //        .Where(s => s.TypeId == 1 && s.EntityId == objectId && s.UserId != null)
        //        .Select(s => s.UserId)
        //        .Distinct()
        //        .ToList();
            
        //    Dictionary<int, float> activity_score = new Dictionary<int, float>(visitorsId.Count());
        //    foreach (int visitorId in visitorsId)
        //    {
        //        float visitor_score_sum = 0;
        //        var review = _context.Reviews
        //            .FirstOrDefault(
        //                s => s.TypeId == 1 &&
        //                s.EntityId == objectId &&
        //                s.UserId == visitorId);
        //        visitor_score_sum += (float)(review != null ? 0.23 : 0);
        //        var sharing = _context.Sharings
        //            .Where(
        //                s => s.TypeId == 1 &&
        //                s.EntityId == objectId &&
        //                s.UserId == visitorId);
        //        visitor_score_sum += (float)(sharing != null ? sharing.Count() * 0.16 : 0);
        //        var visits_count = _context.ViewStatistics
        //            .Where(
        //                s => s.TypeId == 1 &&
        //                s.EntityId == objectId &&
        //                s.UserId == visitorId)
        //            .Count();
        //        visitor_score_sum += (float)(visits_count * 0.19);
        //        activity_score.Add(visitorId, visitor_score_sum);
        //    }
        //    float object_score_sum = activity_score.Values.Sum();
        //    foreach (int visitorId in visitorsId)
        //    {
        //        var activity_coin = activity_score[visitorId] / object_score_sum * 100;
        //        var visitor = _context.Users.First(s => s.IdUser == visitorId);
        //        visitor.ActivityCoin += activity_coin;
        //        _context.Entry(visitor).Property(x => x.ActivityCoin).IsModified = true;
        //        _context.SaveChanges();
        //    }
        //    return Ok();
        //}
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Object>>> GetObject()
        //{
        //    var prevDate = DateTime.UtcNow.AddDays(-7);
        //    var @objects = _context.Objects.Where(s =>
        //        _context.Reviews.Any(e => 
        //            e.TypeId == 1 && 
        //            DateTime.Compare(prevDate, e.PostedDate) < 0 &&
        //            e.EntityId == s.IdEntity) ||
        //        _context.Sharings.Any(e =>
        //            e.TypeId == 1 &&
        //            DateTime.Compare(prevDate, e.ShareTime) < 0 &&
        //            e.EntityId == s.IdEntity) ||
        //        _context.Favourites.Any(e =>
        //            e.TypeId == 1 &&
        //            DateTime.Compare(prevDate, e.AddedTime) < 0 &&
        //            e.EntityId == s.IdEntity) ||
        //        _context.ViewStatistics.Any(e =>
        //            e.TypeId == 1 &&
        //            DateTime.Compare(prevDate, e.VisitedTime) < 0 &&
        //            e.EntityId == s.IdEntity)
        //        ).ToList();
        //    return objects;
        //}

        //[HttpGet]
        //[Route("[action]")]
        //public int GetUserForRefresh(string refreshToken)
        //{
        //    //var user = await _context.Users
        //    //        .SingleOrDefaultAsync(s => s.RefreshTokens
        //    //            .Any(t => t.TokenStr == refreshToken));
        //    //if (user == null)
        //    //    return NotFound();
        //    //else
        //    //    return user;
        //    int userId = Int32.Parse(User.FindFirst("id").Value);
        //    return userId;
        //}
    }
}
