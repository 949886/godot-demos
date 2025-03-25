using System;
using Godot;
using Newtonsoft.Json;

namespace Data.Json;

public partial class JsonTest: Node
{
    public override void _Ready()
    {
        NewtonsoftJsonTest();
        // SystemTextJsonTest();
    }
    
    private void NewtonsoftJsonTest()
    {
        // var obj = new JsonCallbacksTest();
        // var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
        // var obj2 = JsonConvert.DeserializeObject<JsonCallbacksTest>(json);
        //
        // Console.WriteLine(json);
        
        var obj3 = JsonConvert.DeserializeObject<JsonConverterTest>(JsonConverterTest.Json, new ColorHexConverter());
        var json2 = JsonConvert.SerializeObject(obj3, Formatting.Indented, new ColorHexConverter());
        
        Console.WriteLine(json2);
    }
    
    private void SystemTextJsonTest()
    {
        // var obj = new JsonCallbacksTest();
        // var json = System.Text.Json.JsonSerializer.Serialize(obj);
        // var obj2 = System.Text.Json.JsonSerializer.Deserialize<JsonCallbacksTest>(json);
        //
        // Console.WriteLine(json);
        
        var obj3 = System.Text.Json.JsonSerializer.Deserialize<JsonConverterTest>(JsonConverterTest.Json);
        var json2 = System.Text.Json.JsonSerializer.Serialize(obj3);
        
        Console.WriteLine(json2);
    }
}