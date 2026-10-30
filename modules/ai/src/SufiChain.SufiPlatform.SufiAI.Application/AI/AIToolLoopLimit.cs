using Microsoft.SemanticKernel;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// sufi-chain#340: bounds one tool-calling turn so it can't loop without an answer.
/// <see cref="MaxTurnSeconds"/> 0 keeps the 9.4 behavior: only the caller token ends a slow turn.
/// sufi-chain#311: the tenant setting <c>SufiAI.LlmGateway.MaxTurnSeconds</c> supplies a timeout when it is above 0.
/// </summary>
public class AIToolLoopOptions
{
    /// <summary>Most model requests in one turn that may run tools. The default of 128 auto-invoke attempts is far too many.</summary>
    public int MaxToolRounds { get; set; } = 10;

    /// <summary>Wall-clock seconds for one tool turn. 0 does not start a timer.</summary>
    public int MaxTurnSeconds { get; set; }
}

/// <summary>
/// sufi-chain#340: stops auto-invoked tool calls once the round budget is spent — a loop guard, not a
/// slow-model guard. The executor then ends the turn with a visible message instead of letting the model loop.
/// </summary>
public sealed class AIToolLoopLimitFilter : IAutoFunctionInvocationFilter
{
    private readonly int _maxRounds;

    public AIToolLoopLimitFilter(int maxRounds)
    {
        _maxRounds = Math.Max(1, maxRounds);
    }

    /// <summary>True once a tool call was refused because the round budget was spent.</summary>
    public bool LimitReached { get; private set; }

    public async Task OnAutoFunctionInvocationAsync(
        AutoFunctionInvocationContext context,
        Func<AutoFunctionInvocationContext, Task> next)
    {
        if (context.RequestSequenceIndex >= _maxRounds)
        {
            LimitReached = true;
            context.Result = new FunctionResult(context.Function, "Tool call limit reached for this turn.");
            context.Terminate = true;
            return;
        }

        await next(context);
    }
}
