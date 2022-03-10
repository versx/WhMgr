# Migrating v4 to v5  

- Copy folders from existing v4 instances `bin` folder to v5 `bin` folder.  
- Move config files to `bin/configs` folder.  
- Rename `bin/alerts` to `bin/embeds`  
- Update existing configs with new format.  
- Update existing Discord server configs with new format.  
- Run the following to fix renaming of properties for filters and alarms.  
```
sed -i 's/alerts/embeds/g' alarms/*.json
sed -i 's/onlyEx/only_ex/g' filters/*.json
sed -i 's/ignoreMissing/ignore_missing/g' filters/*.json
sed -i 's/isShiny/is_shiny/g' filters/*.json
```
- Run the `migrate-v4-to-v5.sql` database migration script on your v4 database.  

TODO: Expand on more (config migration, filter migration, etc)