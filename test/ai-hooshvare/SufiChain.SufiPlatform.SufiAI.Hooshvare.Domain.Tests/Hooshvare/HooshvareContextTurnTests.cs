using System;

using System.Linq;

using Shouldly;

using Xunit;



namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;



public class HooshvareContextTurnTests

{

    [Fact]

    public void Turn_payload_does_not_carry_model_configuration_id()

    {

        typeof(HooshvareContextTurn)

            .GetProperties()

            .Select(property => property.Name)

            .ShouldNotContain("ModelConfigurationId");

    }

}

