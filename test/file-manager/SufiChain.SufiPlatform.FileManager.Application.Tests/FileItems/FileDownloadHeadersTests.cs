using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.FileManager.Configuration;
using SufiChain.SufiPlatform.FileManager.Controllers;
using SufiChain.SufiPlatform.FileManager.FileItems;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager.Application.Tests.FileItems;

public class FileDownloadHeadersTests
{
    [Fact]
    public void Null_content_type_is_rejected_by_the_file_result()
    {
        var exception = Should.Throw<FormatException>(() => new FileContentResult(new byte[] { 1 }, (string)null!));
        exception.Message.ShouldContain("invalid values");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("image/png\r\nX-Evil: 1")]
    public void Missing_or_unsafe_content_type_falls_back_to_octet_stream(string? mimeType)
    {
        FileDownloadHeaders.NormalizeContentType(mimeType).ShouldBe(FileDownloadHeaders.OctetStream);
    }

    [Fact]
    public void Known_content_type_is_preserved()
    {
        FileDownloadHeaders.NormalizeContentType(" image/webp ").ShouldBe("image/webp");
    }

    [Fact]
    public void Persian_file_name_uses_an_ascii_filename_and_rfc5987_filename_star()
    {
        const string fileName = "گزارش مالی.pdf";
        var header = FileDownloadHeaders.Attachment(fileName);

        foreach (var ch in header)
        {
            (ch <= 127).ShouldBeTrue();
        }

        header.ShouldStartWith("attachment; filename=\"");
        header.ShouldContain("filename*=UTF-8''");
        header.ShouldNotContain("\r");
        header.ShouldNotContain("\n");
        ReadFileNameStar(header).ShouldBe(fileName);

        var http = new DefaultHttpContext();
        http.Response.Headers.ContentDisposition = header;
        http.Response.Headers.ContentDisposition.ToString().ShouldBe(header);
    }

    [Fact]
    public void Quoted_and_newline_file_names_cannot_break_the_header()
    {
        var header = FileDownloadHeaders.Attachment("say \"hi\"\r\n.pdf");

        foreach (var ch in header)
        {
            (ch <= 127).ShouldBeTrue();
        }

        header.ShouldNotContain("\r");
        header.ShouldNotContain("\n");
        var fallback = header.Split("filename=\"", StringSplitOptions.None)[1].Split('"', 2)[0];
        fallback.ShouldNotContain("\"");
        ReadFileNameStar(header).ShouldBe("say \"hi\".pdf");
    }

    [Fact]
    public async Task Download_returns_404_for_a_missing_item_and_403_for_a_forbidden_item()
    {
        var missing = CreateController();
        missing.AppService.GetDownloadContentAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Returns(new FileContentResultDto());

        var missingResult = await missing.Controller.DownloadAsync(Guid.NewGuid());
        missingResult.ShouldBeOfType<NotFoundResult>();

        var forbidden = CreateController();
        forbidden.AppService.GetDownloadContentAsync(Arg.Any<Guid>(), Arg.Any<string?>())
            .Returns(new FileContentResultDto { IsForbidden = true, Content = new FileContentDto { Content = new byte[] { 1 } } });

        var forbiddenResult = await forbidden.Controller.DownloadAsync(Guid.NewGuid());
        forbiddenResult.ShouldBeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Download_uses_octet_stream_and_a_safe_disposition_for_a_persian_name()
    {
        var harness = CreateController();
        var id = Guid.NewGuid();
        harness.AppService.GetDownloadContentAsync(id, null)
            .Returns(new FileContentResultDto
            {
                Content = new FileContentDto
                {
                    Content = new byte[] { 4, 5 },
                    MimeType = " ",
                    FileName = "گزارش.pdf"
                }
            });

        var result = await harness.Controller.DownloadAsync(id);

        var file = result.ShouldBeOfType<FileContentResult>();
        file.ContentType.ShouldBe(FileDownloadHeaders.OctetStream);
        file.FileDownloadName.ShouldBeNullOrWhiteSpace();
        file.FileContents.ShouldBe(new byte[] { 4, 5 });
        var header = harness.HttpContext.Response.Headers.ContentDisposition.ToString();
        ReadFileNameStar(header).ShouldBe("گزارش.pdf");
    }

    [Fact]
    public async Task Stream_and_thumbnail_fall_back_when_the_content_type_is_missing()
    {
        var harness = CreateController();
        var id = Guid.NewGuid();
        harness.AppService.GetStreamContentAsync(id, null)
            .Returns(new StreamContentResultDto
            {
                Content = new StreamContentDto
                {
                    Stream = new MemoryStream(new byte[] { 1 }),
                    MimeType = ""
                }
            });
        harness.AppService.GetThumbnailContentAsync(id, null)
            .Returns(new FileContentResultDto
            {
                Content = new FileContentDto
                {
                    Content = new byte[] { 2 },
                    MimeType = null!
                }
            });

        var stream = await harness.Controller.StreamAsync(id);
        var thumbnail = await harness.Controller.GetThumbnailAsync(id);

        stream.ShouldBeOfType<FileStreamResult>().ContentType.ShouldBe(FileDownloadHeaders.OctetStream);
        thumbnail.ShouldBeOfType<FileContentResult>().ContentType.ShouldBe(FileDownloadHeaders.OctetStream);
    }

    private static string ReadFileNameStar(string header)
    {
        const string marker = "filename*=UTF-8''";
        var index = header.IndexOf(marker, StringComparison.Ordinal);
        index.ShouldBeGreaterThanOrEqualTo(0);
        return Uri.UnescapeDataString(header[(index + marker.Length)..]);
    }

    private static ControllerHarness CreateController()
    {
        var appService = Substitute.For<IFileItemAppService>();
        var controller = new FileItemController(
            appService,
            Options.Create(new FileManagerOptions()),
            NullLogger<FileItemController>.Instance);
        var http = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = http };
        return new ControllerHarness(controller, appService, http);
    }

    private sealed class ControllerHarness
    {
        public ControllerHarness(FileItemController controller, IFileItemAppService appService, DefaultHttpContext httpContext)
        {
            Controller = controller;
            AppService = appService;
            HttpContext = httpContext;
        }

        public FileItemController Controller { get; }
        public IFileItemAppService AppService { get; }
        public DefaultHttpContext HttpContext { get; }
    }
}
