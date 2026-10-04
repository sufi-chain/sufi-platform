using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.Projects;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.Projects;

public class HelpDeskProjectDescriptionTests
{
    [Fact]
    public void SetDescription_Should_Store_Trimmed_Text()
    {
        var project = new HelpDeskProject(Guid.NewGuid(), "QA Support", "qa-support");

        project.SetDescription("  QA P13 desc  ");

        project.Description.ShouldBe("QA P13 desc");
    }

    [Fact]
    public void SetDescription_Should_Store_A_Persian_Description()
    {
        var project = new HelpDeskProject(Guid.NewGuid(), "QA Support", "qa-support");

        project.SetDescription("  توضیحات پروژه  ");

        project.Description.ShouldBe("توضیحات پروژه");
    }

    [Fact]
    public void SetDescription_Should_Clear_Whitespace_And_Null()
    {
        var project = new HelpDeskProject(Guid.NewGuid(), "QA Support", "qa-support");
        project.SetDescription("QA P13 desc");

        project.SetDescription("   ");
        project.Description.ShouldBeNull();

        project.SetDescription("QA P13 desc");
        project.SetDescription(null);
        project.Description.ShouldBeNull();
    }
}
