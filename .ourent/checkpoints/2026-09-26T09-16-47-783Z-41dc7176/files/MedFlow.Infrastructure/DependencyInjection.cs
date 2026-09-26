using MedFlow.Application.Common.Interfaces;
using MedFlow.Infrastructure.Auth;
using MedFlow.Infrastructure.FileStorage;
using MedFlow.Infrastructure.Notifications;
using MedFlow.Infrastructure.Telegram;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MedFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IFileStorageService, MinioFileStorageService>();
        services.AddSingleton<INotificationSender, LoggingNotificationSender>();
        services.AddHttpClient<ITelegramOtpSender, TelegramOtpSender>();
        services.AddHttpClient(nameof(TelegramBotPollingService));
        services.AddHostedService<TelegramBotPollingService>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
