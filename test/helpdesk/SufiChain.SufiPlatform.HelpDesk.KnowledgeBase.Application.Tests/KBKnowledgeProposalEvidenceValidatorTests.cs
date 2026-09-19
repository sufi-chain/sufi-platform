using System.Security.Cryptography;
using System.Text;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Guidance;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Relations;
using SufiChain.SufiPlatform.SufiAI.Knowledge;
using Volo.Abp.Authorization;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase;

public class KBKnowledgeProposalEvidenceValidatorTests
{
    [Theory]
    [InlineData("valid", true)]
    [InlineData("hash", false)]
    [InlineData("version", false)]
    [InlineData("scope", false)]
    [InlineData("article", false)]
    [InlineData("length", false)]
    [InlineData("unavailable", false)]
    [InlineData("denied", false)]
    [InlineData("actor", false)]
    [InlineData("tenant", false)]
    [InlineData("locator", false)]
    [InlineData("changed-context", false)]
    public async Task Should_Require_Exact_Current_Authorized_Passage(string scenario, bool expected)
    {
        var actor = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var project = Guid.NewGuid();
        var article = Guid.NewGuid();
        var version = Guid.NewGuid();
        const string passage = "Resolution evidence";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(passage))).ToLowerInvariant();
        var tenant = Substitute.For<ICurrentTenant>();
        tenant.Id.Returns(scenario == "tenant" ? null : tenantId);
        var user = Substitute.For<ICurrentUser>();
        user.IsAuthenticated.Returns(true);
        user.Id.Returns(scenario == "actor" ? Guid.NewGuid() : actor);
        var reader = Substitute.For<IHelpDeskKbEvidenceMcpAppService>();
        reader.GetArticleEvidenceAsync(Arg.Any<GetArticleEvidenceInput>()).Returns(call =>
        {
            if (scenario == "denied") throw new AbpAuthorizationException();
            if (scenario == "changed-context") tenant.Id.Returns(Guid.NewGuid());
            var input = call.Arg<GetArticleEvidenceInput>();
            input.ProjectId.ShouldBe(project);
            input.ArticleId.ShouldBe(article);
            input.VersionId.ShouldBe(version);
            input.LanguageCode.ShouldBe("en");
            input.Offset.ShouldBe(0);
            input.MaxCharacters.ShouldBe(passage.Length);
            return Task.FromResult(new ArticleEvidenceResultDto
            {
                Status = scenario == "unavailable" ? "unavailable" : "ok",
                Content = scenario == "length" ? "short" : passage,
                Citation = new ProjectGuidanceCitationDto
                {
                    ProjectId = scenario == "scope" ? Guid.NewGuid() : project,
                    ArticleId = scenario == "article" ? Guid.NewGuid() : article,
                    VersionId = scenario == "version" ? Guid.NewGuid() : version,
                    LanguageCode = "en"
                }
            });
        });
        var evidence = new KnowledgeProposalEvidence(tenantId, "helpdesk.project", project, "helpdesk.article", article,
            version.ToString("N"), scenario == "hash" ? new string('0', 64) : hash,
            scenario == "locator" ? "utf16:0:999999:en" : $"utf16:0:{passage.Length}:en");
        var validator = new KBKnowledgeProposalEvidenceValidator(reader, tenant, user);
        (await validator.IsCurrentAndAccessibleAsync(evidence, actor)).ShouldBe(expected);
        if (scenario is "actor" or "tenant" or "locator")
            await reader.DidNotReceive().GetArticleEvidenceAsync(Arg.Any<GetArticleEvidenceInput>());
    }
}
