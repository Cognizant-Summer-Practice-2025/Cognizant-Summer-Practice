using BackendMessages.Models.Email;

namespace BackendMessages.Services.Abstractions
{
    public interface ISmtpClientService
    {
        Task<bool> SendEmailAsync(EmailMessage emailMessage);
    }
} 