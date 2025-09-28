using System.Text.Json;
using System.Text.Json.Serialization;
using DrumBuddy.Core.Models;

namespace DrumBuddy.Core.Helpers;

public class BpmJsonConverter : JsonConverter<Bpm>
{
    public override Bpm Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            var value = 0;

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read();

                    if (propertyName.Equals("value", StringComparison.OrdinalIgnoreCase)) value = reader.GetInt32();
                }

            // Create Bpm with the extracted value
            return new Bpm(value);
        }

        throw new JsonException("Invalid JSON format for Bpm");
    }

    public override void Write(Utf8JsonWriter writer, Bpm value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("value", value.Value);
        writer.WriteEndObject();
    }
}