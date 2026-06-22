using System.Text.Json;
using System.Text.Json.Serialization;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.DependencyInjection;

namespace SufiChain.SufiAbp.Calendar.AI.Tools;

/// <summary>
/// Base class for Calendar AI tools.
/// </summary>
public abstract class CalendarAIToolBase : ISufiAITool, ITransientDependency
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public abstract string Name { get; }

    public abstract string Description { get; }

    public abstract string ParameterSchema { get; }

    public virtual string Source => "SufiChain.SufiAbp.Calendar";

    public abstract Task<SufiAIToolExecutionResult> ExecuteAsync(
        SufiAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);

    protected virtual TInput ReadInput<TInput>(Dictionary<string, object?> parameters)
    {
        var json = JsonSerializer.Serialize(parameters, JsonOptions);
        return JsonSerializer.Deserialize<TInput>(json, JsonOptions)
               ?? throw new InvalidOperationException($"Invalid parameters for {Name}.");
    }

    protected virtual Task<SufiAIToolExecutionResult> SuccessAsync(object? result)
    {
        return Task.FromResult(SufiAIToolExecutionResult.CreateSuccess(result, 0));
    }
}
