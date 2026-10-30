using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.AI;

/// <summary>
/// sufi-chain#311: the chat gateway owns retries, the breaker, and stream failover.
/// </summary>
public class LlmChatGatewayTests
{
    [Fact]
    public async Task A_429_waits_for_Retry_After_and_then_succeeds()
    {
        var calls = 0;
        var delay = new RecordingDelay();
        var gateway = Gateway(delay);
        var invocation = Invocation(
            _ =>
            {
                calls++;
                if (calls == 1)
                {
                    throw new LlmProviderCallException("busy", statusCode: 429, retryAfter: TimeSpan.FromSeconds(2));
                }

                return Task.FromResult(new ChatCompletionResponse { Content = "ok" });
            });

        var response = await gateway.SendAsync(invocation);

        response.Content.ShouldBe("ok");
        calls.ShouldBe(2);
        delay.Waits.ShouldBe(new[] { TimeSpan.FromSeconds(2) });
    }

    [Fact]
    public async Task A_5xx_is_retried_and_a_later_success_is_returned()
    {
        var calls = 0;
        var gateway = Gateway(new RecordingDelay());
        var response = await gateway.SendAsync(Invocation(_ =>
        {
            calls++;
            if (calls == 1)
            {
                throw new LlmProviderCallException("down", statusCode: 503);
            }

            return Task.FromResult(new ChatCompletionResponse { Content = "recovered" });
        }));

        response.Content.ShouldBe("recovered");
        calls.ShouldBe(2);
    }

    [Fact]
    public async Task A_timeout_is_retried()
    {
        var calls = 0;
        var gateway = Gateway(new RecordingDelay());
        var response = await gateway.SendAsync(Invocation(_ =>
        {
            calls++;
            if (calls == 1)
            {
                throw new TimeoutException("timed out");
            }

            return Task.FromResult(new ChatCompletionResponse { Content = "late" });
        }));

        response.Content.ShouldBe("late");
        calls.ShouldBe(2);
    }

    [Fact]
    public async Task A_4xx_is_not_retried()
    {
        var calls = 0;
        var delay = new RecordingDelay();
        var gateway = Gateway(delay);

        var error = await Should.ThrowAsync<Volo.Abp.BusinessException>(() => gateway.SendAsync(Invocation(_ =>
        {
            calls++;
            throw new LlmProviderCallException("bad request", statusCode: 400);
        })));

        calls.ShouldBe(1);
        delay.Waits.ShouldBeEmpty();
        error.Code.ShouldBe(AIErrorCodes.ProviderRequestFailed);
        error.Message.ShouldNotContain("http");
    }

    [Fact]
    public async Task Content_filter_and_validation_failures_are_not_retried()
    {
        var filterCalls = 0;
        var gateway = Gateway(new RecordingDelay());
        await Should.ThrowAsync<Volo.Abp.BusinessException>(() => gateway.SendAsync(Invocation(_ =>
        {
            filterCalls++;
            throw new LlmProviderCallException("filtered", statusCode: 400, errorCode: "content_filter");
        })));
        filterCalls.ShouldBe(1);

        var validationCalls = 0;
        await Should.ThrowAsync<Volo.Abp.BusinessException>(() => gateway.SendAsync(Invocation(_ =>
        {
            validationCalls++;
            throw new Volo.Abp.BusinessException("SufiAI:ValidationFailed");
        })));
        validationCalls.ShouldBe(1);
    }

    [Fact]
    public async Task An_exhausted_retry_budget_does_not_retry()
    {
        var calls = 0;
        var budget = new LlmRetryBudget();
        budget.Seed(totalRequests: 100, retriesUsed: 10);
        var gateway = Gateway(new RecordingDelay(), budget);
        await Should.ThrowAsync<Volo.Abp.BusinessException>(() => gateway.SendAsync(Invocation(_ =>
        {
            calls++;
            throw new LlmProviderCallException("down", statusCode: 503);
        })));

        calls.ShouldBe(1);
    }

    [Fact]
    public async Task The_breaker_opens_half_opens_and_closes()
    {
        var clock = new ManualClock();
        var store = new MemoryLlmCircuitStore();
        var gateway = Gateway(new RecordingDelay(), clock: clock, store: store);
        var calls = 0;
        Task<ChatCompletionResponse> Fail(CancellationToken _)
        {
            calls++;
            throw new LlmProviderCallException("down", statusCode: 503);
        }

        await Should.ThrowAsync<Volo.Abp.BusinessException>(() => gateway.SendAsync(Invocation(Fail)));
        await Should.ThrowAsync<Volo.Abp.BusinessException>(() => gateway.SendAsync(Invocation(Fail)));
        var open = await store.GetAsync("primary");
        open.State.ShouldBe(LlmCircuitState.Open);

        var blocked = calls;
        await Should.ThrowAsync<Volo.Abp.BusinessException>(() => gateway.SendAsync(Invocation(Fail)));
        calls.ShouldBe(blocked);

        clock.UtcNow = clock.UtcNow.AddSeconds(31);
        calls = 0;
        var response = await gateway.SendAsync(Invocation(_ =>
        {
            calls++;
            return Task.FromResult(new ChatCompletionResponse { Content = "probe" });
        }));
        response.Content.ShouldBe("probe");
        calls.ShouldBe(1);
        (await store.GetAsync("primary")).State.ShouldBe(LlmCircuitState.Closed);

        await gateway.SendAsync(Invocation(_ =>
        {
            calls++;
            return Task.FromResult(new ChatCompletionResponse { Content = "again" });
        }));
        calls.ShouldBe(2);
    }

    [Fact]
    public async Task Fallback_is_used_when_the_primary_breaker_is_open()
    {
        var clock = new ManualClock();
        var store = new MemoryLlmCircuitStore();
        await store.SaveAsync("primary", new LlmCircuitSnapshot
        {
            State = LlmCircuitState.Open,
            OpenedAt = clock.UtcNow
        });
        var primaryCalls = 0;
        var fallbackCalls = 0;
        var gateway = Gateway(new RecordingDelay(), clock: clock, store: store);
        var response = await gateway.SendAsync(new LlmGatewayInvocation
        {
            PrimaryCircuitKey = "primary",
            FallbackCircuitKey = "fallback",
            PrimaryModelId = "primary-model",
            FallbackModelId = "fallback-model",
            Primary = _ =>
            {
                primaryCalls++;
                throw new LlmProviderCallException("should not run", 503);
            },
            Fallback = _ =>
            {
                fallbackCalls++;
                return Task.FromResult(new ChatCompletionResponse { Content = "from fallback" });
            }
        });

        primaryCalls.ShouldBe(0);
        fallbackCalls.ShouldBe(1);
        response.Content.ShouldBe("from fallback");
    }

    [Fact]
    public async Task An_empty_fallback_is_rejected()
    {
        var gateway = Gateway(new RecordingDelay());
        var error = await Should.ThrowAsync<Volo.Abp.BusinessException>(() => gateway.SendAsync(new LlmGatewayInvocation
        {
            PrimaryCircuitKey = "primary",
            FallbackCircuitKey = "fallback",
            Primary = _ => throw new LlmProviderCallException("bad", 400),
            Fallback = _ => Task.FromResult(new ChatCompletionResponse { Content = "   " })
        }));

        error.Code.ShouldBe(AIErrorCodes.ProviderRequestFailed);
        LlmCallFailure.LooksSensitive(error.Message).ShouldBeFalse();
    }

    [Fact]
    public async Task A_stream_error_before_the_first_chunk_fails_over_without_duplicate_text()
    {
        var fallbackCalls = 0;
        var gateway = Gateway(new RecordingDelay());
        var chunks = await Collect(gateway.StreamAsync(new LlmGatewayInvocation
        {
            PrimaryCircuitKey = "primary",
            FallbackCircuitKey = "fallback",
            PrimaryModelId = "primary-model",
            FallbackModelId = "fallback-model",
            PrimaryStream = _ => Boom(),
            FallbackStream = _ =>
            {
                fallbackCalls++;
                return Text("fallback only");
            }
        }));

        fallbackCalls.ShouldBe(1);
        chunks.Select(chunk => chunk.Content).ShouldBe(new[] { "fallback only" });
    }

    [Fact]
    public async Task A_stream_error_after_text_yields_interrupted_and_does_not_replay()
    {
        var fallbackCalls = 0;
        var gateway = Gateway(new RecordingDelay());
        var chunks = await Collect(gateway.StreamAsync(new LlmGatewayInvocation
        {
            PrimaryCircuitKey = "primary",
            FallbackCircuitKey = "fallback",
            PrimaryStream = _ => TextThenBoom("Hello"),
            FallbackStream = _ =>
            {
                fallbackCalls++;
                return Text("should not run");
            }
        }));

        fallbackCalls.ShouldBe(0);
        chunks.Count.ShouldBe(2);
        chunks[0].Content.ShouldBe("Hello");
        chunks[1].Content.ShouldBe(string.Empty);
        chunks[1].FinishReason.ShouldBe(LlmGatewayFinishReasons.Interrupted);
        string.Concat(chunks.Select(chunk => chunk.Content)).ShouldBe("Hello");
    }

    [Fact]
    public void Full_jitter_is_random_up_to_the_capped_backoff()
    {
        var policy = new LlmGatewayPolicy { BaseDelayMs = 200, MaxDelayMs = 1000 };
        LlmFullJitter.Delay(0, policy, 0).ShouldBe(TimeSpan.Zero);
        LlmFullJitter.Delay(0, policy, 1).TotalMilliseconds.ShouldBe(200);
        LlmFullJitter.Delay(3, policy, 1).TotalMilliseconds.ShouldBe(1000);
    }

    private static LlmChatGateway Gateway(
        RecordingDelay delay,
        LlmRetryBudget? budget = null,
        ManualClock? clock = null,
        MemoryLlmCircuitStore? store = null)
    {
        return new LlmChatGateway(
            store ?? new MemoryLlmCircuitStore(),
            budget ?? new LlmRetryBudget(),
            new FixedPolicy(),
            delay,
            new ZeroJitter(),
            clock ?? new ManualClock());
    }

    private static LlmGatewayInvocation Invocation(Func<CancellationToken, Task<ChatCompletionResponse>> primary)
    {
        return new LlmGatewayInvocation
        {
            PrimaryCircuitKey = "primary",
            PrimaryModelId = "primary-model",
            Primary = primary
        };
    }

    private static async Task<List<ChatCompletionResponse>> Collect(IAsyncEnumerable<ChatCompletionResponse> stream)
    {
        var chunks = new List<ChatCompletionResponse>();
        await foreach (var chunk in stream)
        {
            chunks.Add(chunk);
        }

        return chunks;
    }

    private static async IAsyncEnumerable<ChatCompletionResponse> Boom()
    {
        await Task.Yield();
        if (DateTime.UtcNow.Year > 0)
        {
            throw new LlmProviderCallException("down", statusCode: 503);
        }

        yield break;
    }

    private static async IAsyncEnumerable<ChatCompletionResponse> Text(string content)
    {
        yield return new ChatCompletionResponse { Content = content };
        await Task.CompletedTask;
    }

    private static async IAsyncEnumerable<ChatCompletionResponse> TextThenBoom(string content)
    {
        yield return new ChatCompletionResponse { Content = content };
        await Task.Yield();
        throw new LlmProviderCallException("mid", statusCode: 503);
    }

    private sealed class FixedPolicy : ILlmGatewayPolicyReader
    {
        public Task<LlmGatewayPolicy> GetAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new LlmGatewayPolicy
            {
                MaxAttempts = 4,
                BaseDelayMs = 200,
                MaxDelayMs = 1000,
                RetryBudgetFraction = 0.10,
                BreakerMinimumSamples = 2,
                BreakerWindow = 4,
                BreakerErrorRate = 0.5,
                BreakerOpen = TimeSpan.FromSeconds(30)
            });
        }
    }

    private sealed class RecordingDelay : ILlmDelay
    {
        public List<TimeSpan> Waits { get; } = new();

        public Task WaitAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            Waits.Add(delay);
            return Task.CompletedTask;
        }
    }

    private sealed class ZeroJitter : ILlmJitter
    {
        public double NextUnit() => 0;
    }

    private sealed class ManualClock : ILlmClock
    {
        public DateTimeOffset UtcNow { get; set; } = new(2026, 10, 30, 0, 0, 0, TimeSpan.Zero);
    }
}
