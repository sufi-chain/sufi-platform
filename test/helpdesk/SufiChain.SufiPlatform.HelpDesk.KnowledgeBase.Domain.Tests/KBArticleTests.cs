using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Articles;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.ETOs;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase;

public class KBArticleTests
{
    [Fact]
    public void Publish_should_change_status_to_published()
    {
        var article = CreateArticle();
        article.Publish(DateTime.UtcNow);
        article.Status.ShouldBe(ArticleStatus.Published);
    }

    [Fact]
    public void Unpublish_should_require_published_article()
    {
        var article = CreateArticle();
        Should.Throw<ArticleNotPublishedException>(() => article.Unpublish(DateTime.UtcNow));
    }

    [Fact]
    public void Publish_should_add_article_published_distributed_event()
    {
        var now = DateTime.UtcNow;
        var article = CreateArticle();

        article.Publish(now);

        var eventData = article.GetDistributedEvents().Single().EventData.ShouldBeOfType<KBArticlePublishedEto>();
        eventData.ArticleId.ShouldBe(article.Id);
        eventData.Status.ShouldBe(ArticleStatus.Published);
        eventData.OccurredAt.ShouldBe(now);
    }

    [Fact]
    public void UpdateTranslation_should_mark_article_as_not_indexed_and_add_event()
    {
        var article = CreateArticle();
        article.MarkAsIndexed(DateTime.UtcNow);

        article.UpdateTranslation("en", "Updated title", "updated content", DateTime.UtcNow);

        var translation = article.GetTranslation("en");
        translation.Title.ShouldBe("Updated title");
        translation.Content.ShouldBe("updated content");
        article.IsIndexed.ShouldBeFalse();
        article.LastIndexedTime.ShouldBeNull();
        article.GetDistributedEvents().Single().EventData.ShouldBeOfType<KBArticleUpdatedEto>();
    }

    [Fact]
    public void UpdateTranslation_should_not_allow_archived_article()
    {
        var article = CreateArticle();
        article.Archive(DateTime.UtcNow);
        article.ClearDistributedEvents();

        var exception = Should.Throw<BusinessException>(() =>
            article.UpdateTranslation("en", "Updated title", "updated content", DateTime.UtcNow));

        exception.Code.ShouldBe(KnowledgeBaseErrorCodes.CannotUpdateArchivedArticle);
        var translation = article.GetTranslation("en");
        translation.Title.ShouldBe("Title");
        translation.Content.ShouldBe("content");
        article.GetDistributedEvents().ShouldBeEmpty();
    }

    [Fact]
    public void AddVersion_should_require_matching_article_id()
    {
        var article = CreateArticle();
        var other = CreateArticle("Other", "other", "content");
        var version = KBArticleVersion.CreateFromArticle(
            Guid.NewGuid(),
            other,
            other.GetTranslation("en"),
            "Initial version");

        var exception = Should.Throw<BusinessException>(() => article.AddVersion(version));

        exception.Code.ShouldBe(KnowledgeBaseErrorCodes.ArticleVersionMismatch);
    }

    [Fact]
    public void AddAttachment_should_add_file_and_mark_article_as_not_indexed()
    {
        var article = CreateArticle();
        article.MarkAsIndexed(DateTime.UtcNow);
        var fileId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        article.AddAttachment(fileId, "guide.pdf", 1024, "application/pdf", now);

        var attachment = article.Attachments.Single();
        attachment.FileId.ShouldBe(fileId);
        attachment.FileName.ShouldBe("guide.pdf");
        attachment.FileSize.ShouldBe(1024);
        attachment.MimeType.ShouldBe("application/pdf");
        attachment.AttachedAt.ShouldBe(now);
        article.IsIndexed.ShouldBeFalse();
        article.LastIndexedTime.ShouldBeNull();
    }

    [Fact]
    public void AddAttachment_should_reject_duplicates()
    {
        var article = CreateArticle();
        var fileId = Guid.NewGuid();
        article.AddAttachment(fileId, "guide.pdf", 1024, "application/pdf", DateTime.UtcNow);

        var exception = Should.Throw<BusinessException>(() => article.AddAttachment(fileId, "guide.pdf", 1024, "application/pdf", DateTime.UtcNow));

        exception.Code.ShouldBe(KnowledgeBaseErrorCodes.ArticleAttachmentAlreadyExists);
    }

    [Fact]
    public void AddAttachment_should_enforce_max_attachment_count()
    {
        var article = CreateArticle();
        for (var index = 0; index < KnowledgeBaseConsts.MaxArticleAttachmentCount; index++)
        {
            article.AddAttachment(Guid.NewGuid(), $"guide-{index}.pdf", 1024, "application/pdf", DateTime.UtcNow);
        }

        var exception = Should.Throw<BusinessException>(() => article.AddAttachment(Guid.NewGuid(), "extra.pdf", 1024, "application/pdf", DateTime.UtcNow));

        exception.Code.ShouldBe(KnowledgeBaseErrorCodes.ArticleAttachmentLimitExceeded);
    }

    [Fact]
    public void RemoveAttachment_should_remove_file_and_mark_article_as_not_indexed()
    {
        var article = CreateArticle();
        var fileId = Guid.NewGuid();
        article.AddAttachment(fileId, "guide.pdf", 1024, "application/pdf", DateTime.UtcNow);
        article.MarkAsIndexed(DateTime.UtcNow);

        article.RemoveAttachment(fileId);

        article.Attachments.ShouldBeEmpty();
        article.IsIndexed.ShouldBeFalse();
        article.LastIndexedTime.ShouldBeNull();
    }

    [Fact]
    public void AddAttachment_should_not_allow_archived_article()
    {
        var article = CreateArticle();
        article.Archive(DateTime.UtcNow);

        var exception = Should.Throw<BusinessException>(() => article.AddAttachment(Guid.NewGuid(), "guide.pdf", 1024, "application/pdf", DateTime.UtcNow));

        exception.Code.ShouldBe(KnowledgeBaseErrorCodes.CannotAttachFileToArchivedArticle);
    }

    private static KBArticle CreateArticle(
        string title = "Title",
        string slug = "title",
        string content = "content",
        string language = "en")
    {
        var article = new KBArticle(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        article.AddTranslation(Guid.NewGuid(), language, title, slug, content);
        return article;
    }
}
