using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication.Dokobit;

public class DokobitPostConfigureOptions : IPostConfigureOptions<DokobitOptions>
{
    private readonly IDataProtectionProvider _dp;

    public DokobitPostConfigureOptions(IDataProtectionProvider dataProtection)
    {
        _dp = dataProtection;
    }

    public void PostConfigure(string name, DokobitOptions options)
    {
        options.DataProtectionProvider ??= _dp;

        options.StateDataFormat ??= new SecureDataFormat<AuthenticationProperties>(new PropertiesSerializer(),
            options.DataProtectionProvider.CreateProtector(typeof(DokobitHandler).FullName, name, "v1"));

        if (options.Backchannel == null)
        {
            options.Backchannel = new HttpClient(options.BackchannelHttpHandler ?? new HttpClientHandler());
            options.Backchannel.Timeout = options.BackchannelTimeout;
            options.Backchannel.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB
            options.Backchannel.DefaultRequestHeaders.Accept.ParseAdd("*/*");
            options.Backchannel.DefaultRequestHeaders.UserAgent.ParseAdd("Microsoft ASP.NET Core Dokobit handler");
            options.Backchannel.DefaultRequestHeaders.ExpectContinue = false;
        }
    }
}