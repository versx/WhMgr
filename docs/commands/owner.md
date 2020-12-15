# Owner Commands

**gyms convert** - Check for any pokestops that have converted to gyms and delete them from the database.  
**nests** / **nests list** - Post nests in nest channels.  
**isbanned** - Check if IP banned from PTC or NIA.  
**clean-departed** - Clean departed Discord member subscriptions.  
**reset-quests** - Reset and delete quest channels.  
**shiny-stats** - Manually post shiny stats.  

## **event** | **ev**  

### Sub Commands  

**list** - List of Pokemon set as event Pokemon.  
Alias: **l**  
Usage: `event list`  

**set** - Set Pokemon as event Pokemon list. (overwrites current list)  
Alias: **s**  
Usage: `event set <pokemon>`  

* `<pokemon>` - Comma delimited list of Pokemon ids.  

**add** - Add Pokemon from existing event Pokemon list.  
Alias: **a**  
Usage: `event add <pokemon>`  

* `<pokemon>` - Comma delimited list of Pokemon ids.  

**remove** - Remove a Pokemon from event Pokemon list.  
Alias: **rm** | **r**  
Usage: `event remove <pokemon>`  

* `<pokemon>` - Comma delimited list of Pokemon ids.  