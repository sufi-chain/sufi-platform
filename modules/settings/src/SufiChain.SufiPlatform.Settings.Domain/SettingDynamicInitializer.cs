using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Polly;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;
using Volo.Abp.Threading;

namespace SufiChain.SufiPlatform.Settings;

public class SettingDynamicInitializer : ITransientDependency
{
    public ILogger<SettingDynamicInitializer> Logger { get; set; }

    protected IServiceProvider ServiceProvider { get; }

    public SettingDynamicInitializer(IServiceProvider serviceProvider)
    {
        Logger = NullLogger<SettingDynamicInitializer>.Instance;
        ServiceProvider = serviceProvider;
    }

    public virtual Task InitializeAsync(bool runInBackground, CancellationToken cancellationToken = default)
    {
        var options = ServiceProvider
            .GetRequiredService<IOptions<SettingsOptions>>()
            .Value;

        if (!options.SaveStaticSettingsToDatabase && !options.IsDynamicSettingStoreEnabled)
        {
            return Task.CompletedTask;
        }

        if (runInBackground)
        {
            var applicationLifetime = ServiceProvider.GetService<IHostApplicationLifetime>();
            Task.Run(async () =>
            {
                if (cancellationToken == default && applicationLifetime?.ApplicationStopping != null)
                {
                    cancellationToken = applicationLifetime.ApplicationStopping;
                }

                await ExecuteInitializationAsync(options, cancellationToken);
            }, cancellationToken);

            return Task.CompletedTask;
        }

        return ExecuteInitializationAsync(options, cancellationToken);
    }

    protected virtual async Task ExecuteInitializationAsync(
        SettingsOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var cancellationTokenProvider = ServiceProvider.GetRequiredService<ICancellationTokenProvider>();
            using (cancellationTokenProvider.Use(cancellationToken))
            {
                if (cancellationTokenProvider.Token.IsCancellationRequested)
                {
                    return;
                }

                await SaveStaticSettingsToDatabaseAsync(options, cancellationToken);

                if (cancellationTokenProvider.Token.IsCancellationRequested)
                {
                    return;
                }

                await PreCacheDynamicSettingsAsync(options);
            }
        }
        catch
        {
            // No need to log here since inner calls log
        }
    }

    protected virtual async Task SaveStaticSettingsToDatabaseAsync(
        SettingsOptions options,
        CancellationToken cancellationToken)
    {
        if (!options.SaveStaticSettingsToDatabase)
        {
            return;
        }

        var staticSettingSaver = ServiceProvider.GetService<IStaticSettingSaver>();
        if (staticSettingSaver == null)
        {
            return;
        }

        await Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                8,
                retryAttempt => TimeSpan.FromSeconds(
                    Volo.Abp.RandomHelper.GetRandom(
                        (int)Math.Pow(2, retryAttempt) * 8,
                        (int)Math.Pow(2, retryAttempt) * 12)
                )
            )
            .ExecuteAsync(async _ =>
            {
                try
                {
                    await staticSettingSaver.SaveAsync();
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                    throw;
                }
            }, cancellationToken);
    }

    protected virtual async Task PreCacheDynamicSettingsAsync(SettingsOptions options)
    {
        if (!options.IsDynamicSettingStoreEnabled)
        {
            return;
        }

        var dynamicSettingDefinitionStore = ServiceProvider.GetService<IDynamicSettingDefinitionStore>();
        if (dynamicSettingDefinitionStore == null)
        {
            return;
        }

        try
        {
            await dynamicSettingDefinitionStore.GetAllAsync();
        }
        catch (Exception ex)
        {
            Logger.LogException(ex);
            throw;
        }
    }
}
