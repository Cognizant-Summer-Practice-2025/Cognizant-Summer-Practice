using backend_portfolio.Services.Abstractions;

namespace backend_portfolio.Services
{
    public class ImageUploadUtility : IImageUploadUtility
    {
        private readonly ILogger<ImageUploadUtility> _logger;
        private readonly string _serverPath;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

        public ImageUploadUtility(ILogger<ImageUploadUtility> logger, IConfiguration configuration)
        {
            _logger = logger;
            _serverPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "server", "portfolio");
        }

        public async Task<string> SaveImageAsync(IFormFile imageFile, string subfolder)
        {
            try
            {
                ValidateImageFile(imageFile);

                // Create subfolder if it doesn't exist
                var targetDirectory = Path.Combine(_serverPath, subfolder);
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                    _logger.LogInformation("Created directory: {Directory}", targetDirectory);
                }

                // Generate random filename with original extension
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                var randomFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(targetDirectory, randomFileName);

                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Return the relative path that can be used to access the image
                var relativePath = $"server/portfolio/{subfolder}/{randomFileName}";
                
                _logger.LogInformation("Image saved successfully: {FilePath}", relativePath);
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving image");
                throw;
            }
        }

        public bool DeleteImage(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    return false;

                // Convert relative path to absolute path
                var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", imagePath);
                
                if (File.Exists(absolutePath))
                {
                    File.Delete(absolutePath);
                    _logger.LogInformation("Image deleted successfully: {FilePath}", imagePath);
                    return true;
                }

                _logger.LogWarning("Image file not found: {FilePath}", imagePath);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting image: {FilePath}", imagePath);
                return false;
            }
        }

        private void ValidateImageFile(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                throw new ArgumentException("No file was uploaded or file is empty");

            if (imageFile.Length > _maxFileSize)
                throw new ArgumentException($"File size exceeds maximum allowed size of {_maxFileSize / (1024 * 1024)}MB");

            var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(fileExtension))
                throw new ArgumentException($"File type {fileExtension} is not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");

            // Validate that the file is actually an image by checking magic bytes
            if (!IsValidImageFile(imageFile))
            {
                throw new ArgumentException("The uploaded file is not a valid image");
            }
        }

        private bool IsValidImageFile(IFormFile imageFile)
        {
            try
            {
                using var stream = imageFile.OpenReadStream();
                
                // Read the first few bytes to check magic numbers
                var buffer = new byte[12];
                var bytesRead = stream.Read(buffer, 0, buffer.Length);
                
                if (bytesRead < 4)
                    return false;

                // Check for common image file signatures (magic bytes)
                
                // JPEG: FF D8 FF
                if (buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF)
                    return true;

                // PNG: 89 50 4E 47 0D 0A 1A 0A
                if (bytesRead >= 8 && 
                    buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47 &&
                    buffer[4] == 0x0D && buffer[5] == 0x0A && buffer[6] == 0x1A && buffer[7] == 0x0A)
                    return true;

                // GIF: "GIF8"
                if (bytesRead >= 4 && 
                    buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x38)
                    return true;

                // WebP: "RIFF" followed by "WEBP" at offset 8
                if (bytesRead >= 12 &&
                    buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46 &&
                    buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error validating image file");
                return false;
            }
        }

        public List<string> GetSupportedSubfolders()
        {
            return new List<string> { "blog_posts", "projects", "profile_images" };
        }

        public bool IsValidSubfolder(string subfolder)
        {
            return GetSupportedSubfolders().Contains(subfolder, StringComparer.OrdinalIgnoreCase);
        }
    }
} 