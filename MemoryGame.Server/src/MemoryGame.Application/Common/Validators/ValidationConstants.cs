namespace MemoryGame.Application.Common.Validators;

/// <summary>
/// Centralized validation constants shared across all command validators.
/// </summary>
public static class ValidationConstants
{
    public static class Email
    {
        public const int MaxLength = 50;
        public const string Pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
    }

    public static class Password
    {
        public const int MinLength = 8;
        public const int MaxLength = 100;
    }

    public static class Username
    {
        public const int MinLength = 3;
        public const int MaxLength = 30;
        public const string Pattern = @"^[a-zA-Z0-9_\-]+$";
    }

    public static class PersonalInfo
    {
        public const int NameMaxLength = 50;
    }

    public static class Pin
    {
        public const int Length = 6;
        public const string Pattern = @"^\d{6}$";
    }

    public static class Avatar
    {
        public const int MaxSizeBytes = 5 * 1024 * 1024;
    }

    public static class SocialAccount
    {
        public const int MaxLength = 50;
    }
}
