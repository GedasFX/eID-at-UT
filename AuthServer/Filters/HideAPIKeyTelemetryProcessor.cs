using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace AuthServer;

public class HideAccessTokenTelemetryProcessor : ITelemetryProcessor
{
    private readonly ITelemetryProcessor _next;

    public HideAccessTokenTelemetryProcessor(ITelemetryProcessor next)
    {
        _next = next;
    }

    public void Process(ITelemetry item)
    {
        if (item is DependencyTelemetry { Type: "Http", Target: "id.dokobit.com" or "id-sandbox.dokobit.com" } dependency)
        {
            var q = new Uri(dependency.Data);
            dependency.Data = $"{q.Scheme}://{q.Host}{q.AbsolutePath}";
        }

        _next.Process(item);
    }
}