namespace KatyaKatya.Helpers;

public static class ErrorResolver
{
    public static string Resolve(string? errorCode)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
            return "An unknown error occurred. Please try again.";

        return errorCode switch
        {
            // AUTH
            "AUTH_EMAIL_ALREADY_REGISTERED" => "This email is already registered.",
            "AUTH_EMAIL_ALREADY_IN_USE" => "This email is already in use.",
            "AUTH_USERNAME_ALREADY_TAKEN" => "This username is already taken.",
            "AUTH_INVALID_CREDENTIALS" => "Invalid username or password.",
            "AUTH_GUEST_CANNOT_LOGIN" => "Guests cannot log in.",
            "AUTH_EMAIL_NOT_VERIFIED" => "Your email is not verified yet.",
            "AUTH_REGISTRATION_NOT_FOUND" => "Registration session not found.",
            "AUTH_PIN_INVALID" => "Invalid verification PIN.",
            "AUTH_REFRESH_TOKEN_INVALID" => "Session expired. Please log in again.",

            // USER
            "USER_NOT_FOUND" => "User not found.",
            "USER_USERNAME_EMPTY" => "Username cannot be empty.",
            "USER_USERNAME_TOO_LONG" => "Username is too long.",
            "USER_NAME_TOO_LONG" => "First name is too long.",
            "USER_LAST_NAME_TOO_LONG" => "Last name is too long.",
            "USER_AVATAR_NULL" => "Avatar data cannot be empty.",
            "USER_GUEST_CANNOT_CHANGE_PASSWORD" => "Guests cannot change password.",
            "USER_EMAIL_ALREADY_VERIFIED" => "Email is already verified.",
            "USER_NOT_A_GUEST" => "User is not a guest.",
            "USER_PASSWORD_INCORRECT" => "The password you entered is incorrect.",

            // SOCIAL
            "SOCIAL_FRIEND_REQUEST_NOT_FOUND" => "Friend request not found.",
            "SOCIAL_FRIEND_REQUEST_ALREADY_SENT" => "A friend request has already been sent to this user.",
            "SOCIAL_ALREADY_FRIENDS" => "You are already friends with this user.",
            "SOCIAL_NOT_FRIENDS" => "You are not friends with this user.",
            "SOCIAL_NETWORK_NOT_FOUND" => "Social network account not found.",

            // LOBBY
            "LOBBY_NOT_FOUND" => "Lobby not found.",
            "LOBBY_FULL" => "The lobby is full.",
            "LOBBY_GAME_IN_PROGRESS" => "The lobby game is already in progress.",
            "LOBBY_NOT_ENOUGH_PLAYERS" => "Not enough players in the lobby.",
            "LOBBY_CODE_TAKEN" => "This lobby code is already taken.",
            "LOBBY_NOT_IN_LOBBY" => "You are not in a lobby.",

            // VALIDATION
            "VALIDATION_USERNAME_REQUIRED" => "Username is required.",
            "VALIDATION_PASSWORD_REQUIRED" => "Password is required.",
            "VALIDATION_EMAIL_REQUIRED" => "Email is required.",
            "VALIDATION_EMAIL_INVALID_FORMAT" => "Invalid email format.",
            "VALIDATION_PIN_REQUIRED" => "Verification PIN is required.",
            "VALIDATION_AVATAR_REQUIRED" => "Avatar is required.",

            // CLIENT-SIDE / GENERIC
            "CONNECTION_ERROR" => "Could not connect to the server. Please check your internet connection.",
            _ => $"An error occurred: {errorCode}"
        };
    }
}
