using Microsoft.AspNetCore.Http;

namespace backend_portfolio.Services.Abstractions
{
    public interface IImageUploadUtility
    {
        Task<string> SaveImageAsync(IFormFile imageFile, string subfolder);
        bool DeleteImage(string imagePath);
        List<string> GetSupportedSubfolders();
        bool IsValidSubfolder(string subfolder);
    }
} 