namespace Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}


if (!msg.ApplicationProperties.ContainsKey("retryCount")) //normal message not one retried
{
    statusCode, delayMessage = await _dispatch.SendAsync(msgContent);
    if (statusCode == HttpStatusCode.OK)
    {
        //call successful (i.e. no server error) 
        await args.CompleteMessageAsync(msg);
    } 
    else
    {
        //unfortunately call not successful, let's abandon the message, maybe next time it will work
        _logger.LogInformation($"MessageHandler Abandon... HttpStatusCode={statusCode},SequenceNumber={msg.SequenceNumber},MessageId={msg.MessageId}");

        // If delayMessage is greater than 0, schedule the message to be processed after that delay
        if (delayMessage > 0)
        {
            var scheduleTime = DateTime.UtcNow.AddSeconds(delayMessage);
            await args.DeferAsync(msg.SystemProperties.LockToken, new Dictionary<string, object>
            {
                { "scheduleTime", scheduleTime }
            });
        }
        else
        {
            await args.AbandonMessageAsync(msg);
        }
    }
}
