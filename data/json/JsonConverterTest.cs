using System;
using System.Drawing;
using System.Text.Json.Serialization;

namespace Data.Json;

public class JsonConverterTest
{
    public const string Json =
        """
        {
          "color": "#FF00FF00",
          "godotColor": "#FF00FF00"
        }
        """;

    public Color color { get; set; }
    // public Godot.Color godotColor;
}