using SufiChain.SufiAbp.Users;

namespace SufiChain.SufiAbp.Users.Blazor.Public.Components;

public abstract class SufiUserLookupInlineComponentBase : UsersPublicComponentBase
{
    protected static class LoadingKeys
    {
        public const string LoadUsers = "load-users";
    }

    protected const int DefaultMaxResultCount = 20;

    protected IUserLookupAppService UserLookupAppService => LazyGetRequiredService(ref _userLookupAppService);
    private IUserLookupAppService? _userLookupAppService;

    protected static string FormatUserLabel(UserLookupDto user)
    {
        return $"{user.DisplayName} ({user.UserName})";
    }

    protected static void MergeUsers(List<UserLookupDto> target, IEnumerable<UserLookupDto> users)
    {
        foreach (var user in users)
        {
            if (target.All(existing => existing.Id != user.Id))
            {
                target.Add(user);
            }
        }
    }

    protected virtual async Task<List<UserLookupDto>> SearchUsersAsync(string? filter, int maxResultCount = DefaultMaxResultCount)
    {
        var result = await UserLookupAppService.SearchAsync(new UserLookupSearchInput
        {
            Filter = filter,
            SkipCount = 0,
            MaxResultCount = maxResultCount
        });

        return result.Items.ToList();
    }

    protected virtual async Task<UserLookupDto?> TryGetUserAsync(Guid userId)
    {
        try
        {
            return await UserLookupAppService.GetAsync(userId);
        }
        catch
        {
            return null;
        }
    }
}
