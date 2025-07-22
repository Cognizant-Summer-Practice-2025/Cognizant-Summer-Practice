using System.ComponentModel.DataAnnotations;

namespace backend_portfolio.DTO.Request
{
    /// <summary>
    /// DTO for image upload request
    /// </summary>
    public class ImageUploadRequest
    {
        [Required]
        public IFormFile ImageFile { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Subfolder { get; set; } = string.Empty;
    }
} 