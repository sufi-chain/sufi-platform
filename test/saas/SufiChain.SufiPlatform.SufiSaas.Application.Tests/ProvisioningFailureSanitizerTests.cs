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

    [Fact]
    public void Missing_admin_connection_string_is_not_retried_and_names_the_setting()
    {
        var exception = Should.Throw<BusinessException>(() =>
            TenantDatabaseProvisioningConfiguration.EnsureConfigured(null, "Server=.;Database={databaseName}"));

        ProvisioningFailureSanitizer.IsNonRetryable(exception).ShouldBeTrue();
        var sanitized = ProvisioningFailureSanitizer.Sanitize(exception);
        sanitized.ShouldBe(
            "SufiSaas:TenantDatabaseProvisioningUnavailable: SufiSaas:TenantDatabases:AdminConnectionString is required.");
    }

    [Fact]
    public void Missing_connection_string_template_is_not_retried()
    {
        var exception = Should.Throw<BusinessException>(() =>
            TenantDatabaseProvisioningConfiguration.EnsureConfigured("Server=.;", " "));

        ProvisioningFailureSanitizer.IsNonRetryable(exception).ShouldBeTrue();
        ProvisioningFailureSanitizer.Sanitize(exception).ShouldContain(
            "SufiSaas:TenantDatabases:ApplicationConnectionStringTemplate is required.");
    }

    [Fact]
    public void A_runtime_provisioning_failure_stays_retryable()
    {
        var exception = new InvalidOperationException("SQL timeout");

        ProvisioningFailureSanitizer.IsNonRetryable(exception).ShouldBeFalse();
    }

    [Fact]
    public void Blank_admin_connection_string_is_the_startup_warning_condition()
    {
        TenantDatabaseProvisioningConfiguration.IsAdminConnectionStringMissing(null).ShouldBeTrue();
        TenantDatabaseProvisioningConfiguration.IsAdminConnectionStringMissing("  ").ShouldBeTrue();
        TenantDatabaseProvisioningConfiguration.IsAdminConnectionStringMissing("Server=.;").ShouldBeFalse();
    }
}
