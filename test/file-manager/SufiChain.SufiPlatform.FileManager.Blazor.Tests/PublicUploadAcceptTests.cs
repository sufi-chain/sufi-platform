using Shouldly;
using SufiChain.SufiPlatform.FileManager.Blazor.Public.Components.Upload;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Tests;

public class PublicUploadAcceptTests
{
    private const string LogoAccept = "image/png,image/jpeg,image/webp,image/svg+xml,.png,.jpg,.jpeg,.webp,.svg";

    [Fact]
    public void A_logo_accept_list_keeps_images_and_rejects_text()
    {
        PublicUploadAccept.IsAllowed(LogoAccept, "logo.png", "image/png").ShouldBeTrue();
        PublicUploadAccept.IsAllowed(LogoAccept, "logo.jpg", "image/jpeg").ShouldBeTrue();
        PublicUploadAccept.IsAllowed(LogoAccept, "logo.webp", "image/webp").ShouldBeTrue();
        PublicUploadAccept.IsAllowed(LogoAccept, "logo.svg", "image/svg+xml").ShouldBeTrue();
        PublicUploadAccept.IsAllowed(LogoAccept, "notes.txt", "text/plain").ShouldBeFalse();
    }

    [Fact]
    public void The_default_uploader_accept_still_allows_a_text_file()
    {
        const string generalAccept = "image/*,application/pdf,video/*,.docx,.html,.htm,.txt,.md";
        PublicUploadAccept.IsAllowed(generalAccept, "notes.txt", "text/plain").ShouldBeTrue();
    }
}
