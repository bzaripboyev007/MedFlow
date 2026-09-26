using MedFlow.Application.MedFlowServis.SessionServices.Concrete;

namespace MedFlow.BackgroundServices;

/// <summary>
/// Davriy ravishda muddati o'tgan (grace-period'dan keyin ham 'Pending' qolgan) seanslarni
/// 'Missed'ga o'tkazib, kursning qolgan seanslarini 1 kunga suradi (spec 3-band, 5-qadam).
/// </summary>
public class MissedSessionSweepService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MissedSessionSweepService> _logger;

    public MissedSessionSweepService(IServiceScopeFactory scopeFactory, ILogger<MissedSessionSweepService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
                var count = await sessionService.SweepMissedSessionsAsync(stoppingToken);

                if (count > 0)
                {
                    _logger.LogInformation("{Count} ta seans 'Missed' deb belgilandi, qolganlari surildi.", count);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "MissedSessionSweepService bir marta ishlashda xatolikka uchradi.");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // shutdown
            }
        }
    }
}
