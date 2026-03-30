namespace MemoryGame.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendVerificationPinAsync(string toEmail, string pin);
    Task SendGuestUpgradeVerificationAsync(string toEmail, string pin);

    /// <summary>
    /// Sends a lobby invitation email to a player who is not currently online.
    /// </summary>
    Task SendLobbyInviteAsync(string toEmail, string toUsername, string fromUsername, string gameCode);
}
