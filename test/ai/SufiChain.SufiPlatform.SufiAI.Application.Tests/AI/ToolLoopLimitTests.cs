using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.AI;

/// <summary>
/// sufi-chain#340: a model that keeps calling tools, or a very slow model reply, used to keep the turn open
/// with no answer. Semantic Kernel allows up to 128 auto-invoke rounds and there was no turn deadline. Usage
/// was only recorded when the call returned.
/// </summary>
public class ToolLoopLimitTests
{
    private const string Fallback = "This took too long, so I stopped. Any changes already shown are kept. Try a smaller request.";

    [Fact]
    public async Task Without_a_limit_a_tool_looping_model_runs_far_past_ten_rounds()
    {
        using var handler = new LoopingModel();
        var (kernel, calls) = CreateKernel(handler);

        await kernel.GetRequiredService<IChatCompletionService>().GetChatMessageContentAsync(
            History(), Settings(), kernel);

        handler.Requests.ShouldBeGreaterThan(50); // the cause of the unbounded Writer turn
        calls().ShouldBeGreaterThan(50);
    }

    [Fact]
    public async Task Executor_stops_at_the_round_limit_with_a_visible_message_even_if_usage_recording_fails()
    {
        using var handler = new LoopingModel();
        var (kernel, calls) = CreateKernel(handler);
        var recorder = Substitute.For<IAIUsageRecorder>();
        recorder.RecordAsync(Arg.Any<AIUsageRecord>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ObjectDisposedException("LifetimeScope"));

        var result = await Executor(recorder, new AIToolLoopOptions { MaxToolRounds = 3, MaxTurnSeconds = 60 })
            .ExecuteAsync(Request(kernel));

        result.Response.Role.ShouldBe(AuthorRole.Assistant);
        result.Response.Content.ShouldBe(Fallback);
        calls().ShouldBe(3);
        handler.Requests.ShouldBeLessThanOrEqualTo(4);
    }

    [Fact]
    public async Task Executor_stops_at_the_time_limit_with_a_visible_message()
    {
        using var handler = new LoopingModel(slowAfterFirst: TimeSpan.FromSeconds(30));
        var (kernel, _) = CreateKernel(handler);
        var recorder = Substitute.For<IAIUsageRecorder>();
        var stopwatch = Stopwatch.StartNew();

        var result = await Executor(recorder, new AIToolLoopOptions { MaxToolRounds = 10, MaxTurnSeconds = 1 })
            .ExecuteAsync(Request(kernel));

        stopwatch.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(15));
        result.Response.Content.ShouldBe(Fallback);
        await recorder.Received(1).RecordAsync(
            Arg.Is<AIUsageRecord>(r => !r.IsSuccess && r.ErrorMessage == AIErrorCodes.ToolLoopLimitReached),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Caller_cancellation_is_still_a_cancellation_not_a_limit_message()
    {
        using var handler = new LoopingModel(slowAfterFirst: TimeSpan.FromSeconds(30));
        var (kernel, _) = CreateKernel(handler);
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

        await Should.ThrowAsync<OperationCanceledException>(() =>
            Executor(Substitute.For<IAIUsageRecorder>(), new AIToolLoopOptions { MaxTurnSeconds = 60 })
                .ExecuteAsync(Request(kernel), cts.Token));
    }

    private static AIToolChatExecutor Executor(IAIUsageRecorder recorder, AIToolLoopOptions options) =>
        new(Substitute.For<IWorkspaceRuntimeConfigurationResolver>(), null!,
            Substitute.For<IWorkspaceGuardrailService>(), recorder, toolLoopOptions: Options.Create(options));

    private static AIToolChatExecutionRequest Request(Kernel kernel) => new()
    {
        Kernel = kernel,
        Configuration = new WorkspaceRuntimeConfiguration
        {
            Workspace = new Workspace(Guid.NewGuid(), "test", AIProviderType.OpenAI, "test-model"),
            ModelId = "test-model", Provider = AIProviderType.OpenAI, OpenAIApiMode = OpenAIApiMode.ChatCompletions
        },
        History = History(),
        ExecutionSettings = Settings(),
        RequiresToolCalling = true
    };

    private static ChatHistory History()
    {
        var history = new ChatHistory();
        history.AddUserMessage("Rewrite the hero section");
        return history;
    }

    private static OpenAIPromptExecutionSettings Settings() =>
        new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };

    private static (Kernel Kernel, Func<int> Calls) CreateKernel(LoopingModel handler)
    {
        var client = new HttpClient(handler, disposeHandler: false) { BaseAddress = new Uri("https://test.invalid/v1") };
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("test-model", "test-key", httpClient: client);
        var kernel = builder.Build();
        var calls = 0;
        kernel.Plugins.AddFromFunctions("cms", [KernelFunctionFactory.CreateFromMethod(
            () => { Interlocked.Increment(ref calls); return "section html"; }, "get_section")]);
        return (kernel, () => calls);
    }

    /// <summary>A model that answers every request with another tool call.</summary>
    private sealed class LoopingModel(TimeSpan? slowAfterFirst = null) : HttpMessageHandler
    {
        private int _requests;
        public int Requests => _requests;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var n = Interlocked.Increment(ref _requests);
            if (slowAfterFirst is { } delay && n > 1)
            {
                await Task.Delay(delay, cancellationToken);
            }

            var body = JsonSerializer.Serialize(new
            {
                id = "c" + n, @object = "chat.completion", created = 1, model = "test-model",
                choices = new[]
                {
                    new
                    {
                        index = 0, finish_reason = "tool_calls",
                        message = new
                        {
                            role = "assistant", content = (string?)null,
                            tool_calls = new[] { new { id = "call_" + n, type = "function", function = new { name = "cms-get_section", arguments = "{}" } } }
                        }
                    }
                },
                usage = new { prompt_tokens = 10, completion_tokens = 5, total_tokens = 15 }
            });
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
        }
    }
}
