using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Chat.Controllers;
using SufiChain.SufiPlatform.SufiCom.Chat.Permissions;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Controllers;

public class ChatComposerUploadAuthorization_Tests
{
    [Fact]
    public void Http_upload_still_requires_the_send_permission()
    {
        var method = typeof(ChatComposerUploadController).GetMethod(nameof(ChatComposerUploadController.UploadAsync));
        var authorize = method!
            .GetCustomAttributes<AuthorizeAttribute>(inherit: true)
            .Single();

        authorize.Policy.ShouldBe(ChatPermissions.Messages.Send);
    }
}
