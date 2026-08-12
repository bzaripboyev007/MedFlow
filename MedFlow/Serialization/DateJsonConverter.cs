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
        => DateTime.ParseExact(reader.GetString()!, Format, CultureInfo.InvariantCulture);

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
}
