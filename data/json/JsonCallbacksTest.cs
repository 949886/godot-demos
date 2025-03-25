using System;
using System.Runtime.Serialization;

namespace Data.Json;

public class JsonCallbacksTest
{
    public string Name { get; set; } = "Hello World!";
    public int Index { get; set; } = 123;

    
    public bool ShouldSerializeIndex()
    {
        Console.WriteLine($"After OnSerializing");
        if (Index > 100)
            return false;
        return true;
    }

    [OnSerializing] void OnSerializingMethod(StreamingContext context) 
        => Console.WriteLine("Before serialization");
    [OnSerialized] void OnSerializedMethod(StreamingContext context) 
        => Console.WriteLine("After serialization");
    [OnDeserializing] void OnDeserializingMethod(StreamingContext context) 
        => Console.WriteLine("Before deserialization");
    [OnDeserialized] void OnDeserializedMethod(StreamingContext context) 
        => Console.WriteLine("After deserialization");
}