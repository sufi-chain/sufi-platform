using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Internal;

/// <summary>
/// Binds JSON parameters to method arguments.
/// </summary>
public class MethodParameterBinder
{
    public object?[] BindParameters(
        MethodInfo method,
        Dictionary<string, object?> parameters)
    {
        var methodParams = method.GetParameters();
        var boundArgs = new object?[methodParams.Length];
        
        for (int i = 0; i < methodParams.Length; i++)
        {
            var param = methodParams[i];
            
            // Skip CancellationToken - will be provided by executor
            if (param.ParameterType == typeof(System.Threading.CancellationToken))
            {
                boundArgs[i] = System.Threading.CancellationToken.None;
                continue;
            }
            
            if (parameters.TryGetValue(param.Name!, out var value))
            {
                try
                {
                    boundArgs[i] = ConvertValue(value, param.ParameterType);
                }
                catch (Exception ex)
                {
                    throw new BusinessException(AIErrorCodes.MCPToolParameterBindingFailed)
                        .WithData("ParameterName", param.Name)
                        .WithData("ExpectedType", param.ParameterType.Name)
                        .WithData("ActualValue", value?.ToString() ?? "null")
                        .WithData("Error", ex.Message);
                }
            }
            else if (param.IsOptional)
            {
                boundArgs[i] = param.DefaultValue;
            }
            else if (Nullable.GetUnderlyingType(param.ParameterType) != null)
            {
                boundArgs[i] = null;
            }
            else
            {
                throw new BusinessException(AIErrorCodes.MCPToolParameterBindingFailed)
                    .WithData("ParameterName", param.Name)
                    .WithData("Reason", "Required parameter missing");
            }
        }
        
        return boundArgs;
    }
    
    private object? ConvertValue(object? value, Type targetType)
    {
        if (value == null)
            return null;
        
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        
        // Handle JsonElement (from System.Text.Json deserialization)
        if (value is JsonElement jsonElement)
        {
            return ConvertJsonElement(jsonElement, underlyingType);
        }
        
        // Direct assignment if types match
        if (underlyingType.IsInstanceOfType(value))
            return value;
        
        // String conversion
        if (underlyingType == typeof(string))
            return value.ToString();
        
        // Numeric conversions
        if (underlyingType == typeof(int))
            return Convert.ToInt32(value);
        if (underlyingType == typeof(long))
            return Convert.ToInt64(value);
        if (underlyingType == typeof(float))
            return Convert.ToSingle(value);
        if (underlyingType == typeof(double))
            return Convert.ToDouble(value);
        if (underlyingType == typeof(decimal))
            return Convert.ToDecimal(value);
        
        // Boolean
        if (underlyingType == typeof(bool))
            return Convert.ToBoolean(value);
        
        // Guid
        if (underlyingType == typeof(Guid))
            return Guid.Parse(value.ToString()!);
        
        // DateTime
        if (underlyingType == typeof(DateTime))
            return DateTime.Parse(value.ToString()!);

        // TimeSpan
        if (underlyingType == typeof(TimeSpan))
            return TimeSpan.Parse(value.ToString()!);
        
        // Enum
        if (underlyingType.IsEnum)
            return Enum.Parse(underlyingType, value.ToString()!, ignoreCase: true);
        
        // Complex objects - deserialize from JSON
        if (underlyingType.IsClass || (underlyingType.IsGenericType && underlyingType.GetGenericTypeDefinition() == typeof(List<>)))
        {
            var json = value is string str ? str : JsonSerializer.Serialize(value);
            return JsonSerializer.Deserialize(json, underlyingType);
        }
        
        throw new InvalidOperationException($"Cannot convert value to type {targetType.Name}");
    }
    
    private object? ConvertJsonElement(JsonElement element, Type targetType)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Null:
                return null;
            case JsonValueKind.String:
                var str = element.GetString();
                if (targetType == typeof(string))
                    return str;
                if (targetType == typeof(Guid))
                    return Guid.Parse(str!);
                if (targetType == typeof(DateTime))
                    return DateTime.Parse(str!);
                if (targetType == typeof(TimeSpan))
                    return TimeSpan.Parse(str!);
                if (targetType.IsEnum)
                    return Enum.Parse(targetType, str!, ignoreCase: true);
                return str;
            case JsonValueKind.Number:
                if (targetType == typeof(int))
                    return element.GetInt32();
                if (targetType == typeof(long))
                    return element.GetInt64();
                if (targetType == typeof(float))
                    return element.GetSingle();
                if (targetType == typeof(double))
                    return element.GetDouble();
                if (targetType == typeof(decimal))
                    return element.GetDecimal();
                return element.GetDouble();
            case JsonValueKind.True:
            case JsonValueKind.False:
                return element.GetBoolean();
            case JsonValueKind.Array:
            case JsonValueKind.Object:
                return JsonSerializer.Deserialize(element.GetRawText(), targetType);
            default:
                throw new InvalidOperationException($"Unsupported JsonElement kind: {element.ValueKind}");
        }
    }
}
