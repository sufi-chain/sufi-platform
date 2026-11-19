using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Editions;

/// <summary>
/// Lets the edition editor save a commercial price without the Editions module referencing
/// the module that owns plans (SufiSaas).
/// </summary>
public interface IEditionPriceSaver : ITransientDependency
{
    Task SaveAsync(EditionPriceSaveInput input);
}

public class EditionPriceSaveInput
{
    public Guid EditionId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>Stored edition display name, which may be a localization key.</summary>
    public string DisplayName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;
}
