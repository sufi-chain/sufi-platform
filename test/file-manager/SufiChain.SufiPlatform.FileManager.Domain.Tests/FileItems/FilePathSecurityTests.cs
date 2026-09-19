using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager.FileItems;

public class FilePathSecurityTests
{
    [Theory]
    [InlineData("photo.png")]
    [InlineData("report.PDF")]
    [InlineData("my-file 1.jpeg")]
    public void Safe_Upload_Names_Should_Pass(string fileName)
    {
        FilePathSecurity.IsSafeUploadFileName(fileName).ShouldBeTrue();
        FilePathSecurity.IsBlockedExtension(fileName).ShouldBeFalse();
    }

    [Theory]
    [InlineData("../etc/passwd")]
    [InlineData("..\\windows\\system32\\config")]
    [InlineData("/etc/passwd")]
    [InlineData("C:\\Windows\\win.ini")]
    [InlineData("folder/photo.png")]
    [InlineData("..")]
    [InlineData(".")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("photo.png/")]
    [InlineData("photo\0.png")]
    public void Traversal_Absolute_And_Nested_Names_Should_Fail(string fileName)
    {
        FilePathSecurity.IsSafeUploadFileName(fileName).ShouldBeFalse();
    }

    [Theory]
    [InlineData("payload.exe")]
    [InlineData("payload.bat")]
    [InlineData("payload.cmd")]
    [InlineData("payload.ps1")]
    [InlineData("payload.js")]
    [InlineData("payload.php")]
    [InlineData("payload.aspx")]
    [InlineData("payload.html")]
    [InlineData("payload.htm")]
    [InlineData("payload.svg")]
    [InlineData("shell.jsp")]
    [InlineData("drop.sh")]
    public void Executable_And_Script_Extensions_Should_Be_Blocked(string fileName)
    {
        FilePathSecurity.IsBlockedExtension(fileName).ShouldBeTrue();
    }

    [Theory]
    [InlineData("photo.png.exe")]
    [InlineData("invoice.pdf.php")]
    [InlineData("avatar.jpg.js")]
    public void Double_Extension_Should_Use_The_Final_Extension(string fileName)
    {
        FilePathSecurity.IsSafeUploadFileName(fileName).ShouldBeTrue();
        FilePathSecurity.IsBlockedExtension(fileName).ShouldBeTrue();
    }

    [Fact]
    public void SanitizeFolderName_Should_Strip_Traversal_And_Invalid_Chars()
    {
        FilePathSecurity.SanitizeFolderName("..").ShouldBeEmpty();
        FilePathSecurity.SanitizeFolderName(".").ShouldBeEmpty();
        FilePathSecurity.SanitizeFolderName("../secret").ShouldBeEmpty();
        FilePathSecurity.SanitizeFolderName("My Folder").ShouldBe("my-folder");
        FilePathSecurity.SanitizeFolderName("a/b").ShouldBe("ab");
        FilePathSecurity.SanitizeFolderName("ok-name").ShouldBe("ok-name");
    }
}
