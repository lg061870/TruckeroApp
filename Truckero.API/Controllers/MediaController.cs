using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Truckero.Core.DTOs.Media;
using Truckero.Core.Interfaces.Services;

namespace Truckero.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly IMediaService _mediaService;
        private readonly ILogger<MediaController> _logger;

        public MediaController(IMediaService mediaService, ILogger<MediaController> logger)
        {
            _mediaService = mediaService;
            _logger = logger;
        }

        /// <summary>
        /// Uploads an image file
        /// </summary>
        /// <param name="fileType">Type of file being uploaded (e.g., profile, license_front)</param>
        /// <param name="file">The image file to upload</param>
        /// <returns>Response with URL if successful</returns>
        [HttpPost("upload")]
        [Authorize]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
        public async Task<ActionResult<ImageUploadResponse>> UploadImage([FromForm] string fileType, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(ImageUploadResponse.Failed("No file was uploaded"));
                }

                // Create the upload request
                var request = new ImageUploadRequest
                {
                    FileType = fileType,
                    FileName = file.FileName,
                    FileStream = file.OpenReadStream()
                };

                // Process the upload
                var result = await _mediaService.UploadImageAsync(request);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return StatusCode(500, ImageUploadResponse.Failed("An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Deletes an image by its file ID
        /// </summary>
        /// <param name="fileId">The ID of the file to delete</param>
        /// <returns>Success or failure status</returns>
        [HttpDelete("{fileId}")]
        [Authorize]
        public async Task<ActionResult> DeleteImage(string fileId)
        {
            try
            {
                var result = await _mediaService.DeleteImageAsync(fileId);
                if (result)
                {
                    return Ok(new { success = true, message = "Image deleted successfully" });
                }
                else
                {
                    return NotFound(new { success = false, message = "Image not found or could not be deleted" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {FileId}", fileId);
                return StatusCode(500, new { success = false, message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Gets a temporary URL for an image
        /// </summary>
        /// <param name="fileId">The ID of the file</param>
        /// <param name="expirationMinutes">Number of minutes until the URL expires (default: 60)</param>
        /// <returns>Temporary URL to access the image</returns>
        [HttpGet("url/{fileId}")]
        [Authorize]
        public async Task<ActionResult<string>> GetTemporaryUrl(string fileId, [FromQuery] int expirationMinutes = 60)
        {
            try
            {
                var url = await _mediaService.GetTemporaryUrlAsync(fileId, expirationMinutes);
                return Ok(new { url });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating temporary URL: {FileId}", fileId);
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }
    }
}
