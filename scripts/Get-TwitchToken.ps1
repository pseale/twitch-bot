$ErrorActionPreference = "Stop"

$text = twitch token -u --scopes 'moderation:read user:read:email chat:edit chat:read' 2>&1
if (!$?) { throw "Error getting a twitch token: $text" }

$token = ($text | Select-String 'User Access Token:').ToString().Split(" ")[-1]
[Environment]::SetEnvironmentVariable("TWITCH_USER_ACCESS_TOKEN", $token)


