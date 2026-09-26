using System.ComponentModel;
using System.Globalization;

namespace MedFlow.Serialization;

/// <summary>
/// net6.0'da 'DateOnly' uchun TypeConverter yo'q, shuning uchun query-string/route
/// model binding (masalan [FromQuery] DateOnly date) ishlamaydi - shu converter buni tuzatadi.
/// </summary>
public class DateOnlyTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string s && DateOnly.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
        {
            return result;
        }

        return base.ConvertFrom(context, culture, value);
    }
}
