# Alarms

Alarms are used to define what Pokemon, raids, eggs, field research quests, Team Rocket invasions, lures, gyms are sent to which channels.  

There is no limit to the amount of alarms you can add under the `alarms` property list.  

### Example
```js
{
    //Enable or disable Pokemon alarms globally
    "enablePokemon": true,
    //Enable or disable Raid alarms globally
    "enableRaids": true,
    //Enable or disable Quest alarms globally
    "enableQuests": true,
    //Enable or disable Pokestop alarms globally
    "enablePokestops": true,
    //Enable or disable Gym alarms globally
    "enableGyms": true, 
    //List of alarms
	"alarms":
	[
		{
            //Alarm name
            "name":"City1-Rare",
            //Alerts file location (used to structure how the message will look)
            "alerts": "default.json",
            //Alarm filters
            "filters":"all.json",
            //Geofence file name
            "geofence":"City1.txt",
            //Discord webhook url address
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
            //100% IV alarm for City1
            "name":"City1-100iv",
            //
            "alerts": "default.json",
            //
            "filters":"100iv.json",
            //
            "geofence":"City1.txt",
            //
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			"name":"City1-Raids",
			"alerts": "default.json",
			"filters":"raids.json",
			"geofence":"City1.txt",
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			"name":"City1-LegendaryRaids",
			"alerts": "default.json",
			"filters":"legendary_raids.json",
			"geofence":"City1.txt",
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			"name":"City1-ExRaids",
			"alerts": "default.json",
			"filters":"ex_raids.json",
			"geofence":"City1.txt",
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			"name": "City1-Quests",
			"alerts": "default.json",
			"filters": "quests.json",
			"geofence": "City1.txt",
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			"name": "City1-Lures",
			"alerts": "default.json",
			"filters": "lures.json",
			"geofence": "City1.txt",
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			"name": "City1-Invasions",
			"alerts": "default.json",
			"filters": "invasions.json",
			"geofence": "City1.txt",
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			"name": "City1-Gyms",
			"alerts": "default.json",
			"filters": "gyms.json",
			"geofence": "City1.txt",
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			"name":"City2-Rare",
			"alerts": "default.json",
			"filters":"all.json",
			"geofence":"City2.txt",
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			"name":"City2-100iv",
			"alerts": "default.json",
			"filters":"100iv.json",
			"geofence":"City2.txt",
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			"name":"City2-Raids",
			"alerts": "default.json",
			"filters":"raids.json",
			"geofence":"City2.txt",
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			"name":"City2-LegendaryRaids",
			"alerts": "default.json",
			"filters":"legendary_raids.json",
			"geofence":"City2.txt",
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			"name":"City2-ExRaids",
			"alerts": "default.json",
			"filters":"ex_raids.json",
			"geofence":"City2.txt",
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			"name": "City2-Quests",
			"alerts": "default.json",
			"filters": "quests.json",
			"geofence": "City2.txt",
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			"name": "City2-Lures",
			"alerts": "default.json",
			"filters": "lures.json",
			"geofence": "City2.txt",
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			"name": "City2-Invasions",
			"alerts": "default.json",
			"filters": "invasions.json",
			"geofence": "City2.txt",
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			"name": "City2-Gyms",
			"alerts": "default.json",
			"filters": "gyms.json",
			"geofence": "City2.txt",
			"webhook":"<DISCORD_WEBHOOK_URL>"
		},
		{
			"name":"Absol-Quests",
			"alerts": "default.json",
			"filters":"quests_absol.json",
			"geofence":"City2.txt",
			"webhook":"<DISCORD_WEBHOOK_URL>"
		}
	]
}
```