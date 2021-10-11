[![Build](https://github.com/versx/WhMgr/workflows/.NET%20Core/badge.svg)](https://github.com/versx/WhMgr/actions)
[![Documentation Status](https://readthedocs.org/projects/whmgr/badge/?version=latest)](https://whmgr.rtfd.io)
[![GitHub Release](https://img.shields.io/github/release/versx/WhMgr.svg)](https://github.com/versx/WhMgr/releases/)
[![GitHub Contributors](https://img.shields.io/github/contributors/versx/WhMgr.svg)](https://github.com/versx/WhMgr/graphs/contributors/)
[![Discord](https://img.shields.io/discord/552003258000998401.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2)](https://discord.gg/zZ9h9Xa)  

# Webhook Manager  

**PokeAlarm, PoracleJS, WDR, Novabot, etc alternative.**  

Works with the following backends:  
- [RealDeviceMap](https://github.com/123FLO321/RealDeviceMap)  
- [Chuck](https://github.com/WatWowMap/Chuck)  
- [ChuckDeviceController](https://github.com/versx/ChuckDeviceController)  


## Description  
Developed in C#, runs on .NET 5.0 CLR. Cross platform compatibility, can run on Windows, macOS, and Linux operating systems.  

Sends Discord notifications based on pre-defined filters for Pokemon, raids, raid eggs, field research quests, Team Rocket invasions, Pokestop lures, gym team changes, and weather. Also supports Discord user's subscribing to Pokemon, PvP, raid, quest, Team Rocket invasion, and Pokestop lure notifications via direct messages.

## Features  
- Supports multiple Discord servers.  
- Discord channel alarm reports for Pokemon, raids, eggs, quests, lures, invasions, gym team changes, and weather.  
- Per user custom Discord notifications for Pokemon, raids, quests, invasions, lures, and gyms.  
- User interface to configure custom Discord subscription notifications with ease. [WhMgr-UI](https://github.com/versx/WhMgr-UI)  
- Subscription notifications based on pre-defined distance and geofence areas.  
- Customizable alert messages with dynamic text replacement/substitution.  
- Support for multiple cities/areas using geofences per server.  
- Daily shiny and IV stats reporting.  
- Automatic quest message purge at midnight.  
- Support for Donors/Supporters only custom notifications.  
- Pokemon, PvP, and Raid subscription notifications based on specific forms or costumes.  
- Custom prefix support as well as mentionable bot user string for commands.  
- Raid subscription notifications for specific gyms.  
- Twilio text message alerts for ultra rare Pokemon.  
- Custom image support for Discord alarm reports.  
- Custom icon style selection for Discord user notifications.  
- External emoji server support.  
- Custom static map format support, including pokestop and gym marker placements.  
- Support for language translation per instance (per server planned).  
- Multi threaded, low processing consumption.  
- Rate limit backlog queue.
- [UIcons](https://github.com/uicons/uicons) standard image support.
- Lots more...  


## [Documentation](https://whmgr.rtfd.io/)  

### [Getting Started Guide](https://whmgr.readthedocs.io/en/latest/install/getting-started)  
<hr>  

## Previews  
*All examples are completely customizable using Dynamic Text Replacement/Substitution*  

__Pokemon Notifications__  
![Pokemon Notifications](images/pkmn.png "Pokemon Notifications")  

__Pokemon PVP Notifications__  
![Pokemon Notifications](images/pvp.png "Pokemon PVP Notifications")  

__Raid Boss Notifications__  
![Raid Boss Notifications](images/raids.png "Raid Boss Notifications")  

__Raid Egg Notifications__  
![Egg Notifications](images/eggs.png "Egg Notifications")  

__Quest Notifications__  
![Quest Notifications](images/quests.png "Quest Notifications")  

__Lure Notifications__  
![Lure Notifications](images/lure.png "Lure Notifications")  

__Lure (Glacial) Notifications__  
![Lure (Glacial) Notifications](images/lure_glacial.png "Lure (Glacial) Notifications")  

__Lure (Mossy) Notifications__  
![Lure (Mossy) Notifications](images/lure_mossy.png "Lure (Mossy) Notifications")  

__Lure (Magnetic) Notifications__  
![Lure (Magnetic) Notifications](images/lure_magnetic.png "Lure (Magnetic) Notifications")  

__Gym Team Takeover Notifications__  
![Gym Team Takeover Notifications](images/gyms.png "Gym Team Takeover Notifications")  

__Team Rocket Invasion Notifications__  
![Team Rocket Invasion Notifications](images/invasions.png "Team Rocket Invasion Notifications")  

__Weather Notifications__  
![Weather Notifications](images/weather.png "Weather Notifications")  


## Credits  
[versx](https://github.com/versx) - Developer  
[PokeAlarm](https://github.com/PokeAlarm/PokeAlarm) - Dynamic Text Substitution idea  
[WDR](https://github.com/PartTimeJS/WDR) - masterfile.json file  
[Contributors](https://github.com/versx/WhMgr/contributors)  