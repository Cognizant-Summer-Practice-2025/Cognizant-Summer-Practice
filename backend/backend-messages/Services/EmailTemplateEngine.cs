using BackendMessages.Models;
using BackendMessages.Models.Email;
using BackendMessages.Services.Abstractions;

namespace BackendMessages.Services
{
    public class EmailTemplateEngine : IEmailTemplateEngine
    {
        public (string htmlBody, string textBody) GenerateUnreadMessagesTemplate(UnreadMessagesNotification notification)
        {
            var senderNamesText = notification.SenderNames?.Count > 0 
                ? string.Join(", ", notification.SenderNames) 
                : "various users";

            var htmlBody = CreateUnreadMessagesHtml(senderNamesText, notification.UnreadCount);
            var textBody = CreateUnreadMessagesText(senderNamesText, notification.UnreadCount);

            return (htmlBody, textBody);
        }

        public (string htmlBody, string textBody) GenerateContactRequestTemplate(ContactRequestNotification notification)
        {
            var htmlBody = CreateContactRequestHtml(notification.Sender);
            var textBody = CreateContactRequestText(notification.Sender);

            return (htmlBody, textBody);
        }

        private static string CreateUnreadMessagesHtml(string senderNamesText, int unreadCount)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                </head>
                <body style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 40px auto; background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); overflow: hidden;'>
                        <div style='background: linear-gradient(135deg, #ff6b6b 0%, #ee5a24 100%); padding: 30px; text-align: center;'>
                            <h1 style='color: #ffffff; margin: 0; font-size: 28px; font-weight: 300; letter-spacing: 1px;'>📬 Unread Messages</h1>
                        </div>
                        
                        <div style='padding: 40px 30px;'>
                            <div style='background-color: #f8f9ff; border-radius: 8px; padding: 25px; margin-bottom: 20px; border-left: 4px solid #667eea;'>
                                <div style='display: flex; align-items: center; margin-bottom: 15px;'>
                                    <div style='width: 40px; height: 40px; background-color: #667eea; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin-right: 15px;'>
                                        <span style='color: white; font-weight: bold; font-size: 16px;'></span>
                                    </div>
                                    <div style='flex: 1;'>
                                        <p style='margin: 0; color: #666; font-size: 14px; text-transform: uppercase; letter-spacing: 0.5px;'>FROM</p>
                                        <p style='margin: 5px 0 0 0; color: #333; font-size: 16px; font-weight: 600; word-wrap: break-word;'>{senderNamesText}</p>
                                    </div>
                                </div>
                            </div>
                            
                            <div style='background-color: #fff5f5; border-radius: 8px; padding: 25px; border-left: 4px solid #ff6b6b;'>
                                <div style='display: flex; align-items: center;'>
                                    <div style='width: 40px; height: 40px; background-color: #ff6b6b; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin-right: 15px;'>
                                        <span style='color: white; font-weight: bold; font-size: 16px;'></span>
                                    </div>
                                    <div>
                                        <p style='margin: 0; color: #666; font-size: 14px; text-transform: uppercase; letter-spacing: 0.5px;'>UNREAD MESSAGES</p>
                                        <p style='margin: 5px 0 0 0; color: #333; font-size: 24px; font-weight: 700;'>{unreadCount}</p>
                                    </div>
                                </div>
                            </div>
                        </div>
                        
                        <div style='background-color: #f8f9fa; padding: 20px 30px; text-align: center; border-top: 1px solid #e9ecef;'>
                            <p style='margin: 0; color: #6c757d; font-size: 12px;'>Twice daily digest • {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private static string CreateUnreadMessagesText(string senderNamesText, int unreadCount)
        {
            return $@"
UNREAD MESSAGES DIGEST

FROM: {senderNamesText}
UNREAD MESSAGES: {unreadCount}
";
        }

        private static string CreateContactRequestHtml(SearchUser sender)
        {
            var fullNameDisplay = !string.IsNullOrEmpty(sender.FullName) && sender.FullName != sender.Username 
                ? $"<p style='margin: 2px 0 0 0; color: #666; font-size: 14px;'>{sender.FullName}</p>" 
                : "";

            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                </head>
                <body style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 40px auto; background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); overflow: hidden;'>
                        <div style='background: linear-gradient(135deg, #4CAF50 0%, #45a049 100%); padding: 30px; text-align: center;'>
                            <h1 style='color: #ffffff; margin: 0; font-size: 28px; font-weight: 300; letter-spacing: 1px;'> Contact Request</h1>
                        </div>
                        
                        <div style='padding: 40px 30px;'>
                            <div style='text-align: center; margin-bottom: 30px;'>
                                <h2 style='color: #333; margin: 0 0 10px 0; font-size: 24px;'>{sender.Username} wants to contact you</h2>
                                <p style='color: #666; margin: 0; font-size: 16px;'>Someone new would like to start a conversation with you.</p>
                            </div>
                            
                            <div style='background-color: #f8f9ff; border-radius: 8px; padding: 25px; margin-bottom: 20px; border-left: 4px solid #4CAF50;'>
                                <div style='display: flex; align-items: center; margin-bottom: 15px;'>
                                    <div style='width: 50px; height: 50px; background-color: #4CAF50; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin-right: 20px;'>
                                        <span style='color: white; font-weight: bold; font-size: 20px;'></span>
                                    </div>
                                    <div>
                                        <p style='margin: 0; color: #666; font-size: 14px; text-transform: uppercase; letter-spacing: 0.5px;'>FROM</p>
                                        <p style='margin: 5px 0 0 0; color: #333; font-size: 20px; font-weight: 600;'>{sender.Username}</p>
                                        {fullNameDisplay}
                                    </div>
                                </div>
                            </div>
                            
                            <div style='text-align: center; margin: 30px 0;'>
                                <p style='color: #666; font-size: 16px; line-height: 1.5;'>
                                    This person has started a new conversation with you. 
                                    Log in to your account to view their message and respond.
                                </p>
                            </div>
                        </div>
                        
                        <div style='background-color: #f8f9fa; padding: 20px 30px; text-align: center; border-top: 1px solid #e9ecef;'>
                            <p style='margin: 0; color: #6c757d; font-size: 12px;'>Contact request • {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private static string CreateContactRequestText(SearchUser sender)
        {
            var fullNameText = !string.IsNullOrEmpty(sender.FullName) && sender.FullName != sender.Username 
                ? $" ({sender.FullName})" 
                : "";

            return $@"
CONTACT REQUEST

{sender.Username} wants to contact you

FROM: {sender.Username}{fullNameText}

This person has started a new conversation with you. 
Log in to your account to view their message and respond.";
        }
    }
} 