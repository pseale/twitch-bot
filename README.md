# Dead-Simple Twitch Bot skeleton in .NET 8

This is a minimal twitch bot with logging and error handling. It currently connects to one channel and logs chat messages to the console.

# Installation

1. Register a Twitch Application on the [Twitch Developer Console](https://dev.twitch.tv/console/apps).
2. Install and configure the Twitch CLI.
    - Install: https://dev.twitch.tv/docs/cli/
    - Configure: https://dev.twitch.tv/docs/cli/configure-command/
3. Set the following environment variables:
    - `TWITCH_BOT_USERNAME`
    - `TWITCH_CHANNEL`
4. Store a twitch user authentication token in the environment variable `TWITCH_USER_ACCESS_TOKEN` using one of two options:
    - Do it manually
    - Run `./scripts/Get-TwitchToken.ps1`
5. Run
    ``` PowerShell
    cd ./src/TwitchBot.Worker
    dotnet run
    ```

# Notes

- This has not been fully tested. Notably, I have never seen a `Disconnected` event or an `Error` event.
- The Twitch user authentication token does not refresh itself, and needs to be refreshed regularly.

