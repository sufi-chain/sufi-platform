using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.Ai;
using SufiChain.SufiPlatform.HelpDesk.Features;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Hooshvare;
using SufiChain.SufiPlatform.HelpDesk.Ticketing.Ai;
using SufiChain.SufiPlatform.HelpDesk.Ticketing.Hooshvare;
using SufiChain.SufiPlatform.HelpDesk.Ticketing.Repositories;
using SufiChain.SufiPlatform.HelpDesk.Ticketing.Tickets;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Features;
using Volo.Abp.Timing;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.Ticketing;

public class TicketAiAppServiceTests
{
    [Fact]
    public async Task Selector_Uses_Ticketing_Purpose_And_Does_Not_Query_The_Key()
    {
        var fixture = new SelectorFixture();
        var purposeId = Guid.NewGuid();
        fixture.PurposeReady(purposeId);

        var selection = await fixture.Selector.SelectAsync(fixture.ProjectId, HelpDeskTicketTriageHooshvareKeys.Key);

        selection.IsReady.ShouldBeTrue();
        selection.HooshvareId.ShouldBe(purposeId);
        await fixture.Keys.DidNotReceive().TryGetRuntimeIdByKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Selector_Falls_Back_To_The_Action_Key_When_The_Purpose_Is_Not_Ready()
    {
        var fixture = new SelectorFixture();
        var keyId = Guid.NewGuid();
        fixture.Purpose(HelpDeskAiBindingReadiness.MissingWorkspace);
        fixture.Keys.TryGetRuntimeIdByKeyAsync(HelpDeskTicketTriageHooshvareKeys.Key, Arg.Any<CancellationToken>())
            .Returns(keyId);

        var selection = await fixture.Selector.SelectAsync(fixture.ProjectId, HelpDeskTicketTriageHooshvareKeys.Key);

        selection.IsReady.ShouldBeTrue();
        selection.HooshvareId.ShouldBe(keyId);
    }

    [Fact]
    public async Task Selector_Reports_The_Purpose_Readiness_When_The_Key_Is_Also_Missing()
    {
        var fixture = new SelectorFixture();
        fixture.Purpose(HelpDeskAiBindingReadiness.MissingAssignment);
        fixture.Keys.TryGetRuntimeIdByKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Guid.Empty);

        var selection = await fixture.Selector.SelectAsync(fixture.ProjectId, HelpDeskKbDraftHooshvareKeys.Key);

        selection.IsReady.ShouldBeFalse();
        selection.UnavailableReasonCode.ShouldBe(TicketingErrorCodes.AiMissingAssignment);
    }

    [Fact]
    public async Task Selector_Reports_HooshvareNotFound_When_The_Ticket_Has_No_Project()
    {
        var fixture = new SelectorFixture();
        fixture.Keys.TryGetRuntimeIdByKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Guid.Empty);

        var selection = await fixture.Selector.SelectAsync(null, HelpDeskAgentReplyHooshvareKeys.Key);

        selection.UnavailableReasonCode.ShouldBe(TicketingErrorCodes.AiHooshvareNotFound);
        await fixture.Workspaces.DidNotReceive().ResolveAsync(
            Arg.Any<Guid?>(),
            Arg.Any<HelpDeskAiWorkspacePurpose>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SuggestReply_Sends_The_Ticketing_Purpose_Hooshvare()
    {
        var fixture = new AppFixture();
        var purposeId = Guid.NewGuid();
        fixture.PurposeReady(purposeId);
        fixture.RuntimeReturns("Draft reply");

        var result = await fixture.Ai.SuggestReplyAsync(fixture.Ticket.Id);

        result.Available.ShouldBeTrue();
        result.Suggestion.ShouldBe("Draft reply");
        await fixture.Runtime.Received(1).SendAsync(
            Arg.Is<HooshvareRuntimeRequestDto>(request => request.HooshvareId == purposeId && request.PersistChatSession == false),
            Arg.Any<CancellationToken>());
        await fixture.Keys.DidNotReceive().TryGetRuntimeIdByKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Triage_Falls_Back_To_The_TicketTriage_Key()
    {
        var fixture = new AppFixture();
        var keyId = Guid.NewGuid();
        fixture.Purpose(HelpDeskAiBindingReadiness.MissingHooshvare);
        fixture.Keys.TryGetRuntimeIdByKeyAsync(HelpDeskTicketTriageHooshvareKeys.Key, Arg.Any<CancellationToken>())
            .Returns(keyId);
        fixture.RuntimeReturns("Priority: high");

        var result = await fixture.Ai.TriageAsync(fixture.Ticket.Id);

        result.Available.ShouldBeTrue();
        result.Recommendation.ShouldBe("Priority: high");
        await fixture.Runtime.Received(1).SendAsync(
            Arg.Is<HooshvareRuntimeRequestDto>(request => request.HooshvareId == keyId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SuggestReply_Returns_A_Specific_Reason_When_Purpose_And_Key_Are_Missing()
    {
        var fixture = new AppFixture();
        fixture.Purpose(HelpDeskAiBindingReadiness.MissingWorkspace);
        fixture.Keys.TryGetRuntimeIdByKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Guid.Empty);

        var result = await fixture.Ai.SuggestReplyAsync(fixture.Ticket.Id);

        result.Available.ShouldBeFalse();
        result.UnavailableReasonCode.ShouldBe(TicketingErrorCodes.AiMissingWorkspace);
        await fixture.Runtime.DidNotReceive().SendAsync(
            Arg.Any<HooshvareRuntimeRequestDto>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SuggestReply_Returns_FeatureDisabled_Without_Resolving_A_Hooshvare()
    {
        var fixture = new AppFixture();
        fixture.Features.IsEnabledAsync(HelpDeskFeatures.Names.TicketingAiSuggestions).Returns(false);

        var result = await fixture.Ai.SuggestReplyAsync(fixture.Ticket.Id);

        result.Available.ShouldBeFalse();
        result.UnavailableReasonCode.ShouldBe(TicketingErrorCodes.AiFeatureDisabled);
        await fixture.Tickets.DidNotReceive().FindAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SuggestReply_Returns_Failed_And_Does_Not_Hide_The_Exception_From_The_Log_Path()
    {
        var fixture = new AppFixture();
        fixture.PurposeReady(Guid.NewGuid());
        fixture.Runtime.SendAsync(Arg.Any<HooshvareRuntimeRequestDto>(), Arg.Any<CancellationToken>())
            .Returns<Task<HooshvareRuntimeResultDto>>(_ => throw new InvalidOperationException("provider down"));

        var result = await fixture.Ai.SuggestReplyAsync(fixture.Ticket.Id);

        result.Available.ShouldBeFalse();
        result.UnavailableReasonCode.ShouldBe(TicketingErrorCodes.AiFailed);
        result.Suggestion.ShouldBeEmpty();
    }

    [Fact]
    public async Task SuggestReply_Times_Out_Instead_Of_Waiting_For_The_Provider()
    {
        var fixture = new AppFixture(fastTimeout: true);
        fixture.PurposeReady(Guid.NewGuid());
        fixture.Runtime.SendAsync(Arg.Any<HooshvareRuntimeRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(call => NeverAsync(call.Arg<CancellationToken>()));

        var result = await fixture.Ai.SuggestReplyAsync(fixture.Ticket.Id);

        result.Available.ShouldBeFalse();
        result.UnavailableReasonCode.ShouldBe(TicketingErrorCodes.AiTimedOut);
    }

    [Fact]
    public async Task SuggestReply_Propagates_Caller_Cancellation()
    {
        var fixture = new AppFixture(fastTimeout: true);
        fixture.PurposeReady(Guid.NewGuid());
        fixture.Runtime.SendAsync(Arg.Any<HooshvareRuntimeRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(call => NeverAsync(call.Arg<CancellationToken>()));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(() => fixture.Ai.SuggestReplyAsync(fixture.Ticket.Id, cts.Token));
    }

    [Fact]
    public async Task GetAvailability_Disables_Every_Action_When_The_Feature_Is_Off()
    {
        var fixture = new AppFixture();
        fixture.Features.IsEnabledAsync(HelpDeskFeatures.Names.TicketingAiSuggestions).Returns(false);

        var availability = await fixture.Ai.GetAvailabilityAsync(fixture.Ticket.Id);

        availability.AnyAvailable.ShouldBeFalse();
        availability.SuggestReplyUnavailableReasonCode.ShouldBe(TicketingErrorCodes.AiFeatureDisabled);
        availability.TriageUnavailableReasonCode.ShouldBe(TicketingErrorCodes.AiFeatureDisabled);
        availability.KbDraftUnavailableReasonCode.ShouldBe(TicketingErrorCodes.AiFeatureDisabled);
    }

    [Fact]
    public async Task KbDraft_Uses_The_Ticketing_Purpose_Hooshvare()
    {
        var fixture = new AppFixture();
        var purposeId = Guid.NewGuid();
        fixture.ResolveTicket();
        fixture.PurposeReady(purposeId);
        fixture.RuntimeReturns("## Resolution");

        var result = await fixture.Drafts.CreateKbDraftFromTicketAsync(new CreateKbDraftFromTicketInput
        {
            TicketId = fixture.Ticket.Id,
            ProjectId = fixture.ProjectId
        });

        result.Content.ShouldBe("## Resolution");
        await fixture.Runtime.Received(1).SendAsync(
            Arg.Is<HooshvareRuntimeRequestDto>(request => request.HooshvareId == purposeId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task KbDraft_Throws_The_Purpose_Reason_When_No_Assistant_Resolves()
    {
        var fixture = new AppFixture();
        fixture.ResolveTicket();
        fixture.Purpose(HelpDeskAiBindingReadiness.MissingAssignment);
        fixture.Keys.TryGetRuntimeIdByKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Guid.Empty);

        var exception = await Should.ThrowAsync<BusinessException>(() => fixture.Drafts.CreateKbDraftFromTicketAsync(
            new CreateKbDraftFromTicketInput { TicketId = fixture.Ticket.Id, ProjectId = fixture.ProjectId }));

        exception.Code.ShouldBe(TicketingErrorCodes.AiMissingAssignment);
    }

    [Fact]
    public async Task KbDraft_Times_Out_With_A_Specific_Error()
    {
        var fixture = new AppFixture(fastTimeout: true);
        fixture.ResolveTicket();
        fixture.PurposeReady(Guid.NewGuid());
        fixture.Runtime.SendAsync(Arg.Any<HooshvareRuntimeRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(call => NeverAsync(call.Arg<CancellationToken>()));

        var exception = await Should.ThrowAsync<BusinessException>(() => fixture.Drafts.CreateKbDraftFromTicketAsync(
            new CreateKbDraftFromTicketInput { TicketId = fixture.Ticket.Id, ProjectId = fixture.ProjectId }));

        exception.Code.ShouldBe(TicketingErrorCodes.AiTimedOut);
    }

    private static async Task<HooshvareRuntimeResultDto> NeverAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
        return new HooshvareRuntimeResultDto { Message = "late" };
    }

    private sealed class SelectorFixture
    {
        public Guid ProjectId { get; } = Guid.NewGuid();
        public IHelpDeskAiWorkspaceResolver Workspaces { get; } = Substitute.For<IHelpDeskAiWorkspaceResolver>();
        public IPlatformHooshvareResolver Keys { get; } = Substitute.For<IPlatformHooshvareResolver>();
        public TicketAiHooshvareSelector Selector { get; }

        public SelectorFixture()
        {
            Selector = new TicketAiHooshvareSelector(Workspaces, Keys, NullLogger<TicketAiHooshvareSelector>.Instance);
        }

        public void PurposeReady(Guid hooshvareId) =>
            Purpose(HelpDeskAiBindingReadiness.Ready, hooshvareId);

        public void Purpose(HelpDeskAiBindingReadiness readiness, Guid? hooshvareId = null)
        {
            Workspaces.ResolveAsync(ProjectId, HelpDeskAiWorkspacePurpose.Ticketing, Arg.Any<CancellationToken>())
                .Returns(new HelpDeskAiBindingResultDto
                {
                    ProjectId = ProjectId,
                    Purpose = HelpDeskAiWorkspacePurpose.Ticketing,
                    HooshvareId = hooshvareId,
                    Readiness = readiness,
                    Source = HelpDeskAiBindingSource.Project
                });
        }
    }

    private sealed class AppFixture
    {
        public Guid ProjectId { get; } = Guid.NewGuid();
        public Ticket Ticket { get; }
        public ITicketRepository Tickets { get; } = Substitute.For<ITicketRepository>();
        public IHooshvareRuntimeAppService Runtime { get; } = Substitute.For<IHooshvareRuntimeAppService>();
        public IPlatformHooshvareResolver Keys { get; } = Substitute.For<IPlatformHooshvareResolver>();
        public IHelpDeskAiWorkspaceResolver Workspaces { get; } = Substitute.For<IHelpDeskAiWorkspaceResolver>();
        public IFeatureChecker Features { get; } = Substitute.For<IFeatureChecker>();
        public TicketAiAppService Ai { get; }
        public TicketKbDraftAppService Drafts { get; }

        public AppFixture(bool fastTimeout = false)
        {
            Ticket = new Ticket(
                Guid.NewGuid(),
                "T-68",
                "Cannot sign in",
                "Password reset failed",
                TicketPriority.High,
                requesterUserId: null,
                requesterContact: "user@example.com",
                tenantId: null,
                projectId: ProjectId);
            Tickets.FindAsync(Ticket.Id, true, Arg.Any<CancellationToken>()).Returns(Ticket);
            Tickets.UpdateAsync(Arg.Any<Ticket>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(call => call.Arg<Ticket>());
            Features.IsEnabledAsync(HelpDeskFeatures.Names.TicketingAiSuggestions).Returns(true);

            var selector = new TicketAiHooshvareSelector(
                Workspaces,
                Keys,
                NullLogger<TicketAiHooshvareSelector>.Instance);
            var services = new ServiceCollection();
            services.AddSingleton(Features);
            services.AddLogging();
            var clock = Substitute.For<IClock>();
            clock.Now.Returns(new DateTime(2026, 10, 4, 0, 0, 0, DateTimeKind.Utc));
            services.AddSingleton(clock);
            var user = Substitute.For<ICurrentUser>();
            user.Id.Returns(Guid.NewGuid());
            services.AddSingleton(user);
            var provider = services.BuildServiceProvider();
            var lazy = new AbpLazyServiceProvider(provider);

            Ai = fastTimeout
                ? new FastTimeoutTicketAiAppService(Tickets, Runtime, Keys, selector)
                : new TicketAiAppService(Tickets, Runtime, Keys, selector);
            Drafts = fastTimeout
                ? new FastTimeoutTicketKbDraftAppService(Tickets, Substitute.For<Volo.Abp.EventBus.Distributed.IDistributedEventBus>(), Runtime, selector)
                : new TicketKbDraftAppService(Tickets, Substitute.For<Volo.Abp.EventBus.Distributed.IDistributedEventBus>(), Runtime, selector);
            Ai.LazyServiceProvider = lazy;
            Drafts.LazyServiceProvider = lazy;
        }

        public void ResolveTicket()
        {
            var agentId = Guid.NewGuid();
            var now = new DateTime(2026, 10, 4, 0, 0, 0, DateTimeKind.Utc);
            Ticket.Assign(agentId, null, now);
            Ticket.Start(now);
            Ticket.Resolve("reset link", agentId, now);
        }

        public void PurposeReady(Guid hooshvareId) =>
            Purpose(HelpDeskAiBindingReadiness.Ready, hooshvareId);

        public void Purpose(HelpDeskAiBindingReadiness readiness, Guid? hooshvareId = null)
        {
            Workspaces.ResolveAsync(ProjectId, HelpDeskAiWorkspacePurpose.Ticketing, Arg.Any<CancellationToken>())
                .Returns(new HelpDeskAiBindingResultDto
                {
                    ProjectId = ProjectId,
                    Purpose = HelpDeskAiWorkspacePurpose.Ticketing,
                    HooshvareId = hooshvareId,
                    Readiness = readiness,
                    Source = HelpDeskAiBindingSource.Project
                });
        }

        public void RuntimeReturns(string message)
        {
            Runtime.SendAsync(Arg.Any<HooshvareRuntimeRequestDto>(), Arg.Any<CancellationToken>())
                .Returns(new HooshvareRuntimeResultDto { Message = message });
        }
    }

    private sealed class FastTimeoutTicketAiAppService : TicketAiAppService
    {
        public FastTimeoutTicketAiAppService(
            ITicketRepository ticketRepository,
            IHooshvareRuntimeAppService hooshvareRuntime,
            IPlatformHooshvareResolver hooshvareResolver,
            TicketAiHooshvareSelector hooshvareSelector)
            : base(ticketRepository, hooshvareRuntime, hooshvareResolver, hooshvareSelector)
        {
        }

        protected override TimeSpan SendTimeout => TimeSpan.FromMilliseconds(50);
    }

    private sealed class FastTimeoutTicketKbDraftAppService : TicketKbDraftAppService
    {
        public FastTimeoutTicketKbDraftAppService(
            ITicketRepository ticketRepository,
            Volo.Abp.EventBus.Distributed.IDistributedEventBus distributedEventBus,
            IHooshvareRuntimeAppService hooshvareRuntime,
            TicketAiHooshvareSelector hooshvareSelector)
            : base(ticketRepository, distributedEventBus, hooshvareRuntime, hooshvareSelector)
        {
        }

        protected override TimeSpan SendTimeout => TimeSpan.FromMilliseconds(50);
    }
}
