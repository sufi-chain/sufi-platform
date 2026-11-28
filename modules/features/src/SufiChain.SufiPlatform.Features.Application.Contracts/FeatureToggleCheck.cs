namespace SufiChain.SufiPlatform.Features;

public enum FeatureToggleCheckState
{
    Off = 0,
    Indeterminate = 1,
    On = 2
}

public static class FeatureToggleCheck
{
    public static FeatureToggleCheckState FromCounts(int enabled, int total)
    {
        if (total <= 0 || enabled <= 0)
        {
            return FeatureToggleCheckState.Off;
        }

        return enabled >= total
            ? FeatureToggleCheckState.On
            : FeatureToggleCheckState.Indeterminate;
    }
}
