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
    public class TypesController : Controller
    {
        private readonly GeosocdbContext _context;
        private readonly IConfiguration _configuration;

        public TypesController(GeosocdbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Get existing entities types
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<Type>), 200)]
        [ProducesResponseType(500)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Type>>> GetTypes()
        {
            return await _context.Types.ToListAsync();
        }
    }
}
