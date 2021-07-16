# Getting Started  

## Installation

Run the following to install .NET Core runtime, clone respository, and copy example Alerts, Filters, Geofences, config and alarm files.  

**Linux/macOS**  
```
wget https://raw.githubusercontent.com/versx/WhMgr/master/install.sh && chmod +x install.sh && ./install.sh && rm install.sh  
```
**Windows**  
```
bitsadmin /transfer dotnet-install-job /download /priority FOREGROUND https://raw.githubusercontent.com/versx/WhMgr/master/install.bat install.bat | start install.bat  
```

## Configuration  
1.) Edit `config.json` either open in Notepad/++ or `vi config.json`. [Config Instructions](./config.md)  

  - [Create bot token](https://github.com/reactiflux/discord-irc/wiki/Creating-a-discord-bot-&-getting-a-token)  
  - Input your bot token and config options.  
  - Fill out the rest of the config options.

2.) Edit `alarms/alarms.json` either open in Notepad/++ or `vi alarms/alarms.json`.  

3.) Fill out the alarms file. [Alarm Instructions](./alarms.md)  

4.) Create directory `bin/geofences` if it doesn't already exist.  

5.) Create/copy geofence files to `geofences` folder. [Geofence Instructions](./geofences.md)  

6.) Add `dotnet` to your environment path if it isn't already (optional):  
```sh
export PATH=~/.dotnet/:$PATH
```  

## Running  
To run via command line arguments [click here](../other/commandline.md).  

1.) Build executable:
```
dotnet build ../../..
```
2.) Start WhMgr:
```
dotnet WhMgr.dll
```
3.) Optional User Interface for members to create subscriptions from a website instead of using Discord commands. [WhMgr UI](https://github.com/versx/WhMgr-UI)  
4.) Optional reverse location lookup with OpenStreetMaps Nominatim instead of Google Maps, install instructions [here](https://nominatim.org/release-docs/develop/admin/Installation/)  


## Discord Permissions  
Discord recently enabled a new feature that requires you to enable the Global Intents options in the [Discord Developers Portal](https://discord.com/developers) to access Discord member lists.  

The bot needs the following Discord permissions:  

- Read Messages  
- Send Messages  
- Manage Messages (Prune quest channels)  
- Manage Roles (If cities are enabled)  
- Manage Emojis  
- Embed Links  
- Attach Files (`export` command)  
- Use External Emojis  


## Notes
- If `dotnet` is not in your path, you'll need to use `~/.dotnet/dotnet` instead of just `dotnet` for commands.  
- If you ran the original install command as `root`, `dotnet` will be located at `/root/.dotnet/dotnet` and you'll need to either use that for build commands or replace the `~/.dotnet/dotnet` path with it when adding to your path.
- Upon starting, database tables will be automatically created if `subscriptions.enabled` is set to `true`. Emoji icons are also created in the specified `EmojiGuildId` upon connecting to Discord.  
