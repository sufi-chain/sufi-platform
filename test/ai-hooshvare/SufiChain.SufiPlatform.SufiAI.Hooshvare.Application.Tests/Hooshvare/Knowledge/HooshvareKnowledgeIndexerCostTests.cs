using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.FileManager;
using Volo.Abp.Timing;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare.Knowledge;

public class HooshvareKnowledgeIndexerCostTests
{
    private const string WorkspaceName = "hooshvare-workspace";

    private static readonly byte[] PdfBytes = Encoding.ASCII.GetBytes("%PDF-1.7 sample");

    private readonly IFileStorageTrustedService _storage = Substitute.For<IFileStorageTrustedService>();
    private readonly IHooshvareMarkdownConverter _converter = Substitute.For<IHooshvareMarkdownConverter>();
    private readonly Guid _derivedId = Guid.NewGuid();
    private readonly Guid _newDerivedId = Guid.NewGuid();

    public HooshvareKnowledgeIndexerCostTests()
    {
        _storage.GetContentAsync(Arg.Any<Guid>()).Returns(new FileContentBytesDto { Content = PdfBytes });
        _storage.FindAsync(_derivedId).Returns(new FileReferenceDto { Id = _derivedId, FileName = "report.md" });
        _storage.UploadAsync(Arg.Any<FileUploadRequest>()).Returns(new FileReferenceDto { Id = _newDerivedId, FileName = "report.md" });
        _converter.ConvertAsync(Arg.Any<HooshvareMarkdownConversionRequest>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var request = call.Arg<HooshvareMarkdownConversionRequest>();
                request.Usage.AddCall(1200, 300, null);
                request.Usage.AddSkipped();
                return "# گزارش";
            });
    }

    [Fact]
    public async Task Unchanged_File_Should_Reuse_Derived_Markdown_Without_Model_Calls()
    {
        var file = ConvertedFile(HooshvareKnowledgeIndexer.ComputeContentHash(PdfBytes), HooshvareKnowledgeLibraryConsts.ConverterVersion);

        await CreateIndexer().StoreAsync(file, forceReconvert: false);

        await _converter.DidNotReceiveWithAnyArgs().ConvertAsync(default!, default);
        await _storage.DidNotReceiveWithAnyArgs().UploadAsync(default!);
    }

    [Theory]
    [InlineData(true, "current", "1")]
    [InlineData(false, "stale-hash", "1")]
    [InlineData(false, "current", "0")]
    public async Task Forced_Changed_Or_Outdated_File_Should_Reconvert_And_Record_Usage(bool force, string hash, string version)
    {
        var file = ConvertedFile(hash == "current" ? HooshvareKnowledgeIndexer.ComputeContentHash(PdfBytes) : hash, version);

        await CreateIndexer().StoreAsync(file, force);

        await _converter.Received(1).ConvertAsync(Arg.Any<HooshvareMarkdownConversionRequest>(), Arg.Any<CancellationToken>());
        await _storage.Received(1).SetPropertiesAsync(file.Id, Arg.Is<IReadOnlyDictionary<string, string?>>(properties =>
            properties[HooshvareKnowledgeLibraryConsts.DerivedFileIdProperty] == _newDerivedId.ToString("D")
            && properties[HooshvareKnowledgeLibraryConsts.ContentHashProperty] == HooshvareKnowledgeIndexer.ComputeContentHash(PdfBytes)
            && properties[HooshvareKnowledgeLibraryConsts.ConverterVersionProperty] == HooshvareKnowledgeLibraryConsts.ConverterVersion
            && properties[HooshvareKnowledgeLibraryConsts.ConversionTokensProperty] == "1500"
            && properties[HooshvareKnowledgeLibraryConsts.ConversionCallsProperty] == "1"
            && properties[HooshvareKnowledgeLibraryConsts.SkippedPartsProperty] == "1"));
    }

    [Fact]
    public async Task Missing_Derived_File_Should_Reconvert_Even_When_Hash_Matches()
    {
        _storage.FindAsync(_derivedId).Returns((FileReferenceDto?)null);
        var file = ConvertedFile(HooshvareKnowledgeIndexer.ComputeContentHash(PdfBytes), HooshvareKnowledgeLibraryConsts.ConverterVersion);

        await CreateIndexer().StoreAsync(file, forceReconvert: false);

        await _converter.Received(1).ConvertAsync(Arg.Any<HooshvareMarkdownConversionRequest>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(100, 100, false)]
    [InlineData(101, 100, true)]
    [InlineData(long.MaxValue, 0, false)]
    public void Size_Limit_Should_Apply_Only_When_Positive(long size, long limit, bool expected)
    {
        HooshvareKnowledgeIndexer.IsTooLarge(size, new HooshvareKnowledgeOptions { MaxFileSizeBytes = limit }).ShouldBe(expected);
    }

    private FileReferenceDto ConvertedFile(string contentHash, string converterVersion) => new()
    {
        Id = Guid.NewGuid(),
        FileName = "report.pdf",
        MimeType = "application/pdf",
        SizeInBytes = PdfBytes.Length,
        Properties = new Dictionary<string, string>
        {
            [HooshvareKnowledgeLibraryConsts.DerivedFileIdProperty] = _derivedId.ToString("D"),
            [HooshvareKnowledgeLibraryConsts.ContentHashProperty] = contentHash,
            [HooshvareKnowledgeLibraryConsts.ConverterVersionProperty] = converterVersion
        }
    };

    private TestIndexer CreateIndexer() => new(_storage, _converter);

    private sealed class TestIndexer(IFileStorageTrustedService storage, IHooshvareMarkdownConverter converter)
        : HooshvareKnowledgeIndexer(
            Substitute.For<IHooshvareDefinitionRepository>(),
            storage,
            converter,
            Substitute.For<IHooshvareWorkspaceResolver>(),
            Substitute.For<ISufiAIRagService>(),
            Substitute.For<IClock>(),
            Microsoft.Extensions.Options.Options.Create(new HooshvareKnowledgeOptions()),
            NullLogger<HooshvareKnowledgeIndexer>.Instance)
    {
        public Task StoreAsync(FileReferenceDto file, bool forceReconvert) =>
            StoreDerivedMarkdownAsync(file, WorkspaceName, forceReconvert, CancellationToken.None);
    }
}
