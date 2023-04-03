using Azure.Messaging.ServiceBus;

namespace ... ;

public static class Injector
{
    public static IServiceCollection Collection(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceBusCollectorConfig>(configuration.GetSection("ServiceBus"));

        var serviceBusConnectionString = configuration["ServiceBusConnectionString"];
        var queueName = configuration["ServiceBus:QueueName"];

        services.AddSingleton<ServiceBusClient>(new ServiceBusClient(serviceBusConnectionString));

        services.AddSingleton<ServiceBusSessionProcessor>(provider =>
        {
            var serviceBusClient = provider.GetService<ServiceBusClient>();
            var processorOptions = new ServiceBusSessionProcessorOptions
            {
                MaxConcurrentSessions = 1,
                SessionIdleTimeout = TimeSpan.FromMinutes(1),
            };

            return serviceBusClient.CreateSessionProcessor(queueName, processorOptions);
        });

        return services;
    }

    public static IServiceCollection Forward(this IServiceCollection services, IConfiguration configuration)
    {

        var aad = configuration["Aad"];
        
        services.Configure<Config>(options =>
        {  
            /*
             we bind WebServiceConfig to options object, then we set AadClientSecret property of the WebService options object to the secret value
             this way the injected instance of WebService will contain the actual secret value
            */
            configuration.GetSection("Config").Bind(options);
            options.AadClientSecret = aad;
        });
        
        services.AddSingleton<ITokenGenerator, TokenGenerator>();

        services.AddSingleton<ServiceBusSender>(provider =>
        {
            var client = new ServiceBusClient(configuration["ServiceBusConnectionString"]);
            var sender = client.CreateSender(configuration["ServiceBus:QueueName"]);
            return sender;
        });

        services.AddSingleton<ServiceBusMessageFactory>();

        
        return services;
    }
}