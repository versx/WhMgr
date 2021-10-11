[![Build](https://github.com/versx/WhMgr/workflows/.NET%20Core/badge.svg)](https://github.com/versx/WhMgr/actions)
[![Documentation Status](https://readthedocs.org/projects/whmgr/badge/?version=latest)](https://whmgr.rtfd.io)
[![GitHub Release](https://img.shields.io/github/release/versx/WhMgr.svg)](https://github.com/versx/WhMgr/releases/)
[![GitHub Contributors](https://img.shields.io/github/contributors/versx/WhMgr.svg)](https://github.com/versx/WhMgr/graphs/contributors/)
[![Discord](https://img.shields.io/discord/552003258000998401.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2)](https://discord.gg/zZ9h9Xa)  

# Welcome to Webhook Manager

**PokeAlarm, PoracleJS, WDR, Novabot, etc alternative.**  

Works with the following backends:  
- [RealDeviceMap](https://github.com/123FLO321/RealDeviceMap)  
- [Chuck](https://github.com/WatWowMap/Chuck)  
- [ChuckDeviceController](https://github.com/versx/ChuckDeviceController)  


### Description  
Developed in C#, runs on .NET 5.0 CLR. Cross platform compatibility, can run on Windows, macOS, and Linux operating systems.  

Sends Discord notifications based on pre-defined filters for Pokemon, raids, raid eggs, field research quests, Team Rocket invasions, Pokestop lures, gym team changes, and weather. Also supports Discord user's subscribing to Pokemon, PvP, raid, quest, Team Rocket invasion, and Pokestop lure notifications via direct messages.

### Features  
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

### Subscription Notification Filters  

- Pokemon  
  o Pokemon IDs  
  o Pokemon Forms  
  <s>- Pokemon Minimum CP</s>  
  o Pokemon Minimum IV Percentage  
  o Pokemon Minimum Level  
  o Pokemon Maximum Level  
  o List of Pokemon Attack/Defense/Stamina values  
  o Pokemon Gender  
  o Pokemon Size  
  o Custom Location Distance (meters)  
  o City  

- Player vs Player (PvP)  
  o Pokemon IDs  
  o Pokemon Forms  
  o PvP League  
  o Minimum Rank  
  o Minimum Stat Product Percentage  
  o Custom Location Distance (meters)  
  o City  

- Raids  
  o Raid Boss IDs  
  o Raid Boss Forms  
  o Is EX Eligible  
  o Custom Location Distance (meters)  
  o City  

- Gyms  
  o Gym Name  
  o Raid Boss IDs  
  o Minimum Raid Level  
  o Maximum Raid Level  
  o Is EX Eligible  
  o Custom Location Distance (meters)  

- Quests  
  o Pokestop Name  
  o Quest Reward Name  
  o Custom Location Distance (meters)  
  o City  

- Invasions  
  o Pokestop Name  
  o Invasion Grunt Type IDs  
  o Invasion Reward Pokemon IDs  
  o Custom Location Distance (meters)  
  o City  

- Lures  
  o Pokestop Name  
  o Pokestop Lure Type IDs  
  o Custom Location Distance (meters)  
  o City  

### Frameworks and Libraries
- .NET v5.0.212  
- DSharpPlus v4.1.0  
- DSharpPlus.CommandsNext v4.1.0  
- DSharpPlus.Interactivity v4.1.0  
- GeoTimeZone v4.1.0  
- Handlebars.Net v2.0.9  
- Handlebars.Net.Helpers v2.2.1  
- Microsoft.EntityFrameworkCore v5.0.10  
- Microsoft.EntityFrameworkCore.Design v5.0.10  
- Microsoft.NET.Test.Sdk v16.11.0  
- Microsoft.VisualStudio.Azure.Containers.Tools.Targets v1.11.1  
- NetTopologySuite v2.4.0  
- NetTopologySuite.Features v2.4.0  
- NUnit v3.13.2  
- NUnit3TestAdapter v4.0.0  
- POGOProtos.Core v2.55.0.1  
- Pomelo.EntityFrameworkCore.MySql v5.0.2  
- Swashbuckle.AspNetCore v6.2.2  
- System.Runtime.Caching v5.0.0  
- TimeZoneConverter v3.5.0  


**[Click here](install/getting-started.md) to get started!**  