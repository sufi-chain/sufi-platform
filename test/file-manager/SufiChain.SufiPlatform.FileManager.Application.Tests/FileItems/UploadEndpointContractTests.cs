using System;
using System.IO;
using System.Reflection;
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

/// <summary>
/// sufi-chain#479: a small .txt must upload without an HTTP 400. The two suspects the issue named - an
/// antiforgery token the Blazor XHR never sends, and a multipart binding the endpoint might reject - are
/// pinned here against the real controller method and its attributes, so a regression fails a test instead
/// of stage QA. This complements the structure/MIME tests in
/// <see cref="FileUploadValidationSecurityTests"/> (the General structure's allow-list) and the row/dialog
/// tests in <c>FileUploadFollowUpTests</c>.
/// </summary>
public class UploadEndpointContractTests
{
    [Fact]
    public void Upload_endpoints_do_not_require_an_antiforgery_token()
    {
        // The uploader sends an XHR with a bearer token and no antiforgery header; dropping
        // [IgnoreAntiforgeryToken] is exactly what would turn every upload into a 400.
        foreach (var name in UploadActions())
        {
            Method(name).GetCustomAttributes(typeof(IgnoreAntiforgeryTokenAttribute), inherit: true)
                .ShouldNotBeEmpty($"{name} must ignore the antiforgery token for XHR uploads");
        }
    }

    [Fact]
    public void Upload_endpoints_accept_multipart_form_data()
    {
        foreach (var name in UploadActions())
        {
            Method(name).GetCustomAttributes(typeof(ConsumesAttribute), inherit: true)
                .ShouldNotBeEmpty($"{name} must consume multipart/form-data");
        }
    }

    [Fact]
    public async Task A_small_txt_through_the_upload_endpoint_returns_200_and_the_stored_item()
    {
        var appService = Substitute.For<IFileItemAppService>();
        appService.ValidateUploadAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<long>())
            .Returns(new UploadValidationResult { IsValid = true });

        var stored = new FileItemDto
        {
            Id = Guid.NewGuid(),
            OriginalName = "notes.txt",
            Name = "notes",
            MimeType = "text/plain"
        };
        appService.UploadAsync(Arg.Any<UploadFileInput>()).Returns(stored);

        var controller = CreateController(appService);
        var request = new UploadFileFormRequest
        {
            File = TextFile("notes.txt", "text/plain", "hello"),
            StructureKey = "General"
        };

        var result = await controller.UploadFromFormAsync(request);

        // A 400 here was the #479 symptom; the endpoint must hand back the stored item instead.
        var json = result.ShouldBeOfType<JsonResult>();
        json.Value.ShouldBeSameAs(stored);
        await appService.Received(1).UploadAsync(Arg.Is<UploadFileInput>(input =>
            input.FileName == "notes.txt" &&
            input.MimeType == "text/plain" &&
            input.StructureKey == "General"));
    }

    private static string[] UploadActions() =>
    [
        nameof(FileItemController.UploadFromFormAsync),
        nameof(FileItemController.UploadStreamAsync),
        nameof(FileItemController.UploadMultipleFromFormAsync)
    ];

    private static MethodInfo Method(string name)
    {
        var method = typeof(FileItemController).GetMethod(name, BindingFlags.Public | BindingFlags.Instance);
        method.ShouldNotBeNull($"{name} should exist on {nameof(FileItemController)}");
        return method!;
    }

    private static FileItemController CreateController(IFileItemAppService appService)
    {
        var controller = new FileItemController(
            appService,
            Options.Create(new FileManagerOptions()),
            NullLogger<FileItemController>.Instance);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        return controller;
    }

    private static FormFile TextFile(string fileName, string contentType, string content)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);

        // The five-argument FormFile constructor leaves Headers null, so assigning ContentType throws a
        // NullReferenceException. Pass headers explicitly (the six-argument overload) so ContentType is set.
        var headers = new HeaderDictionary
        {
            ["Content-Disposition"] = $"form-data; name=\"File\"; filename=\"{fileName}\"",
            ["Content-Type"] = contentType
        };

        return new FormFile(stream, 0, bytes.Length, "File", fileName, headers);
    }
}
