namespace MemoryGame.Domain.Common;

/// <summary>
/// Stable error codes used in <see cref="DomainException"/> for client-side i18n.
/// Each constant maps to a translation key on the client.
/// </summary>
public static class DomainErrors
{
    /// <summary>Authentication and registration errors.</summary>
    public static class Auth
    {
        public const string EmailAlreadyRegistered     = "AUTH_EMAIL_ALREADY_REGISTERED";
        public const string UsernameAlreadyTaken       = "AUTH_USERNAME_ALREADY_TAKEN";
        public const string InvalidCredentials         = "AUTH_INVALID_CREDENTIALS";
        public const string GuestCannotLogin           = "AUTH_GUEST_CANNOT_LOGIN";
        public const string EmailNotVerified           = "AUTH_EMAIL_NOT_VERIFIED";
        public const string RegistrationNotFound       = "AUTH_REGISTRATION_NOT_FOUND";
        public const string PinInvalid                 = "AUTH_PIN_INVALID";
        public const string RefreshTokenInvalid        = "AUTH_REFRESH_TOKEN_INVALID";
        public const string EmailAlreadyInUse          = "AUTH_EMAIL_ALREADY_IN_USE";
    }

    /// <summary>User aggregate errors.</summary>
    public static class User
    {
        public const string NotFound                   = "USER_NOT_FOUND";
        public const string UsernameEmpty              = "USER_USERNAME_EMPTY";
        public const string UsernameTooLong            = "USER_USERNAME_TOO_LONG";
        public const string NameTooLong                = "USER_NAME_TOO_LONG";
        public const string LastNameTooLong            = "USER_LAST_NAME_TOO_LONG";
        public const string AvatarNull                 = "USER_AVATAR_NULL";
        public const string GuestCannotChangePassword  = "USER_GUEST_CANNOT_CHANGE_PASSWORD";
        public const string EmailAlreadyVerified       = "USER_EMAIL_ALREADY_VERIFIED";
        public const string NotAGuest                  = "USER_NOT_A_GUEST";
        public const string PasswordIncorrect          = "USER_PASSWORD_INCORRECT";
    }

    /// <summary>Session errors.</summary>
    public static class Session
    {
        public const string TokenEmpty                 = "SESSION_TOKEN_EMPTY";
    }

    /// <summary>Pending registration errors.</summary>
    public static class PendingRegistration
    {
        public const string PinInvalidFormat           = "PENDING_REGISTRATION_PIN_INVALID_FORMAT";
    }

    /// <summary>Social and friendship errors.</summary>
    public static class Social
    {
        public const string FriendRequestNotFound      = "SOCIAL_FRIEND_REQUEST_NOT_FOUND";
        public const string FriendRequestAlreadySent   = "SOCIAL_FRIEND_REQUEST_ALREADY_SENT";
        public const string AlreadyFriends             = "SOCIAL_ALREADY_FRIENDS";
        public const string NotFriends                 = "SOCIAL_NOT_FRIENDS";
        public const string SocialNetworkNotFound      = "SOCIAL_NETWORK_NOT_FOUND";
    }

    /// <summary>Match errors.</summary>
    public static class Match
    {
        public const string NotFound                   = "MATCH_NOT_FOUND";
    }
}
