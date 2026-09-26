using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MedFlow.Application.Common;
using MedFlow.Application.Common.Interfaces;
using MedFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MedFlow.Infrastructure.Telegram;

/// <summary>
/// Telegram botni "long polling" usulida tinglaydi: foydalanuvchi /start bosganda
/// telefon raqamini ulashishni so'raydi, kontakt kelganda TELEGRAM_LINKS jadvaliga yozadi.
/// Webhook o'rniga polling ishlatiladi — lokal (localhost) muhitda ommaviy URL shart emas.
/// </summary>
public class TelegramBotPollingService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelegramBotPollingService> _logger;
    private readonly string? _botToken;
    private long _offset;

    public TelegramBotPollingService(
        IHttpClientFactory httpClientFactory,
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<TelegramBotPollingService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _botToken = configuration["Telegram:BotToken"];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_botToken))
        {
            _logger.LogWarning("Telegram:BotToken sozlanmagan - bot tinglash xizmati ishga tushmadi.");
            return;
        }

        var http = _httpClientFactory.CreateClient(nameof(TelegramBotPollingService));

        _logger.LogInformation("Telegram bot polling boshlandi.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var url = $"https://api.telegram.org/bot{_botToken}/getUpdates?offset={_offset}&timeout=30";
                var response = await http.GetFromJsonAsync<TelegramUpdatesResponse>(url, stoppingToken);

                if (response?.Result is null || response.Result.Count == 0)
                    continue;

                foreach (var update in response.Result)
                {
                    _offset = update.UpdateId + 1;
                    await HandleUpdateAsync(http, update, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Ilova to'xtatilmoqda - kutilgan holat.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Telegram getUpdates so'rovida xatolik. 5 soniyadan keyin qayta urinamiz.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ContinueWith(_ => { });
            }
        }
    }

    private async Task HandleUpdateAsync(HttpClient http, TelegramUpdate update, CancellationToken ct)
    {
        var message = update.Message;
        if (message?.Chat is null)
            return;

        var chatId = message.Chat.Id;

        if (message.Contact is not null)
        {
            await LinkPhoneAsync(http, chatId, message.Contact, message.From?.Username, ct);
            return;
        }

        if (string.Equals(message.Text?.Trim(), "/start", StringComparison.OrdinalIgnoreCase))
        {
            await SendRequestContactPromptAsync(http, chatId, ct);
        }
    }

    private async Task SendRequestContactPromptAsync(HttpClient http, long chatId, CancellationToken ct)
    {
        var payload = new
        {
            chat_id = chatId,
            text = "👋 MedFlow botiga xush kelibsiz!\n\nRo'yxatdan o'tish/kirish paytida telefon raqamingizni tasdiqlash uchun, pastdagi tugma orqali raqamingizni ulashing.",
            reply_markup = new
            {
                keyboard = new[]
                {
                    new[]
                    {
                        new { text = "📱 Telefon raqamni ulashish", request_contact = true }
                    }
                },
                resize_keyboard = true,
                one_time_keyboard = true
            }
        };

        await http.PostAsJsonAsync($"https://api.telegram.org/bot{_botToken}/sendMessage", payload, ct);
    }

    private async Task LinkPhoneAsync(HttpClient http, long chatId, TelegramContact contact, string? username, CancellationToken ct)
    {
        // Raqam kanonik "+998..." ko'rinishida saqlanadi - OTP/auth qidiruvi ham shu formatda.
        var phoneNumber = PhoneNumberNormalizer.Normalize(contact.PhoneNumber);
        if (phoneNumber.Length == 0)
        {
            await SendMessageAsync(http, chatId, "❌ Telefon raqamini aniqlab bo'lmadi. Iltimos, qaytadan urinib ko'ring.", ct);
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // PHONE_NUMBER va CHAT_ID bo'yicha alohida izlaymiz: bitta "OR" so'rovi ikkala
        // unique index'ga tegishli qatorlarni aralashtirib yuborardi (X-9).
        var byPhone = await uow.TelegramLinks.Query().FirstOrDefaultAsync(l => l.PhoneNumber == phoneNumber, ct);
        var byChat = await uow.TelegramLinks.Query().FirstOrDefaultAsync(l => l.ChatId == chatId, ct);

        TelegramLink? target;

        if (byPhone is not null && byChat is not null && byPhone.Id != byChat.Id)
        {
            // Konflikt: bu chat boshqa raqamga, bu raqam esa boshqa chatga bog'langan.
            // Yangi kontakt ustuvor: eski chat bog'lanishini o'chirib, CHAT_ID slotini
            // alohida saqlashda bo'shatamiz (delete va update tartibi deterministik bo'lsin).
            uow.TelegramLinks.Remove(byChat);
            await uow.SaveChangesAsync(ct);

            target = byPhone;
        }
        else
        {
            target = byPhone ?? byChat;
        }

        if (target is not null)
        {
            target.PhoneNumber = phoneNumber;
            target.ChatId = chatId;
            target.TelegramUsername = username;
            target.LinkedAt = DateTimeOffset.UtcNow;
            uow.TelegramLinks.Update(target);
        }
        else
        {
            await uow.TelegramLinks.AddAsync(new TelegramLink
            {
                PhoneNumber = phoneNumber,
                ChatId = chatId,
                TelegramUsername = username,
                LinkedAt = DateTimeOffset.UtcNow
            }, ct);
        }

        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            // Masalan, parallel yangilanish sababli unique index to'qnashuvi: polling
            // sikli qulab tushmasligi uchun xatoni ushlaymiz va foydalanuvchiga aytamiz.
            _logger.LogWarning(ex, "Telegram bog'lanishini saqlashda konflikt (ChatId={ChatId}).", chatId);
            await SendMessageAsync(http, chatId, "❌ Bu chat allaqachon boshqa raqamga bog'langan. Iltimos, administratorga murojaat qiling.", ct);
            return;
        }

        await SendMessageAsync(
            http,
            chatId,
            $"✅ Raqamingiz ({phoneNumber}) ulandi! Endi MedFlow'da ro'yxatdan o'tish/kirish paytida tasdiqlash kodini shu chatga yuboramiz.",
            ct,
            removeKeyboard: true);
    }

    private async Task SendMessageAsync(HttpClient http, long chatId, string text, CancellationToken ct, bool removeKeyboard = false)
    {
        var payload = new
        {
            chat_id = chatId,
            text,
            reply_markup = removeKeyboard ? new { remove_keyboard = true } : null
        };

        await http.PostAsJsonAsync($"https://api.telegram.org/bot{_botToken}/sendMessage", payload, ct);
    }

    private sealed class TelegramUpdatesResponse
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("result")]
        public List<TelegramUpdate>? Result { get; set; }
    }

    private sealed class TelegramUpdate
    {
        [JsonPropertyName("update_id")]
        public long UpdateId { get; set; }

        [JsonPropertyName("message")]
        public TelegramMessage? Message { get; set; }
    }

    private sealed class TelegramMessage
    {
        [JsonPropertyName("chat")]
        public TelegramChat? Chat { get; set; }

        [JsonPropertyName("from")]
        public TelegramUser? From { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("contact")]
        public TelegramContact? Contact { get; set; }
    }

    private sealed class TelegramChat
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
    }

    private sealed class TelegramUser
    {
        [JsonPropertyName("username")]
        public string? Username { get; set; }
    }

    private sealed class TelegramContact
    {
        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; } = null!;
    }
}
