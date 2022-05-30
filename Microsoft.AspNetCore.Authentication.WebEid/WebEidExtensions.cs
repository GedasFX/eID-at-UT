using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication.WebEid;

public static class WebEidExtensions
{
    public static AuthenticationBuilder AddWebEid(this AuthenticationBuilder builder, Action<WebEidOptions> configureOptions)
        => builder.AddWebEid(WebEidDefaults.AuthenticationScheme, configureOptions);

    public static AuthenticationBuilder AddWebEid(this AuthenticationBuilder builder, string authenticationScheme, Action<WebEidOptions> configureOptions)
        => builder.AddWebEid(authenticationScheme, WebEidDefaults.DisplayName, configureOptions);

    public static AuthenticationBuilder AddWebEid(this AuthenticationBuilder builder, string authenticationScheme, string displayName,
        Action<WebEidOptions> configureOptions)
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<WebEidOptions>, WebEidPostConfigureOptions>());
        return builder.AddRemoteScheme<WebEidOptions, WebEidHandler>(authenticationScheme, displayName, configureOptions);
    }
}