using KatyaKatya.Localization;

namespace KatyaKatya.Helpers;

public static class ErrorResolver
{
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
            "USER_NOT_FOUND" => "Error_UNKNOWN",
            "USER_USERNAME_EMPTY" => "Validation_USERNAME_EMPTY",
            "USER_USERNAME_TOO_LONG" => "Error_UNKNOWN",
            "USER_NAME_TOO_LONG" => "Error_UNKNOWN",
            "USER_LAST_NAME_TOO_LONG" => "Error_UNKNOWN",
            "USER_AVATAR_NULL" => "Error_UNKNOWN",
            "USER_GUEST_CANNOT_CHANGE_PASSWORD" => "Error_UNKNOWN",
            "USER_EMAIL_ALREADY_VERIFIED" => "Error_UNKNOWN",
            "USER_NOT_A_GUEST" => "Error_UNKNOWN",
            "USER_PASSWORD_INCORRECT" => "Error_UNKNOWN",

            // SOCIAL
            "SOCIAL_FRIEND_REQUEST_NOT_FOUND" => "Error_UNKNOWN",
            "SOCIAL_FRIEND_REQUEST_ALREADY_SENT" => "Error_UNKNOWN",
            "SOCIAL_ALREADY_FRIENDS" => "Error_UNKNOWN",
            "SOCIAL_NOT_FRIENDS" => "Error_UNKNOWN",
            "SOCIAL_NETWORK_NOT_FOUND" => "Error_UNKNOWN",

            // LOBBY
            "LOBBY_NOT_FOUND" => "Error_LOBBY_NOT_FOUND",
            "LOBBY_FULL" => "Error_LOBBY_FULL",
            "LOBBY_GAME_IN_PROGRESS" => "Error_LOBBY_GAME_IN_PROGRESS",
            "LOBBY_NOT_ENOUGH_PLAYERS" => "Error_LOBBY_NOT_ENOUGH_PLAYERS",
            "LOBBY_CODE_TAKEN" => "Error_LOBBY_CODE_TAKEN",
            "LOBBY_NOT_IN_LOBBY" => "Error_LOBBY_NOT_IN_LOBBY",

            // VALIDATION
            "VALIDATION_USERNAME_REQUIRED" => "Validation_USERNAME_EMPTY",
            "VALIDATION_USERNAME_EMPTY" => "Validation_USERNAME_EMPTY",
            "VALIDATION_PASSWORD_REQUIRED" => "Error_UNKNOWN",
            "VALIDATION_EMAIL_REQUIRED" => "Error_UNKNOWN",
            "VALIDATION_EMAIL_INVALID_FORMAT" => "Error_UNKNOWN",
            "VALIDATION_PIN_REQUIRED" => "Error_UNKNOWN",
            "VALIDATION_AVATAR_REQUIRED" => "Error_UNKNOWN",

            // CLIENT-SIDE / GENERIC
            "CONNECTION_ERROR" => "Error_CONNECTION_ERROR",
            _ => "Error_UNKNOWN"
        };

        return LocalizationManager.Instance[key];
    }
}
