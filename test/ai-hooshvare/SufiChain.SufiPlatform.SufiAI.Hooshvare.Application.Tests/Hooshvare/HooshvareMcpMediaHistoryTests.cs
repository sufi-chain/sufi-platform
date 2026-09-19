using Microsoft.Extensions.Logging.Abstractions;

using Microsoft.Extensions.Options;

using Microsoft.SemanticKernel;

using Microsoft.SemanticKernel.ChatCompletion;

using Shouldly;

using Xunit;



namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;



public class HooshvareMcpMediaHistoryTests

{

    [Theory]

    [InlineData(false)]

    [InlineData(true)]

    public void Mcp_history_keeps_images_and_includes_text_once(bool explicitTextPart)

    {

        var message = new SufiAIChatMessage { Role = "user", Content = "Use this layout" };

        if (explicitTextPart)

            message.ContentParts.Add(new SufiAIChatContentPart { Type = "text", Text = message.Content });

        message.ContentParts.Add(new SufiAIChatContentPart { Type = "image", DataUrl = "data:image/png;base64,AQID" });



        var history = new Runtime().History(new SufiAIChatRequest { Messages = [message] });



        history.Count.ShouldBe(1);

        history[0].Items.OfType<TextContent>().Single().Text.ShouldBe("Use this layout");

        history[0].Items.OfType<Microsoft.SemanticKernel.ImageContent>().Count().ShouldBe(1);

    }



    private sealed class Runtime() : HooshvareRuntimeAppService(

        null!, null!, null!, null!, null!, null!, null!, null!, null!, null!, null!, null!,

        Options.Create(new HooshvareContextOptions()), null!, null!, null!, null!,

        NullLogger<HooshvareRuntimeAppService>.Instance)

    {

        public ChatHistory History(SufiAIChatRequest request) => BuildMcpChatHistory(request);

    }

}

