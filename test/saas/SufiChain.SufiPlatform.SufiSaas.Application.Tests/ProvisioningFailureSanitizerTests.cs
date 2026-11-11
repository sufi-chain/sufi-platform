using Shouldly;
using SufiChain.SufiPlatform.SufiSaas.Provisioning;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiSaas;

public class ProvisioningFailureSanitizerTests
{
    [Fact]
    public void Business_exception_keeps_its_code()
    {
        var sanitized = ProvisioningFailureSanitizer.Sanitize(new BusinessException("SufiSaas:BillingUnavailable"));

        sanitized.ShouldBe("SufiSaas:BillingUnavailable");
    }

    [Fact]
    public void Connection_string_secrets_are_removed_and_the_text_is_capped()
    {
        var message = "Login failed for Server=127.0.0.1;Password=Sup3rSecret;User ID=sa; " + new string('x', 3000);
        var sanitized = ProvisioningFailureSanitizer.Sanitize(new InvalidOperationException(message));

        sanitized.ShouldNotContain("Sup3rSecret");
        sanitized.ShouldNotContain("User ID=sa");
        sanitized.ShouldContain("Password=***");
        sanitized.Length.ShouldBeLessThanOrEqualTo(SufiSaasConsts.MaxProvisioningErrorLength);
    }
}
