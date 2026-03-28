using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users.ValueObjects;

namespace MemoryGame.Domain.Users;

public class User : BaseEntity
{
    public string Username { get; private set; } = null!;
    public string? Name { get; private set; }
    public string? LastName { get; private set; }
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public byte[]? Avatar { get; private set; }
    public bool IsGuest { get; private set; }
    public bool VerifiedEmail { get; private set; }
    public DateTime RegistrationDate { get; private set; }

    private User() { }

    public static User CreateRegistered(string username, string email, string passwordHash)
    {
        return new User
        {
            Username = ValidateUsername(username),
            Email = Email.Create(email),
            PasswordHash = passwordHash,
            IsGuest = false,
            VerifiedEmail = false,
            RegistrationDate = DateTime.UtcNow
        };
    }

    public static User CreateGuest(string username)
    {
        return new User
        {
            Username = ValidateUsername(username),
            Email = Email.Create($"{username}@guest.memorygame"),
            PasswordHash = string.Empty,
            IsGuest = true,
            VerifiedEmail = false,
            RegistrationDate = DateTime.UtcNow
        };
    }

    public void ChangeUsername(string newUsername)
    {
        Username = ValidateUsername(newUsername);
    }

    public void UpdatePersonalInfo(string? name, string? lastName)
    {
        if (name is not null && name.Length > 50)
            throw new DomainException("Name cannot exceed 50 characters.");

        if (lastName is not null && lastName.Length > 50)
            throw new DomainException("Last name cannot exceed 50 characters.");

        Name = name;
        LastName = lastName;
    }

    public void UpdateAvatar(byte[] avatar)
    {
        Avatar = avatar ?? throw new DomainException("Avatar cannot be null.");
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (IsGuest)
            throw new DomainException("Guest users cannot change password.");

        PasswordHash = newPasswordHash;
    }

    public void VerifyEmail()
    {
        if (VerifiedEmail)
            throw new DomainException("Email is already verified.");

        VerifiedEmail = true;
    }

    public void PromoteFromGuest(string email, string passwordHash)
    {
        if (!IsGuest)
            throw new DomainException("User is not a guest.");

        Email = Email.Create(email);
        PasswordHash = passwordHash;
        IsGuest = false;
    }

    private static string ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("Username cannot be empty.");

        if (username.Length > 30)
            throw new DomainException("Username cannot exceed 30 characters.");

        return username;
    }
}
