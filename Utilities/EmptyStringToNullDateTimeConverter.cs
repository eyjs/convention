using System.Text.Json;
using System.Text.Json.Serialization;

namespace LocalRAG.Utilities;

public class EmptyStringToNullDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }
            if (DateTime.TryParse(stringValue, out var date))
            {
                return date;
            }
        }
        else if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        // Let the default converter handle other cases or throw an exception
        return JsonSerializer.Deserialize<DateTime>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value.Value);
        }
    }
}