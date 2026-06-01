using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Encryption;

namespace SufiChain.Chat.Connectors.Email.Settings;

public interface IChatEmailConnectorSettingsEncryption
{
    string? Encrypt(string? value);

    string? Decrypt(string? value);
}

public class ChatEmailConnectorSettingsEncryption : IChatEmailConnectorSettingsEncryption, ITransientDependency
{
    protected IStringEncryptionService StringEncryptionService { get; }

    public ChatEmailConnectorSettingsEncryption(IStringEncryptionService stringEncryptionService)
    {
        StringEncryptionService = stringEncryptionService;
    }

    public virtual string? Encrypt(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return StringEncryptionService.Encrypt(value);
    }

    public virtual string? Decrypt(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return StringEncryptionService.Decrypt(value);
    }
}
