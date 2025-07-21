namespace backend_portfolio.Services.Abstractions
{
    public interface IExternalUserService
    {
        Task<UserInformation?> GetUserInformationAsync(Guid userId);
    }

    public record UserInformation(
        string FullName,
        string JobTitle, 
        string Location,
        string ProfilePictureUrl
    );
} 