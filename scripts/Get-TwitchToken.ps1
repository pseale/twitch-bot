$ErrorActionPreference = "Stop"

$text = twitch token -u --scopes 'channel:manage:broadcast channel:read:subscriptions moderation:read user:read:email chat:edit chat:read' 2>&1
if ($?) { throw "Error getting a twitch token" }

$token = ($text | sls 'User Access Token:').ToString().Split(" ")[-1]
[Environment]::SetEnvironmentVariable("TWITCH_USER_ACCESS_TOKEN", $token)


