using System.Drawing;
using System.Drawing.Imaging;

namespace backend_portfolio.Services
{
    public class ImageUploadUtility
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

            // Validate that the file is actually an image
            try
            {
                using var stream = imageFile.OpenReadStream();
                using var image = Image.FromStream(stream);
                // If we get here, it's a valid image
            }
            catch
            {
                throw new ArgumentException("The uploaded file is not a valid image");
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