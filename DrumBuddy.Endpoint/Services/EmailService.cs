using System.Net;
using System.Net.Mail;

namespace DrumBuddy.Endpoint.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly string _senderEmail;
    private readonly string _senderPassword;
    private readonly int _smtpPort;
    private readonly string _smtpHost;
    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        _smtpHost = _configuration["EmailSettings:SmtpHost"] ?? throw new InvalidOperationException("SMTP Host is not configured.");
        _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
        _senderEmail = _configuration["SENDER_EMAIL"] ?? throw new InvalidOperationException("Sender email is not configured.");
        _senderPassword = _configuration["SENDER_PASSWORD"] ?? throw new InvalidOperationException("Sender password is not configured.");
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
                client.Timeout = 10000;
                await client.SendMailAsync(mailMessage);
                return true;
            }
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}
