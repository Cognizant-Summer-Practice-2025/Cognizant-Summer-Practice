namespace backend_portfolio.DTO.Response
{
    /// <summary>
    /// DTO for image upload response
    /// </summary>
    public class ImageUploadResponse
    {
        public string ImagePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Subfolder { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public string Message { get; set; } = "Image uploaded successfully";
    }

    /// <summary>
    /// DTO for image upload error response
    /// </summary>
    public class ImageUploadErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public List<string> SupportedFormats { get; set; } = new();
        public List<string> SupportedSubfolders { get; set; } = new();
    }
} 