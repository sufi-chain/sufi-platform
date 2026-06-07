using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.Identity;
using Volo.Abp;
using Volo.Abp.Users;

namespace SufiChain.Chat.Contacts;

public class IdentityChatContactProvider : IChatContactProvider
{
    protected IIdentityUserRepository UserRepository { get; }
    protected ICurrentUser CurrentUser { get; }

    public IdentityChatContactProvider(
        IIdentityUserRepository userRepository,
        ICurrentUser currentUser)
    {
        UserRepository = userRepository;
        CurrentUser = currentUser;
    }

    public virtual async Task<PagedResultDto<ChatContactDto>> SearchAsync(SearchChatContactsInput input)
    {
        var filter = input.Filter?.Trim();
        if (filter.IsNullOrWhiteSpace() || filter.Length < ChatContactSearchConsts.MinFilterLength)
        {
            return new PagedResultDto<ChatContactDto>(0, []);
        }

        var users = await UserRepository.GetListAsync(
            sorting: input.Sorting,
            maxResultCount: input.MaxResultCount,
            skipCount: input.SkipCount,
            filter: filter,
            includeDetails: false);

        var contacts = users
            .Where(user => user.IsActive && user.Id != CurrentUser.Id)
            .Select(MapToContact)
            .ToList();

        return new PagedResultDto<ChatContactDto>(contacts.Count, contacts);
    }

    protected virtual ChatContactDto MapToContact(IdentityUser user)
    {
        var displayName = $"{user.Name} {user.Surname}".Trim();
        if (displayName.IsNullOrWhiteSpace())
        {
            displayName = user.UserName;
        }

        return new ChatContactDto
        {
            Id = user.Id,
            TenantId = user.TenantId,
            UserName = user.UserName,
            Name = user.Name,
            Surname = user.Surname,
            Email = user.Email,
            DisplayName = displayName,
            IsOnline = false
        };
    }
}
