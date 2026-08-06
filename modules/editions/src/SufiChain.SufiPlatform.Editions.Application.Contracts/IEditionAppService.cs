using System;
using SufiChain.SufiPlatform.Application.Services;

namespace SufiChain.SufiPlatform.Editions;

public interface IEditionAppService :
    ISufiCrudAppService<EditionDto, Guid, GetEditionsInput, EditionCreateDto, EditionUpdateDto>
{
}
