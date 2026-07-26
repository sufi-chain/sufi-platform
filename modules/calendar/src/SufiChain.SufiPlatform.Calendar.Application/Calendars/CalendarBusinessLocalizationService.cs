using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Data;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Calendar.Calendars;

/// <summary>
/// Resolves seeded calendar display-name keys stored in <see cref="Calendar.Name"/>.
/// </summary>
public class CalendarBusinessLocalizationService : ITransientDependency
{
    public const string ResourceName = "Calendar";

    protected IStringLocalizerFactory StringLocalizerFactory { get; }

    public CalendarBusinessLocalizationService(IStringLocalizerFactory stringLocalizerFactory)
    {
        StringLocalizerFactory = stringLocalizerFactory;
    }

    public virtual string ResolveDisplayName(string? storedName)
    {
        if (string.IsNullOrWhiteSpace(storedName))
        {
            return string.Empty;
        }

        return BusinessLocalizationHelper.ResolveText(
            StringLocalizerFactory,
            ResourceName,
            storedName,
            storedName);
    }
}
