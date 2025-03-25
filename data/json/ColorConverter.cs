using System;
using System.Drawing;
using Newtonsoft.Json;

namespace Data.Json;


public class ColorHexConverter : JsonConverter<Color>
{
    public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
    {
        // Serialize the color as a hex string (e.g., #FFFF0000 for red)
        string hexColor = $"#{value.A:X2}{value.R:X2}{value.G:X2}{value.B:X2}";
        writer.WriteValue(hexColor);
    }

    public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        // Read and parse hex string and create the color
        string hexColor = (string)reader.Value;
        return ColorTranslator.FromHtml(hexColor);
    }
}