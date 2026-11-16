#if NET
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SufiChain.SufiPlatform.FileManager.Storage;

/// <summary>
/// Reads storage secrets from JSON and never writes them back.
/// </summary>
public sealed class FileStructureStorageConfigDtoConverter : JsonConverter<FileStructureStorageConfigDto>
{
    public override FileStructureStorageConfigDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        var config = new FileStructureStorageConfigDto();

        if (TryGet(root, "storageProvider", out var provider))
        {
            config.StorageProvider = ReadProvider(provider);
        }

        config.DatabaseConnectionString = ReadString(root, "databaseConnectionString");
        config.FileSystemBasePath = ReadString(root, "fileSystemBasePath");
        config.MinioEndPoint = ReadString(root, "minioEndPoint");
        config.MinioAccessKey = ReadString(root, "minioAccessKey");
        config.MinioSecretKey = ReadString(root, "minioSecretKey");
        config.MinioBucketName = ReadString(root, "minioBucketName");
        config.HasDatabaseConnectionString = ReadBool(root, "hasDatabaseConnectionString");
        config.HasMinioAccessKey = ReadBool(root, "hasMinioAccessKey");
        config.HasMinioSecretKey = ReadBool(root, "hasMinioSecretKey");
        config.S3EndPoint = ReadString(root, "s3EndPoint");
        config.S3Region = ReadString(root, "s3Region");
        config.S3AccessKeyId = ReadString(root, "s3AccessKeyId");
        config.S3SecretAccessKey = ReadString(root, "s3SecretAccessKey");
        config.S3ContainerName = ReadString(root, "s3ContainerName");
        config.HasS3AccessKey = ReadBool(root, "hasS3AccessKey");
        config.HasS3SecretKey = ReadBool(root, "hasS3SecretKey");
        return config;
    }

    public override void Write(Utf8JsonWriter writer, FileStructureStorageConfigDto value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("storageProvider", value.StorageProvider.ToString());
        WriteString(writer, "fileSystemBasePath", value.FileSystemBasePath);
        WriteString(writer, "minioEndPoint", value.MinioEndPoint);
        WriteString(writer, "minioBucketName", value.MinioBucketName);
        writer.WriteBoolean("hasDatabaseConnectionString", value.HasDatabaseConnectionString);
        writer.WriteBoolean("hasMinioAccessKey", value.HasMinioAccessKey);
        writer.WriteBoolean("hasMinioSecretKey", value.HasMinioSecretKey);
        WriteString(writer, "s3EndPoint", value.S3EndPoint);
        WriteString(writer, "s3Region", value.S3Region);
        WriteString(writer, "s3ContainerName", value.S3ContainerName);
        writer.WriteBoolean("hasS3AccessKey", value.HasS3AccessKey);
        writer.WriteBoolean("hasS3SecretKey", value.HasS3SecretKey);
        writer.WriteEndObject();
    }

    private static FileStructureStorageProvider ReadProvider(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var number))
        {
            return (FileStructureStorageProvider)number;
        }

        if (element.ValueKind == JsonValueKind.String
            && Enum.TryParse<FileStructureStorageProvider>(element.GetString(), ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        return FileStructureStorageProvider.Database;
    }

    private static string? ReadString(JsonElement root, string name)
    {
        return TryGet(root, name, out var element) && element.ValueKind == JsonValueKind.String
            ? element.GetString()
            : null;
    }

    private static bool ReadBool(JsonElement root, string name)
    {
        return TryGet(root, name, out var element)
            && element.ValueKind is JsonValueKind.True or JsonValueKind.False
            && element.GetBoolean();
    }

    private static bool TryGet(JsonElement root, string camelName, out JsonElement element)
    {
        if (root.TryGetProperty(camelName, out element))
        {
            return true;
        }

        var pascal = char.ToUpperInvariant(camelName[0]) + camelName[1..];
        return root.TryGetProperty(pascal, out element);
    }

    private static void WriteString(Utf8JsonWriter writer, string name, string? value)
    {
        if (value == null)
        {
            writer.WriteNull(name);
            return;
        }

        writer.WriteString(name, value);
    }
}
#endif
