using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI;

public sealed class LlmGatewayInvocation
{
    public string PrimaryCircuitKey { get; init; } = string.Empty;

    public string? FallbackCircuitKey { get; init; }

    public string? PrimaryModelId { get; init; }

    public string? FallbackModelId { get; init; }

    public Func<CancellationToken, Task<ChatCompletionResponse>> Primary { get; init; } =
        _ => Task.FromResult(new ChatCompletionResponse());

    public Func<CancellationToken, Task<ChatCompletionResponse>>? Fallback { get; init; }

    public Func<CancellationToken, IAsyncEnumerable<ChatCompletionResponse>>? PrimaryStream { get; init; }

    public Func<CancellationToken, IAsyncEnumerable<ChatCompletionResponse>>? FallbackStream { get; init; }
}

public interface ILlmDelay
{
    Task WaitAsync(TimeSpan delay, CancellationToken cancellationToken);
}

// ABP exposes an interface only when the class name ends with the interface name.
// LlmTaskDelay does not end with LlmDelay, so ILlmDelay would never be registered.
[ExposeServices(typeof(ILlmDelay))]
public class LlmTaskDelay : ILlmDelay, ISingletonDependency
{
    public Task WaitAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        return delay <= TimeSpan.Zero ? Task.CompletedTask : Task.Delay(delay, cancellationToken);
    }
}

public interface ILlmJitter
{
    double NextUnit();
}

// LlmRandomJitter does not end with LlmJitter, so the same exposure rule skips ILlmJitter.
[ExposeServices(typeof(ILlmJitter))]
public class LlmRandomJitter : ILlmJitter, ISingletonDependency
{
    public double NextUnit() => Random.Shared.NextDouble();
}

public interface ILlmClock
{
    DateTimeOffset UtcNow { get; }
}

public class SystemLlmClock : ILlmClock, ISingletonDependency
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

/// <summary>
/// The only retry layer for chat completions. The provider SDK stays at maxRetries 0.
/// </summary>
public class LlmChatGateway : ITransientDependency
{
    private readonly ILlmCircuitStore _circuits;
    private readonly LlmRetryBudget _budget;
    private readonly ILlmGatewayPolicyReader _policies;
    private readonly ILlmDelay _delay;
    private readonly ILlmJitter _jitter;
    private readonly ILlmClock _clock;
    private readonly ILogger<LlmChatGateway> _logger;

    public LlmChatGateway(
        ILlmCircuitStore circuits,
        LlmRetryBudget budget,
        ILlmGatewayPolicyReader policies,
        ILlmDelay delay,
        ILlmJitter jitter,
        ILlmClock clock,
        ILogger<LlmChatGateway>? logger = null)
    {
        _circuits = circuits;
        _budget = budget;
        _policies = policies;
        _delay = delay;
        _jitter = jitter;
        _clock = clock;
        _logger = logger ?? NullLogger<LlmChatGateway>.Instance;
    }

    public LlmRetryBudget Budget => _budget;

    public async Task<ChatCompletionResponse> SendAsync(
        LlmGatewayInvocation invocation,
        CancellationToken cancellationToken = default)
    {
        var policy = await _policies.GetAsync(cancellationToken);
        _budget.AddRequest();
        if (!await AdmitAsync(invocation.PrimaryCircuitKey, policy, cancellationToken))
        {
            return await RunFallbackOrThrowAsync(invocation, policy, failure: null, cancellationToken);
        }

        Exception? last = null;
        var attempts = policy.ClampAttempts();
        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                var response = await invocation.Primary(cancellationToken);
                await RecordAsync(invocation.PrimaryCircuitKey, success: true, policy, cancellationToken);
                return response;
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException && cancellationToken.IsCancellationRequested)
                {
                    throw;
                }

                last = ex;
                var failure = LlmCallFailure.Describe(ex);
                var canRetry = failure.IsTransient
                               && attempt < attempts
                               && _budget.TryConsume(policy.RetryBudgetFraction);
                if (!canRetry)
                {
                    await RecordAsync(invocation.PrimaryCircuitKey, success: false, policy, cancellationToken);
                    return await RunFallbackOrThrowAsync(invocation, policy, failure, cancellationToken);
                }

                var delay = failure.RetryAfter ?? LlmFullJitter.Delay(attempt - 1, policy, _jitter.NextUnit());
                await _delay.WaitAsync(delay, cancellationToken);
            }
        }

        throw LlmCallFailure.Sanitize(last);
    }

    public async IAsyncEnumerable<ChatCompletionResponse> StreamAsync(
        LlmGatewayInvocation invocation,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var policy = await _policies.GetAsync(cancellationToken);
        if (!await AdmitAsync(invocation.PrimaryCircuitKey, policy, cancellationToken))
        {
            if (invocation.FallbackStream == null)
            {
                throw LlmCallFailure.Sanitize(null);
            }

            LogFailover(invocation, status: null);
            await foreach (var chunk in ReadStreamAsync(
                               invocation.FallbackStream,
                               invocation.FallbackCircuitKey,
                               policy,
                               interruptedInsteadOfThrow: true,
                               cancellationToken))
            {
                yield return chunk;
            }

            yield break;
        }

        var yielded = false;
        var failover = false;
        var enumerator = ReadStreamAsync(
            invocation.PrimaryStream ?? (_ => Empty()),
            invocation.PrimaryCircuitKey,
            policy,
            interruptedInsteadOfThrow: false,
            cancellationToken).GetAsyncEnumerator(cancellationToken);
        try
        {
            while (true)
            {
                bool moved;
                try
                {
                    moved = await enumerator.MoveNextAsync();
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException && cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }

                    if (yielded || invocation.FallbackStream == null)
                    {
                        throw LlmCallFailure.Sanitize(ex);
                    }

                    LogFailover(invocation, LlmCallFailure.Describe(ex).StatusCode);
                    failover = true;
                    break;
                }

                if (!moved)
                {
                    yield break;
                }

                yielded = true;
                yield return enumerator.Current;
            }
        }
        finally
        {
            await enumerator.DisposeAsync();
        }

        if (!failover || invocation.FallbackStream == null)
        {
            yield break;
        }

        await foreach (var chunk in ReadStreamAsync(
                           invocation.FallbackStream,
                           invocation.FallbackCircuitKey,
                           policy,
                           interruptedInsteadOfThrow: true,
                           cancellationToken))
        {
            yield return chunk;
        }
    }

    private async IAsyncEnumerable<ChatCompletionResponse> ReadStreamAsync(
        Func<CancellationToken, IAsyncEnumerable<ChatCompletionResponse>> source,
        string? circuitKey,
        LlmGatewayPolicy policy,
        bool interruptedInsteadOfThrow,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var sawContent = false;
        ChatCompletionResponse? interrupted = null;
        ExceptionDispatchInfo? failure = null;
        var enumerator = source(cancellationToken).GetAsyncEnumerator(cancellationToken);
        try
        {
            while (true)
            {
                bool moved;
                try
                {
                    moved = await enumerator.MoveNextAsync();
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException && cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }

                    if (!string.IsNullOrWhiteSpace(circuitKey))
                    {
                        await RecordAsync(circuitKey, success: false, policy, cancellationToken);
                    }

                    if (sawContent)
                    {
                        interrupted = new ChatCompletionResponse
                        {
                            Content = string.Empty,
                            FinishReason = LlmGatewayFinishReasons.Interrupted
                        };
                        break;
                    }

                    failure = ExceptionDispatchInfo.Capture(
                        interruptedInsteadOfThrow ? LlmCallFailure.Sanitize(ex) : ex);
                    break;
                }

                if (!moved)
                {
                    if (!string.IsNullOrWhiteSpace(circuitKey))
                    {
                        await RecordAsync(circuitKey, success: true, policy, cancellationToken);
                    }

                    yield break;
                }

                var chunk = enumerator.Current;
                if (!string.IsNullOrEmpty(chunk.Content))
                {
                    sawContent = true;
                }

                yield return chunk;
            }
        }
        finally
        {
            await enumerator.DisposeAsync();
        }

        if (interrupted != null)
        {
            yield return interrupted;
            yield break;
        }

        failure?.Throw();
    }

    private async Task<ChatCompletionResponse> RunFallbackOrThrowAsync(
        LlmGatewayInvocation invocation,
        LlmGatewayPolicy policy,
        LlmCallFailure? failure,
        CancellationToken cancellationToken)
    {
        if (invocation.Fallback == null)
        {
            throw LlmCallFailure.Sanitize(failure?.Exception);
        }

        LogFailover(invocation, failure?.StatusCode);
        try
        {
            var response = await invocation.Fallback(cancellationToken);
            if (!IsAcceptableFallback(response))
            {
                _logger.LogWarning(
                    "LLM fallback output rejected. From={From} To={To}",
                    invocation.PrimaryModelId,
                    invocation.FallbackModelId);
                throw new Volo.Abp.BusinessException(AIErrorCodes.ProviderRequestFailed);
            }

            if (!string.IsNullOrWhiteSpace(invocation.FallbackCircuitKey))
            {
                await RecordAsync(invocation.FallbackCircuitKey, success: true, policy, cancellationToken);
            }

            return response;
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException && cancellationToken.IsCancellationRequested)
            {
                throw;
            }

            if (!string.IsNullOrWhiteSpace(invocation.FallbackCircuitKey))
            {
                await RecordAsync(invocation.FallbackCircuitKey, success: false, policy, cancellationToken);
            }

            throw LlmCallFailure.Sanitize(ex);
        }
    }

    private async Task<bool> AdmitAsync(string key, LlmGatewayPolicy policy, CancellationToken cancellationToken)
    {
        var snapshot = await _circuits.GetAsync(key, cancellationToken);
        var admission = LlmCircuitBreaker.Admit(snapshot, _clock.UtcNow, policy);
        await _circuits.SaveAsync(key, admission.Snapshot, cancellationToken);
        return !admission.RejectPrimary;
    }

    private async Task RecordAsync(string key, bool success, LlmGatewayPolicy policy, CancellationToken cancellationToken)
    {
        var snapshot = await _circuits.GetAsync(key, cancellationToken);
        var recorded = LlmCircuitBreaker.Record(snapshot, success, _clock.UtcNow, policy);
        await _circuits.SaveAsync(key, recorded, cancellationToken);
    }

    private void LogFailover(LlmGatewayInvocation invocation, int? status)
    {
        _logger.LogWarning(
            "LLM failover. Circuit={Circuit} From={From} To={To} Status={Status}",
            invocation.PrimaryCircuitKey,
            invocation.PrimaryModelId,
            invocation.FallbackModelId,
            status);
    }

    private static bool IsAcceptableFallback(ChatCompletionResponse? response)
    {
        return response != null
               && !string.IsNullOrWhiteSpace(response.Content)
               && !GuardModelReply.IsVerdict(response.Content);
    }

    private static async IAsyncEnumerable<ChatCompletionResponse> Empty()
    {
        await Task.CompletedTask;
        yield break;
    }
}
