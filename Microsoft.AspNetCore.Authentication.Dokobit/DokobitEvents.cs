namespace Microsoft.AspNetCore.Authentication.Dokobit;

public class DokobitEvents : RemoteAuthenticationEvents
{
    public Func<RedirectContext<DokobitOptions>, Task> OnRedirectToAuthorizationEndpoint { get; set; } = context =>
    {
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    public virtual Task RedirectToAuthorizationEndpoint(RedirectContext<DokobitOptions> context) => OnRedirectToAuthorizationEndpoint(context);
}