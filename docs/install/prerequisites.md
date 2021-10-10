# Prerequisites  
- .NET 5 SDK  

**.NET 5 SDK**  

Run the following to install .NET Core runtime, clone respository, and copy example Alerts, Filters, Geofences, config and alarm files.  

**Linux/macOS**  
```
wget https://raw.githubusercontent.com/versx/WhMgr/master/install.sh && chmod +x install.sh && ./install.sh && rm install.sh  
```
**Windows**  
```
bitsadmin /transfer dotnet-install-job /download /priority FOREGROUND https://raw.githubusercontent.com/versx/WhMgr/master/install.bat install.bat | start install.bat  
```