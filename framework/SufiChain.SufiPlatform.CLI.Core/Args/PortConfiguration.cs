namespace SufiChain.SufiPlatform.CLI.Args;

/// <summary>
/// Configuration for randomly generated ports used in scaffolded solutions.
/// </summary>
public class PortConfiguration
{
    /// <summary>
    /// HTTPS port for HttpApi.Host (API-only in tiered, or API + Auth in non-tiered).
    /// </summary>
    public int ApiPort { get; set; }
    
    /// <summary>
    /// HTTPS port for AuthServer (dedicated identity/OIDC authority host).
    /// Only used in tiered architecture.
    /// </summary>
    public int AuthServerPort { get; set; }
    
    /// <summary>
    /// HTTPS port for Blazor.WebApp (admin panel).
    /// </summary>
    public int BlazorPort { get; set; }
    
    /// <summary>
    /// HTTP port for Blazor.WebApp.
    /// </summary>
    public int BlazorHttpPort { get; set; }
    
    /// <summary>
    /// Port for Blazor WebAssembly client.
    /// </summary>
    public int BlazorWasmPort { get; set; }
    
    /// <summary>
    /// HTTPS port for Blazor.WebSite.
    /// </summary>
    public int PublicPort { get; set; }
    
    /// <summary>
    /// HTTP port for Blazor.WebSite.
    /// </summary>
    public int PublicHttpPort { get; set; }
    
    /// <summary>
    /// Port for MVC Web project (if used).
    /// </summary>
    public int WebPort { get; set; }

    /// <summary>
    /// HTTPS port for WebApp architecture host.
    /// </summary>
    public int SingleHostPort { get; set; }

    /// <summary>
    /// HTTP port for WebApp architecture host.
    /// </summary>
    public int SingleHostHttpPort { get; set; }

    /// <summary>
    /// HTTPS port for Blazor.WebApp.Client dev server.
    /// </summary>
    public int BlazorWebAppClientPort { get; set; }

    /// <summary>
    /// HTTP port for Blazor.WebApp.Client dev server.
    /// </summary>
    public int BlazorWebAppClientHttpPort { get; set; }

    /// <summary>
    /// HTTPS port for Blazor.WebSite.Client dev server.
    /// </summary>
    public int WebSiteClientPort { get; set; }

    /// <summary>
    /// HTTP port for Blazor.WebSite.Client dev server.
    /// </summary>
    public int WebSiteClientHttpPort { get; set; }

    /// <summary>
    /// HTTPS port for WebApp architecture client project.
    /// </summary>
    public int SingleClientPort { get; set; }

    /// <summary>
    /// HTTP port for WebApp architecture client project.
    /// </summary>
    public int SingleClientHttpPort { get; set; }

    /// <summary>
    /// Original port values from the demo templates.
    /// Aligned with .dev/hosts/layered and .dev/hosts/layered-tiered.
    /// </summary>
    public static class OriginalPorts
    {
        /// <summary>HttpApi.Host - shared by layered (44305) and layered-tiered.</summary>
        public const int ApiPort = 44305;
        public const int AuthServerPort = 44306;
        /// <summary>Blazor.WebApp in layered-tiered (launchSettings, appsettings).</summary>
        public const int BlazorPort = 44316;
        public const int BlazorHttpPort = 44317;
        public const int BlazorWasmPort = 44307;
        public const int PublicPort = 60927;
        public const int PublicHttpPort = 60928;
        public const int WebPort = 44302;
        public const int SingleHostPort = 44338;
        public const int SingleHostHttpPort = 44339;
        /// <summary>Blazor.WebApp.Client in layered-tiered.</summary>
        public const int BlazorWebAppClientPort = 62577;
        public const int BlazorWebAppClientHttpPort = 62578;
        public const int WebSiteClientPort = 65419;
        public const int WebSiteClientHttpPort = 65420;
        public const int SingleClientPort = 65463;
        public const int SingleClientHttpPort = 65464;
        /// <summary>Blazor.WebApp in layered (.dev/hosts/layered) - different from tiered to avoid port conflicts.</summary>
        public const int LayeredBlazorPort = 44350;
        public const int LayeredBlazorHttpPort = 44351;
        /// <summary>Blazor.WebApp.Client in layered (.dev/hosts/layered).</summary>
        public const int LayeredBlazorWebAppClientPort = 62590;
        public const int LayeredBlazorWebAppClientHttpPort = 62591;
        /// <summary>Blazor.WebSite in tiered (OpenIddict, DbMigrator, AuthServer appsettings).</summary>
        public const int PublicPortTiered = 44320;
    }

    /// <summary>
    /// Generates a new PortConfiguration with random unique ports.
    /// Each scaffold gets different ports to avoid conflicts between multiple scaffolded projects.
    /// </summary>
    public static PortConfiguration GenerateRandom()
    {
        // Use time-based seed for better randomness across scaffold invocations
        var random = new Random(Guid.NewGuid().GetHashCode());
        var used = new HashSet<int>();

        int NextUnique(int min, int max)
        {
            int port;
            do port = random.Next(min, max); while (used.Contains(port));
            used.Add(port);
            return port;
        }

        // Ranges chosen so base+1 for pairs does not overlap next range
        var apiPort = NextUnique(44300, 44400);
        var authServerPort = NextUnique(44400, 44450);
        var blazorPort = NextUnique(44450, 44499);       // 44499+1=44500, next starts 44500
        var blazorWasmPort = NextUnique(44500, 44600);
        var publicPort = NextUnique(44600, 44699);      // pair: base+1 <= 44700
        var webPort = NextUnique(44700, 44800);
        var singleHostPort = NextUnique(44800, 44898);  // pair: 44898+1=44899
        var singleClientPort = NextUnique(44900, 44998); // pair
        var blazorWebAppClientPort = NextUnique(45000, 45098); // pair
        var webSiteClientPort = NextUnique(45100, 45198);    // pair

        return new PortConfiguration
        {
            ApiPort = apiPort,
            AuthServerPort = authServerPort,
            BlazorPort = blazorPort,
            BlazorHttpPort = blazorPort + 1,
            BlazorWasmPort = blazorWasmPort,
            PublicPort = publicPort,
            PublicHttpPort = publicPort + 1,
            WebPort = webPort,
            SingleHostPort = singleHostPort,
            SingleHostHttpPort = singleHostPort + 1,
            SingleClientPort = singleClientPort,
            SingleClientHttpPort = singleClientPort + 1,
            BlazorWebAppClientPort = blazorWebAppClientPort,
            BlazorWebAppClientHttpPort = blazorWebAppClientPort + 1,
            WebSiteClientPort = webSiteClientPort,
            WebSiteClientHttpPort = webSiteClientPort + 1
        };
    }

    /// <summary>
    /// Returns the original demo template ports (no randomization).
    /// </summary>
    public static PortConfiguration GetOriginal()
    {
        return new PortConfiguration
        {
            ApiPort = OriginalPorts.ApiPort,
            AuthServerPort = OriginalPorts.AuthServerPort,
            BlazorPort = OriginalPorts.BlazorPort,
            BlazorHttpPort = OriginalPorts.BlazorHttpPort,
            BlazorWasmPort = OriginalPorts.BlazorWasmPort,
            PublicPort = OriginalPorts.PublicPort,
            PublicHttpPort = OriginalPorts.PublicHttpPort,
            WebPort = OriginalPorts.WebPort,
            SingleHostPort = OriginalPorts.SingleHostPort,
            SingleHostHttpPort = OriginalPorts.SingleHostHttpPort,
            BlazorWebAppClientPort = OriginalPorts.BlazorWebAppClientPort,
            BlazorWebAppClientHttpPort = OriginalPorts.BlazorWebAppClientHttpPort,
            WebSiteClientPort = OriginalPorts.WebSiteClientPort,
            WebSiteClientHttpPort = OriginalPorts.WebSiteClientHttpPort,
            SingleClientPort = OriginalPorts.SingleClientPort,
            SingleClientHttpPort = OriginalPorts.SingleClientHttpPort
        };
    }
}
