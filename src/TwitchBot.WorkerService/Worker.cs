using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace TwitchBot.WorkerService;

public class Worker(IHostApplicationLifetime hostApplicationLifetime, ILogger<Worker> logger, IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var twitchBotUsername = configuration["TWITCH_BOT_USERNAME"];
        if (string.IsNullOrWhiteSpace(twitchBotUsername))
            throw new Exception("TWITCH_BOT_USERNAME environment variable is missing");

        var twitchChannel = configuration["TWITCH_CHANNEL"];
        if (string.IsNullOrWhiteSpace(twitchBotUsername))
            throw new Exception("TWITCH_CHANNEL configuration variable is missing");

        var userAccessToken = configuration["TWITCH_USER_ACCESS_TOKEN"];
        if (string.IsNullOrWhiteSpace(userAccessToken))
            throw new Exception("TWITCH_USER_ACCESS_TOKEN configuration variable is missing");

        var credentials = new ConnectionCredentials(twitchBotUsername, userAccessToken);
        var clientOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30)
        };
        var customClient = new WebSocketClient(clientOptions);
        var client = new TwitchClient(customClient);
        client.Initialize(credentials, twitchChannel);


        client.OnJoinedChannel += (_, e) => logger.LogInformation("TwitchLib: joined channel:{Channel} username:{BotUsername}", e.Channel, e.BotUsername);
        client.OnLog += (_, logArgs) =>
        {
            if (logArgs.Data.StartsWith("Received: PING") || logArgs.Data.StartsWith("Received: PONG") || logArgs.Data.StartsWith("Writing: PONG"))
            {
                logger.LogTrace("TwitchLib log message: {Data}", logArgs.Data); // logging these noisy (useless) logs as Trace
            }
            else
            {
                logger.LogDebug("TwitchLib log message: {Data}", logArgs.Data);
            }
        };
        client.OnConnectionError +=  (_, errorArgs) =>  GracefullyTerminate(errorArgs.Error.Message);
        client.OnError +=  (_, errorArgs) =>  GracefullyTerminate("TwitchLib error: {Exception}", errorArgs.Exception);
        client.OnDisconnected +=  (_, _) =>  logger.LogInformation("Disconnected from channel.");
        client.OnIncorrectLogin +=  (_, args) =>  GracefullyTerminate("Login failure: {Exception}", args.Exception);


        // ----------- START HERE -----------
        // OnMessageReceived processes messages from chat - this is where you begin implementing your chatbot
        client.OnMessageReceived += (_, receivedArgs) => logger.LogInformation("[CHAT] {DisplayName}: {Message} ", receivedArgs.ChatMessage.DisplayName, receivedArgs.ChatMessage.Message);
        // ----------------------------------

        client.Connect();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
    private void GracefullyTerminate(string message, params object?[] messageArgs)
    {
        logger.LogCritical(message, messageArgs);
        hostApplicationLifetime.StopApplication(); // https://stackoverflow.com/a/59503361
    }
}
