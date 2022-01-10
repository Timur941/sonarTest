using mapapi.Models.SocialAction;
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
    public class ViewStatisticsController : Controller
    {
        private readonly GeosocdbContext _context;
        private readonly IConfiguration _configuration;

        public ViewStatisticsController(GeosocdbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Get view statistics for entity
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<ViewStatistics>), 200)]
        [ProducesResponseType(500)]
        [Route("[action]/{typeId}/{entityId}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ViewStatistics>>> GetViewStatisticsForEntity(int typeId, long entityId)
        {
            return await _context.ViewStatistics.Where(i => i.TypeId == typeId && i.EntityId == entityId)
                .ToListAsync();
        }

        /// <summary>
        /// Get information about specified veiw statistic
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Specified statistic row was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(ViewStatistics), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Route("{id}")]
        [HttpGet]
        public async Task<ActionResult<ViewStatistics>> GetViewStatistic(long id)
        {
            return await _context.ViewStatistics.FirstOrDefaultAsync(s => s.IdView == id);
        }

        /// <summary>
        /// Add view statistic row
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="400">The request is incorrect</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(ViewStatistics), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpPost]
        public async Task<ActionResult<ViewStatistics>> PostStatisticssForEntity(ViewStatistics statistic)
        {
            _context.ViewStatistics.Add(statistic);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetViewStatistic", new { id = statistic.IdView }, statistic);
        }

        /// <summary>
        /// Delete view statistic row
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Specified statistic row was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(ViewStatistics), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ViewStatistics>> DeleteStatistic(long id)
        {
            var statistic = await _context.ViewStatistics.FindAsync(id);
            if (statistic == null)
                return NotFound();
            _context.ViewStatistics.Remove(statistic);
            await _context.SaveChangesAsync();

            return statistic;
        }
    }
}
