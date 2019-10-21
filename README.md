# Brock Webhook Manager

### PokeAlarm alternative.  
Works with [RealDeviceMap](https://github.com/123FLO321/RealDeviceMap)  


## Description:  
Sends Discord notifications based on pre-defined filters for Pokemon, raids, raid eggs, and field research quests. Also supports Discord user's subscribing to Pokemon, raid, or quest notifications via DM.


## Getting Started:  

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
  "donorRoleIds": [000000000000, 000000000001],
  
  //List of Discord server moderator role ids.
  "moderators": [000000000000],
  
  //Discord server guild id.
  "guildId": 000000000000,
  
  //Listening port to accept webhook data.
  "webhookPort": 8002,
  
  //Enable discord user subscriptions for custom notifications to Quests/Raids/Pokemon, if enabled database information is required below.
  "enableSubscriptions": false,
  
  //MySQL database connection string.
  "connectionString": "Uid=user;Password=password;Server=127.0.0.1;Port=3306;Database=brockdb",
  
  //MySQL database connection string for RealDeviceMap scanner.
  "scannerConnectionString": "Uid=user;Password=password;Server=127.0.0.1;Port=3306;Database=brockdb",
  
  //City roles to filter by geofence.
  "cityRoles": [
    "City1",
    "City2"
  ],
  
  //Assigning city roles require a donor role.
  "citiesRequireSupporterRole": false,
  
  //Bot command prefix, if empty/null the bot's mention prefix is set as default.
  "commandPrefix": null,
  
  //Channel IDs of quest channels to clear messages at midnight.
  "questChannelIds": [
    000000000000,
	000000000001
  ],
  
  //Shiny statistics
  "shinyStats": {
    //Enables or disables shiny statistics posting.
    "enabled": false,
	
	//Clear all old shiny statistic reports.
    "clearMessages": true,
	
	//Channel ID to post the shiny statistics to.
    "channelId": 000000000000
  },
  
  //Image URLs
  "urls": {
    //Pokemon images repository path.
    "pokemonImage": "https://example.com/pogo/monsters/{0:D3}_{1:D3}.png",
	
	//Raid egg images repository path.
	"eggImage": "https://example.com/pogo/eggs/{0}.png",
	
	//Field research quest images repository path.
	"questImage": "https://example.com/pogo/quests/{0}.png",
	
	//Static map images template.
	"staticMap": "https://example.com/staticmap.php?center={0},{1}&markers={0},{1},red-pushpin&zoom=14&size=300x175&maptype=mapnik",
  }
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
  
  //Global switch for Pokestop notifications.
  "enablePokestops": false,
  
  //Global switch for Gym notifications.
  "enableGyms": false,
  
  //List of alarms
  "alarms": 
  [
    {
      //Alarm name.
      "name":"Alarm1",
	  
      //Alerts file.
      "alerts":"default.json",
	  
      //Alarm filters.
      "filters":"default.json",
	  
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
[City1]
34.00,-117.00
34.01,-117.01
34.02,-117.02
34.03,-117.03
[City2]
33.00,-118.00
33.01,-118.01
33.02,-118.02
33.03,-118.03
```
6.)
Upload Discord emojis that are in the emojis folder.  
7.) Start WhMgr.exe as Administrator privileges.  

*Notes:  
- Upon starting, database tables will be automatically created if `enableSubscriptions` is set to `true`. Emoji icons are also created upon connecting to Discord.*  
- DM notifications can be sent to users based on:
    - Pokemon IV
    - Pokemon Level
    - Pokemon Attack/Defense/Stamina values
    - Pokemon Gender
    - Raid Boss
    - Raid City
    - Raid Distance
    - Gym Name
    - Quest Reward
    - Invasion Type  

## TODO:  
- Allow Pokemon id and name in Pokemon filter lists. 
- Pokemon form support.  
- Finish Localization.  
- Wiki.  
- Finish dynamic text replacement for alarm text.  
- ~~Raid lobby manager.~~ [RaidLobbyist](https://github.com/versx/RaidLobbyist)


## Examples:
Discord Pokemon Notifications:  
![Pokemon Notifications](images/pkmn.png "Pokemon Notifications")  

Discord Raid Notifications:  
![Raid Notifications](images/raid.png "Raid Notifications")  

Discord Raid Egg Notifications:  
![Egg Notifications](images/egg.png "Egg Notifications")  

Discord Quest Notifications:  
![Quest Notifications](images/quests.png "Quest Notifications")  