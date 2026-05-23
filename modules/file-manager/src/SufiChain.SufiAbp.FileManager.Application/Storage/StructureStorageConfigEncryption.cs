using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Encryption;

namespace SufiChain.SufiAbp.FileManager.Storage;

public class StructureStorageConfigEncryption : IStructureStorageConfigEncryption, ITransientDependency
{
    private readonly IStringEncryptionService _stringEncryptionService;

    public StructureStorageConfigEncryption(IStringEncryptionService stringEncryptionService)
    {
        _stringEncryptionService = stringEncryptionService;
    }

    public string? EncryptSensitiveValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return _stringEncryptionService.Encrypt(value);
    }

    public string? DecryptSensitiveValue(string? encryptedValue)
    {
        if (string.IsNullOrEmpty(encryptedValue))
        {
            return null;
        }

        return _stringEncryptionService.Decrypt(encryptedValue);
    }
}
