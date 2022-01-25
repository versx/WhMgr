# Discord Server Configs
Copy your Discord specific configs to the `bin/discords` folder and reference them in the main config under the servers section.  

```json
{
  // Discord bot general config options
  "bot": {
    // Bot command prefix, leave blank to use @mention <command>
    "commandPrefix": ".",
    // Discord guild ID.
    "guildId": 000000000000000000,
    // Discord Emoji server ID. (Can be same as `guildId`)  
    "emojiGuildId": 000000000000000001,
    // Discord bot token with user.
    "token": "<DISCORD_BOT_TOKEN>",
    // Channel ID(s) bot commands can be executed in. (currently not implemented)
    "channelIds": [],
    // Custom Discord status per server, leave blank or null to use current version.  
    "status": null
  },
  // Discord server owner ID.
  "ownerId": 000000000000000000,
  // Donor/Supporter role ID(s) config.
  "donorRoleIds": {
    // Discord server role id and subscription permissions
    "000000000000000000": ["pokemon", "pvp", "raids", "quests", "invasions", "lures", "gyms"],
    // User has access to nothing
    "000000000000000001": [],
    // Users with role will only have access to Pokestops and Gyms
    "000000000000000002": ["quests", "gyms"]
  },
  // Discord free role name, if set allows non-donors/supporters to use the .feedme commands to assign city roles (optional, good for free promotional periods)
  "freeRoleName": "",
  // Moderator role IDs
  "moderatorRoleIds": [
    000000000000000001,
    000000000000000002
  ],
  // Discord alarms config file name to use
  "alarms": "alarms.json",
  // Discord server related geofences
  "geofences": [
    "City1.txt",
    "City2.json"
  ],
  // Subscriptions config
  "subscriptions": {
    // Determines whether subscriptions are enabled for the Discord server or not.
    "enabled": false,
    // Maximum notifications per minutes per subscriber before rate limited.
    "maxNotificationsPerMinute": 10,
    // Maximum Pokemon subscriptions in total
    "maxPokemonSubscriptions": 0,
    // Maximum PvP subscriptions in total
    "maxPvPSubscriptions": 0,
    // Maximum Raid subscriptions in total
    "maxRaidSubscriptions": 0,
    // Maximum Quest subscriptions in total
    "maxQuestSubscriptions": 0,
    // Maximum Invasion subscriptions in total
    "maxInvasionSubscriptions": 0,
    // Maximum Lure subscriptions in total
    "maxLureSubscriptions": 0,
    // Maximum Gym subscriptions in total
    "maxGymSubscriptions": 0,
    // Webhook Manager UI home page url
    "url": "http://127.0.0.1:8009",
    // Subscriptions DM embeds file.
    "embedsFile": "default.json"
  },
  // Discord geofence roles config
  "geofenceRoles": {
    // Determines whether assignable/unassignable geofence roles for donors of the server are enabled
    "enabled": false,
    // Determines whether access removed automatically removes assigned geofence roles (highly recommended)
    "autoRemove": true,
    // Assigning geofence city roles requires donor/supporter role
    "requiresDonorRole": true
  },
  // Automatic quest alarms purge from Discord channels based on timezones at midnight
  "questsPurge": {
    // Enables quest alarm messages purge from Discord channels
    "enabled": false,
    // Channels based on timezone
    "channels": {
      // Denver Timezone
      "America/Denver": [
        // Channel 1...
        000000000000000000,
        // Channel 2...
        000000000000000001
      ],
      // New York timezone
      "America/New_York": [
        // Channel 1...
        000000000000000000,
        // Channel 2...
        000000000000000001
      ]
    }
  },
  // Nest postings config
  "nests": {
    // Determines whether nest posting is enabled.
    "enabled": false,
    // Channel id to post nest postings to.
    "channelId": 0,
    // Minimum amount per hour to post nest posting.
    "minimumPerHour": 2
  },
  // Daily stats config
  "dailyStats": {
    // Shiny stats config
    "shiny": {
      // Determines whether to post shiny stats or not
      "enabled": false,
      // Clear messages before posting
      "clearMessages": false,
      // Channel ID for posting shiny stats
      "channelId": 0
    },
    // IV stats config
    "iv": {
      // Determines whether to post IV stats or not
      "enabled": false,
      // Clear messages before posting
      "clearMessages": false,
      // Channel ID for posting IV stats
      "channelId": 0
    }
  },
  // Icon style for postings from Discord server.
  "iconStyle": "Default"
}
```