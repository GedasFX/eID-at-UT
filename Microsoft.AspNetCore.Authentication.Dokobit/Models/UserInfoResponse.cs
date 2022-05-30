using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.Authentication.Dokobit;

public record UserInfoResponse
{
    [JsonPropertyName("status")]
    public string Status { get; init; } = null!;

    [JsonPropertyName("certificate")]
    public object Certificate { get; init; } = null!;

    [JsonPropertyName("code")]
    public string Code { get; init; } = null!;

    [JsonPropertyName("country_code")]
    public string CountryCode { get; init; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    [JsonPropertyName("surname")]
    public string Surname { get; init; } = null!;

    [JsonPropertyName("authentication_method")]
    public string AuthenticationMethod { get; init; } = null!;

    [JsonPropertyName("date_authenticated")]
    public DateTimeOffset DateAuthenticated { get; init; }
}