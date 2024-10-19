using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace TwitchBot.WorkerService;

public class Worker(ILogger<Worker> logger, IConfiguration configuration) : BackgroundService
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

        var fatalErrorMessage = "";

        client.OnJoinedChannel += (_, e) => logger.LogInformation("TwitchLib: joined channel:" + e.Channel + " username:" + e.BotUsername);
        client.OnMessageReceived += (_, receivedArgs) => logger.LogInformation("[CHAT] " + receivedArgs.ChatMessage.DisplayName + ": " + receivedArgs.ChatMessage.Message);
        client.OnConnectionError += (_, errorArgs) =>
        {
            fatalErrorMessage = errorArgs.Error.Message;
            logger.LogCritical("TwitchLib connection error: " + fatalErrorMessage);
        };
        client.OnError += (_, errorArgs) =>
        {
            fatalErrorMessage = errorArgs.Exception.ToString();
            logger.LogCritical("TwitchLib error: " + fatalErrorMessage);
        };
        client.OnDisconnected += (_, _) =>
        {
            fatalErrorMessage = "Disconnected from channel.";
            logger.LogCritical(fatalErrorMessage);
        };
        client.OnLog += (_, logArgs) =>
        {
            if (logArgs.Data.StartsWith("Received: PONG"))
            {
                logger.LogTrace("TwitchLib log message: " + logArgs.Data); // logging this noisy (useless) log as Trace
            }
            else
            {
                logger.LogDebug("TwitchLib log message: " + logArgs.Data);
            }
        };

        client.Connect();

        while (!stoppingToken.IsCancellationRequested)
        {
            if (!string.IsNullOrWhiteSpace(fatalErrorMessage))
                break;
            await Task.Delay(1000, stoppingToken);
        }
    }
}