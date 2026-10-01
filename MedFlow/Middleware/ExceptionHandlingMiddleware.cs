using System.Net;
using System.Text.Json;

namespace MedFlow.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Klient so'rovni o'zi uzgan (sahifa yopildi/qayta yuklandi) - bu server xatosi emas.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kutilmagan xatolik");

            // Javob yuborila boshlangan bo'lsa (streaming/serializatsiya paytida), header va
            // statusni o'zgartirish "response headers already sent" xatoligini keltirib chiqaradi (X-12).
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Javob allaqachon boshlangan - xatolik javobini yuborib bo'lmaydi.");
                return;
            }

            await WriteProblemAsync(context, HttpStatusCode.InternalServerError, "Kutilmagan xatolik yuz berdi.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        var payload = JsonSerializer.Serialize(new { error = message });
        await context.Response.WriteAsync(payload);
    }
}
