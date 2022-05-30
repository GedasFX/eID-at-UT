using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication.Dokobit;

public class DokobitHandler : RemoteAuthenticationHandler<DokobitOptions>
{
    protected new DokobitEvents Events
    {
        get => (DokobitEvents)base.Events;
        set => base.Events = value;
    }

    private HttpClient Backchannel => Options.Backchannel;

    public DokobitHandler(IOptionsMonitor<DokobitOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
    {
        var properties = Options.StateDataFormat.Unprotect(Request.Cookies[Options.StateCookie.Name!]);
        if (properties == null)
            return HandleRequestResult.Fail("Invalid state");

        var returnToken = Request.Query["session_token"];
        var response = await ExecuteRequestAsync(HttpMethod.Get, $"/api/authentication/{returnToken}/status");
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(Context.RequestAborted);
        using var user = await JsonDocument.ParseAsync(stream);

        var sessionToken = user.RootElement.GetProperty("session_token").GetString();
        if (properties.Items["session_token"] != sessionToken)
            return HandleRequestResult.Fail("Unexpected session_token received", properties);

        Response.Cookies.Delete(Options.StateCookie.Name!);

        var ticket = await CreateTicketAsync(new ClaimsIdentity(ClaimsIssuer), properties, user.RootElement);
        return HandleRequestResult.Success(ticket);
    }

    protected virtual Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, JsonElement user)
    {
        var pid = user.GetProperty("code").GetString();
        var countryCode = user.GetProperty("country_code").GetString()?.ToUpperInvariant();

        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, $"{countryCode}/{pid}", ClaimValueTypes.String, ClaimsIssuer));

        foreach (var action in Options.ClaimActions)
            action.Run(user, identity, ClaimsIssuer);

        return Task.FromResult(new AuthenticationTicket(new ClaimsPrincipal(identity), properties, Scheme.Name));
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        if (string.IsNullOrEmpty(properties.RedirectUri))
            properties.RedirectUri = OriginalPathBase + OriginalPath + Request.QueryString;

        var body = JsonSerializer.Serialize(new { return_url = BuildRedirectUri(Options.CallbackPath) });
        var response = await ExecuteRequestAsync(HttpMethod.Post, "/api/authentication/create", body);

        response.EnsureSuccessStatusCode();
        using var sessionResponse = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(Context.RequestAborted));
        var root = sessionResponse.RootElement;

        properties.Items["session_token"] = root.GetString("session_token");

        Response.Cookies.Append(Options.StateCookie.Name!, Options.StateDataFormat.Protect(properties), Options.StateCookie.Build(Context, Clock.UtcNow));
        var redirectContext = new RedirectContext<DokobitOptions>(Context, Scheme, Options, properties, root.GetString("url")!);

        await Events.RedirectToAuthorizationEndpoint(redirectContext);
    }

    private async Task<HttpResponseMessage> ExecuteRequestAsync(HttpMethod httpMethod, string path, string? jsonData = null)
    {
        var host = Options.Environment == DokobitEnvironment.Testing ? "id-sandbox.dokobit.com" : "id.dokobit.com";
        var url = QueryHelpers.AddQueryString($"https://{host}{path}", "access_token", Options.ApiKey);

        var request = new HttpRequestMessage(httpMethod, url);
        request.Headers.Add("Accept", "application/json");

        if (jsonData != null)
            request.Content = new StringContent(jsonData, Encoding.UTF8, MediaTypeNames.Application.Json);

        return await Backchannel.SendAsync(request, Context.RequestAborted);
    }
}