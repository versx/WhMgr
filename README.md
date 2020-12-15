[![Build](https://github.com/versx/WhMgr/workflows/.NET%20Core/badge.svg)](https://github.com/versx/WhMgr/actions)
[![Documentation Status](https://readthedocs.org/projects/whmgr/badge/?version=latest)](https://whmgr.rtfd.io)
[![GitHub Release](https://img.shields.io/github/release/versx/WhMgr.svg)](https://github.com/versx/WhMgr/releases/)
[![GitHub Contributors](https://img.shields.io/github/contributors/versx/WhMgr.svg)](https://github.com/versx/WhMgr/graphs/contributors/)
[![Discord](https://img.shields.io/discord/552003258000998401.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2)](https://discord.gg/zZ9h9Xa)  
# Webhook Manager v4  

### PokeAlarm, PoracleJS, WDR, Novabot, etc alternative.  
Works with [RealDeviceMap](https://github.com/123FLO321/RealDeviceMap)  
Works with [Chuck](https://github.com/WatWowMap/Chuck)  


## Description:  
Sends Discord notifications based on pre-defined filters for Pokemon, raids, raid eggs, field research quests, Team Rocket invasions, gym team changes, and weather. Also supports Discord user's subscribing to Pokemon, raid, quest, Team Rocket invasion, and Pokestop lure notifications via direct messages.


## Features:  
- Supports multiple Discord servers.  
- Discord channel alarm reports for Pokemon, raids, eggs, quests, lures, invasions, gym team changes, and weather.  
- Per user custom Discord notifications for Pokemon, raids, quests, invasions, and lures.  
- User interface to configure Discord notifications with ease (as well as Discord commands). (https://github.com/versx/WhMgr-UI)  
- Subscription notifications based on pre-defined distance.  
- Customizable alert messages with dynamic text replacement.  
- Support for multiple cities/areas using geofences per server.  
- Daily shiny stats reporting.  
- Automatic quest message purge at midnight.  
- Support for Donors/Supporters only notifications.  
- Direct messages of Pokemon notifications based on city roles assigned.  
- Pokemon and Raid subscription notifications based on specific forms.  
- Custom prefix support as well as mentionable user support for commands.  
- Raid subscription notifications for specific gyms.  
- Twilio text message alerts for ultra rare Pokemon.  
- Custom image support for Discord alarm reports.  
- Custom icon style selection for Discord user notifications.  
- External emoji server support.  
- Custom static map format support.  
- Support for language translation.  
- Multi threaded, low processing consumption.  
- [I.C.O.N.S.](https://github.com/Mygod/pokemon-icon-postprocessor) standard image support.
- Lots more...  

## Documentation:  
[ReadTheDocs](https://whmgr.rtfd.io/)  


## Getting Started:  

1.) Run the following to install .NET Core runtime, clone respository, and copy example Alerts, Filters, Geofences, config and alarm files.  
**Linux/macOS:**  
```
wget https://raw.githubusercontent.com/versx/WhMgr/master/install.sh && chmod +x install.sh && ./install.sh && rm install.sh  
```
**Windows:**  
```
bitsadmin /transfer dotnet-install-job /download /priority FOREGROUND https://raw.githubusercontent.com/versx/WhMgr/master/install.bat install.bat | start install.bat  
```
2.) Edit `config.json` either open in Notepad/++ or `vi config.json`.  
  - [Create bot token](https://github.com/reactiflux/discord-irc/wiki/Creating-a-discord-bot-&-getting-a-token)  
  - Input your bot token and config options.  



## Updating  
The update scripts will pull latest repository changes, build latest WhMgr.dll, and copy latest locale translation and master files.
If you'd like to copy any of the latest example files (alerts, filters, templates, geofences) you can provide a parameter when running the script to include them.  
```
update.sh examples
Will copy examples to build folder

update.sh geofences
Will copy geofences to build folder

update.sh all
Will copy examples and geofences to build folder
```  

## TODO  
- Allow Pokemon id and name in Pokemon filter lists.  
- Individual filters per Pokemon. (PA style, maybe)  
- PvP ranks DTS


## Previews  
*All examples are completely customizable using Dynamic Text Replacement/Substitution*  
Discord Pokemon Notifications:  
![Pokemon Notifications](images/pkmn.png "Pokemon Notifications")  

Discord Pokemon PVP Notifications:  
![Pokemon Notifications](images/pvp.png "Pokemon PVP Notifications")  

Discord Raid Notifications:  
![Raid Notifications](images/raids.png "Raid Notifications")  

Discord Raid Egg Notifications:  
![Egg Notifications](images/eggs.png "Egg Notifications")  

Discord Quest Notifications:  
![Quest Notifications](images/quests.png "Quest Notifications")  

Discord Lure Notifications:  
![Lure Notifications](images/lure.png "Lure Notifications")  

Discord Lure (Glacial) Notifications:  
![Lure (Glacial) Notifications](images/lure_glacial.png "Lure (Glacial) Notifications")  

Discord Lure (Mossy) Notifications:  
![Lure (Mossy) Notifications](images/lure_mossy.png "Lure (Mossy) Notifications")  

Discord Lure (Magnetic) Notifications:  
![Lure (Magnetic) Notifications](images/lure_magnetic.png "Lure (Magnetic) Notifications")  

Discord Gym Team Takeover Notifications:  
![Gym Team Takeover Notifications](images/gyms.png "Gym Team Takeover Notifications")  

Discord Team Rocket Invasion Notifications:  
![Team Rocket Invasion Notifications](images/invasions.png "Team Rocket Invasion Notifications")  

Discord Weather Notifications:  
![Weather Notifications](images/weather.png "Weather Notifications")  


## Credits  
[versx](https://github.com/versx) - Developer  
[PokeAlarm](https://github.com/PokeAlarm/PokeAlarm) - Dynamic Text Substitution idea  
[WDR](https://github.com/PartTimeJS/WDR) - masterfile.json file  
