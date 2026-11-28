using Volo.Abp;

namespace SufiChain.SufiPlatform.Tags.Tags;

public static class TagPickerLimit
{
    public static int? Resolve(int? explicitMax, int? policyMax) => explicitMax ?? policyMax;

    public static bool BlocksAnother(int selectedCount, int? max) =>
        max is int limit && selectedCount >= limit;

    public static void EnsureRoom(int existingCount, int max)
    {
        if (existingCount >= max)
        {
            throw new BusinessException(TagsErrorCodes.MaxTagsPerEntityExceeded)
                .WithData("MaxTagsPerEntity", max);
        }
    }
}
