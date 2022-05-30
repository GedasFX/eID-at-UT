using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication.Dokobit;

public class DokobitOptions : RemoteAuthenticationOptions
{
    private const string DefaultStateCookieName = "__DokobitState";

    public string ApiKey { get; set; } = null!;

    public DokobitEnvironment Environment { get; set; } = DokobitEnvironment.Production;

    public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; } = default!;

    public ClaimActionCollection ClaimActions { get; } = new();

    public CookieBuilder StateCookie { get; }

    public DokobitOptions()
    {
        CallbackPath = new PathString("/signin-dokobit");
        BackchannelTimeout = TimeSpan.FromMinutes(1);
        Events = new DokobitEvents();

        StateCookie = new DokobitCookieBuilder(this)
        {
            Name = DefaultStateCookieName,
            SecurePolicy = CookieSecurePolicy.SameAsRequest,
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
        };
    }

    public override void Validate()
    {
        base.Validate();
        if (string.IsNullOrEmpty(ApiKey))
        {
            throw new NotSupportedException($"{nameof(ApiKey)} must be provided");
        }
    }

    private sealed class DokobitCookieBuilder : CookieBuilder
    {
        private readonly DokobitOptions _options;

        public DokobitCookieBuilder(DokobitOptions options)
        {
            _options = options;
        }

        public override CookieOptions Build(HttpContext context, DateTimeOffset expiresFrom)
        {
            var options = base.Build(context, expiresFrom);
            if (!Expiration.HasValue)
            {
                options.Expires = expiresFrom.Add(_options.RemoteAuthenticationTimeout);
            }

            return options;
        }
    }
}

public enum DokobitEnvironment
{
    Production,
    Testing
}