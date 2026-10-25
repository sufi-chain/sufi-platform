using Microsoft.SemanticKernel;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// sufi-chain#340: bounds one tool-calling turn so it can't loop without an answer. 9.4: the turn has no
/// wall-clock cutoff — a slow model may run as long as it needs. Only the user (Stop), the session or circuit
/// closing, or app shutdown end a turn, through the caller's CancellationToken.
/// </summary>
public class AIToolLoopOptions
{
    /// <summary>Most model requests in one turn that may run tools. The default of 128 auto-invoke attempts is far too many.</summary>
    public int MaxToolRounds { get; set; } = 10;
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
