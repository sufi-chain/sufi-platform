using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Encryption;

namespace SufiChain.SufiPlatform.SufiAI;

public class AICredentialResolver : IAICredentialResolver, ITransientDependency
{
    protected IStringEncryptionService StringEncryptor { get; }

    public AICredentialResolver(IStringEncryptionService stringEncryptor)
    {
        StringEncryptor = stringEncryptor;
    }

    public virtual string? DecryptApiKey(string? encryptedApiKey)
    {
        if (string.IsNullOrWhiteSpace(encryptedApiKey))
        {
            return null;
        }

        try
        {
            var decrypted = StringEncryptor.Decrypt(encryptedApiKey);
            return string.IsNullOrWhiteSpace(decrypted) ? null : decrypted;
        }
        catch
        {
            return encryptedApiKey;
        }
    }
}
