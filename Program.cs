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
                builder.AddAzureKeyVault(new Uri(builder.Build()["KeyVault"]), new DefaultAzureCredential(false));
            }

            //  builder.AddEnvironmentVariables();
            builder.AddCommandLine(args);

        })

        .ConfigureServices((hostingContext, services) =>
        {
            services.Collectio(hostingContext.Configuration);
            services.Forward(hostingContext.Configuration);
            services.AddHostedService<Worker>();
        });
}