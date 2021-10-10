# Welcome to Webhook Manager

Works with the following backends:  

- [RealDeviceMap](https://github.com/123FLO321/RealDeviceMap)  
- [Chuck](https://github.com/WatWowMap/Chuck)  
- [ChuckDeviceController](https://github.com/versx/ChuckDeviceController)  


Made in C#, runs on .NET Core CLR. Cross platform compatibility, can run on Windows, macOS, and Linux operating systems.  
Sends Discord notifications based on pre-defined filters for Pokemon, raids, raid eggs, field research quests, Team Rocket invasions, Pokestop lures, gym team changes, and weather. Also supports Discord user's subscribing to Pokemon, PvP, raid, quest, Team Rocket invasion, and Pokestop lure notifications via direct messages.

## Features  
- Supports multiple Discord servers.  
- Discord channel alarm reports for Pokemon, raids, eggs, quests, lures, invasions, gym team changes, and weather.  
- Per user custom Discord notifications for Pokemon, raids, quests, invasions, lures, and gyms.  
- User interface to configure Discord notifications with ease. [WhMgr-UI](https://github.com/versx/WhMgr-UI)  
- Subscription notifications based on pre-defined distance and geofence areas.  
- Customizable alert messages with dynamic text replacement/substitution.  
- Support for multiple cities/areas using geofences per server.  
- Daily shiny and IV stats reporting.  
- Automatic quest message purge at midnight.  
- Support for Donors/Supporters only custom notifications.  
- Pokemon, PvP, and Raid subscription notifications based on specific forms.  
- Custom prefix support as well as mentionable bot user string for commands.  
- Raid subscription notifications for specific gyms.  
- Twilio text message alerts for ultra rare Pokemon.  
- Custom image support for Discord alarm reports.  
- Custom icon style selection for Discord user notifications.  
- External emoji server support.  
- Custom static map format support, including pokestop and gym marker placements.  
- Support for language translation.  
- Multi threaded, low processing consumption.  
- Rate limit backlog queue.
- [UIcons](https://github.com/uicons/uicons) standard image support.
- Lots more...  

## Direct Message Notification Filters  
- Pokemon  
  - Pokemon IDs  
  - Pokemon Forms  
  ~~- Pokemon Minimum CP~~  
  - Pokemon Minimum IV Percentage  
  - Pokemon Minimum Level  
  - Pokemon Maximum Level  
  - List of Pokemon Attack/Defense/Stamina values  
  - Pokemon Gender  
  - Pokemon Size  
  - Custom Location Distance (meters)  
  - City  
- Player vs Player (PvP)  
  - Pokemon IDs  
  - Pokemon Forms  
  - PvP League  
  - Minimum Rank  
  - Minimum Stat Product Percentage  
  - Custom Location Distance (meters)  
  - City  
- Raids  
  - Raid Boss IDs  
  - Raid Boss Forms  
  - Is EX Eligible  
  - Custom Location Distance (meters)  
  - City  
- Gyms  
  - Gym Name  
  - Raid Boss IDs  
  - Minimum Raid Level  
  - Maximum Raid Level  
  - Is EX Eligible  
  - Custom Location Distance (meters)  
- Quests  
  - Pokestop Name  
  - Quest Reward Name  
  - Custom Location Distance (meters)  
  - City  
- Invasions  
  - Pokestop Name  
  - Invasion Grunt Type IDs  
  - Invasion Reward Pokemon IDs  
  - Custom Location Distance (meters)  
  - City  
- Lures  
  - Pokestop Name  
  - Pokestop Lure Type IDs  
  - Custom Location Distance (meters)  
  - City  

## Frameworks and Libraries
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


**[Click here](config/config.md) to get started!**  