using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.Authentication.Dokobit;

public record SessionTokenResponse
{
    [JsonPropertyName("status")]
    public string Status { get; init; } = null!;

    [JsonPropertyName("session_token")]
    public string SessionToken { get; init; } = null!;

    [JsonPropertyName("url")]
    public string Url { get; init; } = null!;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }
}