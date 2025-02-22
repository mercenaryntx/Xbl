using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xbl.Client.Models.Xbl;

public class PlatformConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return "Xbox360";
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}