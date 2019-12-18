# Subscription Commands  

### General  

**enable** - Enable direct message subscription notifications.  
**disable** - Disable direct message subscription notifications.  
**info** - List all Pokemon, Raid, Quest, Invasion, and Gym subscriptions and settings.  
**expire** / **expires** - Check stripe API when Donor/Supporter subscription expires.  
**set-distance** - Set minimum distance to Pokemon, raids, quests, invasions and gyms need to be within. (Measured in kilometers)  
Usage: `set-distance <kilometers> <latitude>,<longitude>`  

* `<kilometers>` - Distance in kilometers from location.  
* `<latitude>` - Latitude part of the locations' coordinate pair.  
* `<longitude>` - Longitude part of the location's coordinate pair.  

Examples:  

* `.set-distance 10 34.00001, -119.22222`

### Pokemon  

**pokeme** - Subscribe to specific Pokemon notifications.  
Usage: `pokeme <pokemon> <iv> <level> <gender>`  

* `<pokemon>` - Parameter can take a list of Ids or names or the `all` keyword for everything.  
* `<iv>` - Minimum IV value.  
* `<level>` - Minimum level value.  

Examples:  

* `.pokeme pikachu 100 35 f`  
* `.pokeme pikachu 100`  
* `.pokeme all 100 35`  
<br>  

**pokemenot** - Unsubscribe from specific Pokemon notifications.  
Usage: `pokemenot <pokemon>`  

* `<pokemon>` - Parameter can take a list of Ids or names or the `all` keyword for everything.  

Examples:  

* `.pokemenot pikachu`  
* `.pokemenot all`  
<br>  

### PvP  

**pvpme** - Subscribe to PvP ranked Pokemon notifications.  
Usage: `pvpme <pokemon> <league> <rank> <percent>`  

* `<pokemon>` - Parameter can take a list of Ids or names or the `all` keyword for everything.  
* `<league>` - `great`, `ultra`, or `master` (`master` not current implemented).  
* `<rank>` - Minimum great or ultra league ranking.  
* `<percent>` - Minimum league ranking product percentage.  

Examples:  

* `.pvpme skarmory great 5 99.3`  
* `.pvpme roselia ultra 1 100`  
<br>  

**pvpmenot** - Unsubscribe from PvP ranked Pokemon notifications.  
Usage: `pvpmenot <pokemon> <league>`  

* `<pokemon>` - Parameter can take a list of Ids or names or the `all` keyword for everything.  
* `<league>` - `great`, `ultra`, or `master` (`master` not current implemented).  

Examples: 

* `.pvpmenot skarmory great`  
* `.pvpmenot all great`  
* `.pvpmenot all ultra`  
<br>  

### Raids  

**raidme** - Subscribe to specific Raid notifications.  
Usage: `raidme <pokemon> [city]`  

* `<pokemon>` - Parameter can take a list of Ids or names or the `all` keyword for everything.  
* `[city]` - (Optional) City name to get the notifications for or leave blank for all available cities.  

Examples:  

* `.raidme Tyranitar`  
* `.raidme mewtwo city1`  
<br>  

**raidmenot** - Unsubscribe from specific Raid notifications.  
Usage: `raidmenot <pokemon> [city]`  

* `<pokemon>` - Parameter can take a list of Ids or names or the `all` keyword for everything.  
* `[city]` - (Optional) City name to get the notifications for or leave blank for all available cities.  

Examples:  

* `.raidmenot Tyranitar`  
* `.raidmenot mewtwo city1`  
* `.raidmenot all`  
* `.raidmenot all city1`  
<br>  

### Quests  

**questme** - Subscribe to specific field research quest notifications.  
Usage: `questme <reward> [city]`  

* `<reward>` - Reward keyword of the field research quest.  
* `[city]` - (Optional) City name to get the notifications for or leave blank for all available cities.  

Examples:  

* `.questme chansey`  
* `.questme dratini city1`  
* `.questme nanab`  
<br>  

**questmenot** - Unsubscribe from specific field research quest notifications.  
Usage: `questmenot <reward> [city]`  

* `<reward>` - Reward keyword of the field research quest or the `all` keyword for everything.  
* `[city]` - (Optional) City name to get the notifications for or leave blank for all available cities.  

Examples:  

* `.questmenot chansey`  
* `.questmenot dratini city1`  
* `.questmenot nanab`  
* `.questmenot all`  
<br>  

### Team Rocket Invasions  

**invme** - Subscribe to specific Team Rocket invasion notifications.  
Usage: `invme <grunt_type>-<gender> [city]`  

* `<grunt_type>` - Grunt Pokemon type i.e. `fire`, `water`  
* `<gender>` - Grunt gender i.e. `male` | `m` | `female` | `f`  
* `[city]` - (Optional) City name to get the notifications for or leave blank for all available cities.  

Examples:  

* `.invme tier2-f`  
* `.invme ground-male city1`  
<br>  

**invmenot** - Unsubscribe from specific Team Rocket invasion notifications.  
Usage: `invmenot <grunt_type>-<gender> [city]`  

* `<grunt_type>` - Grunt Pokemon type i.e. `fire`, `water`  
* `<gender>` - Grunt gender i.e. `male` | `m` | `female` | `f`  
* `[city]` - (Optional) City name to get the notifications for or leave blank for all available cities.  

Examples:  

* `.invmenot tier2-f`  
* `.invmenot ground-male city1`  
* `.invmenot all`  
<br>  

### Management  

**import** - Import saved subscriptions file.  
**export** - Export subscriptions config file.  

### Icon Style  

**icons**- List available icon styles to choose from.  
**set-icons** - Set icon style to use for direct message notifications.  

### City Role Assignment  

**cities** / **feeds** - List all available city roles.  
**feedme** - Assign city role.   
**feedmenot** - Unassign city role.  