using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication.WebEid;

public class WebEidOptions : RemoteAuthenticationOptions
{
    public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; } = null!;
    public CookieBuilder StateCookie { get; }

    public WebEidOptions()
    {
        CallbackPath = new PathString("/signin-web-eid");

        StateCookie = new CookieBuilder
        {
            Name = "__Web-eID",
            SecurePolicy = CookieSecurePolicy.SameAsRequest,
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            IsEssential = true,
        };
    }
}