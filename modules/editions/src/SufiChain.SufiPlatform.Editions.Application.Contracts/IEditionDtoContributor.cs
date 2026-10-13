using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Editions;

/// <summary>
/// Lets other modules enrich <see cref="EditionDto"/> without the Editions module
/// taking a project reference on them (same extension spirit as
/// <see cref="SufiEditionsOptions.DisplayNameResources"/>).
/// </summary>
public interface IEditionDtoContributor : ITransientDependency
{
    Task ContributeAsync(IReadOnlyList<EditionDto> editions);
}
