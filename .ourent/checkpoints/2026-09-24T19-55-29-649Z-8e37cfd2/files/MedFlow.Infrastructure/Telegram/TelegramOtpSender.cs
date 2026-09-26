using System.Net.Http.Json;
using MedFlow.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MedFlow.Infrastructure.Telegram;

/// <summary>Telegram Bot API (sendMessage) orqali xabar yuboradi.</summary>
public class TelegramOtpSender : ITelegramOtpSender
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TelegramOtpSender> _logger;
    private readonly string? _botToken;

    public TelegramOtpSender(HttpClient httpClient, IConfiguration configuration, ILogger<TelegramOtpSender> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _botToken = configuration["Telegram:BotToken"];
    }

    public async Task<bool> SendMessageAsync(long chatId, string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_botToken))
        {
            _logger.LogWarning("Telegram:BotToken sozlanmagan - xabar yuborilmadi (chatId={ChatId}).", chatId);
            return false;
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"https://api.telegram.org/bot{_botToken}/sendMessage",
                new { chat_id = chatId, text },
                ct);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Telegram sendMessage muvaffaqiyatsiz (chatId={ChatId}): {Body}", chatId, body);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Telegram sendMessage'da xatolik (chatId={ChatId}).", chatId);
            return false;
        }
    }
}
