using System.Diagnostics;
using Microsoft.SemanticKernel;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>sufi-chain#340: bounds one tool-calling turn so it can't run on without an answer.</summary>
public class AIToolLoopOptions
{
    /// <summary>Most model requests in one turn that may run tools. The default of 128 auto-invoke attempts is far too many.</summary>
    public int MaxToolRounds { get; set; } = 10;

    /// <summary>Wall-clock limit for the whole turn (tool rounds plus model replies), in seconds.</summary>
    public int MaxTurnSeconds { get; set; } = 150;
}

/// <summary>
/// sufi-chain#340: stops auto-invoked tool calls once the round or time budget is spent. The executor then
/// ends the turn with a visible message instead of letting the model loop.
/// </summary>
public sealed class AIToolLoopLimitFilter : IAutoFunctionInvocationFilter
{
    private readonly int _maxRounds;
    private readonly TimeSpan _maxDuration;
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    public AIToolLoopLimitFilter(int maxRounds, TimeSpan maxDuration)
    {
        _maxRounds = Math.Max(1, maxRounds);
        _maxDuration = maxDuration;
    }

    /// <summary>True once a tool call was refused because the budget was spent.</summary>
    public bool LimitReached { get; private set; }

    public async Task OnAutoFunctionInvocationAsync(
        AutoFunctionInvocationContext context,
        Func<AutoFunctionInvocationContext, Task> next)
    {
        if (context.RequestSequenceIndex >= _maxRounds || _stopwatch.Elapsed >= _maxDuration)
        {
            LimitReached = true;
            context.Result = new FunctionResult(context.Function, "Tool call limit reached for this turn.");
            context.Terminate = true;
            return;
        }

        await next(context);
    }
}
