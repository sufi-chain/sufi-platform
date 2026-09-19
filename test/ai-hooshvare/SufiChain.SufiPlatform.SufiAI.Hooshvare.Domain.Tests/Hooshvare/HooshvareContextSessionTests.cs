using System;

using Shouldly;

using Xunit;



namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;



public class HooshvareContextSessionTests

{

    [Fact]

    public void Should_Treat_Model_As_Active_Route_And_Refresh_Window()

    {

        var session = new HooshvareContextSession(

            Guid.NewGuid(),

            tenantId: null,

            Guid.NewGuid(),

            Guid.NewGuid(),

            "workspace",

            "workspace-default",

            200000);



        session.Model.ShouldBe("workspace-default");

        session.MaxContextTokens.ShouldBe(200000);



        session.SetActiveRoute("route-model", 128000);



        session.Model.ShouldBe("route-model");

        session.MaxContextTokens.ShouldBe(128000);

    }



    [Fact]

    public void Should_Fallback_To_Default_Window_When_Route_Window_Is_Invalid()

    {

        var session = new HooshvareContextSession(

            Guid.NewGuid(),

            tenantId: null,

            Guid.NewGuid(),

            Guid.NewGuid(),

            "workspace",

            "route-model",

            0);



        session.MaxContextTokens.ShouldBe(200000);



        session.UpdateWorkspace(Guid.NewGuid(), "other-workspace", "next-route", -1);



        session.WorkspaceName.ShouldBe("other-workspace");

        session.Model.ShouldBe("next-route");

        session.MaxContextTokens.ShouldBe(200000);

    }



    [Fact]

    public void Different_route_windows_produce_different_session_budgets()

    {

        var session = new HooshvareContextSession(

            Guid.NewGuid(),

            tenantId: null,

            Guid.NewGuid(),

            Guid.NewGuid(),

            "workspace",

            "small-route",

            8000);



        session.MaxContextTokens.ShouldBe(8000);



        session.SetActiveRoute("large-route", 128000);



        session.Model.ShouldBe("large-route");

        session.MaxContextTokens.ShouldBe(128000);

        session.MaxContextTokens.ShouldNotBe(8000);

    }

}

