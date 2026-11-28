namespace SufiChain.SufiPlatform.Account;

/// <summary>
/// The profile form shows the value just saved. A later fetch must not replace it
/// with the previous name.
/// </summary>
public static class ProfileSaveBinding
{
    public static ProfileDto Shown(ProfileDto? saved, ProfileDto? fetched) =>
        saved ?? fetched ?? new ProfileDto();
}
