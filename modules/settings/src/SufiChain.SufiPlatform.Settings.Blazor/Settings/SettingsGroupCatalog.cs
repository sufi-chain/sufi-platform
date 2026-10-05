using System.Diagnostics;

namespace SufiChain.SufiPlatform.Settings.Blazor.Settings;

/// <summary>
/// Builds the settings sections a caller is allowed to see.
/// A contributor that denies access, or whose permission check throws, adds no section.
/// </summary>
public static class SettingsGroupCatalog
{
    public static async Task<IReadOnlyList<SettingComponentGroup>> LoadAsync(
        IEnumerable<ISettingComponentContributor> contributors,
        IServiceProvider serviceProvider)
    {
        var context = new SettingComponentCreationContext(serviceProvider);
        foreach (var contributor in contributors)
        {
            var before = context.Groups.Count;
            try
            {
                if (!await contributor.CheckPermissionsAsync(context))
                {
                    continue;
                }

                await contributor.ConfigureAsync(context);
            }
            catch (Exception exception)
            {
                if (context.Groups.Count > before)
                {
                    context.Groups.RemoveRange(before, context.Groups.Count - before);
                }

                Trace.TraceWarning(
                    "Settings group {0} was hidden because its permission check failed: {1}",
                    contributor.GetType().Name,
                    exception.Message);
            }
        }

        context.Normalize();
        return context.Groups.ToList();
    }
}
