using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace SufiChain.SufiAbp.AIManagement.MCP.Internal;

/// <summary>
/// Generates JSON schemas for method parameters (OpenAI function calling format).
/// </summary>
public class JsonSchemaGenerator
{
    public string GenerateSchema(MethodInfo method)
    {
        var parameters = method.GetParameters();
        
        var schema = new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = GenerateProperties(parameters),
            ["required"] = GetRequiredParameters(parameters)
        };
        
        return JsonSerializer.Serialize(schema);
    }
    
    private Dictionary<string, object> GenerateProperties(ParameterInfo[] parameters)
    {
        var properties = new Dictionary<string, object>();
        
        foreach (var param in parameters)
        {
            // Skip CancellationToken
            if (param.ParameterType == typeof(System.Threading.CancellationToken))
                continue;
            
            properties[param.Name!] = GeneratePropertySchema(param.ParameterType, param);
        }
        
        return properties;
    }
    
    private Dictionary<string, object> GeneratePropertySchema(Type type, ParameterInfo param)
    {
        var schema = new Dictionary<string, object>();
        
        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        
        if (underlyingType == typeof(string))
        {
            schema["type"] = "string";
        }
        else if (underlyingType == typeof(int) || underlyingType == typeof(long))
        {
            schema["type"] = "integer";
        }
        else if (underlyingType == typeof(float) || underlyingType == typeof(double) || underlyingType == typeof(decimal))
        {
            schema["type"] = "number";
        }
        else if (underlyingType == typeof(bool))
        {
            schema["type"] = "boolean";
        }
        else if (underlyingType == typeof(DateTime) || underlyingType == typeof(DateTimeOffset))
        {
            schema["type"] = "string";
            schema["format"] = "date-time";
        }
        else if (underlyingType == typeof(Guid))
        {
            schema["type"] = "string";
            schema["format"] = "uuid";
        }
        else if (underlyingType.IsEnum)
        {
            schema["type"] = "string";
            schema["enum"] = Enum.GetNames(underlyingType);
        }
        else if (underlyingType.IsArray || (underlyingType.IsGenericType && 
                 underlyingType.GetGenericTypeDefinition() == typeof(List<>)))
        {
            schema["type"] = "array";
            var elementType = underlyingType.IsArray 
                ? underlyingType.GetElementType()! 
                : underlyingType.GetGenericArguments()[0];
            schema["items"] = GeneratePropertySchema(elementType, param);
        }
        else if (underlyingType.IsClass && underlyingType != typeof(object))
        {
            // Complex object - generate nested schema
            schema["type"] = "object";
            var properties = new Dictionary<string, object>();
            foreach (var prop in underlyingType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                properties[ToCamelCase(prop.Name)] = GeneratePropertySchemaForProperty(prop);
            }
            schema["properties"] = properties;
        }
        else
        {
            schema["type"] = "string"; // Fallback
        }
        
        // Add description from XML comments if available
        var description = GetParameterDescription(param);
        if (!string.IsNullOrEmpty(description))
        {
            schema["description"] = description;
        }
        
        return schema;
    }
    
    private Dictionary<string, object> GeneratePropertySchemaForProperty(PropertyInfo property)
    {
        var schema = new Dictionary<string, object>();
        var type = property.PropertyType;
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        
        if (underlyingType == typeof(string))
            schema["type"] = "string";
        else if (underlyingType == typeof(int) || underlyingType == typeof(long))
            schema["type"] = "integer";
        else if (underlyingType == typeof(float) || underlyingType == typeof(double) || underlyingType == typeof(decimal))
            schema["type"] = "number";
        else if (underlyingType == typeof(bool))
            schema["type"] = "boolean";
        else if (underlyingType == typeof(DateTime) || underlyingType == typeof(DateTimeOffset))
        {
            schema["type"] = "string";
            schema["format"] = "date-time";
        }
        else if (underlyingType == typeof(Guid))
        {
            schema["type"] = "string";
            schema["format"] = "uuid";
        }
        else
            schema["type"] = "string";
        
        return schema;
    }
    
    private List<string> GetRequiredParameters(ParameterInfo[] parameters)
    {
        return parameters
            .Where(p => !p.IsOptional && 
                       p.ParameterType != typeof(System.Threading.CancellationToken) &&
                       Nullable.GetUnderlyingType(p.ParameterType) == null)
            .Select(p => p.Name!)
            .ToList();
    }
    
    private string GetParameterDescription(ParameterInfo param)
    {
        // TODO: Extract from XML documentation if available
        return string.Empty;
    }
    
    private string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;
        return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }
}
