using System.Reflection;
using Shouldly;
using SufiChain.SufiPlatform.Calendar.AI.Tools;
using SufiChain.SufiPlatform.SufiAI;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.AI;

public class CalendarWriteToolEffectTests
{
    [Theory]
    [InlineData(typeof(CalendarCreateEventTool), nameof(CalendarCreateEventTool.CreateEventAsync), "calendar.create_event")]
    [InlineData(typeof(CalendarMoveEventTool), nameof(CalendarMoveEventTool.MoveEventAsync), "calendar.move_event")]
    [InlineData(typeof(CalendarCancelEventTool), nameof(CalendarCancelEventTool.CancelEventAsync), "calendar.cancel_event")]
    [InlineData(typeof(CalendarMoveOccurrenceTool), nameof(CalendarMoveOccurrenceTool.MoveOccurrenceAsync), "calendar.move_occurrence")]
    [InlineData(typeof(CalendarCancelOccurrenceTool), nameof(CalendarCancelOccurrenceTool.CancelOccurrenceAsync), "calendar.cancel_occurrence")]
    public void Mutation_tools_declare_an_explicit_write_effect(Type toolType, string methodName, string toolName)
    {
        var attribute = toolType.GetMethod(methodName)!.GetCustomAttribute<SufiAiMcpToolAttribute>()!;
        attribute.Name.ShouldBe(toolName);
        attribute.Effect.ShouldBe(AgentActionEffect.Write);
        attribute.TryGetEffect(out var effect, out var refusal).ShouldBeTrue();
        refusal.ShouldBeNull();
        effect.ShouldBe(AgentActionEffect.Write);

        var declared = toolType.GetProperty(nameof(ISufiAITool.Effect));
        declared.ShouldNotBeNull();
        declared.DeclaringType.ShouldBe(toolType);
    }
}
