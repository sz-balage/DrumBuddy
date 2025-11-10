using System.Net;
using System.Net.Mail;
using DrumBuddy.Endpoint.Configuration;

namespace DrumBuddy.Endpoint.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _senderEmail;
    private readonly string _senderPassword;
    private readonly int _smtpPort;
    private readonly string _smtpHost;
    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        _smtpHost = _configuration["EmailSettings:SmtpHost"];
        _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
        _senderEmail = Secrets.SenderEmail;
        _senderPassword = Secrets.SenderPassword;
    }
    

    public async Task<bool> SendPasswordResetEmailAsync(string email, string resetLink)
    {
        try
        {
            using (var client = new SmtpClient(_smtpHost, _smtpPort))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(_senderEmail, _senderPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_senderEmail, "DrumBuddy"),
                    Subject = "Reset Your Password",
                    Body = $@"
                        <html>
                            <body>
                                <h2>Password Reset Request</h2>
                                <p>Click the link below to reset your password:</p>
                                <p><a href='{resetLink}'>Reset Password</a></p>
                                <p>This link expires in 24 hours.</p>
                                <p>If you didn't request this, ignore this email.</p>
                            </body>
                        </html>
                    ",
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Password reset email sent to {email}");
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending email to {email}: {ex.Message}");
            return false;
        }
    }
}
