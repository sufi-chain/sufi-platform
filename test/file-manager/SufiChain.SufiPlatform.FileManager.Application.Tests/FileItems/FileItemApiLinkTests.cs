using System;
using Shouldly;
using SufiChain.SufiPlatform.FileManager.FileItems;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager.Application.Tests.FileItems;

public class FileItemApiLinkTests
{
    private static readonly Guid DemoTenantId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    [Fact]
    public void Download_Thumbnail_And_Stream_Ignore_Absolute_Console_Base()
    {
        const string console = "https://stage.console.example";
        var fileId = Guid.Parse("11111111-2222-3333-4444-555555555555");

        foreach (var kind in new[] { "download", "thumbnail", "stream" })
        {
            var url = WithTenant(FileItemApiLink.Build(console, $"/api/file-manager/file-items/{fileId}/{kind}"));
            url.ShouldStartWith("/api/file-manager/");
            url.ShouldContain(kind);
            url.ShouldContain($"__tenant={DemoTenantId:N}");
            url.ShouldNotContain("stage.console");
            url.ShouldNotContain("://");
        }
    }

    [Fact]
    public void Unset_Base_Stays_Relative_And_Keeps_Tenant()
    {
        var url = WithTenant(FileItemApiLink.Build(null, "/api/file-manager/file-items/abc/download"));
        url.ShouldBe($"/api/file-manager/file-items/abc/download?__tenant={DemoTenantId:N}");
    }

    [Fact]
    public void Relative_Base_Is_Kept()
    {
        var url = FileItemApiLink.Build("/files/", "/api/file-manager/file-items/abc/download");
        url.ShouldBe("/files/api/file-manager/file-items/abc/download");
    }

    private static string WithTenant(string url)
    {
        return url + (url.Contains('?') ? "&" : "?") + $"__tenant={DemoTenantId:N}";
    }
}
