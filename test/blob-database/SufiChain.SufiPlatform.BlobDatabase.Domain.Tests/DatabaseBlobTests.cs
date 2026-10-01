using Shouldly;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.BlobDatabase;

public class DatabaseBlobTests
{
    [Fact]
    public void Constructor_Should_Store_Content_Under_The_Limit()
    {
        var previous = DatabaseBlobConsts.MaxContentLength;
        try
        {
            DatabaseBlobConsts.MaxContentLength = 8;
            var blob = new DatabaseBlob(Guid.NewGuid(), Guid.NewGuid(), "note.txt", [1, 2, 3]);
            blob.Name.ShouldBe("note.txt");
            blob.Content.Length.ShouldBe(3);
        }
        finally
        {
            DatabaseBlobConsts.MaxContentLength = previous;
        }
    }

    [Fact]
    public void Constructor_Should_Reject_Content_At_The_Limit()
    {
        var previous = DatabaseBlobConsts.MaxContentLength;
        try
        {
            DatabaseBlobConsts.MaxContentLength = 4;
            Should.Throw<AbpException>(() => new DatabaseBlob(Guid.NewGuid(), Guid.NewGuid(), "note.txt", new byte[4]));
        }
        finally
        {
            DatabaseBlobConsts.MaxContentLength = previous;
        }
    }
}
