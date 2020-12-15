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