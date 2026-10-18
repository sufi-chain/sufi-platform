using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.Tenants;

// sufi-chain#441: duplicate tenant name must be a coded, localized business error.
public class TenantErrorLocalizationTests
{
    [Fact]
    public async Task Duplicate_name_throws_coded_business_exception()
    {
        var existing = new Tenant(Guid.NewGuid(), "Demo", "DEMO");
        var repository = Substitute.For<ITenantRepository>();
        repository.FindByNameAsync("DEMO").Returns(existing);
        var validator = new SufiTenantValidator(repository);

        var ex = await Should.ThrowAsync<BusinessException>(() => validator.ValidateAsync(new Tenant(Guid.NewGuid(), "Demo", "DEMO")));

        ex.Code.ShouldBe(TenantsErrorCodes.DuplicateTenantName);
        ex.Data["Name"].ShouldBe("Demo");
    }

    [Fact]
    public async Task Same_tenant_keeps_its_name()
    {
        var id = Guid.NewGuid();
        var repository = Substitute.For<ITenantRepository>();
        repository.FindByNameAsync("DEMO").Returns(new Tenant(id, "Demo", "DEMO"));

        await new SufiTenantValidator(repository).ValidateAsync(new Tenant(id, "Demo", "DEMO"));
    }

    [Theory]
    [InlineData("en")]
    [InlineData("fa")]
    [InlineData("ar")]
    [InlineData("es")]
    public void Every_error_code_is_localized(string culture)
    {
        var assembly = typeof(TenantsErrorCodes).Assembly;
        var resource = assembly.GetManifestResourceNames()
            .Single(n => n.EndsWith("Localization.Tenants." + culture + ".json", StringComparison.OrdinalIgnoreCase));
        using var stream = assembly.GetManifestResourceStream(resource)!;
        using var doc = JsonDocument.Parse(stream, new JsonDocumentOptions { AllowTrailingCommas = true });
        var texts = doc.RootElement.GetProperty("texts");

        var codes = typeof(TenantsErrorCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.Name != nameof(TenantsErrorCodes.Namespace))
            .Select(f => (string)f.GetValue(null)!);

        foreach (var code in codes)
        {
            texts.TryGetProperty(code, out var text).ShouldBeTrue($"{culture} misses {code}");
            text.GetString().ShouldNotBeNullOrWhiteSpace();
        }
    }
}
