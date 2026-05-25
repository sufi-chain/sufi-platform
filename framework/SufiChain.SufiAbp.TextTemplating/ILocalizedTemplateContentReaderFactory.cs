using System.Threading.Tasks;

namespace SufiChain.SufiAbp.TextTemplating;

public interface ILocalizedTemplateContentReaderFactory
{
    Task<ILocalizedTemplateContentReader> CreateAsync(TemplateDefinition templateDefinition);
}
