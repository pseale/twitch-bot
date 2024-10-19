# Simple Twitch Bot skeleton in .NET 8

This is an **absolutely minimal** twitch bot with logging and error handling. It currently connects to one channel and logs chat messages to the console.

The twitch bot serves as an example for how to set up TwitchLib, the Twitch CLI, and Twitch user access tokens. It does nothing useful by itself.

# Installation

1. Set the following environment variables:
    - `TWITCH_BOT_USERNAME`
    - `TWITCH_CHANNEL`
2. Store a twitch user authentication token with scopes `chat:read chat:edit` in the environment variable `TWITCH_USER_ACCESS_TOKEN`. One way to do this is shown below:
    1. Register a Twitch Application on the [Twitch Developer Console](https://dev.twitch.tv/console/apps).
    2. Install and configure the Twitch CLI.
        - Install: https://dev.twitch.tv/docs/cli/
        - Configure: https://dev.twitch.tv/docs/cli/configure-command/
    2. Run `./scripts/Get-TwitchToken.ps1`. This should open a browser window. If necessary, grant permissions for the requested scopes.
3. Run the bot:
    ``` PowerShell
    cd ./src/TwitchBot.Worker
    dotnet run
    ```

# Notes

- This has not been fully tested. Notably, I have never seen a `Disconnected` event or an `Error` event.
- The Twitch user authentication token does not refresh itself, and presumably needs to be refreshed regularly.

## Logging

- Logging is set to `Debug` by default.
- For less noise, set the log level to `Information`.

## Configuration

This leverages .NET's Configuration system. While the instructions above use environment variables, it is possible instead to use an `appsettings.json` file.
