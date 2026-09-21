namespace SufiChain.SufiPlatform.Account;

public interface IExternalAuthLoginProviderFilter
{
    Task<IReadOnlyList<string>> FilterEnabledAsync(IEnumerable<string> schemeNames);
}
