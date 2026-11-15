using System.Collections.Concurrent;
using NSubstitute;
using SufiChain.SufiPlatform.SufiSaas.Licensing;

namespace SufiChain.SufiPlatform.SufiSaas;

internal static class IssuedLicenseRepositoryDouble
{
    public static IIssuedLicenseRepository Create(ConcurrentDictionary<string, IssuedLicense>? store = null)
    {
        store ??= new ConcurrentDictionary<string, IssuedLicense>(StringComparer.Ordinal);
        var repo = Substitute.For<IIssuedLicenseRepository>();
        repo.FindByLicenseIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call => store.TryGetValue(call.Arg<string>(), out var entity) ? entity : null);
        repo.FindByTenantRequestIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(call => store.Values.FirstOrDefault(x => x.TenantRequestId == call.Arg<Guid>()));
        repo.InsertAsync(Arg.Any<IssuedLicense>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var entity = call.Arg<IssuedLicense>();
                store[entity.LicenseId] = entity;
                return entity;
            });
        repo.UpdateAsync(Arg.Any<IssuedLicense>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var entity = call.Arg<IssuedLicense>();
                store[entity.LicenseId] = entity;
                return entity;
            });
        return repo;
    }
}
