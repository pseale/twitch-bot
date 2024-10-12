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

        void OnClientOnOnJoinedChannel(object? _, OnJoinedChannelArgs e)
        {
            logger.LogInformation("TwitchLib: joined channel: " + e.Channel + " " + e.BotUsername);
        }

        var fatalErrorMessage = "";

        client.OnJoinedChannel += OnClientOnOnJoinedChannel;
        client.OnMessageReceived += (_, receivedArgs) =>
            logger.LogDebug("TwitchLib message received: " + receivedArgs.ChatMessage.Message);
        client.OnConnectionError += (_, errorArgs) =>
        {
            fatalErrorMessage = errorArgs.Error.Message;
            logger.LogError("TwitchLib connection error: " + fatalErrorMessage);
        };
        client.OnError += (_, errorArgs) =>
        {
            fatalErrorMessage = errorArgs.Exception.ToString();
            logger.LogError("TwitchLib connection error: " + fatalErrorMessage);
        };
        client.OnDisconnected += (_, _) => fatalErrorMessage = "Disconnected";
        client.OnLog += (_, logArgs) =>
        {
            if (logArgs.Data.Contains("PONG"))
                logger.LogTrace("TwitchLib log message: " + logArgs.Data);
            else
                logger.LogDebug("TwitchLib log message: " + logArgs.Data);
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