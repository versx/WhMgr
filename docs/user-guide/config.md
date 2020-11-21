# Configuration

At a minimum you'll want to make sure your have your webhook listening port set as well as one Discord server added to the `servers` property.

### Properties

| Key | Example | Description |  
|---|---|---|  
__**Main Properties**__  
| port | `8008` | Listening port used to receive incoming json webhooks. |  
| locale | `en` | Two letter country code used to translate bot messages. |  
| shortUrlApiUrl | `` | |  
| stripeApiKey | `rk_32uo3j2lkjlkj3l2kjdlk2j3ldj2` | Stripe API key used with `expire` command to let users check their subscription expiration date. |  
| eventPokemonIds | `[123,43,483]` | List of Pokemon ID(s) to treat as event Pokemon. Event Pokemon are not reported to any channels or DM unless 90% or higher IV. |  
| iconStyles | `{ "default": "icon_path", ... }` | List key value pairs demonstrating a list of icon styles to choose from. |  
| database.main | `{}` | Main database used to save subscriptions. |  
| database.scanner | `{}` | RDM scanner database used to retrieve pokestops table. |  
| database.nests | `{}` | PMSF nests database used for reporting nests. |  
__**Database**__  `database`  
| host | `127.0.0.1` | Hostname or IP address of database server. |  
| port | `3306` | Listening port for database server. |  
| username | `root` | Database username to use when authenticating. |  
| password | `password` | Database password to use when authenticating. |  
| database | `rdmdb` | Database name |  
__**Discord Server Specific**__ `servers`  
| commandPrefix | `!` | Prefix for all commands, leave blank to use bot mention string. |  
| guildId | `4032948092834` | Discord guild ID the bot will be connecting to. |  
| emojiGuildId | `3984729874298` | Discord guild ID to use emojis from. (Can be same as `guildId`) |  
| ownerId | `8184229834297` | Bot owner's unique Discord ID. |  
| donorRoleIds | `[00000001,00000002,...]` | List of donor/support role IDs to use with permissions. |  
| moderatorIds | `[09020021,09029302,...]` | List of Discord IDs for moderators. |  
| token | `lkj2l8sl98o9slil.o32oumjj3lkjlkA` | Bot Discord authentication token. |  
| alarms | `alarms-test.json` | File path to alarms file that'll be used with the Discord server. |  
| enableSubscriptions | `true` | Allow users to subscribe to specific Pokemon, Raids, Quests, and Invasions with their own pre-defined filters.|  
| enableCities | `true` | Enable the city roles used to differentiate between the different areas. |  
| cityRoles | `["City1","City2"]` | List of city role names users will be able to subscribe to. |  
| citiesRequireSupporterRole | `true` | If `true`, any city role assignment command will require the user have a donor/supporter role. |  
| pruneQuestChannels | `true` | If `true`, prune designated quest channels every day at midnight. |  
| questChannelIds | `[098309389,987398790,391878179]` | |  
| nestsChannelId | `1347092710` | |  
| shinyStats.enabled | `true` | If `true`, enable shiny stats posting. |  
| shinyStats.clearMessages | `false` | Clear previous shiny stat messages. |  
| shinyStats.channelId | `1347092710` | Channel ID to post shiny stats. |  
| iconStyle | `Default` | Icon style to use for Pokemon, Raid, Quest, and Invasion images. |  
| botChannelIds | `[098309389,987398790,391878179]` | Prevents the bot from executing commands outside of listed channels. |  
__**Image Urls**__ `urls`  
| pokemonImage | `https://cdn.com/mon/{0:D3}_{1:D3}.png` | Pokemon images repository path. |  
| eggImage | `https://cdn.com/eggs/{0}.png` | Raid egg images repository path. |  
| questImage | `https://cdn.com/quests/{0}.png` | Field research quest images repository path. |  
| staticMap | `http://tiles.com/{0}/{1}/15/300/175/1/png` | Static tile map images template. |  

### Example
```js
{
    // Http listening interface for raw webhook data.
    "host": "10.0.0.10",
    // Http listener port for raw webhook data.
    "port": 8008,
    // Locale language translation
    "locale": "en",
    // ShortURL API (yourls.org API, i.e. `https://domain.com/yourls-api.php?signature=XXXXXX`)
    "shortUrlApiUrl": null,
    // Stripe API key (Stripe production API key, i.e. rk_3824802934
    "stripeApiKey": ""
    // List of Discord servers to connect and post webhook messages to.
    "servers": {
        // Discord server #1 guild ID (replace `000000000000000001` with guild id of server)
        "000000000000000001": {
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
            // Moderator Discord role ID(s).
            "moderatorRoleIds": [
                000000000000000000
            ],
            // Discord bot token with user.
            "token": "<DISCORD_BOT_TOKEN>",
            // Alarms file path.
            "alarms": "alarms.json",
            // Enable custom direct message notification subscriptions.
            "enableSubscriptions": false,
            // Enable city role assignments.
            "enableCities": false,
            // City/geofence role(s)
            "cityRoles": [
                "City1",
                "City2"
            ],
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
            "status": ""
        },
        "000000000000000002": {
            "commandPrefix": ".",
            "emojiGuildId": 000000000000000001,
            "ownerId": 000000000000000000,
            "donorRoleIds": [
                000000000000000000
            ],
            "moderatorIds": [
                000000000000000000
            ],
            "token": "<DISCORD_BOT_TOKEN>",
            "alarms": "alarms2.json",
            "enableSubscriptions": false,
            "enableCities": false,
            "cityRoles": [
                "City3",
                "City4"
            ],
            "citiesRequireSupporterRole": true,
            "pruneQuestChannels": true,
            "questChannelIds": [
                000000000000000000
            ],
            "nestsChannelId": 000000000000000000,
            "nestsMinimumPerHour": 2,
            "shinyStats": {
                "enabled": true,
                "clearMessages": false,
                "channelId": 000000000000000000
            },
            "iconStyle": "Default",
            "botChannelIds": [
                000000000000000000
            ],
            "status": null
        }
    },
    // Database configuration
    "database": {
        // Database to store notification subscriptions.
        "main": {
            // Database hostname or IP address.
            "host": "127.0.0.1",
            // Database connection port.
            "port": 3306,
            // Database user account name.
            "username": "root",
            // Database user account password.
            "password": "password",
            // Brock database name.
            "database": "brock3"
        },
        // Scanner database config
        "scanner": {
            // Database hostname or IP address.
            "host": "127.0.0.1",
            // Database connection port.
            "port": 3306,
            // Database user account name.
            "username": "root",
            // Database user account password.
            "password": "password",
            // RDM database name.
            "database": "rdmdb"
        },
        // PMSF Nests database config
        "nests": {
            // Database hostname or IP address.
            "host": "127.0.0.1",
            // Database connection port.
            "port": 3306,
            // Database user account name.
            "username": "root",
            // Database user account password.
            "password": "password",
            // PMSF nests database name.
            "database": "manualdb"
        }
    },
    // List of Pokemon IDs to treat as event and restrict postings and subscriptions to 90% IV or higher. (Filled in automatically with `event set` command)  
    "eventPokemonIds": [
        129,
        456,
        320
    ],
	// Minimum IV value for an event Pokemon to have to meet in order to post via Discord channel alarm or direct message subscription.
    "eventMinimumIV": "90",
    // Image URL config
    "urls": {
        // Static tile map images template.
        "staticMap": "http://tiles.example.com:8080/static/klokantech-basic/{0}/{1}/15/300/175/1/png",
        // Scanner map DTS option for embeds as `scanmaps_url`  
        "scannerMap": "http://map.example.com/@/{0}/{1}/15"
    },
    // Available icon styles
    "iconStyles": {
        "Default": "https://raw.githubusercontent.com/versx/WhMgr-Assets/master/original/",
        "Shuffle": "https://raw.githubusercontent.com/versx/WhMgr-Assets/master/shuffle/",
        "Home": "https://raw.githubusercontent.com/nileplumb/PkmnHomeIcons/master/pmsf_OS_128/"
    },
    // Custom static map template files for each alarm type
    "staticMaps": {
        // Static map template for Pokemon
        "pokemon": "pokemon.example.json",
        // Static map template for Raids and Eggs
        "raids": "raids.example.json",
        // Static map template for field research quests
        "quests": "quests.example.json",
        // Static map template for Team Rocket invasions
        "invasions": "invasions.example.json",
        // Static map template for Pokestop lures
        "lures": "lures.example.json",
        // Static map template for Gym team control changes
        "gyms": "gyms.example.json",
        // Static map template for nest postings
        "nests": "nests.example.json",
        // Static map template for weather changes
        "weather": "weather.example.json"
    },
    // Get text message alerts with Twilio.com
    "twilio": {
        // Determines if text message alerts are enabled
        "enabled": false,
        // Twilio account SID (Get via Twilio dashboard)
        "accountSid": "",
        // Twilio account auth token (Get via Twilio dashboard)
        "authToken": "",
        // Twilio phone number that will be sending the text message alert
        "from": "",
        // List of Discord user ids that can receive text message alerts
        "userIds": [],
        // List of acceptable Pokemon to receive text message alerts for
        "pokemonIds": [201, 480, 481, 482, 443, 444, 445, 633, 634, 635, 610, 611, 612],
        // Minimum acceptable IV value for Pokemon if not ultra rare (Unown, Lake Trio)
        "minIV": 100
    },
    // Needed if you want to use the address lookup DTS
    "gmapsKey": "",
    // Minimum despawn time in minutes a Pokemon must have in order to send the alarm (default: 5 minutes)
    "despawnTimeMinimumMinutes": 5,
    // Log webhook payloads to a file for debugging
    "debug": false,
    // Only show logs with higher or equal priority levels (Trace, Debug, Info, Warning, Error, Fatal, None)
    "logLevel": "Trace"
}
```
3.) Edit `alarms.json` either open in Notepad/++ or `vi alarms.json`.  
4.) Fill out the alarms file.  
```js
{
    //Global switch for Pokemon notifications.
    "enablePokemon": false,
  
    //Global switch for Raid/Egg notifications.
    "enableRaids": false,
  
    //Global switch for Quest notifications.
    "enableQuests": false,
  
    //Global switch for Pokestop notifications.
    "enablePokestops": false,
  
    //Global switch for Gym notifications.
    "enableGyms": false,
    
    //Global switch for Weather notifications.
    "enableWeather": false,
  
    //List of alarms
    "alarms": [{
        //Alarm name.
        "name":"Alarm1",
        
        //DTS compatible mention description.      
        "description":"<!@324234324> <iv> L<lvl> <geofence>",
      
        //Alerts file.
        "alerts":"default.json",
      
        //Alarm filters.
        "filters":"default.json",
      
        //Path to geofence file.
        "geofence":"geofence1.txt",
    
        //Discord webhook url address.
        "webhook":"<DISCORD_WEBHOOK_URL>"
    },{
        //Alarm name.
        "name":"Alarm2",
        
        //DTS compatible mention description.      
        "description":"",
      
        //Alerts file.
        "alerts":"default.json",
      
        //Alarm filters.
        "filters":"100iv.json",
      
        //Path to geofence file.
        "geofence":"geofence1.txt",
      
        //Discord webhook url address.
        "webhook":"<DISCORD_WEBHOOK_URL>"
    }]
}
```