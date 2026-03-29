namespace MemoryGame.Domain.Common;

/// <summary>
/// Exception thrown when a domain rule is violated.
/// The <see cref="ErrorCode"/> is a stable identifier intended for client-side i18n lookups.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Stable error code the client uses to resolve the localized message.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Initializes the exception with a stable error code and an optional server-side message for logging.
    /// </summary>
    /// <param name="errorCode">Stable identifier, e.g. <c>AUTH_EMAIL_ALREADY_REGISTERED</c>.</param>
    /// <param name="message">Human-readable description for server logs. Defaults to <paramref name="errorCode"/> when omitted.</param>
    public DomainException(string errorCode, string? message = null) : base(message ?? errorCode)
    {
        ErrorCode = errorCode;
    }
}
