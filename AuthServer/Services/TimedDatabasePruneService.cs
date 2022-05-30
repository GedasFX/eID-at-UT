using AuthServer.Data;

namespace AuthServer.Services;

public sealed class TimedDatabasePruneService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private Timer? _t;

    public TimedDatabasePruneService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _t = new Timer(_ => OnTimerFiredAsync(cancellationToken), null, GetWaitLength(), Timeout.Infinite);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _t?.Dispose();
        return Task.CompletedTask;
    }

    private async void OnTimerFiredAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await dbContext.Database.EnsureDeletedAsync(cancellationToken);
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }
        finally
        {
            _t?.Change(GetWaitLength(), Timeout.Infinite);
        }
    }

    private static int GetWaitLength()
    {
        // Wait until midnight
        return (int)(DateTime.Today.AddDays(1) - DateTime.Now).TotalMilliseconds;
    }
}