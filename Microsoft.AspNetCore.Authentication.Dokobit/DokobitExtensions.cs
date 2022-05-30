using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication.Dokobit;

/// <summary>
/// Extension methods to configure Twitter OAuth authentication.
/// </summary>
public static class DokobitExtensions
{
    public static AuthenticationBuilder AddDokobit(this AuthenticationBuilder builder, Action<DokobitOptions> configureOptions)
        => builder.AddDokobit(DokobitDefaults.AuthenticationScheme, configureOptions);

    public static AuthenticationBuilder AddDokobit(this AuthenticationBuilder builder, string authenticationScheme, Action<DokobitOptions> configureOptions)
        => builder.AddDokobit(authenticationScheme, DokobitDefaults.DisplayName, configureOptions);

    public static AuthenticationBuilder AddDokobit(this AuthenticationBuilder builder, string authenticationScheme, string displayName,
        Action<DokobitOptions> configureOptions)
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<DokobitOptions>, DokobitPostConfigureOptions>());
        return builder.AddRemoteScheme<DokobitOptions, DokobitHandler>(authenticationScheme, displayName, configureOptions);
    }
}