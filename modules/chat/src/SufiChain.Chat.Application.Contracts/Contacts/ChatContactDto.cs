using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.Chat.Contacts;

public class ChatContactDto : EntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public string? UserName { get; set; }

    public string? Name { get; set; }

    public string? Surname { get; set; }

    public string? Email { get; set; }

    public string? DisplayName { get; set; }

    public bool IsOnline { get; set; }
}
