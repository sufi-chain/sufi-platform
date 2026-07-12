using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace SufiChain.SufiPlatform.Tags.Tags;

public class TagManager : DomainService
{
    private readonly ITagRepository _tagRepository;

    public TagManager(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public virtual async Task<Tag> CreateAsync(string name, string scope, string? color = null, Guid? tenantId = null)
    {
        var normalizedName = name.Trim().ToUpperInvariant();
        var existing = await _tagRepository.FindByNameAsync(scope, normalizedName, tenantId);
        if (existing != null)
        {
            throw new BusinessException(TagsErrorCodes.TagAlreadyExists)
                .WithData("Name", name)
                .WithData("Scope", scope);
        }

        return new Tag(GuidGenerator.Create(), name, scope, tenantId, color);
    }
}

