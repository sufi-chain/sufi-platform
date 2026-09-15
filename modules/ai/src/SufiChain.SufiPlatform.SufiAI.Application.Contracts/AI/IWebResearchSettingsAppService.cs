using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.SufiAI;

public interface IWebResearchSettingsAppService : IApplicationService
{
    Task<WebResearchSettingsDto> GetAsync();
    Task UpdateAsync(UpdateWebResearchSettingsInput input);
    Task<WebResearchConnectionDto> TestAsync();
}

public class WebResearchSettingsDto
{
    public bool Enabled { get; set; }
    [StringLength(2048)] public string Endpoint { get; set; } = "";
    public bool HasStoredToken { get; set; }
    [Range(0, 2)] public int SafeSearch { get; set; } = 1;
    [Range(2, 30)] public int SearchTimeoutSeconds { get; set; } = 8;
    [Range(2, 30)] public int FetchTimeoutSeconds { get; set; } = 8;
    [Range(1, 10)] public int MaxSearchResults { get; set; } = 5;
    [Range(0, 5)] public int MaxPagesToFetch { get; set; } = 3;
    [Range(1024, 5_242_880)] public int MaxPageBytes { get; set; } = 1_048_576;
    [Range(100, 24_000)] public int MaxExtractedCharactersPerPage { get; set; } = 12_000;
    [Range(1000, 60_000)] public int MaxTotalContextCharacters { get; set; } = 30_000;
}

public class UpdateWebResearchSettingsInput : WebResearchSettingsDto
{
    [StringLength(4096)] public string? AuthorizationBearerToken { get; set; }
    public bool ClearToken { get; set; }
}

public class WebResearchConnectionDto
{
    public bool Ready { get; set; }
    public string? FailureCode { get; set; }
}
