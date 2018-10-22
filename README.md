#WebHook Manager#  

`PokeAlarm alternative.`

1.) Copy `config.example.json` to `config.json`.  
  a.) Create bot token. https://github.com/reactiflux/discord-irc/wiki/Creating-a-discord-bot-&-getting-a-token  
  b.) Input your bot token and config options.  
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
			"ignoreMissing": true //Ignore raids missing information.
		}
	},
	"geofence":"Geofences\\geofence1.txt", //Path to geofence file.
	"webhook":"<DISCORD_WEBHOOK_URL>" //Discord webhook url address.
}
```
4.) Create directory `Geofences` in root of executable file.  
5.) Create/copy geofence files to `Geofences` folder.  

*Note:* Geofence file format is the following:  
```
[GeofenceName]
34.00,-117.00
34.01,-117.01
34.02,-117.02
34.03,-117.03
```