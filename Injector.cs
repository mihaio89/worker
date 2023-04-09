using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;

namespace Mediation.Consumer.Dispatch;


public static class ServiceInjector
{
    public static IServiceCollection AddCollection(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceBusCollectorConfig>(configuration.GetSection("ServiceBus"));

        services.AddSingleton<ServiceBusSessionProcessor>(client =>
        {
            ServiceBusCollectorConfig config = client.GetRequiredService<IOptions<ServiceBusCollectorConfig>>().Value;
            ServiceBusClient sb = new ServiceBusClient(config.Namespace, client.GetRequiredService<DefaultAzureCredential>());
            var processorOptions = new ServiceBusSessionProcessorOptions
            {
                MaxConcurrentSessions = 1,
                SessionIdleTimeout = TimeSpan.FromMinutes(1),
            };

            return sb.CreateSessionProcessor(config.QueueName, processorOptions);
        });

        return services;
    }

    public static IServiceCollection AddForwarding(this IServiceCollection services, IConfiguration configuration)
    {

        var aad = configuration["Aad"];
        
        services.Configure<WebServiceConfig>(options =>
        {  
            /*
             we bind WebServiceConfig to options object, 
             then we set AadClientSecret property of the WebService options object to the secret value
             this way the injected instance of WebService will contain the actual secret value
            */
            configuration.GetSection("WebService").Bind(options);
            options.AadClientSecret = aad;
        });
        
        services.AddSingleton<ITokenGenerator, TokenGenerator>();

        services.AddSingleton<IDispatch, Dispatch>();

        services.AddSingleton(client => AddServiceBusForwarding(client));

        services.AddSingleton<ServiceBusMessageFactory>();

        services.Configure<BlobStorageConfig>(configuration.GetSection("BlobStorageConfig"));
        services.AddSingleton<IBlobClient>(client =>
        {
            var clientSecretCredentialBlobUpload = client.GetRequiredService<DefaultAzureCredential>();
            AzureBlobClient blobContainerClient = new AzureBlobClient(clientSecretCredentialBlobUpload);
            return blobContainerClient;
        });


        services.AddSingleton<IStorage, Storage>();


        return services;
    }

    public static ServiceBusSender AddServiceBusForwarding(IServiceProvider client)
    {

        ServiceBusCollectorConfig config = client.GetRequiredService<IOptions<ServiceBusCollectorConfig>>().Value;

        var credential = client.GetRequiredService<DefaultAzureCredential>();

        ServiceBusClient sbClient = new ServiceBusClient(config.Namespace, credential);
        ServiceBusSender sender = sbClient.CreateSender(config.QueueName);

        return sender;
    }

    public static IServiceCollection AddProcessing(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(cred =>
        {
            var options = new DefaultAzureCredentialOptions()
            {
                ManagedIdentityClientId = configuration["UserAssignedMI"]
            };

            return new DefaultAzureCredential(options);
        });

        return services;
    }
}