# Configuration

At a minimum you'll want to make sure your have your webhook listening port set as well as one Discord server added to the `servers` property.

## Main Properties  
| Key | Example | Description |  
|---|---|---|  
| host | `10.0.0.2` | Listening interface to receive webhook data. |  
| port | `8008` | Listening port used to receive incoming json webhooks. |  
| locale | `en` | Two letter country code used to translate bot messages. |  
| shortUrlApiUrl | `http://site.com/api.php?signature=` | Yourls.org URL shortener endpoint |  
| stripeApiKey | `rk_32uo3j2lkjlkj3l2kjdlk2j3ldj2` | Stripe API key used with `expire` command to let users check their subscription expiration date. |  
| eventPokemonIds | `[123,43,483]` | List of Pokemon ID(s) to treat as event Pokemon. Event Pokemon are not reported to any channels or DM unless 90% or higher IV. |  
| iconStyles | `{ "default": "icon_path", ... }` | List key value pairs demonstrating a list of icon styles to choose from. |  
| database.main | `{}` | Main database used to save subscriptions. |  
| database.scanner | `{}` | RDM scanner database used to retrieve pokestops table. |  
| database.nests | `{}` | PMSF nests database used for reporting nests. |  
| gmapsKey | `testkeyljdsflkjsdflkj=` | Needed if you want to use the address lookup DTS. |  
| despawnTimeMinimumMinutes | `5` | Minimum despawn time in minutes a Pokemon must have in order to send the alarm (default: 5) |  
| reloadSubscriptionChangesMinutes | `1` | Reload subscriptions every minute to sync with WhMgr-UI changes (default: 1) |  
| maxNotificationsPerMinute | `10` | Maximum amount of notifications a user can receive per minute before being rate limited |  
| debug | `false` | Log webhook payloads to a file for debugging (do not enable unless you're having issues receiving data |  
| logLevel | `Info` | Only show logs with higher or equal priority levels (Trace, Debug, Info, Warning, Error, Fatal, None) |  

## Discord Server Specific `servers`  
| Key | Example | Description |  
|---|---|---|  
| commandPrefix | `!` | Prefix for all commands, leave blank to use bot mention string. |  
| guildId | `4032948092834` | Discord guild ID the bot will be connecting to. |  
| emojiGuildId | `3984729874298` | Discord guild ID to use emojis from. (Can be same as `guildId`) |  
| ownerId | `8184229834297` | Bot owner's unique Discord ID. |  
| donorRoleIds | `[00000001,00000002,...]` | List of donor/support role IDs to use with permissions. |  
| moderatorRoleIds | `[09020021,09029302,...]` | List of Discord role IDs for moderators. |  
| token | `lkj2l8sl98o9slil.o32oumjj3lkjlkA` | Bot Discord authentication token. |  
| alarms | `alarms-test.json` | File path to alarms file that'll be used with the Discord server. |  
| dmEmbedsFile | `embeds-dm.json` | File path to embeds file that'll be used for DMM subscription notifications. |  
| enableSubscriptions | `true` | Allow users to subscribe to specific Pokemon, Raids, Quests, and Invasions with their own pre-defined filters.|  
| enableCities | `true` | Enable the city roles used to differentiate between the different areas. |  
| cityRoles | `["City1","City2"]` | List of city role names users will be able to subscribe to. |  
| citiesRequireSupporterRole | `true` | If `true`, any city role assignment command will require the user have a donor/supporter role. |  
| pruneQuestChannels | `true` | If `true`, prune designated quest channels every day at midnight. |  
| questChannelIds | `[098309389,987398790,...]` | |  
| nestsChannelId | `1347092710` | |  
| shinyStats.enabled | `true` | If `true`, enable shiny stats posting. |  
| shinyStats.clearMessages | `false` | Clear previous shiny stat messages. |  
| shinyStats.channelId | `1347092710` | Channel ID to post shiny stats. |  
| iconStyle | `Default` | Icon style to use for Pokemon, Raid, Quest, and Invasion images. |  
| botChannelIds | `[098309389,987398790,...]` | Prevents the bot from executing commands outside of listed channels. |  
| status | `Finding Pokemon...` | Custom Discord bot status, leave blank for bot version string |  

## Twilio `twilio`  
| Key | Example | Description |  
|---|---|---|  
| enabled | `false` | Determines if text message alerts are enabled |  
| accountSid | `ACb9ef2a14fa64...` | Twilio account SID (Get via Twilio dashboard) |  
| authToken | `19c2f1c032962f...` | Twilio account auth token (Get via Twilio dashboard) |  
| from | `8181234567` | Twilio phone number that will be sending the text message alert |  
| userIds | `[092830498234,80928340822]` | List of Discord user ids that can receive text message alerts |  
| pokemonIds | `[201,480,481,482,443,633,610]` | List of acceptable Pokemon to receive text message alerts for |  
| minIV | `100` | Minimum acceptable IV value for Pokemon if not ultra rare (Unown, Lake Trio) |  

## Example
```json
{
  // Http listening interface for raw webhook data, use "*" to listen on all interfaces.
  "host": "*",
  // Http listener port for raw webhook data.
  "port": 8008,
  // Locale language translation
  "locale": "en",
  // ShortURL API (yourls.org API, i.e. `https://domain.com/yourls-api.php?signature=XXXXXX`)
  "shortUrlApiUrl": "",
  // Stripe API key (Stripe production API key, i.e. rk_3824802934
  "stripeApiKey": "",
  // Maximum Pokemon ID of available Pokemon.  
  "maxPokemonId": 898,
  // List of Discord servers to connect and post webhook messages to.
  "servers": {
    // Discord server #1 guild ID (replace `000000000000000001` with guild id of server)
    "000000000000000123": "discord1.example.json",
    // 2nd Discord server section (if applicable)
    "000000000000000456": "discord2.example.json"
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
      "database": "brockdb"
    },
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
    "nests": {
      // Database hostname or IP address.
      "host": "127.0.0.1",
      // Database connection port.
      "port": 3306,
      // Database user account name.
      "username": "root",
      // Database user account password.
      "password": "password",
      // PMSF manual nests database name.
      "database": "manualdb"
    }
  },
  // List of Pokemon IDs to treat as event and restrict postings and subscriptions to 90% IV or higher. (Filled in automatically with `event set` command)  
  "eventPokemonIds": [],
  // Minimum IV value for an event Pokemon to have to meet in order to post via Discord channel alarm or direct message subscription.
  "eventMinimumIV": 90,
  // Image URL config
  "urls": {
    // Scanner map url DTS option for embeds as `scanmaps_url`  
    "scannerMap": "http://map.example.com/@/{0}/{1}/15"
  },
  // Available icon styles
  "iconStyles": {
    // Default icon style
    "Default": {
      // Base icon type object to apply to all other icon types
      "Base": {
        // Icon type display name
        "name": "Default",
        // Icon type url path
        "path": "https://raw.githubusercontent.com/WatWowMap/wwm-uicons/main/"
      },
      // Pokemon icon type object to modify
      "Pokemon": {
        // Icon type display name
        "name": "Default_Pokemon",
        // Icon type url path
        "path": "https://raw.githubusercontent.com/nileplumb/PkmnShuffleMap/master/UICONS/pokemon/"
      }
      /*
      "Raid",
      "Egg",
      "Gym",
      "Pokestop",
      "Reward",
      "Invasion",
      "Type",
      "Nest",
      "Team",
      "Weather",
      "Misc",
      */
    },
    // Pokemon Home Icons
    "Home": {
      // Base icon type object to apply to all other icon types
      "Base": {
        // Icon type display name
        "name": "Home",
        // Icon type url path
        "path": "https://raw.githubusercontent.com/nileplumb/PkmnHomeIcons/master/UICONS_OS/"
      }
    },
    // Pokemon Shuffle Icons
    "Shuffle": {
      // Base icon type object to apply to all other icon types
      "Base": {
        // Icon type display name
        "name": "Shuffle",
        // Icon type url path
        "path": "https://raw.githubusercontent.com/nileplumb/PkmnShuffleMap/master/ICONS_STANDARD/"
      }
    },
    // Pokemon Go Application Icons
    "Pokemon Go": {
      // Base icon type object to apply to all other icon types
      "Base": {
        // Icon type display name
        "name": "Pokemon Go",
        // Icon type url path
        "path": "https://raw.githubusercontent.com/whitewillem/PogoAssets/resized/icons_large-uicons"
      }
    },
    // PokeDave Pokemon Shuffle Icons
    "PokeDave Shuffle": {
      // Base icon type object to apply to all other icon types
      "Base": {
        // Icon type display name
        "name": "PokeDave Shuffle",
        // Icon type url path
        "path": "https://raw.githubusercontent.com/jepke/pokedave_shuffle_icons_-PMSF-/master/UICONS/"
      }
    },
    // PMSF Icons
    "PMSF": {
      // Base icon type object to apply to all other icon types
      "Base": {
        // Icon type display name
        "name": "PMSF",
        // Icon type url path
        "path": "https://raw.githubusercontent.com/pmsf/PMSF/develop/static/sprites/"
      }
    }
  },
  // Custom static map template files for each alarm type
  "staticMaps": {
    // Static map template for Pokemon
    "pokemon": {
      // Static map url template for pokemon
      "url": "http://tiles.example.com:8080/staticmap/{{template_name}}?lat={{lat}}&lon={{lon}}&url2={{url2}}",
      // Static map template file name without extension
      "template": "pokemon.example",
      // Include nearby gyms in static map image  
      "includeGyms": false,
      // Include nearby pokestops in static map image
      "includePokestops": false
    },
    // Static map template for Raids and Eggs
    "raids": {
      // Static map url template for raids
      "url": "http://tiles.example.com:8080/staticmap/{{template_name}}?lat={{lat}}&lon={{lon}}&url2={{url2}}&team_id={{team_id}}",
      // Static map template file name without extension
      "template": "raids.example",
      // Include nearby gyms in static map image  
      "includeGyms": false,
      // Include nearby pokestops in static map image
      "includePokestops": false
    },
    // Static map template for Gym team control changes
    "gyms": {
      // Static map url template for gyms
      "url": "http://tiles.example.com:8080/staticmap/{{template_name}}?lat={{lat}}&lon={{lon}}&url2={{url2}}&team_id={{team_id}}",
      // Static map template file name without extension
      "template": "gyms.example",
      // Include nearby gyms in static map image  
      "includeGyms": false,
      // Include nearby pokestops in static map image
      "includePokestops": false
    },
    // Static map template for field research quests
    "quests": {
      // Static map url template for quests
      "url": "http://tiles.example.com:8080/staticmap/{{template_name}}?lat={{lat}}&lon={{lon}}&url2={{url2}}",
      // Static map template file name without extension
      "template": "quests.example",
      // Include nearby gyms in static map image  
      "includeGyms": false,
      // Include nearby pokestops in static map image
      "includePokestops": false
    },
    // Static map template for Team Rocket invasions
    "invasions": {
      // Static map url template for invasions
      "url": "http://tiles.example.com:8080/staticmap/{{template_name}}?lat={{lat}}&lon={{lon}}&url2={{url2}}",
      // Static map template file name without extension
      "template": "invasions.example",
      // Include nearby gyms in static map image  
      "includeGyms": false,
      // Include nearby pokestops in static map image
      "includePokestops": false
    },
    // Static map template for Pokestop lures
    "lures": {
      // Static map url template for lures
      "url": "http://tiles.example.com:8080/staticmap/{{template_name}}?lat={{lat}}&lon={{lon}}&url2={{url2}}",
      // Static map template file name without extension
      "template": "lures.example",
      // Include nearby gyms in static map image  
      "includeGyms": false,
      // Include nearby pokestops in static map image
      "includePokestops": false
    },
    // Static map template for weather changes
    "weather": {
      // Static map url template for weather
      "url": "http://tiles.example.com:8080/staticmap/{{template_name}}?lat={{lat}}&lon={{lon}}&url2={{url2}}&polygon={{polygon}}",
      // Static map template file name without extension
      "template": "weather.example",
      // Include nearby gyms in static map image  
      "includeGyms": false,
      // Include nearby pokestops in static map image
      "includePokestops": false
    },
    // Static map template for nest postings
    "nests": {
      // Static map url template for nests
      "url": "http://tiles.example.com:8080/staticmap/{{template_name}}?lat={{lat}}&lon={{lon}}&url2={{url2}}&polygon={{polygon}}",
      // Static map template file name without extension
      "template": "nests.example",
      // Include nearby gyms in static map image  
      "includeGyms": false,
      // Include nearby pokestops in static map image
      "includePokestops": false
    }
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
    // List of Discord roles that can receive text message alerts
    "roleIds": [],
    // List of acceptable Pokemon to receive text message alerts for
    "pokemonIds": [201, 480, 481, 482, 443, 444, 445, 633, 634, 635, 610, 611, 612],
    // Minimum acceptable IV value for Pokemon if not ultra rare (Unown, Lake Trio)
    "minIV": 100
  },
  "reverseGeocoding": {
    // Reverse geocoding provider
    "provider": "osm", // osm/gmaps
    // Cache reverse geocoding responses to disk to reduce request count
    "cacheToDisk": true,
    // Google Maps reverse geocoding
    "gmaps": {
      // Google maps key for reverse geocoding  
      "key": "",
      // Google maps template schema for embeds  
      "schema": "{{Results.[0].FormattedAddress}}"
    },
    // OpenStreetMaps Nominatim reverse geocoding  
    "nominatim": {
      // OSM Nominatim endpoint
      "endpoint": "",
      // OSM Nominatim template schema for embeds
      "schema": "{{Address.Road}} {{Address.State}} {{Address.Postcode}} {{Address.Country}}"
    }
  },
  // Minimum despawn time in minutes a Pokemon must have in order to send the alarm (default: 5 minutes)
  "despawnTimeMinimumMinutes": 5,
  // Reload subscriptions every minute to sync with WhMgr-UI changes  
  "reloadSubscriptionChangesMinutes": 1,
  // Check for duplicate webhooks
  "checkForDuplicates": true,
  // Log webhook payloads to a file for debugging (do not enable unless you're having issues receiving data
  "debug": false,
  /*
   * Only show logs with higher or equal priority levels:
   * Trace: 0
   * Debug: 1
   * Info: 2
   * Warning: 3
   * Error: 4
   * Critical: 5
   * None: 6
  */
  "logLevel": 0
}
```