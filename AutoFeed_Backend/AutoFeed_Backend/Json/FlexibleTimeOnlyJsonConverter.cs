using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoFeed_Backend.Json;

/// <summary>Accepts "HH:mm:ss" or ISO DateTime (takes time part).</summary>
public sealed class FlexibleTimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Expected string for TimeOnly.");

        var s = reader.GetString();
        if (string.IsNullOrWhiteSpace(s))
            return default;

        // Accept common formats: "HH:mm:ss", "HH:mm", and ISO datetime strings
        var formats = new[] { "HH:mm:ss", "H:mm:ss", "HH:mm", "H:mm" };
        if (TimeOnly.TryParseExact(s, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var tExact))
            return tExact;

        if (TimeOnly.TryParse(s, CultureInfo.InvariantCulture, out var t))
            return t;

        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
            return TimeOnly.FromDateTime(dt);

        throw new JsonException($"Cannot convert to TimeOnly: {s}");
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
    }
}
