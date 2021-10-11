# Getting Started  

### __Installation__  
- [Prerequisites](./prerequisites.md)  

### __Configuration__  
1.) Edit `config.json` either open in Notepad/++ or `vi config.json`. [Config Instructions](../config/config.md)  

  - [Create bot token](https://github.com/reactiflux/discord-irc/wiki/Creating-a-discord-bot-&-getting-a-token)  
  - Input your bot token and config options.  
  - Fill out the rest of the config options.

2.) Edit `alarms/alarms.json` either open in Notepad/++ or `vi alarms/alarms.json`.  

3.) Fill out the alarms file. [Alarm Instructions](../config/alarms.md)  

4.) Create directory `bin/geofences` if it doesn't already exist.  

5.) Create/copy geofence files to `geofences` folder. [Geofence Instructions](../config/geofences.md)  

6.) Add `dotnet` to your environment path if it isn't already (optional):  
```sh
export PATH=~/.dotnet/:$PATH
```  

### __Running__  
To run via command line arguments [click here](../other/commandline.md).  

1.) Build executable:
```
dotnet build ../../..
```
2.) Start WhMgr:
```
dotnet WhMgr.dll
```
3.) User Interface for members to create subscriptions from a website. [WhMgr UI](https://github.com/versx/WhMgr-UI)  
4.) Optional reverse location lookup with OpenStreetMaps Nominatim or Google Maps, instructions [here](../other/geocoding.md)  


### __Discord Permissions__  
Discord recently enabled a new feature that requires you to enable the Privileged Gateway Intents options in the [Discord Developers Portal](https://discord.com/developers/applications) to access Discord member lists.  

The bot requires the following Discord permissions:  

- Read Messages  
- Send Messages  
- Manage Messages (Prune quest channels)  
- Manage Roles (If cities are enabled)  
- Manage Emojis  
- Embed Links  
- Attach Files (`export` command)  
- Use External Emojis  


### __Notes__
- If `dotnet` is not in your path, you'll need to use `~/.dotnet/dotnet` instead of just `dotnet` for commands.  
- If you ran the original install command as `root`, `dotnet` will be located at `/root/.dotnet/dotnet` and you'll need to either use that for build commands or replace the `~/.dotnet/dotnet` path with it when adding to your path.
- Upon starting, database tables will be automatically created if `subscriptions.enabled` is set to `true`. Emoji icons are also created in the specified `EmojiGuildId` upon connecting to Discord.  