using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SmokeTest.Console;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Async(c => c.Console())
            .CreateLogger();

        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = "mongodb://localhost:27017/Sufi_SmokeTest",
                    ["AuthServer:Authority"] = "https://localhost:44300",
                    ["AuthServer:RequireHttpsMetadata"] = "false",
                    ["StringEncryption:DefaultPassPhrase"] = "SufiSmokeTestPassPhrase"
                })
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            using var application = await AbpApplicationFactory.CreateAsync<SufiSmokeTestModule>(options =>
            {
                options.Services.ReplaceConfiguration(configuration);
                options.UseAutofac();
                options.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: false));
            });

            await application.InitializeAsync();
            Log.Information("Sufi smoke test initialized successfully.");
            await application.ShutdownAsync();
            Log.Information("Sufi smoke test shutdown completed successfully.");

            return 0;
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "Sufi smoke test failed.");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
