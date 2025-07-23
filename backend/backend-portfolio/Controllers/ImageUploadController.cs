using backend_portfolio.DTO.Request;
using backend_portfolio.DTO.Response;
using backend_portfolio.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend_portfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageUploadController : ControllerBase
    {
        private readonly ImageUploadUtility _imageUploadUtility;
        private readonly ILogger<ImageUploadController> _logger;

        public ImageUploadController(
            ImageUploadUtility imageUploadUtility,
            ILogger<ImageUploadController> logger)
        {
            _imageUploadUtility = imageUploadUtility;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile imageFile, [FromForm] string subfolder)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                {
                    return BadRequest(new ImageUploadErrorResponse
                    {
                        Error = "InvalidFile",
                        Message = "No file was uploaded or file is empty",
                        SupportedFormats = new List<string> { ".jpg", ".jpeg", ".png", ".gif", ".webp" },
                        SupportedSubfolders = _imageUploadUtility.GetSupportedSubfolders()
                    });
                }

                if (string.IsNullOrWhiteSpace(subfolder))
                {
                    return BadRequest(new ImageUploadErrorResponse
                    {
                        Error = "InvalidSubfolder",
                        Message = "Subfolder is required",
                        SupportedSubfolders = _imageUploadUtility.GetSupportedSubfolders()
                    });
                }

                if (!_imageUploadUtility.IsValidSubfolder(subfolder))
                {
                    return BadRequest(new ImageUploadErrorResponse
                    {
                        Error = "UnsupportedSubfolder",
                        Message = $"Subfolder '{subfolder}' is not supported",
                        SupportedSubfolders = _imageUploadUtility.GetSupportedSubfolders()
                    });
                }

                var imagePath = await _imageUploadUtility.SaveImageAsync(imageFile, subfolder);

                var response = new ImageUploadResponse
                {
                    ImagePath = imagePath,
                    FileName = Path.GetFileName(imagePath),
                    Subfolder = subfolder,
                    FileSize = imageFile.Length,
                    UploadedAt = DateTime.UtcNow,
                    Message = "Image uploaded successfully"
                };

                _logger.LogInformation("Image uploaded successfully: {ImagePath}", imagePath);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed during image upload");
                return BadRequest(new ImageUploadErrorResponse
                {
                    Error = "ValidationFailed",
                    Message = ex.Message,
                    SupportedFormats = new List<string> { ".jpg", ".jpeg", ".png", ".gif", ".webp" },
                    SupportedSubfolders = _imageUploadUtility.GetSupportedSubfolders()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during image upload");
                return StatusCode(500, new ImageUploadErrorResponse
                {
                    Error = "InternalServerError",
                    Message = "An error occurred while uploading the image"
                });
            }
        }

        [HttpDelete("delete")]
        public IActionResult DeleteImage([FromQuery] string imagePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imagePath))
                {
                    return BadRequest(new { message = "Image path is required" });
                }

                var deleted = _imageUploadUtility.DeleteImage(imagePath);
                
                if (deleted)
                {
                    _logger.LogInformation("Image deleted successfully: {ImagePath}", imagePath);
                    return Ok(new { message = "Image deleted successfully" });
                }
                else
                {
                    return NotFound(new { message = "Image not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting image: {ImagePath}", imagePath);
                return StatusCode(500, new { message = "An error occurred while deleting the image" });
            }
        }

        [HttpGet("supported-subfolders")]
        public IActionResult GetSupportedSubfolders()
        {
            try
            {
                var subfolders = _imageUploadUtility.GetSupportedSubfolders();
                return Ok(new { subfolders });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting supported subfolders");
                return StatusCode(500, new { message = "An error occurred while getting supported subfolders" });
            }
        }

        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { 
                status = "healthy", 
                service = "ImageUploadController",
                timestamp = DateTime.UtcNow 
            });
        }
    }
} 