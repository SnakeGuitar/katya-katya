using MailKit.Net.Smtp;
using MailKit.Security;
using MemoryGame.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace MemoryGame.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _user;
    private readonly string _password;
    private readonly string _from;

    public EmailService(IConfiguration configuration)
    {
        _host     = configuration["SMTP_HOST"]     ?? throw new InvalidOperationException("SMTP_HOST not configured.");
        _port     = int.Parse(configuration["SMTP_PORT"] ?? "587");
        _user     = configuration["SMTP_USER"]     ?? throw new InvalidOperationException("SMTP_USER not configured.");
        _password = configuration["SMTP_PASSWORD"] ?? throw new InvalidOperationException("SMTP_PASSWORD not configured.");
        _from     = _user;
    }

    public async Task SendVerificationPinAsync(string toEmail, string pin)
    {
        var subject = "Memory Game — Verify your account";
        var body    = $"Your verification PIN is: <strong>{pin}</strong><br/>It expires in 15 minutes.";
        await SendAsync(toEmail, subject, body);
    }

    public async Task SendGuestUpgradeVerificationAsync(string toEmail, string pin)
    {
        var subject = "Memory Game — Complete your registration";
        var body    = $"Your verification PIN to complete registration is: <strong>{pin}</strong><br/>It expires in 15 minutes.";
        await SendAsync(toEmail, subject, body);
    }

    public async Task SendLobbyInviteAsync(string toEmail, string toUsername, string fromUsername, string gameCode)
    {
        var subject = $"Memory Game — {fromUsername} invited you to a game";
        var body    = $"Hi <strong>{toUsername}</strong>,<br/><br/>" +
                      $"<strong>{fromUsername}</strong> has invited you to join their lobby.<br/>" +
                      $"Use game code: <strong>{gameCode}</strong><br/><br/>" +
                      "Open the game and enter the code in <em>Join Lobby</em> to play.";
        await SendAsync(toEmail, subject, body);
    }

    private async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_from));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body    = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_user, _password);
        await client.SendAsync(message);
        await client.DisconnectAsync(quit: true);
    }
}
