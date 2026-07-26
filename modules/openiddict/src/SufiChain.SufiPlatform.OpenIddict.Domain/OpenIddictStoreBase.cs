#nullable disable
using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Uow;

namespace SufiChain.SufiPlatform.OpenIddict;

/// <summary>
/// Shared helpers for OpenIddict store adapters over Sufi repositories.
/// </summary>
public abstract class OpenIddictStoreBase<TRepository>
    where TRepository : IRepository
{
    public ILogger<OpenIddictStoreBase<TRepository>> Logger { get; set; }

    protected TRepository Repository { get; }
    protected IUnitOfWorkManager UnitOfWorkManager { get; }
    protected IGuidGenerator GuidGenerator { get; }
    protected OpenIddictIdentifierConverter IdentifierConverter { get; }
    protected IOpenIddictDbConcurrencyExceptionHandler ConcurrencyExceptionHandler { get; }
    protected IOptions<OpenIddictStoreOptions> StoreOptions { get; }

    protected OpenIddictStoreBase(
        TRepository repository,
        IUnitOfWorkManager unitOfWorkManager,
        IGuidGenerator guidGenerator,
        OpenIddictIdentifierConverter identifierConverter,
        IOpenIddictDbConcurrencyExceptionHandler concurrencyExceptionHandler,
        IOptions<OpenIddictStoreOptions> storeOptions)
    {
        Repository = repository;
        UnitOfWorkManager = unitOfWorkManager;
        GuidGenerator = guidGenerator;
        IdentifierConverter = identifierConverter;
        ConcurrencyExceptionHandler = concurrencyExceptionHandler;
        StoreOptions = storeOptions;

        Logger = NullLogger<OpenIddictStoreBase<TRepository>>.Instance;
    }

    protected virtual Guid ConvertIdentifierFromString(string identifier)
    {
        return IdentifierConverter.FromString(identifier);
    }

    protected virtual string ConvertIdentifierToString(Guid identifier)
    {
        return IdentifierConverter.ToString(identifier);
    }

    protected virtual string WriteStream(Action<Utf8JsonWriter> action)
    {
        using (var stream = new MemoryStream())
        {
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
                   {
                       Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                       Indented = false
                   }))
            {
                action(writer);
                writer.Flush();
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }

    protected virtual async Task<string> WriteStreamAsync(Func<Utf8JsonWriter, Task> func)
    {
        using (var stream = new MemoryStream())
        {
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
                   {
                       Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                       Indented = false
                   }))
            {
                await func(writer);
                await writer.FlushAsync();
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }
}
