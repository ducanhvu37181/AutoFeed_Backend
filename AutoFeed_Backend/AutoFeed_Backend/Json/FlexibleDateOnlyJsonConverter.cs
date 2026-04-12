using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoFeed_Backend.Json;

/// <summary>Accepts "yyyy-MM-dd" or ISO DateTime (lấy phần ngày).</summary>
public sealed class FlexibleDateOnlyJsonConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Expected string for DateOnly.");

        var s = reader.GetString();
        if (string.IsNullOrWhiteSpace(s))
            return default;

        if (DateOnly.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            return d;

        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
            return DateOnly.FromDateTime(dt);

        throw new JsonException($"Cannot convert to DateOnly: {s}");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }
}
