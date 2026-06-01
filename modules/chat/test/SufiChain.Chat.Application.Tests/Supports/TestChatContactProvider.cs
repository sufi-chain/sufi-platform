using SufiChain.Chat.Contacts;
using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace SufiChain.Chat.Supports;

public class TestChatContactProvider : IChatContactProvider, ISingletonDependency
{
    private readonly ICurrentTenant _currentTenant;
    private readonly Dictionary<Guid, List<ChatContactDto>> _contactsByTenant = new();

    public TestChatContactProvider(ICurrentTenant currentTenant)
    {
        _currentTenant = currentTenant;
    }

    public void SeedContact(Guid tenantId, ChatContactDto contact)
    {
        if (!_contactsByTenant.TryGetValue(tenantId, out var contacts))
        {
            contacts = new List<ChatContactDto>();
            _contactsByTenant[tenantId] = contacts;
        }

        contacts.Add(contact);
    }

    public Task<PagedResultDto<ChatContactDto>> SearchAsync(SearchChatContactsInput input)
    {
        var tenantId = _currentTenant.Id ?? Guid.Empty;
        var contacts = _contactsByTenant.GetValueOrDefault(tenantId) ?? new List<ChatContactDto>();

        var filtered = contacts
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), contact =>
                contact.DisplayName.Contains(input.Filter!, StringComparison.OrdinalIgnoreCase))
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return Task.FromResult(new PagedResultDto<ChatContactDto>(contacts.Count, filtered));
    }
}
