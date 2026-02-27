namespace MyCompany.Yggdrasil.HealthCheck;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Converts TimeSpan values to and from JSON using the standard "c" format and invariant culture.
/// </summary>
internal sealed class TimeSpanConverter : JsonConverter<TimeSpan>
{
    /// <summary>
    /// Reads a TimeSpan value from a JSON string using the standard "c" format.
    /// </summary>
    /// <param name="reader">The reader to read the JSON value from.</param>
    /// <param name="typeToConvert">The type of the value to convert.</param>
    /// <param name="options">Options to control the serialization behavior.</param>
    /// <returns>The parsed TimeSpan value, or the default value if parsing fails.</returns>
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String &&
            TimeSpan.TryParseExact(reader.GetString(), "c", CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return default;
    }

    /// <summary>
    /// Writes a TimeSpan value as a JSON string using the invariant culture and the standard format.
    /// </summary>
    /// <param name="writer">The Utf8JsonWriter used to write the JSON value.</param>
    /// <param name="value">The TimeSpan value to convert and write.</param>
    /// <param name="options">Options to control the serialization behavior.</param>
    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("c", CultureInfo.InvariantCulture));
    }
}