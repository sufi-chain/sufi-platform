using System;
using System.Linq;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

public class CopilotContextTurnTests
{
    [Fact]
    public void Turn_payload_does_not_carry_model_configuration_id()
    {
        typeof(CopilotContextTurn)
            .GetProperties()
            .Select(property => property.Name)
            .ShouldNotContain("ModelConfigurationId");
    }
}
