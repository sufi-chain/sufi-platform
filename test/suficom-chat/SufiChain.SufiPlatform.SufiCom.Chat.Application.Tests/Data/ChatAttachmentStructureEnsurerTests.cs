using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.FileManager.Configuration;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.SufiCom.Chat.Configuration;
using SufiChain.SufiPlatform.SufiCom.Chat.Data;
using Volo.Abp.Guids;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Data;

public class ChatAttachmentStructureEnsurerTests
{
    [Fact]
    public async Task Ensure_inserts_ChatAttachments_once_for_the_current_database()
    {
        var store = new List<FileStructure>();
        var repository = Substitute.For<IFileStructureRepository>();
        repository.FindByKeyAsync(ChatFileStructureKeys.Attachments, Arg.Any<CancellationToken>())
            .Returns(_ => store.FirstOrDefault(structure => structure.Key == ChatFileStructureKeys.Attachments));
        repository.InsertAsync(Arg.Any<FileStructure>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var entity = call.Arg<FileStructure>();
                store.Add(entity);
                return entity;
            });

        var guids = Substitute.For<IGuidGenerator>();
        guids.Create().Returns(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        var ensurer = new ChatAttachmentStructureEnsurer(
            repository,
            guids,
            Options.Create(new ChatOptions()),
            Options.Create(new FileManagerOptions()),
            NullLogger<ChatAttachmentStructureEnsurer>.Instance);

        await ensurer.EnsureAsync();
        await ensurer.EnsureAsync();

        store.Count.ShouldBe(1);
        store[0].Key.ShouldBe(ChatFileStructureKeys.Attachments);
        store[0].AllowedExtensions.Split(',').ShouldContain("png");
        store[0].AllowedMimeTypes.Split(',').ShouldContain("image/png");
        await repository.Received(1).InsertAsync(
            Arg.Any<FileStructure>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }
}
