using MedFlow.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace MedFlow.Infrastructure.Notifications;

/// <summary>
/// Bot bosqichi qo'shilguncha eslatmalarni faqat log'ga yozadigan stand-in implementatsiya.
/// </summary>
public class LoggingNotificationSender : INotificationSender
{
    private readonly ILogger<LoggingNotificationSender> _logger;

    public LoggingNotificationSender(ILogger<LoggingNotificationSender> logger)
    {
        _logger = logger;
    }

    public Task NotifyPatientAsync(long patientId, string message, CancellationToken ct = default)
    {
        _logger.LogInformation("[Notify -> Patient {PatientId}] {Message}", patientId, message);
        return Task.CompletedTask;
    }
}
