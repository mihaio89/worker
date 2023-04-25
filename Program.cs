using Azure.Identity;

namespace <aname>;

public class Program
{
    public static readonly Dictionary<string, string> envMap = new Dictionary<string, string>
                                                        {
                                                            { "DEV", "Development" },
                                                            { "INT", "Staging" },
                                                            { "PROD","Production"}
                                                        };
    public static void Main(string[] args)
    {
        {
            CreateHostBuilder(args).Build().Run();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)

        .ConfigureAppConfiguration((hostingContext, builder) =>
        {
            string environment = envMap.GetValueOrDefault(Environment.GetEnvironmentVariable("Env")?.ToUpperInvariant() ?? "INT");

            builder.Sources.Clear();
            builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);
            
            if (environment.Equals("Development"))
            {
                builder.AddUserSecrets<Program>();
            }

            if (environment.Equals("Staging") || environment.Equals("Production"))
            {
                var builtConfig = builder.Build();
                var options = new DefaultAzureCredentialOptions()
                {
                    ManagedIdentityClientId = builtConfig["<id>"],
                    ExcludeVisualStudioCodeCredential = false,
                    ExcludeVisualStudioCredential = false,
                    ExcludeAzureCliCredential = true,
                    ExcludeEnvironmentCredential = true,
                    ExcludeSharedTokenCacheCredential = true,
                    ExcludeInteractiveBrowserCredential = true
                };
                builder.AddAzureKeyVault(new Uri(builder.Build()["KeyVault"]), new DefaultAzureCredential(options));
            }

            //  builder.AddEnvironmentVariables();
            builder.AddCommandLine(args);

        })

        .ConfigureServices((hostingContext, services) =>
        {
            services.AddProcessing(hostingContext.Configuration); // for MI
            services.Collectio(hostingContext.Configuration);
            services.Forward(hostingContext.Configuration);
            services.AddHostedService<Worker>();
        });
}