# Getting Started  

1.) Run the following to install .NET Core runtime, clone respository, and copy example Alerts, Filters, Geofences, config and alarm files.  
**Linux/macOS:**  
```
wget https://raw.githubusercontent.com/versx/WhMgr/master/install.sh && chmod +x install.sh && ./install.sh && rm install.sh  
```
**Windows:**  
```
bitsadmin /transfer dotnet-install-job /download /priority FOREGROUND https://raw.githubusercontent.com/versx/WhMgr/master/install.bat install.bat | start install.bat  
```
2.) Edit `config.json` either open in Notepad/++ or `vi config.json`. [Config Instructions](./config.md)  
  - [Create bot token](https://github.com/reactiflux/discord-irc/wiki/Creating-a-discord-bot-&-getting-a-token)  
  - Input your bot token and config options.  
  - Fill out the rest of the config options.

3.) Edit `alarms.json` either open in Notepad/++ or `vi alarms.json`.  
4.) Fill out the alarms file. [Alarm Instructions](./alarms.md)  
5.) Create directory `geofences` in `bin/debug/netcoreapp2.1` directory if it doesn't already exist.  
6.) Create/copy geofence files to `geofences` folder. [Geofence Instructions](./geofences.md)  
7.) Add `dotnet` to your environment path if it isn't already (optional):  
```sh
export PATH=~/.dotnet/dotnet:$PATH
```  
8.) Build executable:
```
dotnet build ../../..
```
9.) Start WhMgr:
```
dotnet WhMgr.dll
```

10.) Optional User Interface for members to create subscriptions from a website instead of using Discord commands. [WhMgr UI](https://github.com/versx/WhMgr-UI)  
11.) Optional reverse location lookup with OpenStreetMaps Nominatim instead of Google Maps, install instructions [here](https://nominatim.org/release-docs/develop/admin/Installation/)  


## Discord Permissions
- Read Messages  
- Send Messages  
- Manage Messages (Prune quest channels)  
- Manage Roles (If cities are enabled)  
- Manage Emojis  
- Embed Links  
- Attach Files (`export` command)  
- Use External Emojis  


## Notes:
- If `dotnet` is not in your path, you'll need to use `~/.dotnet/dotnet` instead of just `dotnet` for commands.  
- Upon starting, database tables will be automatically created if `enableSubscriptions` is set to `true`. Emoji icons are also created in the specified `EmojiGuildId` upon connecting to Discord.  