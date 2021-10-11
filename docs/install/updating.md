# Updating  
The update scripts will pull latest repository changes, build latest WhMgr.dll, and copy latest locale translation and master files.
If you'd like to copy any of the latest example files (embeds, filters, templates, geofences) you can provide a parameter when running the script to include them.  

**All update commands will do at least the following:**  
- Pull latest repository changes  
- Build latest WhMgr binary/executable  
- Copy masterfile.json and cpMultipliers.json files to build folder  

<hr>

### Update (Normal)  
```
./scripts/update.sh
```

### Update (Copy example filter and embed files)  
```
./scripts/update.sh examples
```