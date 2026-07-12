using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.TextTemplating;

public interface ITemplateContentContributor
{
    Task<string?> GetOrNullAsync(TemplateContentContributorContext context);
}
