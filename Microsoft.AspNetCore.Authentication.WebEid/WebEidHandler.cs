using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication.WebEid;

public class WebEidHandler : RemoteAuthenticationHandler<WebEidOptions>
{
    public WebEidHandler(IOptionsMonitor<WebEidOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
    {
        if (!Context.Session.TryGetValue("Web-eID.cert", out var x509B))
            return HandleRequestResult.Fail("Certificate not found");

        var properties = Options.StateDataFormat.Unprotect(Request.Cookies[Options.StateCookie.Name!]);
        if (properties == null)
            return HandleRequestResult.Fail("Invalid state");

        var ticket = await CreateTicketAsync(new ClaimsIdentity(ClaimsIssuer), properties, new X509Certificate2(x509B));
        return HandleRequestResult.Success(ticket);
    }

    protected virtual Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, X509Certificate2 certificate)
    {
        var subject = certificate.Subject.Split(", ").Select(x =>
        {
            var s = x.Split('=');
            return (s[0], s[1]);
        }).ToList();

        var country = subject.First(c => c.Item1 == "C").Item2;
        var serialNumber = subject.First(c => c.Item1 == "SERIALNUMBER").Item2;

        identity.AddClaims(new Claim[]
        {
            new(ClaimTypes.NameIdentifier, $"{country}/{serialNumber}"),
            new(ClaimTypes.GivenName, subject.First(c => c.Item1 == "G").Item2),
            new(ClaimTypes.Surname, subject.First(c => c.Item1 == "SN").Item2),
        });

        return Task.FromResult(new AuthenticationTicket(new ClaimsPrincipal(identity), properties, Scheme.Name));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        if (string.IsNullOrEmpty(properties.RedirectUri))
            properties.RedirectUri = OriginalPathBase + OriginalPath + Request.QueryString;

        Response.Cookies.Append(Options.StateCookie.Name!, Options.StateDataFormat.Protect(properties), Options.StateCookie.Build(Context, Clock.UtcNow));

        var redirectContext = new RedirectContext<WebEidOptions>(Context, Scheme, Options, properties, "/auth/webeid");
        redirectContext.Response.Redirect(redirectContext.RedirectUri);

        return Task.CompletedTask;
    }
}