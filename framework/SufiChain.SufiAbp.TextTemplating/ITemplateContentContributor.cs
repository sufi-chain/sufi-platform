using System.Threading.Tasks;

namespace SufiChain.SufiAbp.TextTemplating;

public interface ITemplateContentContributor
{
    Task<string?> GetOrNullAsync(TemplateContentContributorContext context);
}
