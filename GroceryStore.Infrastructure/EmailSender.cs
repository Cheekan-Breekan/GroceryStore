using GroceryStore.Core.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace GroceryStore.Infrastructure;
public class EmailSender(ILogger<EmailSender> logger) : IEmailSender
{
    public async Task<bool> SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            using var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Администрация сайта TestAppMarket", "SegaKupr@mail.ru"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = htmlMessage
            };
            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync("smtp.mail.ru", 465, SecureSocketOptions.SslOnConnect);
                await client.AuthenticateAsync("SegaKupr@mail.ru", "ygCETkpJ5vtvdyVMuRXt");
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return false;
        }
    }
}
