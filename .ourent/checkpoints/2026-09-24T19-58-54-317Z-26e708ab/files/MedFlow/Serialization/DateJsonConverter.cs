using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MedFlow.Serialization;

/// <summary>
/// Faqat sana (vaqtsiz) ma'noli 'DateTime' maydonlar (BirthDate, StartDate, ExpiryDate va h.k. -
/// entity darajasida TypeName="DATE") uchun "yyyy-MM-dd" formatida serialize/deserialize qiladi.
/// </summary>
public class DateJsonConverter : JsonConverter<DateTime>
{
    private const string Format = "yyyy-MM-dd";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (string.IsNullOrWhiteSpace(str))
        {
            throw new JsonException("Sana bo'sh bo'lishi mumkin emas.");
        }

        // Asosiy holat: "yyyy-MM-dd".
        if (DateTime.TryParseExact(str, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
        {
            return exact;
        }

        // Ba'zi klientlar to'liq ISO 8601 yuboradi (masalan "2000-01-15T00:00:00.000Z").
        // Sana qismini matndan ajratamiz - zona siljishisiz, klient tanlagan kalendar sana saqlanadi.
        if (str.Length >= 10 &&
            DateTime.TryParseExact(str[..10], Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var datePart))
        {
            return datePart;
        }

        throw new JsonException($"'{str}' yaroqli sana emas. Kutilgan format: {Format}.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
}
