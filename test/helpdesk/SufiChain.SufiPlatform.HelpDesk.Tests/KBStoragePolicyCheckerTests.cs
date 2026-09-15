using NSubstitute;
using SufiChain.SufiPlatform.HelpDesk.Features;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Projects;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Storage;
using SufiChain.SufiPlatform.HelpDesk.Storage;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Features;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.Tests;

public class KBStoragePolicyCheckerTests
{
    private readonly IFeatureChecker _featureChecker;
    private readonly KBStoragePolicyChecker _policyChecker;

    public KBStoragePolicyCheckerTests()
    {
        _featureChecker = Substitute.For<IFeatureChecker>();
        _policyChecker = new KBStoragePolicyChecker(_featureChecker);
    }

    [Fact]
    public void DefaultStorageType_Should_Be_None()
    {
        _policyChecker.DefaultStorageType.ShouldBe(StorageType.None);
    }

    [Fact]
    public async Task GetAllowedStorageTypesAsync_Should_Return_None_When_No_Features_Enabled()
    {
        _featureChecker.IsEnabledAsync(Arg.Any<string>()).Returns(false);

        var allowed = await _policyChecker.GetAllowedStorageTypesAsync();

        allowed.ShouldBe([StorageType.None]);
    }

    [Fact]
    public async Task GetAllowedStorageTypesAsync_Should_Include_Enabled_Storage_Features()
    {
        _featureChecker.IsEnabledAsync(HelpDeskFeatures.Names.KnowledgeBaseStorageLocalOnly).Returns(true);
        _featureChecker.IsEnabledAsync(HelpDeskFeatures.Names.KnowledgeBaseStorageGitBacked).Returns(true);
        _featureChecker.IsEnabledAsync(Arg.Is<string>(x =>
            x != HelpDeskFeatures.Names.KnowledgeBaseStorageLocalOnly &&
            x != HelpDeskFeatures.Names.KnowledgeBaseStorageGitBacked)).Returns(false);

        var allowed = await _policyChecker.GetAllowedStorageTypesAsync();

        allowed.ShouldBe([StorageType.None, StorageType.LocalOnly, StorageType.GitBacked]);
    }

    [Fact]
    public async Task EnsureAllowedAsync_Should_Throw_When_StorageType_Is_Not_Enabled()
    {
        _featureChecker.IsEnabledAsync(Arg.Any<string>()).Returns(false);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            _policyChecker.EnsureAllowedAsync(StorageType.LocalOnly));

        exception.Code.ShouldBe(HelpDeskErrorCodes.StorageTypeNotAllowed);
    }

    [Fact]
    public async Task GetEffectiveStorageTypeAsync_Should_Fallback_To_None_When_Not_Allowed()
    {
        _featureChecker.IsEnabledAsync(Arg.Any<string>()).Returns(false);

        var effective = await _policyChecker.GetEffectiveStorageTypeAsync(StorageType.GitBacked);

        effective.ShouldBe(StorageType.None);
    }
}
