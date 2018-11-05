# Brock Webhook Manager

### PokeAlarm alternative.
Works with RealDeviceMap https://github.com/123FLO321/RealDeviceMap

1.) Copy `config.example.json` to `config.json`.  
  a.) Create bot token. https://github.com/reactiflux/discord-irc/wiki/Creating-a-discord-bot-&-getting-a-token  
  b.) Input your bot token and config options.  
```
{
  "token": "<DISCORD_BOT_TOKEN>", //Discord bot token with user.
  "enabled": true, //Controls whether alarm filtering is enabled.
  "ownerId": 000000000000, //Discord server's owner id.
  "supporterRoleId": 000000000000, //Discord server's donator role id.
  "moderators": [000000000000], //List of Discord server's moderator role ids.
  "guildId": 000000000000, //Discord server's guild id.
  "webhookPort": 8002,
  "gmapsKey": "<GOOGLE_MAPS_KEY>", //Google maps key.
  "connectionString": "", //RealDeviceMap database connection string.
  "cityRoles": [
	"City1",
	"City2"
  ],
  "commandPrefix": null //Bot command prefix, if empty/null the bot's mention prefix is set as default.
}
```
2.) Copy `alarms.example.json` to `alarms.json`.  
3.) Fill out the alarms file.  
```
{
	"name":"Alarm1", //Alarm name.
	"filters":
	{
		"pokemon":
		{
			"enabled": true, //Determines if pokemon alarms will be enabled.
			"pokemon": [280,337,374],
			"min_iv": 0, //Minimum IV pokemon to report.
			"max_iv": 100, //Maximum IV pokemon to report.
			"type": "Include", //Pokemon filter type, either Include or Exclude.
			"ignoreMissing": true //Ignore pokemon missing information.
		},
		"eggs":
		{
			"enabled": true, //Determines if raid egg alarms will be enabled.
			"min_lvl": 1, //Minimum egg level to report.
			"max_lvl": 5 //Maximum egg level to report.
		},
		"raids":
		{
			"enabled": true, //Determines if raid alarms will be enabled.
			"pokemon": [], //Pokemon to filter, if empty all will be reported.
			"type": "Include", //Raid filter type, either Include or Exclude.
			"ignoreMissing": true //Ignore raids missing information.
		},
		"quests":
		{
			"enabled": true, //Determines if quest alarms will be enabled.
			"rewards": ["spinda", "stardust"], //Filter quest rewards by keyword.
			"type": "Include" //Quest filter type, either Include or Exclude.
		}
	},
	"geofence":"geofence1.txt", //Path to geofence file.
	"webhook":"<DISCORD_WEBHOOK_URL>" //Discord webhook url address.
}
```
4.) Create directory `Geofences` in root directory of executable file.  
5.) Create/copy geofence files to `Geofences` folder.  

*Note:* Geofence file format is the following:  
```
[GeofenceName]
34.00,-117.00
34.01,-117.01
34.02,-117.02
34.03,-117.03
```