using Microsoft.AspNetCore.Components;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Blazor.Public.Components;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Application.Tests.Hooshvare;

public class HooshvareTimelineActionMessageContractTests
{
    [Fact]
    public void Action_Card_Exposes_Confirm_And_Reject_Callbacks()
    {
        var type = typeof(HooshvareTimelineActionMessage);
        var confirm = type.GetProperty(nameof(HooshvareTimelineActionMessage.OnConfirm));
        var reject = type.GetProperty(nameof(HooshvareTimelineActionMessage.OnReject));
        confirm.ShouldNotBeNull();
        reject.ShouldNotBeNull();
        confirm!.PropertyType.ShouldBe(typeof(EventCallback));
        reject!.PropertyType.ShouldBe(typeof(EventCallback));

        var pending = type.GetProperty(nameof(HooshvareTimelineActionMessage.IsPending));
        pending.ShouldNotBeNull();
        pending!.PropertyType.ShouldBe(typeof(bool));
    }
}
