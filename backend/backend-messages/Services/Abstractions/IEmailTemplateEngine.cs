using BackendMessages.Models.Email;

namespace BackendMessages.Services.Abstractions
{
    public interface IEmailTemplateEngine
    {
        (string htmlBody, string textBody) GenerateUnreadMessagesTemplate(UnreadMessagesNotification notification);
        (string htmlBody, string textBody) GenerateContactRequestTemplate(ContactRequestNotification notification);
    }
} 