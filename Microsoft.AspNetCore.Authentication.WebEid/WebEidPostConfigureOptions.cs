using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication.WebEid;

public class WebEidPostConfigureOptions : IPostConfigureOptions<WebEidOptions>
{
    private readonly IDataProtectionProvider _dp;

    public WebEidPostConfigureOptions(IDataProtectionProvider dataProtection)
    {
        _dp = dataProtection;
    }

    public void PostConfigure(string name, WebEidOptions options)
    {
        options.DataProtectionProvider ??= _dp;

        // ReSharper disable once ConstantNullCoalescingCondition
        options.StateDataFormat ??= new SecureDataFormat<AuthenticationProperties>(new PropertiesSerializer(),
            options.DataProtectionProvider.CreateProtector(nameof(WebEidHandler), name, "v1"));
    }
}