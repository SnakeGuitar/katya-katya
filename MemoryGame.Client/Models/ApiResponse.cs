namespace MemoryGame.Client.Models;

/// <summary>
/// Wraps every HTTP response from the API with success/error information.
/// </summary>
public class ApiResponse<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public Dictionary<string, string[]>? ValidationErrors { get; init; }
}

/// <summary>
/// Non-generic version for endpoints that return no data.
/// </summary>
public class ApiResponse
{
    public bool IsSuccess { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public Dictionary<string, string[]>? ValidationErrors { get; init; }
}
