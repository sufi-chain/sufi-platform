using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.Projects;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.Projects;

public class HelpDeskProjectAssignmentTests
{
    [Fact]
    public void SetHooshvareReference_Should_Store_The_Selected_Hooshvare()
    {
        var hooshvareId = Guid.NewGuid();
        var assignment = new ProjectAiWorkspaceAssignment(
            Guid.NewGuid(),
            Guid.NewGuid(),
            HelpDeskAiWorkspacePurpose.RagIndexing,
            hooshvareId,
            "Knowledge",
            Guid.NewGuid());

        assignment.HooshvareId.ShouldBe(hooshvareId);
        assignment.HooshvareName.ShouldBe("Knowledge");
    }

    [Fact]
    public void SetHooshvareReference_Should_Reject_An_Empty_Hooshvare()
    {
        var assignment = new ProjectAiWorkspaceAssignment(
            Guid.NewGuid(),
            Guid.NewGuid(),
            HelpDeskAiWorkspacePurpose.Default,
            Guid.NewGuid(),
            "Chat");

        Should.Throw<ArgumentException>(() => assignment.SetHooshvareReference(Guid.Empty, "Chat"));
    }
}
