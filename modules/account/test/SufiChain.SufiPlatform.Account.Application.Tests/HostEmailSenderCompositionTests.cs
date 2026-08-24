using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class HostEmailSenderCompositionTests
{
    [Theory]
    [InlineData("hosts/SufiChane.SufiPlatform/src/SufiChane.SufiPlatform.Domain/SufiPlatformDomainModule.cs")]
    [InlineData("hosts/SufiChane.SufiPlatform/src/SufiChane.SufiPlatform.Blazor.WebApp/SufiPlatformModule.cs")]
    [InlineData("sufi-platform/templates/app/aspnet-core/src/MyCompanyName.MyProjectName.Blazor.WebApp/DemoAppModule.cs")]
    public void Host_Must_Not_Replace_IEmailSender_With_NullEmailSender(string relativePath)
    {
        var source = File.ReadAllText(FindRepoFile(relativePath));

        source.ShouldNotContain("Replace(ServiceDescriptor.Singleton<IEmailSender, NullEmailSender>())");
        source.ShouldNotContain("Replace(ServiceDescriptor.Transient<IEmailSender, NullEmailSender>())");
    }

    private static string FindRepoFile(string relativePath)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory != null;
             directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException($"Repository file was not found: {relativePath}");
    }
}
