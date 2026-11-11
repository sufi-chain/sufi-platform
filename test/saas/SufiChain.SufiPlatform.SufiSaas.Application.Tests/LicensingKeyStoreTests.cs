using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using SufiChain.SufiPlatform.SufiSaas.Licensing;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiSaas;

public class LicensingKeyStoreTests
{
    [Fact]
    public void Reading_licensing_options_does_not_create_the_key_store()
    {
        var root = Directory.CreateTempSubdirectory("sufi-license-read-").FullName;
        try
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            services.AddOptions<SufiSaasLicensingOptions>()
                .Bind(configuration.GetSection(SufiSaasLicensingOptions.SectionName));

            var options = services.BuildServiceProvider()
                .GetRequiredService<IOptions<SufiSaasLicensingOptions>>()
                .Value;

            options.EnableSelfHosted.ShouldBeFalse();
            options.KeyDirectory.ShouldBe(SufiSaasLicensingKeyStore.DefaultRelativeDirectory);
            Directory.Exists(SufiSaasLicensingKeyStore.ResolveDirectory(options.KeyDirectory, root)).ShouldBeFalse();
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Relative_key_directory_is_resolved_under_the_content_root()
    {
        var resolved = SufiSaasLicensingKeyStore.ResolveDirectory("App_Data/licensing", "/app");

        resolved.ShouldBe(Path.GetFullPath(Path.Combine("/app", "App_Data/licensing")));
    }

    [Fact]
    public void Ensure_creates_keys_only_when_signing_needs_them()
    {
        var root = Directory.CreateTempSubdirectory("sufi-license-write-").FullName;
        try
        {
            var options = new SufiSaasLicensingOptions
            {
                KeyDirectory = "licensing"
            };

            SufiSaasLicensingKeyStore.Ensure(options, root);

            var directory = Path.Combine(root, "licensing");
            File.Exists(Path.Combine(directory, SufiSaasLicensingKeyStore.PrivateFileName)).ShouldBeTrue();
            File.Exists(Path.Combine(directory, SufiSaasLicensingKeyStore.PublicFileName)).ShouldBeTrue();
            options.PrivateKeyPem.ShouldNotBeNullOrWhiteSpace();
            options.PublicKeyPem.ShouldNotBeNullOrWhiteSpace();
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Configured_key_pair_skips_directory_creation()
    {
        var root = Directory.CreateTempSubdirectory("sufi-license-pem-").FullName;
        try
        {
            var options = new SufiSaasLicensingOptions
            {
                KeyDirectory = "licensing",
                PrivateKeyPem = "private",
                PublicKeyPem = "public"
            };

            SufiSaasLicensingKeyStore.Ensure(options, root);

            Directory.Exists(Path.Combine(root, "licensing")).ShouldBeFalse();
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Unwritable_key_store_fails_with_the_licensing_error()
    {
        var root = Directory.CreateTempSubdirectory("sufi-license-blocked-").FullName;
        var blocked = Path.Combine(root, "not-a-directory");
        File.WriteAllText(blocked, "blocked");
        try
        {
            var options = Options.Create(new SufiSaasLicensingOptions
            {
                KeyDirectory = blocked
            });
            var issuer = new SelfHostedLicenseIssuer(
                null!,
                options,
                new ConfigurationBuilder().Build(),
                null!,
                null!);

            var exception = Should.Throw<BusinessException>(() => _ = issuer.PublicKeyPem);

            exception.Code.ShouldBe(SufiSaasErrorCodes.LicenseKeyStoreUnavailable);
            exception.Data["KeyDirectory"].ShouldBe(Path.GetFullPath(blocked));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
