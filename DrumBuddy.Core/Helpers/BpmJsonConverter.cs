using System.Text.Json;
using System.Text.Json.Serialization;
using DrumBuddy.Core.Models;

namespace DrumBuddy.Core.Helpers;


public class BpmJsonConverter : JsonConverter<Bpm>
{
    public override Bpm Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            reader.Read(); // Move to the "value" property
            
            if (reader.GetString() == "value")
            {
                reader.Read(); // Move to the value
                int bpmValue = reader.GetInt32();
                reader.Read(); // Move past the value
                reader.Read(); // Move to EndObject
                
                return new Bpm(bpmValue);
            }
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            int bpmValue = reader.GetInt32();
            return new Bpm(bpmValue);
        }

        throw new JsonException("Invalid BPM format");
    }

    public override void Write(
        Utf8JsonWriter writer,
        Bpm value,
        JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }
}
