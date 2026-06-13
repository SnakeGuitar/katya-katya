using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MemoryGame.Client.Models;

namespace MemoryGame.Client.Services.Network;
using MemoryGame.Client.Services.Interfaces;

/// <summary>
/// Typed HTTP client for the Memory Game REST API.
/// Automatically attaches the JWT bearer token from the current session.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _http;
    private readonly ISessionService _session;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient http, ISessionService session)
    {
        _http = http;
        _session = session;
    }

    /// <summary>
    /// Sends a GET request and deserializes the response.
    /// </summary>
    public async Task<ApiResponse<T>> GetAsync<T>(string url)
    {
        AttachToken();
        try
        {
            var response = await _http.GetAsync(url);
            return await ParseResponse<T>(response);
        }
        catch (HttpRequestException ex)
        {
            return Error<T>("CONNECTION_ERROR", ex.Message);
        }
    }

    /// <summary>
    /// Sends a POST request with a JSON body and deserializes the response.
    /// </summary>
    public async Task<ApiResponse<T>> PostAsync<T>(string url, object? body = null)
    {
        AttachToken();
        try
        {
            var response = await _http.PostAsJsonAsync(url, body);
            return await ParseResponse<T>(response);
        }
        catch (HttpRequestException ex)
        {
            return Error<T>("CONNECTION_ERROR", ex.Message);
        }
    }

    /// <summary>
    /// Sends a POST request with no expected response body.
    /// </summary>
    public async Task<ApiResponse> PostAsync(string url, object? body = null)
    {
        AttachToken();
        try
        {
            var response = await _http.PostAsJsonAsync(url, body);
            return await ParseResponse(response);
        }
        catch (HttpRequestException ex)
        {
            return new ApiResponse { IsSuccess = false, ErrorCode = "CONNECTION_ERROR", ErrorMessage = ex.Message };
        }
    }

    /// <summary>
    /// Sends a PUT request with a JSON body and deserializes the response.
    /// </summary>
    public async Task<ApiResponse<T>> PutAsync<T>(string url, object? body = null)
    {
        AttachToken();
        try
        {
            var response = await _http.PutAsJsonAsync(url, body);
            return await ParseResponse<T>(response);
        }
        catch (HttpRequestException ex)
        {
            return Error<T>("CONNECTION_ERROR", ex.Message);
        }
    }

    /// <summary>
    /// Sends a PUT request with no expected response body.
    /// </summary>
    public async Task<ApiResponse> PutAsync(string url, object? body = null)
    {
        AttachToken();
        try
        {
            var response = await _http.PutAsJsonAsync(url, body);
            return await ParseResponse(response);
        }
        catch (HttpRequestException ex)
        {
            return new ApiResponse { IsSuccess = false, ErrorCode = "CONNECTION_ERROR", ErrorMessage = ex.Message };
        }
    }

    /// <summary>
    /// Sends a DELETE request.
    /// </summary>
    public async Task<ApiResponse> DeleteAsync(string url)
    {
        AttachToken();
        try
        {
            var response = await _http.DeleteAsync(url);
            return await ParseResponse(response);
        }
        catch (HttpRequestException ex)
        {
            return new ApiResponse { IsSuccess = false, ErrorCode = "CONNECTION_ERROR", ErrorMessage = ex.Message };
        }
    }

    private void AttachToken()
    {
        if (_session.Current is not null)
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _session.Current.AccessToken);
    }

    private static async Task<ApiResponse<T>> ParseResponse<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
            return new ApiResponse<T> { IsSuccess = true, Data = data };
        }

        return await ParseErrorResponse<T>(response);
    }

    private static async Task<ApiResponse> ParseResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return new ApiResponse { IsSuccess = true };

        var json = await response.Content.ReadAsStringAsync();
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            return new ApiResponse
            {
                IsSuccess = false,
                ErrorCode = root.TryGetProperty("errorCode", out var code) ? code.GetString() : null,
                ErrorMessage = root.TryGetProperty("message", out var msg) ? msg.GetString() : json
            };
        }
        catch
        {
            return new ApiResponse { IsSuccess = false, ErrorMessage = json };
        }
    }

    private static async Task<ApiResponse<T>> ParseErrorResponse<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            return new ApiResponse<T>
            {
                IsSuccess = false,
                ErrorCode = root.TryGetProperty("errorCode", out var code) ? code.GetString() : null,
                ErrorMessage = root.TryGetProperty("message", out var msg) ? msg.GetString() : json
            };
        }
        catch
        {
            return Error<T>("UNKNOWN", json);
        }
    }

    private static ApiResponse<T> Error<T>(string code, string message)
        => new() { IsSuccess = false, ErrorCode = code, ErrorMessage = message };
}
