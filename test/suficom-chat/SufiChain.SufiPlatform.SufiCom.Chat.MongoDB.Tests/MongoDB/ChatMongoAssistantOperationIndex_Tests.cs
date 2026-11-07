using MongoDB.Driver;
using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.MongoDB;
using Shouldly;
using Volo.Abp.MongoDB;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.MongoDB;

/// <summary>
/// sufi-chain#551: the assistant operation index is created when the Mongo provider starts.
/// </summary>
public class ChatMongoAssistantOperationIndex_Tests : ChatApplicationTestBase<SufiComChatMongoDbTestModule>
{
    [Fact]
    public async Task The_operation_index_uses_exists_and_type_and_rejects_a_duplicate_part()
    {
        using (var uow = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true))
        {
            await GetRequiredService<ChatMongoDbIndexesInitializer>().CreateIndexesAsync();
            var context = await GetRequiredService<IMongoDbContextProvider<ChatMongoDbContext>>().GetDbContextAsync();
            var indexes = await (await context.Messages.Indexes.ListAsync()).ToListAsync();
            var operation = indexes.Single(index => index["name"].AsString == ChatMongoDbIndexesInitializer.AssistantOperationIndexName);
            var filter = operation["partialFilterExpression"].ToString();
            filter.ShouldNotContain("$ne");
            filter.ShouldContain("$exists");
            filter.ShouldContain("$type");
            filter.ShouldContain("5");
            await uow.CompleteAsync();
        }

        var sessionId = Guid.NewGuid();
        var operationId = Guid.Parse("22222222-2222-2222-2222-222222222221");
        using var write = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: true);
        var repository = GetRequiredService<IChatMessageRepository>();
        await repository.InsertAsync(new ChatMessage(Guid.NewGuid(), null, sessionId, "one", ChatMessageSenderKind.Visitor), autoSave: true);
        await repository.InsertAsync(new ChatMessage(Guid.NewGuid(), null, sessionId, "two", ChatMessageSenderKind.Visitor), autoSave: true);
        await repository.InsertAsync(Assistant(sessionId, operationId, 0, "part"), autoSave: true);
        await Should.ThrowAsync<MongoWriteException>(() => repository.InsertAsync(
            Assistant(sessionId, operationId, 0, "again"),
            autoSave: true));
    }

    private static ChatMessage Assistant(Guid sessionId, Guid operationId, int partIndex, string body)
    {
        var message = new ChatMessage(Guid.NewGuid(), tenantId: null, sessionId, body, ChatMessageSenderKind.Assistant);
        message.AssignAssistantOperation(operationId, partIndex);
        return message;
    }
}
