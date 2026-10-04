using System.Diagnostics;
using System.Text;
using Shouldly;
using SufiChain.SufiPlatform.Content.Rendering;
using SufiChain.SufiPlatform.SufiCMS.Html;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCMS;

public class LargeHtmlSectionImportTests
{
    [Fact]
    public void Prepare_Large_Single_File_Section_Keeps_The_Image_And_Finishes()
    {
        var html = BuildLargeSection(out var imageMarker);
        html.Length.ShouldBeGreaterThan(380_000);

        var importer = new CmsHtmlSectionImport(new CmsHtmlAnnotator());
        var watch = Stopwatch.StartNew();
        var prepared = importer.Prepare(html);
        watch.Stop();

        watch.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(5));
        prepared.ShouldContain("data-cms-section");
        prepared.ShouldContain(imageMarker);
        prepared.ShouldNotContain("cms.invalid/inline");
    }

    [Fact]
    public void Parse_And_Insert_Round_Trip_A_Large_Data_Uri()
    {
        var html = BuildLargeSection(out var imageMarker);
        var watch = Stopwatch.StartNew();
        var serialized = CmsHtml.Serialize(CmsHtml.Parse(html));
        var editor = new CmsSectionEditor(new CmsHtmlAnnotator());
        var host = new CmsHtmlAnnotator().Annotate("<section><h1>Host</h1></section>");
        var inserted = editor.InsertSection(host, null, html);
        watch.Stop();

        watch.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(5));
        serialized.ShouldContain(imageMarker);
        serialized.ShouldNotContain("cms.invalid/inline");
        inserted.ShouldContain(imageMarker);
        inserted.ShouldNotContain("cms.invalid/inline");
        inserted.ShouldContain("data-cms-section");
    }

    [Fact]
    public void Small_Data_Uri_Stays_In_The_Markup()
    {
        var html = "<p><img src=\"data:image/png;base64,AAAA\"></p>";
        var serialized = CmsHtml.Serialize(CmsHtml.Parse(html));
        serialized.ShouldContain("data:image/png;base64,AAAA");
        serialized.ShouldNotContain("cms.invalid/inline");
    }

    [Fact]
    public void StyleGuard_Scans_Large_Html_Without_Walking_The_Image()
    {
        var html = BuildLargeSection(out _);
        var watch = Stopwatch.StartNew();
        var issues = new CmsStyleGuard().Check(new CmsStyleGuardInput
        {
            Html = html,
            AllowPageLevelStyles = true
        });
        watch.Stop();

        watch.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(5));
        issues.ShouldNotBeNull();
    }

    [Fact]
    public void Sanitizer_Restores_A_Shielded_Image()
    {
        var html = BuildLargeSection(out var imageMarker);
        var shielded = CmsInlineDataUris.Shield(html);
        shielded.Count.ShouldBeGreaterThan(0);
        shielded.Html.ShouldNotContain(imageMarker);

        var watch = Stopwatch.StartNew();
        var clean = shielded.Restore(new HtmlSanitizerService().Sanitize(shielded.Html, HtmlSanitizerPolicies.CmsPage));
        watch.Stop();

        watch.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(5));
        clean.ShouldContain(imageMarker);
    }

    [Fact]
    public void Prepare_Rejects_Empty_And_Oversized_Html()
    {
        var importer = new CmsHtmlSectionImport(new CmsHtmlAnnotator());
        Should.Throw<BusinessException>(() => importer.Prepare("  ")).Code.ShouldBe(CMSErrorCodes.HtmlSectionEmpty);
        Should.Throw<BusinessException>(() => importer.Prepare(new string('x', CMSConsts.MaxHtmlLength + 1)))
            .Code.ShouldBe(CMSErrorCodes.HtmlSectionTooLarge);
    }

    [Fact]
    public async Task ReadAllText_Uses_Small_Reads_And_Rejects_Oversized_Streams()
    {
        var payload = Encoding.UTF8.GetBytes(new string('h', 20_000));
        var stream = new BoundedReadStream(payload);
        var text = await CmsHtmlSectionText.ReadAllTextAsync(stream);
        text.Length.ShouldBe(20_000);
        stream.MaxRequested.ShouldBeGreaterThan(0);
        stream.MaxRequested.ShouldBeLessThanOrEqualTo(CmsHtmlSectionText.ReadChunkBytes);

        var tooLarge = new MemoryStream(new byte[CMSConsts.MaxHtmlLength + 1]);
        var exception = await Should.ThrowAsync<Volo.Abp.BusinessException>(() => CmsHtmlSectionText.ReadAllTextAsync(tooLarge));
        exception.Code.ShouldBe(CMSErrorCodes.HtmlSectionTooLarge);
    }

    [Fact]
    public async Task Import_Timeout_Becomes_A_Localized_Error()
    {
        var pending = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously).Task;
        var exception = await Should.ThrowAsync<BusinessException>(() =>
            CmsHtmlSectionImport.WithImportTimeout(pending, TimeSpan.FromMilliseconds(30)));
        exception.Code.ShouldBe(CMSErrorCodes.HtmlSectionImportTimedOut);
    }

    private static string BuildLargeSection(out string imageMarker)
    {
        imageMarker = new string('A', 64);
        var image = new string('A', 360_000);
        var styleImage = new string('B', 24_000);
        return "<section class=\"hero\"><style>.hero{background-image:url(data:image/png;base64,"
            + styleImage
            + ")}</style><img alt=\"shot\" src=\"data:image/png;base64,"
            + image
            + "\"><h1>Hello</h1></section>";
    }

    private sealed class BoundedReadStream : Stream
    {
        private readonly byte[] _data;
        private int _position;

        public BoundedReadStream(byte[] data) => _data = data;

        public int MaxRequested { get; private set; }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _data.Length;
        public override long Position { get => _position; set => throw new NotSupportedException(); }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            Note(count);
            return ReadInto(buffer.AsSpan(offset, count));
        }

        public override int Read(Span<byte> buffer)
        {
            Note(buffer.Length);
            return ReadInto(buffer);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Note(buffer.Length);
            return new ValueTask<int>(ReadInto(buffer.Span));
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        private void Note(int count) => MaxRequested = Math.Max(MaxRequested, count);

        private int ReadInto(Span<byte> buffer)
        {
            var count = Math.Min(buffer.Length, _data.Length - _position);
            _data.AsSpan(_position, count).CopyTo(buffer);
            _position += count;
            return count;
        }
    }
}
