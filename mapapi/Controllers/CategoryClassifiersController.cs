using mapapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mapapi.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryClassifiersController : ControllerBase
    {
        private readonly GeosocdbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string connString;

        public CategoryClassifiersController(GeosocdbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            connString = Environment.GetEnvironmentVariable("dbconnstring");
        }

        /// <summary>
        /// Get category classifiers 
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<CategoryClassifier>), 200)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryClassifier>>> GetClassifiers()
        {
            return await _context.CategoryClassifiers.ToListAsync();
        }

        /// <summary>
        /// Get detailed information about category classifier 
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Specified classifier was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(CategoryClassifier), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryClassifier>> GetClassifier(int id)
        {
            var classifier = await _context.CategoryClassifiers.FirstOrDefaultAsync(i => i.IdCategoryClassifier == id);

            if (classifier == null)
            {
                return NotFound();
            }

            return classifier;
        }

        /// <summary>
        /// Edit information about category classifier. Allowed only for administrators
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="400">The request is incorrect (id specified in parameters doen't math the id specified in request body)</response>
        /// <response code="401">Unathorized</response>
        /// <response code="404">Specified classifier was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(CategoryClassifier), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [HttpPut("{id}")]
        public async Task<ActionResult<CategoryClassifier>> PutClassifier(int id, CategoryClassifier classifier)
        {
            if (id != classifier.IdCategoryClassifier)
            {
                return BadRequest();
            }

            _context.Entry(classifier).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClassifierExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (DbUpdateException e)
            {
                return Problem(statusCode: 500, title: e.InnerException.Message);
            }

            return classifier;
        }

        /// <summary>
        /// Add new category classifier
        /// </summary>
        /// <remarks>Allowed only for administrators</remarks>
        /// <response code="200">Request processed successfully</response>
        /// <response code="400">The request is incorrect</response>
        /// <response code="401">Unathorized</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(CategoryClassifier), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpPost]
        public async Task<ActionResult<CategoryClassifier>> PostClassifier(CategoryClassifier classifier)
        {

            try
            {
                _context.CategoryClassifiers.Add(classifier);
                await _context.SaveChangesAsync();
            }
            catch(DbUpdateException e)
            {
                //return BadRequest("test"); //, new { errorMessage = "Classifier with a similar name is already exists"}
                return Problem(statusCode: 500, title: e.InnerException.Message);
            }

            return CreatedAtAction("GetClassifier", new { id = classifier.IdCategoryClassifier }, classifier);
        }

        /// <summary>
        /// Delete information about category classifier. Allowed only for administrators
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="401">Unathorized</response>
        /// <response code="404">Specified classifier was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(CategoryClassifier), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [HttpDelete("{id}")]
        public async Task<ActionResult<CategoryClassifier>> DeleteClassifier(int id)
        {
            var classifier = await _context.CategoryClassifiers.FindAsync(id);
            if (classifier == null)
            {
                return NotFound();
            }

            _context.CategoryClassifiers.Remove(classifier);
            await _context.SaveChangesAsync();

            return classifier;
        }

        private bool ClassifierExists(int id)
        {
            return _context.CategoryClassifiers.Any(e => e.IdCategoryClassifier == id);
        }
    }
}
