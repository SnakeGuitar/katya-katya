namespace MemoryGame.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendVerificationPinAsync(string toEmail, string pin);
    Task SendGuestUpgradeVerificationAsync(string toEmail, string pin);
}
