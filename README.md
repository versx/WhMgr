# Brock Webhook Manager

### PokeAlarm alternative.
Works with [RealDeviceMap](https://github.com/123FLO321/RealDeviceMap)  

1.) Copy `config.example.json` to `config.json`.  
  a.) Create bot token.  
  b.) Input your bot token and config options. [Create bot token](https://github.com/reactiflux/discord-irc/wiki/Creating-a-discord-bot-&-getting-a-token)  
```
{
  //Discord bot token with user.
  "token": "<DISCORD_BOT_TOKEN>",
  
  //Discord server owner id.
  "ownerId": 000000000000,
  
  //Discord server donator role id.
  "supporterRoleId": 000000000000,
  
  //List of Discord server moderator role ids.
  "moderators": [000000000000],
  
  //Discord server guild id.
  "guildId": 000000000000,
  
  //Listening port to accept webhook data.
  "webhookPort": 8002,
  
  //Google maps key.
  "gmapsKey": "<GOOGLE_MAPS_KEY>",
  
  //Enable discord user subscriptions for custom notifications to Quests/Raids/Pokemon, if enabled database information is required below.
  "enableSubscriptions": false,
  
  //MySQL database connection string.
  "connectionString": "Uid=user;Password=password;Server=127.0.0.1;Port=3306;Database=brockdb",
  
  //City roles to filter by geofence.
  "cityRoles": [
  "City1",
  "City2"
  ],
  
  //Bot command prefix, if empty/null the bot's mention prefix is set as default.
  "commandPrefix": null
}
```
2.) Copy `alarms.example.json` to `alarms.json`.  
3.) Fill out the alarms file.  
```
{
  //Global switch for Pokemon notifications.
  "enablePokemon": false,
  
  //Global switch for Raid/Egg notifications.
  "enableRaids": false,
  
  //Global switch for Quest notifications.
  "enableQuests": false,
  "alarms": 
  [
    {
      //Alarm name.
      "name":"Alarm1",
      "filters":
      {
        "pokemon":
        {
          //Determines if pokemon alarms will be enabled.
          "enabled": true,
          
          //Pokemon to filter, if empty all will be reported.
          "pokemon": [280,337,374],
          
          //Minimum IV pokemon to report.
          "min_iv": 0,
          
          //Maximum IV pokemon to report.
          "max_iv": 100,
          
          //Pokemon filter type, either Include or Exclude.
          "type": "Include",
          
          //Ignore pokemon missing information.
          "ignoreMissing": true
        },
        "eggs":
        {
          //Determines if raid egg alarms will be enabled.
          "enabled": true,
          
          //Minimum egg level to report.
          "min_lvl": 1,
          
          //Maximum egg level to report.
          "max_lvl": 5
        },
        "raids":
        {
          //Determines if raid alarms will be enabled.
          "enabled": true,
          
          //Pokemon to filter, if empty all will be reported.
          "pokemon": [],
          
          //Raid filter type, either Include or Exclude.
          "type": "Include",
          
          //Ignore raids missing information.
          "ignoreMissing": true
        },
        "quests":
        {
          //Determines if quest alarms will be enabled.
          "enabled": true,
          
          //Filter quest rewards by keyword.
          "rewards": ["spinda", "stardust"],
          
          //Quest filter type, either Include or Exclude.
          "type": "Include"
        }
      },
      //Path to geofence file.
      "geofence":"geofence1.txt",
      
      //Discord webhook url address.
      "webhook":"<DISCORD_WEBHOOK_URL>"
    }
  ]
}
```
4.) Create directory `Geofences` in root directory of executable file.  
5.) Create/copy geofence files to `Geofences` folder.  

*Note:* Geofence file format is the following:  
```
[Geofence1]
34.00,-117.00
34.01,-117.01
34.02,-117.02
34.03,-117.03
[Geofence2]
33.00,-118.00
33.01,-118.01
33.02,-118.02
33.03,-118.03
```
6.)
Upload Discord emojis that are in the emojis folder.  
7.) Start WhMgr.exe as Administrator.  

## TODO:  
- Allow Pokemon id and name in Pokemon filter lists.  
- Finish Localization  
- Wiki  
- Support for dynamic text replacement for alarm text.  

## Examples:

Discord Pokemon Notifications:
![Pokemon Notifications](images/pkmn.png "Pokemon Notifications")

Discord Raid Notifications:
![Raid Notifications](images/raid.png "Raid Notifications")

Discord Raid Egg Notifications:
![Egg Notifications](images/egg.png "Egg Notifications")

Discord Quest Notifications:
![Quest Notifications](images/quests.png "Quest Notifications")