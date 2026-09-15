using System.Threading.Tasks;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Communication;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Application.Tests.Communication;

public class SufiComServiceTests : SufiComTestBase<SufiComApplicationTestModule>
{
    private readonly ISufiComService _sufiComService;

    public SufiComServiceTests()
    {
        _sufiComService = GetRequiredService<ISufiComService>();
    }

    [Fact]
    public async Task Should_Throw_When_No_Provider_Configured()
    {
        var message = new SmsMessage
        {
            Phone = "+989121234567",
            Content = "Hello"
        };

        var exception = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _sufiComService.SendSmsAsync(message);
        });

        exception.Code.ShouldBe(SufiComDomainErrorCodes.NoProviderConfigured);
    }
}
