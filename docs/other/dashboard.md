# Admin Dashboard  

Used to configure and manage all configuration files needed to run Webhook Manager.  

Access the dashboard by visiting http://127.0.0.1:8008/dashboard  

## Installation  

From the root of the project folder run the following commands:  

- Copy the Admin Dashboard folder to the `bin` folder  
```cp -R src/ClientApp bin/```  
- Change directories to Admin Dashboard folder  
```cd bin/ClientApp```  
- Install packages and dependencies  
```npm install```  
- Build the Admin Dashboard  
```npm build```  
- Copy the example config file  
```cp src/config.example.json src/config.json```  
- Edit the config file  
```nano src/config.json```  
- Change directories back to the `bin` folder  
```cd ..```  
- Run Webhook Manager  
```dotnet WhMgr.dll``` (or restart via pm2)


## Screenshots  

### Dashboard  
![Dashboard](https://raw.githubusercontent.com/versx/WhMgr/v5-rewrite/.github/images/dashboard/dashboard.png "Dashboard")  

### Configs  
![Dashboard](https://raw.githubusercontent.com/versx/WhMgr/v5-rewrite/.github/images/dashboard/configs.png "Configs")  

### Edit Config  
![Dashboard](https://raw.githubusercontent.com/versx/WhMgr/v5-rewrite/.github/images/dashboard/config-edit.png "Edit Config")  

### Discords  
![Dashboard](https://raw.githubusercontent.com/versx/WhMgr/v5-rewrite/.github/images/dashboard/discords.png "Discords")  

### Edit Discord  
![Dashboard](https://raw.githubusercontent.com/versx/WhMgr/v5-rewrite/.github/images/dashboard/discord-edit.png "Edit Discord")  

### Alarms  
![Dashboard](https://raw.githubusercontent.com/versx/WhMgr/v5-rewrite/.github/images/dashboard/alarms.png "Configs")  

### Edit Alarm  
![Dashboard](https://raw.githubusercontent.com/versx/WhMgr/v5-rewrite/.github/images/dashboard/alarm-edit.png "Edit Alarm")  

### Filters  
![Dashboard](https://raw.githubusercontent.com/versx/WhMgr/v5-rewrite/.github/images/dashboard/filters.png "Filters")  

### Edit Filter  
![Dashboard](https://raw.githubusercontent.com/versx/WhMgr/v5-rewrite/.github/images/dashboard/filter-edit.png "Edit Filter")  

### Embeds  
![Dashboard](https://raw.githubusercontent.com/versx/WhMgr/v5-rewrite/.github/images/dashboard/embeds.png "Embeds")  

### New Embed  
![Dashboard](https://raw.githubusercontent.com/versx/WhMgr/v5-rewrite/.github/images/dashboard/embed-new.png "New Embed")  

### Edit Embed  
![Dashboard](https://raw.githubusercontent.com/versx/WhMgr/v5-rewrite/.github/images/dashboard/embed-edit.png "Edit Embed")  

### Geofences  
![Dashboard](https://raw.githubusercontent.com/versx/WhMgr/v5-rewrite/.github/images/dashboard/geofences.png "Geofences")  

### Edit Geofence  
![Dashboard](https://raw.githubusercontent.com/versx/WhMgr/v5-rewrite/.github/images/dashboard/geofence-edit.png "Edit Geofence")  

### Export Geofence  
![Dashboard](https://raw.githubusercontent.com/versx/WhMgr/v5-rewrite/.github/images/dashboard/geofence-export.png "Export Geofence")  

### Discord Roles  
![Dashboard](https://raw.githubusercontent.com/versx/WhMgr/v5-rewrite/.github/images/dashboard/roles.png "Discord Roles")  

### Edit Discord Role  
![Dashboard](https://raw.githubusercontent.com/versx/WhMgr/v5-rewrite/.github/images/dashboard/role-edit.png "Edit Discord Role")  