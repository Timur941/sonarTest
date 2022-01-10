using mapapi.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace mapapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UprofileImagesController : Controller
    {
        private readonly GeosocdbContext _context;
        private readonly IConfiguration _configuration;

        public UprofileImagesController(GeosocdbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Get detailed information about profile image
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Specified image was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(UprofileImg), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Route("{id}")]
        [HttpGet]
        public async Task<ActionResult<UprofileImg>> GetImageDetails(int id)
        {
            var profileImage = await _context.UprofileImgs
                .FirstOrDefaultAsync(s => s.IdUprofileImg == id);
            if (profileImage == null)
            {
                return Ok(new
                {
                    IdUprofileImg = (string) null,
                    UserId = (string) null,
                    IsCurrent = true,
                    UploadedDate = (string) null,
                    Description = "default",
                    ContentType = "image/png"
                });
            }
            return profileImage;
        }

        /// <summary>
        /// Get original profile image file for user
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Profile image was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(File), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Route("[action]/{userId}")]
        [HttpGet]
        public IActionResult GetImgOrig(int userId)
        {
            var profileImage = _context.UprofileImgs
                .FirstOrDefault(s => s.UserId == userId && s.IsCurrent == true);
            if (profileImage == null)
            {
                var defaultProfileImage = System.IO.File.OpenRead("./wwwroot/img/thumb/default.png");
                return File(defaultProfileImage, "image/png");
            }
            return File(profileImage.OrigImgData, profileImage.ContentType);
        }

        /// <summary>
        /// Get preview profile image file for user
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="404">Profile image was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(File), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Route("[action]/{userId}")]
        [HttpGet]
        public IActionResult GetImgThumb(int userId)
        {
            var profileImage = _context.UprofileImgs
                .FirstOrDefault(s => s.UserId == userId && s.IsCurrent == true);
            if (profileImage == null)
            {
                var defaultProfileImage = System.IO.File.OpenRead("./wwwroot/img/thumb/default.png");
                return File(defaultProfileImage, "image/png");
            }
            return File(profileImage.ThumbnailImgData, "image/jpeg");
        }

        /// <summary>
        /// Upload profile image
        /// </summary>
        /// <remarks>
        /// Sample for jsonString:
        ///
        ///     {
        ///         "userId": 1,
        ///         "description": "profilePhotoDesc"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Request processed successfully</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(UprofileImg), 200)]
        [ProducesResponseType(500)]
        [Authorize]
        [HttpPost("UploadProfileImg")]
        public async Task<IActionResult> UploadProfileImg(IFormFile file, [FromForm] string jsonString)
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

            UprofileImg profileImage = JsonConvert.DeserializeObject<UprofileImg>(jsonString);
            byte[] origImageBytes;
            byte[] thumbnailImageBytes;
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                thumbnailImageBytes = ImageThumb.GetReducedImage(200, 200, ms);
                origImageBytes = ms.ToArray();
            }

            profileImage.OrigImgData = origImageBytes;
            profileImage.ThumbnailImgData = thumbnailImageBytes;
            profileImage.UploadedDate = DateTime.UtcNow;
            profileImage.ContentType = contentType;
            profileImage.IsCurrent = true;
            profileImage.UserId = userId;

            _context.UprofileImgs.Add(profileImage);
            UprofileImg prevProfileImage = _context.UprofileImgs
                .FirstOrDefault(s => s.UserId == profileImage.UserId && s.IsCurrent == true);
            if(prevProfileImage != null)
                prevProfileImage.IsCurrent = false;
            //_context.Entry(prevProfileImage).Property(x => x.IsCurrent).IsModified = true;

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetImageDetails", new { id = profileImage.IdUprofileImg }, profileImage);
        }

        /// <summary>
        /// Delete profile image
        /// </summary>
        /// <response code="200">Request processed successfully</response>
        /// <response code="401">Unathorized</response>
        /// <response code="403">Forbidden. Not allowed to delete this image</response>
        /// <response code="404">Specified photo was not found</response>
        /// <response code="500">An unexpected error was encountered on the server side; more detailed error code is provided.</response>
        [ProducesResponseType(typeof(UprofileImg), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "admin,user")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<UprofileImg>> DeleteUProfileImg(int id)
        {
            string userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            int userId = Int32.Parse(User.FindFirst("id").Value);
            var profileImage = await _context.UprofileImgs.FindAsync(id);
            if (profileImage != null)
            {
                if (profileImage.UserId == userId || userRole == "admin")
                {
                    _context.UprofileImgs.Remove(profileImage);
                    await _context.SaveChangesAsync();
                    return profileImage;
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
