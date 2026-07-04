using System.Threading.Tasks;

namespace SufiChain.SufiAbp.Communications.Email;

/// <summary>
/// Configuration interface for email sender
/// </summary>
public interface IEmailSenderConfiguration
{
    /// <summary>
    /// Gets the default "from" email address
    /// </summary>
    Task<string> GetDefaultFromAddressAsync();

    /// <summary>
    /// Gets the default "from" display name
    /// </summary>
    Task<string> GetDefaultFromDisplayNameAsync();
}
