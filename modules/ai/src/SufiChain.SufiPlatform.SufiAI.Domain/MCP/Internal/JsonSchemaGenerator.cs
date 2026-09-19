using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Internal;

/// <summary>Generates the nested camelCase DTO contract consumed by MethodParameterBinder.</summary>
public class JsonSchemaGenerator
{
    public string GenerateSchema(MethodInfo method)
    {
        var parameters = method.GetParameters().Where(p => p.ParameterType != typeof(CancellationToken)).ToArray();
        return JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = parameters.ToDictionary(p => p.Name!, p => Schema(p.ParameterType, new HashSet<Type>())),
            ["required"] = parameters.Where(p => !p.IsOptional && Nullable.GetUnderlyingType(p.ParameterType) == null)
                .Select(p => p.Name!).ToArray()
        });
    }

    private static Dictionary<string, object> Schema(Type type, HashSet<Type> ancestors)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        var schema = new Dictionary<string, object>();
        if (type == typeof(object) || type == typeof(JsonElement)) return schema;
        if (type == typeof(string) || type == typeof(char)) schema["type"] = "string";
        else if (type == typeof(bool)) schema["type"] = "boolean";
        else if (type.IsEnum)
        {
            schema["type"] = "integer";
            schema["enum"] = Enum.GetValues(type).Cast<object>().Select(v => Convert.ToInt64(v)).ToArray();
            schema["description"] = string.Join(", ", Enum.GetValues(type).Cast<object>().Select(v => $"{Convert.ToInt64(v)} = {v}"));
        }
        else if (type == typeof(byte) || type == typeof(short) || type == typeof(int) || type == typeof(long) ||
                 type == typeof(sbyte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong)) schema["type"] = "integer";
        else if (type == typeof(float) || type == typeof(double) || type == typeof(decimal)) schema["type"] = "number";
        else if (type == typeof(Guid)) { schema["type"] = "string"; schema["format"] = "uuid"; }
        else if (type == typeof(DateTime) || type == typeof(DateTimeOffset)) { schema["type"] = "string"; schema["format"] = "date-time"; }
        else if (type == typeof(TimeSpan)) { schema["type"] = "string"; schema["description"] = "Duration as a TimeSpan, for example 00:30:00."; }
        else
        {
            // Bound self-referential DTOs without discarding normal nested collections.
            if (!ancestors.Add(type)) return schema;
            try
            {
                var contracts = type.GetInterfaces().Append(type).ToArray();
                var dictionary = contracts.FirstOrDefault(t => t.IsGenericType &&
                    (t.GetGenericTypeDefinition() == typeof(IDictionary<,>) || t.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)));
                var enumerable = contracts.FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                if (dictionary != null)
                {
                    schema["type"] = "object";
                    schema["additionalProperties"] = Schema(dictionary.GetGenericArguments()[1], ancestors);
                }
                else if (type.IsArray || enumerable != null)
                {
                    schema["type"] = "array";
                    schema["items"] = Schema(type.IsArray ? type.GetElementType()! : enumerable!.GetGenericArguments()[0], ancestors);
                }
                else
                {
                    schema["type"] = "object";
                    var properties = new Dictionary<string, object>();
                    var required = new List<string>();
                    foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0 &&
                            p.GetCustomAttribute<JsonIgnoreAttribute>()?.Condition != JsonIgnoreCondition.Always))
                    {
                        var name = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
                            ?? JsonNamingPolicy.CamelCase.ConvertName(property.Name);
                        var memberSchema = Schema(property.PropertyType, ancestors);
                        if (property.GetCustomAttribute<RequiredAttribute>() != null) required.Add(name);
                        if (property.GetCustomAttribute<MaxLengthAttribute>() is { Length: >= 0 } max)
                            memberSchema[property.PropertyType == typeof(string) ? "maxLength" : "maxItems"] = max.Length;
                        if (property.GetCustomAttribute<MinLengthAttribute>() is { Length: >= 0 } min)
                            memberSchema[property.PropertyType == typeof(string) ? "minLength" : "minItems"] = min.Length;
                        properties[name] = memberSchema;
                    }
                    schema["properties"] = properties;
                    if (required.Count > 0) schema["required"] = required;
                }
            }
            finally { ancestors.Remove(type); }
        }
        return schema;
    }
}
