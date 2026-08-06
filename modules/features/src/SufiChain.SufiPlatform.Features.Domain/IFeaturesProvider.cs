using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SufiChain.SufiPlatform.Features;

namespace SufiChain.SufiPlatform.Features;

public interface IFeaturesProvider
{
    string Name { get; }

    //TODO: Other better method name.
    bool Compatible(string providerName);

    //TODO: Other better method name.
    Task<IAsyncDisposable> HandleContextAsync(string providerName, string providerKey);

    Task<string> GetOrNullAsync([NotNull] FeatureDefinition feature, [CanBeNull] string providerKey);

    Task SetAsync([NotNull] FeatureDefinition feature, [NotNull] string value, [CanBeNull] string providerKey);

    Task ClearAsync([NotNull] FeatureDefinition feature, [CanBeNull] string providerKey);
}
