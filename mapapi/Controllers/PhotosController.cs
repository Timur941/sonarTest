using mapapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace mapapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotosController : Controller
    {
        private readonly GeosocdbContext _context;
        private readonly IConfiguration _configuration;

        public PhotosController(GeosocdbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos()
        {
            return await _context.Photos.ToListAsync();
        }

        /// <summary>
        /// Get original image file for specified photo entity
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Specified photo was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(File), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Route("[action]/{idPhoto}")]
        [HttpGet]
        public IActionResult GetImg(long idPhoto)
        {
            var photo = _context.Photos.FirstOrDefault(s => s.IdPhoto == idPhoto);
            if (photo == null)
            {
                return NotFound();
            }
            return File(photo.OrigImgData, photo.ContentType);
        }

        /// <summary>
        /// Get preview image file for specified photo entity
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Specified photo was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(File), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Route("[action]/{idPhoto}")]
        [HttpGet]
        public IActionResult GetImgThumb(long idPhoto)
        {
            var photo = _context.Photos.FirstOrDefault(s => s.IdPhoto == idPhoto);
            if (photo == null)
            {
                return NotFound();
            }
            return File(photo.ThumbnailImgData, "image/jpeg");
        }
        //[HttpGet("get-file-content/{id}")]
        //public async Task<FileContentResult> DownloadAsync(long id)
        //{
        //    var photo = _context.Photos.FirstOrDefault(s => s.IdPhoto == id);

        //    //if (photo == null)
        //    //{
        //    //    return NotFound();
        //    //}
        //    return new FileContentResult(photo.OrigImgData, "image/jpeg")
        //    {
        //        FileDownloadName = photo.Title
        //    };
        //}
        //[Route("[action]/{TypeId}/{EntitytId}")]
        //[Route("/{typeId}/{entityId}")]
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Photo>>> GetPhotosForEntity(int typeId, long entityId)
        //{
        //    return await _context.Photos
        //        .Where(s => s.TypeId == typeId && s.EntityId == entityId)
        //        .ToListAsync();
        //}

        /// <summary>
        /// Get an array of id photos
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(List<long>), 200)]
        [ProducesResponseType(500)]
        [Route("[action]/{typeId}/{entityId}")]
        [HttpGet]
        public async Task<ActionResult<List<long>>> GetPhotosForEntity(int typeId, long entityId)
        {
            return await _context.Photos
                .Where(s => s.TypeId == typeId && s.EntityId == entityId)
                .Select(s => s.IdPhoto)
                .ToListAsync();
        }

        /// <summary>
        /// Get detailed information about photo
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Specified photo was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Photo), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Route("[action]/{idPhoto}")]
        [HttpGet]
        public async Task<ActionResult<Photo>> GetPhotoDetails(long idPhoto)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(s => s.IdPhoto == idPhoto);
            if (photo == null)
            {
                return NotFound();
            }
            return photo;
        }

        /// <summary>
        /// Upload image of object, place, event or route
        /// </summary>
        /// <remarks>
        /// Sample for jsonString:
        ///
        ///     {
        ///         "typeId": 1,
        ///         "entityId": 10,
        ///         "title": "PhotoTitle",
        ///         "description": "PhotoDesc"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Request processed successfully</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Photo), 200)]
        [ProducesResponseType(500)]
        [Authorize]
        [HttpPost("UploadImage")]
        public async Task<IActionResult> UploadImage(IFormFile file, [FromForm] string jsonString) 
        {
            int userId = Int32.Parse(User.FindFirst("id").Value);
            string contentType = file.ContentType;
            if (!string.Equals(contentType, "image/jpg", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(contentType, "image/jpeg", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(contentType, "image/pjpeg", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(contentType, "image/gif", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(contentType, "image/x-png", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(contentType, "image/png", StringComparison.OrdinalIgnoreCase))
            {
                return Problem(statusCode: 500, detail: $"{contentType} - wrong mime type of the image (jpg, jpeg, pjpeg, gif, x-pmg, png allowed)");
            }
            var fileExtension = Path.GetExtension(file.FileName);
            if (!string.Equals(fileExtension, ".jpg", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(fileExtension, ".png", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(fileExtension, ".gif", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(fileExtension, ".jpeg", StringComparison.OrdinalIgnoreCase))
            {
                return Problem(statusCode: 500, detail: $"{fileExtension} - wrong extension for image (jpg, jpeg, gif, png allowed)");
            }

            Photo photo = JsonConvert.DeserializeObject<Photo>(jsonString);
            byte[] origImageBytes;
            byte[] thumbnailImageBytes;
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                thumbnailImageBytes = ImageThumb.GetReducedImage(400, 400, ms);
                origImageBytes = ms.ToArray();
            }

            photo.UserId = userId;
            photo.OrigImgData = origImageBytes;
            photo.ThumbnailImgData = thumbnailImageBytes;
            photo.PostedDate = DateTime.UtcNow;
            photo.ContentType = contentType;

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPhotoDetails", new { idPhoto = photo.IdPhoto }, photo);
        }

        /// <summary>
        /// Delete image of object, place, event or route
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="401">Unathorized</response>
        /// <response code="403">Forbidden. Not allowed to delete this photo</response>
        /// <response code="404">Specified photo was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(Photo), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "admin,user")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Photo>> DeletePhoto(long id)
        {
            string userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            int userId = Int32.Parse(User.FindFirst("id").Value);
            var photo = await _context.Photos.FindAsync(id);
            if (photo != null)
            {
                if (photo.UserId == userId || userRole == "admin")
                {
                    _context.Photos.Remove(photo);
                    await _context.SaveChangesAsync();
                    return photo;
                }
                else
                {
                    return Forbid();
                }
            }
            else
            {
                return NotFound();
            }
        }
    }
}
