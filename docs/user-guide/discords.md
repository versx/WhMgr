# Discord Server Configs
Copy your Discord specific configs to the `bin/discords` folder and reference them in the main config under the servers section.  

```json
    {
        // Bot command prefix, leave blank to use @mention <command>
        "commandPrefix": ".",
        // Discord Emoji server ID. (Can be same as `guildId`)  
        "emojiGuildId": 000000000000000001,
        // Discord server owner ID.
        "ownerId": 000000000000000000,
        // Donor/Supporter role ID(s).
        "donorRoleIds": [
            000000000000000000
        ],
        // Name of a free role for free weekends etc
        "freeRoleName": "",
        // Moderator Discord role ID(s).
        "moderatorRoleIds": [
            000000000000000000
        ],
        // Discord bot token with user.
        "token": "<DISCORD_BOT_TOKEN>",
        // Alarms file path.
        "alarms": "alarms.json",
        // Geofences related to the Discord guild. **NOT** used for subscriptions.  
        "geofences": [
            "City1.txt",
            "City2.json"
        ],
        // Custom user subscriptions
        "subscriptions": {
            // Enable or disable custom direct message notification subscriptions per user.
            "enabled": false,
            // Maximum amount of Pokemon subscriptions a user can set, set as 0 for no limit.
            "maxPokemonSubscriptions": 0,
            // Maximum amount of PvP subscriptions a user can set, set as 0 for no limit.
            "maxPvPSubscriptions": 0,
            // Maximum amount of Raid subscriptions a user can set, set as 0 for no limit.
            "maxRaidSubscriptions": 0,
            // Maximum amount of Quest subscriptions a user can set, set as 0 for no limit.
            "maxQuestSubscriptions": 0,
            // Maximum amount of Invasion subscriptions a user can set, set as 0 for no limit.
            "maxInvasionSubscriptions": 0,
            // Maximum amount of Gym subscriptions a user can set, set as 0 for no limit.
            "maxGymSubscriptions": 0,
            // Maximum amount of Lure subscriptions a user can set, set as 0 for no limit.
            "maxLureSubscriptions": 0
        },
        // Enable city/geofence role assignments.
        "enableGeofenceRoles": false,
        // Automatically remove any created and assigned city/area/geofence roles when a donor/support role is removed.
        "autoRemoveGeofenceRoles": false,
        // Assigning city roles requires Donor/Supporter role.
        "citiesRequireSupporterRole": true,
        // Prune old field research quests at midnight.
        "pruneQuestChannels": true,
        // Channel ID(s) of quest channels to prune at midnight.
        "questChannelIds": [
            000000000000000000
        ],
        // Channel ID to post nests.
        "nestsChannelId": 000000000000000000,
        // Minimum amount of average spawn count per hour for nest to post
        "nestsMinimumPerHour": 2,
        // Shiny stats configuration
        "shinyStats": {
            // Enable shiny stats posting.
            "enabled": true,
            // Clear previous shiny stat messages.
            "clearMessages": false,
            // Channel ID to post shiny stats.
            "channelId": 000000000000000000
        },
        // Icon style to use.
        "iconStyle": "Default",
        // Channel ID(s) bot commands can be executed in.
        "botChannelIds": [
            000000000000000000
        ],
        // Custom Discord status per server, leave blank or null to use current version.  
        "status": "",
        // Alerts file that will be used for direct message subscription notifications.
        "dmAlertsFile": "default.json",
        // Discord embed colors
        "embedColors": {
            // Embed colors for Pokemon embeds
            "pokemon": {
                // Embed colors for Pokemon with IV embeds
                "iv": [
                    { "min": 0, "max": 0, "color": "#ffffff" },
                    { "min": 1, "max": 89, "color": "#ffff00" },
                    { "min": 90, "max": 99, "color": "#ffa500" },
                    { "min": 100, "max": 100, "color": "#00ff00" }
                ],
                // Embed colors for Pokemon with PvP stats embeds
                "pvp": [
                    { "min": 1, "max": 1, "color": "#000080" },
                    { "min": 6, "max": 25, "color": "#800080" },
                    { "min": 25, "max": 100, "color": "#aa2299" }
                ]
            },
            // Embed colors for Raid embeds
            "raids": {
                "1": "#ff69b4",
                "2": "#ff69b4",
                "3": "#ffff00",
                "4": "#ffff00",
                "5": "#800080",
                "6": "#a52a2a",
                "ex": "#2c2f33"
            },
            // Embed colors for different types of Pokestops
            "pokestops": {
                "quests": "#ffa500",
                "lures": {
                    "normal": "#ff69b4",
                    "glacial": "#6495ed",
                    "mossy": "#507d2a",
                    "magnetic": "#808080"
                },
                "invasions": "#ff0000"
            },
            // Embed colors for Weather embeds
            "weather": {
                "clear": "#ffff00",
                "cloudy": "#99aab5",
                "fog": "#9a9a9a",
                "partlyCloudy": "#808080",
                "rain": "#0000ff",
                "snow": "#ffffff",
                "windy": "#800080"
            }
        }
    }
```