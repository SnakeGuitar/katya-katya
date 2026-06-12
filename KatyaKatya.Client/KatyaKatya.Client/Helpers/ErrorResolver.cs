using KatyaKatya.Localization;

namespace KatyaKatya.Helpers;

/// <summary>
/// Maps server error codes (from DomainErrors / hub) to LocalizationManager keys.
/// Keeps ViewModels free of raw error-code strings.
/// </summary>
public static class ErrorResolver
{
    /// <summary>
    /// Returns the localized error message for a given server error code.
    /// </summary>
    public static string Resolve(string? errorCode)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
            return LocalizationManager.Instance["Error_UNKNOWN"];

        var key = errorCode switch
        {
            // AUTH
            "AUTH_EMAIL_ALREADY_REGISTERED" => "Error_AUTH_EMAIL_ALREADY_REGISTERED",
            "AUTH_EMAIL_ALREADY_IN_USE" => "Error_AUTH_EMAIL_ALREADY_IN_USE",
            "AUTH_USERNAME_ALREADY_TAKEN" => "Error_AUTH_USERNAME_ALREADY_TAKEN",
            "AUTH_INVALID_CREDENTIALS" => "Error_AUTH_INVALID_CREDENTIALS",
            "AUTH_GUEST_CANNOT_LOGIN" => "Error_AUTH_GUEST_CANNOT_LOGIN",
            "AUTH_EMAIL_NOT_VERIFIED" => "Error_AUTH_EMAIL_NOT_VERIFIED",
            "AUTH_REGISTRATION_NOT_FOUND" => "Error_AUTH_REGISTRATION_NOT_FOUND",
            "AUTH_PIN_INVALID" => "Error_AUTH_PIN_INVALID",
            "AUTH_REFRESH_TOKEN_INVALID" => "Error_AUTH_REFRESH_TOKEN_INVALID",

            // USER
            "USER_NOT_FOUND" => "Error_USER_NOT_FOUND",
            "USER_USERNAME_EMPTY" => "Error_USER_USERNAME_EMPTY",
            "USER_USERNAME_TOO_LONG" => "Error_USER_USERNAME_TOO_LONG",
            "USER_NAME_TOO_LONG" => "Error_USER_NAME_TOO_LONG",
            "USER_LAST_NAME_TOO_LONG" => "Error_USER_LAST_NAME_TOO_LONG",
            "USER_AVATAR_NULL" => "Error_USER_AVATAR_NULL",
            "USER_GUEST_CANNOT_CHANGE_PASSWORD" => "Error_USER_GUEST_CANNOT_CHANGE_PASSWORD",
            "USER_EMAIL_ALREADY_VERIFIED" => "Error_USER_EMAIL_ALREADY_VERIFIED",
            "USER_NOT_A_GUEST" => "Error_USER_NOT_A_GUEST",
            "USER_PASSWORD_INCORRECT" => "Error_USER_PASSWORD_INCORRECT",

            // SESSION
            "SESSION_TOKEN_EMPTY" => "Error_SESSION_TOKEN_EMPTY",
            "PENDING_REGISTRATION_PIN_INVALID_FORMAT" => "Error_PENDING_REGISTRATION_PIN_INVALID_FORMAT",

            // SOCIAL
            "SOCIAL_FRIEND_REQUEST_NOT_FOUND" => "Error_SOCIAL_FRIEND_REQUEST_NOT_FOUND",
            "SOCIAL_FRIEND_REQUEST_ALREADY_SENT" => "Error_SOCIAL_FRIEND_REQUEST_ALREADY_SENT",
            "SOCIAL_ALREADY_FRIENDS" => "Error_SOCIAL_ALREADY_FRIENDS",
            "SOCIAL_NOT_FRIENDS" => "Error_SOCIAL_NOT_FRIENDS",
            "SOCIAL_NETWORK_NOT_FOUND" => "Error_SOCIAL_NETWORK_NOT_FOUND",

            // MATCH
            "MATCH_NOT_FOUND" => "Error_MATCH_NOT_FOUND",

            // HUB / LOBBY
            "LOBBY_NOT_FOUND" => "Error_LOBBY_NOT_FOUND",
            "LOBBY_FULL" => "Error_LOBBY_FULL",
            "LOBBY_GAME_IN_PROGRESS" => "Error_LOBBY_GAME_IN_PROGRESS",
            "LOBBY_NOT_ENOUGH_PLAYERS" => "Error_LOBBY_NOT_ENOUGH_PLAYERS",
            "LOBBY_INVALID_CARD_COUNT" => "Error_LOBBY_INVALID_CARD_COUNT",
            "LOBBY_INVALID_TURN_TIME" => "Error_LOBBY_INVALID_TURN_TIME",
            "LOBBY_CODE_TAKEN" => "Error_LOBBY_CODE_TAKEN",
            "LOBBY_NOT_IN_LOBBY" => "Error_LOBBY_NOT_IN_LOBBY",
            "LOBBY_INVITE_SELF" => "Error_LOBBY_INVITE_SELF",
            "LOBBY_CREATE_FAILED" => "Error_UNKNOWN",

            // VALIDATION (server-side input checks)
            // Where an existing Validation_* resx key matches, route to it.
            // Otherwise fall back to Error_UNKNOWN until a key is added.
            "VALIDATION_USERNAME_REQUIRED" => "Validation_Required",
            "VALIDATION_USERNAME_EMPTY" => "Validation_Required",
            "VALIDATION_USERNAME_TOO_SHORT" => "Validation_UsernameTooShort",
            "VALIDATION_USERNAME_TOO_LONG" => "Validation_UsernameTooLong",
            "VALIDATION_USERNAME_INVALID_CHARACTERS" => "Error_UNKNOWN",
            "VALIDATION_PASSWORD_REQUIRED" => "Validation_Required",
            "VALIDATION_PASSWORD_TOO_SHORT" => "Validation_PasswordTooShort",
            "VALIDATION_PASSWORD_TOO_LONG" => "Validation_PasswordTooLong",
            "VALIDATION_PASSWORD_REQUIRES_UPPERCASE" => "Validation_PasswordMissingUpper",
            "VALIDATION_PASSWORD_REQUIRES_LOWERCASE" => "Validation_PasswordMissingLower",
            "VALIDATION_PASSWORD_REQUIRES_DIGIT" => "Validation_PasswordMissingDigit",
            "VALIDATION_PASSWORD_REQUIRES_SPECIAL" => "Validation_PasswordMissingSpecial",
            "VALIDATION_CURRENT_PASSWORD_REQUIRED" => "Validation_Required",
            "VALIDATION_EMAIL_REQUIRED" => "Validation_Required",
            "VALIDATION_EMAIL_INVALID_FORMAT" => "Validation_InvalidEmail",
            "VALIDATION_EMAIL_TOO_LONG" => "Error_UNKNOWN",
            "VALIDATION_PIN_REQUIRED" => "Validation_Required",
            "VALIDATION_PIN_INVALID_FORMAT" => "Validation_PinLength",
            "VALIDATION_AVATAR_REQUIRED" => "Validation_Required",
            "VALIDATION_AVATAR_INVALID_FORMAT" => "Error_UNKNOWN",
            "VALIDATION_AVATAR_TOO_LARGE" => "Error_UNKNOWN",
            "VALIDATION_NAME_TOO_LONG" => "Error_UNKNOWN",
            "VALIDATION_LAST_NAME_TOO_LONG" => "Error_UNKNOWN",
            "VALIDATION_REFRESH_TOKEN_REQUIRED" => "Validation_Required",
            "VALIDATION_SOCIAL_ACCOUNT_REQUIRED" => "Validation_Required",
            "VALIDATION_SOCIAL_ACCOUNT_TOO_LONG" => "Error_UNKNOWN",
            "VALIDATION_ID_INVALID" => "Error_UNKNOWN",
            "VALIDATION_CANNOT_REPORT_SELF" => "Error_UNKNOWN",
            "VALIDATION_CANNOT_REMOVE_SELF" => "Error_UNKNOWN",

            // CLIENT-SIDE / GENERIC
            "CONNECTION_ERROR" => "Error_CONNECTION_ERROR",
            "INTERNAL_ERROR" => "Error_UNKNOWN",

            _ => "Error_UNKNOWN"
        };

        return LocalizationManager.Instance[key];
    }
}
