using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Data.Json;

public class IgnoreJsonProperties : JsonSerializerSettings
{
    public IgnoreJsonProperties(params string[] propNamesToIgnore) 
        => ContractResolver = new IgnorePropertiesResolver(propNamesToIgnore);
}

public class IgnorePropertiesResolver : DefaultContractResolver
{
    private readonly HashSet<string> ignoreProps;

    public IgnorePropertiesResolver(IEnumerable<string> propNamesToIgnore) => 
        this.ignoreProps = new HashSet<string>(propNamesToIgnore);

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);
        if (this.ignoreProps.Contains(property.PropertyName)) 
            property.ShouldSerialize = _ => false;
        return property;
    }
}