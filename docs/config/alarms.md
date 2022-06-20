# Alarms

Alarms are used to define what Pokemon, raids, eggs, field research quests, Team Rocket invasions, lures, gyms, and weather are sent to which channels.  

There is no limit to the amount of alarms you can add under the `alarms` property list, although adding hundreds could potentially affect performance.  

**Notes:** 
- Place your active alarms in your `alarms` folder
- Discord webhook permissions are based on the EVERYONE role permission, if you plan to use an external emoji server you *MUST* ensure the everyone role has the "use external emojis" permission on the destination channel.  Even if you have the channel locked to a donor type role, the everyone role still needs this permission enabled. Remember setting everyone role to allow external emoji's at server level but an explicit deny on a channel will prevent them from showing.


## Example
```json
{
    // Enable or disable Pokemon filters globally
    "enablePokemon": true,
    // Enable or disable Raid filters globally
    "enableRaids": true,
    // Enable or disable Quest filters globally
    "enableQuests": true,
    // Enable or disable Pokestop filters globally
    "enablePokestops": true,
    // Enable or disable Invasion filters globally
    "enableInvasions": true,
    // Enable or disable Gym filters globally
    "enableGyms": true, 
    // Enable or disable Weather filters globally
    "enableWeather": true,
    // List of alarms
	"alarms":
	[
		{
            // Alarm name
            "name":"City1-Rare",
            // Embeds file location (used to structure how the message will look)
            "embeds": "default.json",
            // Alarm filters
            "filters":"all.json",
            // Mentionable string that supports DTS  (!@ for user, @& for role)
            "description": "<!@324234324> <@&12331131> {{iv}} L{{lvl}} {{geofence}}",
            // Either the geofence file path (`geojson` or `ini` format) or the geofence name
            "geofences": ["geofence1.txt", "city1"],
            // Discord webhook url address
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
            // 100% IV alarm for City1
            "name":"City1-100iv",
            // Embeds file location (used to structure how the message will look)
            "embeds": "default.json",
            // Alarm filters
            "filters":"100iv.json",
            // Either the geofence file path (`geojson` or `ini` format) or the geofence name
            "geofences": ["geofence1.txt", "city1"],
            // Discord webhook url address
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			// Alarm name
			"name":"City1-Raids",
			// Embeds file location (used to structure how the message will look)
			"embeds": "default.json",
			// Alarm filters
			"filters":"raids.json",
			// Either the geofence file path (`geojson` or `ini` format) or the geofence name
			"geofences": ["geofence1.txt", "city1"],
			// Discord webhook url address
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			// Alarm name
			"name":"City1-LegendaryRaids",
			// Embeds file location (used to structure how the message will look)
			"embeds": "default.json",
			// Alarm filters
			"filters":"legendary_raids.json",
			// Either the geofence file path (`geojson` or `ini` format) or the geofence name
			"geofences": ["geofence1.txt", "city1"],
			// Discord webhook url address
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			// Alarm name
			"name":"City1-ExRaids",
			// Embeds file location (used to structure how the message will look)
			"embeds": "default.json",
			// Alarm filters
			"filters":"ex_raids.json",
			// Either the geofence file path (`geojson` or `ini` format) or the geofence name
			"geofences": ["geofence1.txt", "city1"],
			// Discord webhook url address
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			// Alarm name
			"name": "City1-Quests",
			// Embeds file location (used to structure how the message will look)
			"embeds": "default.json",
			// Alarm filters
			"filters": "quests.json",
			// Either the geofence file path (`geojson` or `ini` format) or the geofence name
			"geofences": ["geofence1.txt", "city1"],
			// Discord webhook url address
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			// Alarm name
			"name": "City1-Lures",
			// Embeds file location (used to structure how the message will look)
			"embeds": "default.json",
			// Alarm filters
			"filters": "lures.json",
			// Either the geofence file path (`geojson` or `ini` format) or the geofence name
			"geofences": ["geofence1.txt", "city1"],
			// Discord webhook url address
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			// Alarm name
			"name": "City1-Invasions",
			// Embeds file location (used to structure how the message will look)
			"embeds": "default.json",
			// Alarm filters
			"filters": "invasions.json",
			// Either the geofence file path (`geojson` or `ini` format) or the geofence name
			"geofences": ["geofence1.txt", "city1"],
			// Discord webhook url address
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			// Alarm name
			"name": "City1-Gyms",
			// Embeds file location (used to structure how the message will look)
			"embeds": "default.json",
			// Alarm filters
			"filters": "gyms.json",
			// Either the geofence file path (`geojson` or `ini` format) or the geofence name
			"geofences": ["geofence1.txt", "city1"],
			// Discord webhook url address
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			// Alarm name
			"name":"City2-Rare",
			// Embeds file location (used to structure how the message will look)
			"embeds": "default.json",
			// Alarm filters
			"filters":"all.json",
			// Either the geofence file path (`geojson` or `ini` format) or the geofence name
			"geofences": ["geojson1.json", "geofence2.txt", "cityName1"],
			// Discord webhook url address
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			// Alarm name
			"name":"City2-100iv",
			// Embeds file location (used to structure how the message will look)
			"embeds": "default.json",
			// Alarm filters
			"filters":"100iv.json",
			// Either the geofence file path (`geojson` or `ini` format) or the geofence name
			"geofences": ["geojson1.json", "geofence2.txt", "cityName1"],
			// Discord webhook url address
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			// Alarm name
			"name":"City2-Raids",
			// Embeds file location (used to structure how the message will look)
			"embeds": "default.json",
			// Alarm filters
			"filters":"raids.json",
			// Either the geofence file path (`geojson` or `ini` format) or the geofence name
			"geofences": ["geojson1.json", "geofence2.txt", "cityName1"],
			// Discord webhook url address
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			// Alarm name
			"name":"City2-LegendaryRaids",
			// Embeds file location (used to structure how the message will look)
			"embeds": "default.json",
			// Alarm filters
			"filters":"legendary_raids.json",
			// Either the geofence file path (`geojson` or `ini` format) or the geofence name
			"geofences": ["geojson1.json", "geofence2.txt", "cityName1"],
			// Discord webhook url address
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			// Alarm name
			"name":"City2-ExRaids",
			// Embeds file location (used to structure how the message will look)
			"embeds": "default.json",
			// Alarm filters
			"filters":"ex_raids.json",
			// Either the geofence file path (`geojson` or `ini` format) or the geofence name
			"geofences": ["geojson1.json", "geofence2.txt", "cityName1"],
			// Discord webhook url address
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			// Alarm name
			"name": "City2-Quests",
			// Embeds file location (used to structure how the message will look)
			"embeds": "default.json",
			// Alarm filters
			"filters": "quests.json",
			// Either the geofence file path (`geojson` or `ini` format) or the geofence name
			"geofences": ["geojson1.json", "geofence2.txt", "cityName1"],
			// Discord webhook url address
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			// Alarm name
			"name": "City2-Lures",
			// Embeds file location (used to structure how the message will look)
			"embeds": "default.json",
			// Alarm filters
			"filters": "lures.json",
			// Either the geofence file path (`geojson` or `ini` format) or the geofence name
			"geofences": ["geojson1.json", "geofence2.txt", "cityName1"],
			// Discord webhook url address
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			// Alarm name
			"name": "City2-Invasions",
			// Embeds file location (used to structure how the message will look)
			"embeds": "default.json",
			// Alarm filters
			"filters": "invasions.json",
			// Either the geofence file path (`geojson` or `ini` format) or the geofence name
			"geofences": ["geojson1.json", "geofence2.txt", "cityName1"],
			// Discord webhook url address
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			// Alarm name
			"name": "City2-Gyms",
			// Embeds file location (used to structure how the message will look)
			"embeds": "default.json",
			// Alarm filters
			"filters": "gyms.json",
			// Either the geofence file path (`geojson` or `ini` format) or the geofence name
			"geofences": ["geojson1.json", "geofence2.txt", "cityName1"],
			// Discord webhook url address
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			// Alarm name
			"name":"Absol-Quests",
			// Embeds file location (used to structure how the message will look)
			"embeds": "default.json",
			// Alarm filters
			"filters":"quests_absol.json",
			// Either the geofence file path (`geojson` or `ini` format) or the geofence name
			"geofences": ["geojson1.json", "geofence2.txt", "cityName1"],
			// Discord webhook url address
			"webhook":"<DISCORD_WEBHOOK_URL>"
		}
	]
}
```
