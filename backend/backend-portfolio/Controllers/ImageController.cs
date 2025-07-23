using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace backend_portfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly ILogger<ImageController> _logger;
        private readonly string _serverPath;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public ImageController(ILogger<ImageController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _serverPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "server", "portfolio");
        }

        /// <summary>
        /// Serve an image file from the server/portfolio directory
        /// </summary>
        /// <param name="subfolder">The subfolder (blog_posts, projects, profile_images)</param>
        /// <param name="filename">The image filename</param>
        /// <returns>The image file or 404 if not found</returns>
        [HttpGet("{subfolder}/{filename}")]
        public IActionResult GetImage(string subfolder, string filename)
        {
            try
            {
                var allowedSubfolders = new[] { "blog_posts", "projects", "profile_images" };
                if (!allowedSubfolders.Contains(subfolder, StringComparer.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Invalid subfolder requested: {Subfolder}", subfolder);
                    return BadRequest(new { message = "Invalid subfolder" });
                }

                if (string.IsNullOrEmpty(filename) || filename.Contains("..") || filename.Contains("/") || filename.Contains("\\"))
                {
                    _logger.LogWarning("Invalid filename requested: {Filename}", filename);
                    return BadRequest(new { message = "Invalid filename" });
                }

                var fileExtension = Path.GetExtension(filename).ToLowerInvariant();
                if (!_allowedExtensions.Contains(fileExtension))
                {
                    _logger.LogWarning("Invalid file extension requested: {Extension}", fileExtension);
                    return BadRequest(new { message = "Invalid file type" });
                }

                var filePath = Path.Combine(_serverPath, subfolder, filename);

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning("Image file not found: {FilePath}", filePath);
                    return NotFound(new { message = "Image not found" });
                }

                var contentType = GetContentType(fileExtension);

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                
                _logger.LogInformation("Serving image: {Subfolder}/{Filename}", subfolder, filename);
                
                return File(fileBytes, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error serving image: {Subfolder}/{Filename}", subfolder, filename);
                return StatusCode(500, new { message = "Error serving image" });
            }
        }

        /// <summary>
        /// Get image metadata without downloading the full file
        /// </summary>
        /// <param name="subfolder">The subfolder</param>
        /// <param name="filename">The image filename</param>
        /// <returns>Image metadata</returns>
        [HttpHead("{subfolder}/{filename}")]
        public IActionResult GetImageMetadata(string subfolder, string filename)
        {
            try
            {
                var allowedSubfolders = new[] { "blog_posts", "projects", "profile_images" };
                if (!allowedSubfolders.Contains(subfolder, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest();
                }

                if (string.IsNullOrEmpty(filename) || filename.Contains("..") || filename.Contains("/") || filename.Contains("\\"))
                {
                    return BadRequest();
                }

                var fileExtension = Path.GetExtension(filename).ToLowerInvariant();
                if (!_allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest();
                }

                var filePath = Path.Combine(_serverPath, subfolder, filename);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound();
                }

                var fileInfo = new FileInfo(filePath);
                var contentType = GetContentType(fileExtension);

                Response.Headers["Content-Type"] = contentType;
                Response.Headers["Content-Length"] = fileInfo.Length.ToString();
                Response.Headers["Last-Modified"] = fileInfo.LastWriteTimeUtc.ToString("R");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image metadata: {Subfolder}/{Filename}", subfolder, filename);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// List all images in a specific subfolder
        /// </summary>
        /// <param name="subfolder">The subfolder to list</param>
        /// <returns>List of image files</returns>
        [HttpGet("{subfolder}")]
        public IActionResult ListImages(string subfolder)
        {
            try
            {
                var allowedSubfolders = new[] { "blog_posts", "projects", "profile_images" };
                if (!allowedSubfolders.Contains(subfolder, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Invalid subfolder" });
                }

                var subfolderPath = Path.Combine(_serverPath, subfolder);

                if (!Directory.Exists(subfolderPath))
                {
                    return Ok(new { images = new string[0] });
                }

                var imageFiles = Directory.GetFiles(subfolderPath)
                    .Where(file => _allowedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                    .Select(file => new
                    {
                        filename = Path.GetFileName(file),
                        size = new FileInfo(file).Length,
                        lastModified = new FileInfo(file).LastWriteTimeUtc,
                        url = $"/api/Image/{subfolder}/{Path.GetFileName(file)}"
                    })
                    .OrderByDescending(file => file.lastModified)
                    .ToArray();

                return Ok(new { images = imageFiles });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing images in subfolder: {Subfolder}", subfolder);
                return StatusCode(500, new { message = "Error listing images" });
            }
        }

        /// <summary>
        /// Get the MIME content type for a file extension
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <returns>MIME content type</returns>
        private static string GetContentType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
    }
} 