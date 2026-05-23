namespace SufiChain.SufiAbp.FileManager.Storage;

/// <summary>
/// Encrypts and decrypts sensitive values in file structure storage configuration
/// </summary>
public interface IStructureStorageConfigEncryption
{
    /// <summary>
    /// Encrypts a sensitive value for storage. Returns null if input is null or empty.
    /// </summary>
    string? EncryptSensitiveValue(string? value);

    /// <summary>
    /// Decrypts a stored encrypted value. Returns null if input is null or empty.
    /// </summary>
    string? DecryptSensitiveValue(string? encryptedValue);
}
