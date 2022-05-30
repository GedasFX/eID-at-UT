using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace AuthServer.Services;

public class WebEidValidationService
{
    private readonly HttpClient _client;

    private JsonSerializerOptions JsonSerializerOptions { get; } = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public WebEidValidationService(HttpClient client)
    {
        _client = client;
    }

    public async Task<WebEidValidationResult> GetResult(WebIdAuthTokenResponse token, byte[] nonce)
    {
        var res = await _client.PostAsync("web-eid/validate",
            new StringContent(JsonSerializer.Serialize(new WebEidValidationRequest(token, nonce), JsonSerializerOptions), Encoding.UTF8,
                MediaTypeNames.Application.Json));

        res.EnsureSuccessStatusCode();

        await using var stream = await res.Content.ReadAsStreamAsync();
        return (await JsonSerializer.DeserializeAsync<WebEidValidationResult>(stream, JsonSerializerOptions))!;
    }
}

public class WebIdAuthTokenResponse
{
    public string Algorithm { get; set; } = null!;
    public string UnverifiedCertificate { get; set; } = null!;
    public string Signature { get; set; } = null!;
    public string AppVersion { get; set; } = null!;
    public string Format { get; set; } = null!;
}

public class WebEidValidationRequest
{
    public WebIdAuthTokenResponse Token { get; }
    public byte[] Nonce { get; }

    public WebEidValidationRequest(WebIdAuthTokenResponse token, byte[] nonce)
    {
        Token = token;
        Nonce = nonce;
    }
}

public class WebEidValidationResult
{
    public string Status { get; init; }
    public string? Message { get; init; }

    public bool Success => Status == "ok";

    public WebEidValidationResult(string status, string? message = null)
    {
        Status = status;
        Message = message;
    }
}