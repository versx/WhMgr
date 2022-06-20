# Embeds
Embeds depict how Discord embed messages are formatted. Customization is endless.  

`<name>` - Replacement placeholders.  
`<#condition></condition>` - Conditional replacements.  

**Replacement Placeholders**
Placeholders are used to build a template (similar to [mustache](https://mustache.github.io/)) which are replaced with real values from incoming webhooks and used to send outgoing Discord messages.  

**Conditional replacements**  
Enable the ability to only show something if the conditional value evaluates to `true`. A prime example would be if the Pokemon is near a Pokestop, to include the Pokestop name and image. Below is an example of it:  
```
{{#if near_pokestop}}**Near Pokestop:** [{{pokestop_name}}]({{pokestop_url}}){{br}}{{/if}}
```  

`{{pokestop_name}}` - Replaced by the name of the nearby Pokestop.  
`{{pokestop_url}}` - Replaced by the image url of the nearby Pokestop.  
`{{br}}` - Replaced with a new line break to preserve readability and formatting.  

For a list of available dynamic text substitution/replacement options check out the [DTS](../dts/index.md) pages.  

<hr>

## Embed Message Structures
```json
{
    "pokemon": {
        // Embed avatar icon url
        "avatarUrl": "{{pkmn_img_url}}",
        // Embed content text, each array item is treated as a new line break
        "content": [
            "{{pkmn_name}} {{form}}{{gender}} {{iv}} ({{atk_iv}}/{{def_iv}}/{{sta_iv}}) L{{lvl}}",
            "**Despawn:** {{despawn_time}} ({{time_left}} left){{despawn_time_verified}}",
            "**Details:** CP: {{cp}} IV: {{iv}} LV: {{lvl}}",
            "**Size:** {{size}} | {{types_emoji}}{{#if has_weather}} | {{weather_emoji}}{{#if is_weather_boosted}} (Boosted){{/if}}{{/if}}",
            "**Moveset:** {{moveset}}",
            "{{#if near_pokestop}}**Near Pokestop:** [{{pokestop_name}}]({{pokestop_url}})",
            "{{/if}}{{#if is_ditto}}**Catch Pokemon:** {{original_pkmn_name}}",
            "{{/if}}{{#if has_capture_rates}}{{capture_1_emoji}} {{capture_1}}% {{capture_2_emoji}} {{capture_2}}% {{capture_3_emoji}} {{capture_3}}%",
            "{{/if}}{{#if is_event}}Go Fest Spawn",
            "{{/if}}{{#if has_pvp}}",
            "{{#each pvp}}**{{@key}}**",
            "{{#each this}}",
            "#{{rank}} {{getPokemonName pokemonId}} {{getFormName formId}} {{cp}}CP @ L{{level}} {{formatPercentage percentage}}%",
            "{{/each}}{{/each}}",
            "{{/if}}**[Google]({{gmaps_url}}) | [Apple]({{applemaps_url}}) | [Waze]({{wazemaps_url}}) | [Scanner]({{scanmaps_url}})**"
        ],
        // Embed icon image url
        "iconUrl": "{{pkmn_img_url}}",
        // Embed title text
        "title": "{{geofence}}",
        // Embed title url
        "url": "{{gmaps_url}}",
        // Embed author username
        "username": "{{form}} {{pkmn_name}}{{gender}}",
        // Embed bottom image url
        "imageUrl": "{{tilemaps_url}}",
        // Embed footer
        "footer": {
            // Embed footer text
            "text": "{{guild_name}} {{date_time}}",
            // Embed footer icon url
            "iconUrl": "{{guild_img_url}}"
        }
    },
    "pokemonMissingStats": {
        "avatarUrl": "{{pkmn_img_url}}",
        "content": [
            "{{pkmn_name}} {{form}}{{gender}}",
            "**Despawn:** {{despawn_time}} ({{time_left}} left){{despawn_time_verified}}",
            "**Types:** {{types_emoji}}",
            "{{#if near_pokestop}}**Near Pokestop:** [{{pokestop_name}}]({{pokestop_url}})",
            "{{/if}}**[Google]({{gmaps_url}}) | [Apple]({{applemaps_url}}) | [Waze]({{wazemaps_url}}) | [Scanner]({{scanmaps_url}})**"
        ],
        "iconUrl": "{{pkmn_img_url}}",
        "title": "{{geofence}}",
        "url": "{{gmaps_url}}",
        "username": "{{form}} {{pkmn_name}}{{gender}}",
        "imageUrl": "{{tilemaps_url}}",
        "footer": {
            "text": "{{guild_name}} {{date_time}}",
            "iconUrl": "{{guild_img_url}}"
        }
    },
    "gyms": {
        "avatarUrl": "{{gym_url}}",
        "content": [
            "{{#if team_changed}}Gym changed from {{old_gym_team_emoji}} {{old_gym_team}} to {{gym_team_emoji}} {{gym_team}}",
            "{{/if}}{{#if in_battle}}Gym is under attack!",
            "{{/if}}**Slots Available:** {{slots_available}}",
            "{{#if power_up_level}}**Power Level**",
            "Level: {{power_up_level}} | Points: {{power_up_points}}",
            "Time Left: {{power_up_end_time_left}}",
            "{{/if}}{{#if is_ex}}{{ex_gym_emoji}} Gym!",
            "{{/if}}**[Google]({{gmaps_url}}) | [Apple]({{applemaps_url}}) | [Waze]({{wazemaps_url}}) | [Scanner]({{scanmaps_url}})**"
        ],
        "iconUrl": "{{gym_url}}",
        "title": "{{geofence}}: {{gym_name}}",
        "url": "{{gmaps_url}}",
        "username": "{{gym_name}}",
        "imageUrl": "{{tilemaps_url}}",
        "footer": {
            "text": "{{guild_name}} {{date_time}}",
            "iconUrl": "{{guild_img_url}}"
        }
    },
    "raids": {
        "avatarUrl": "{{pkmn_img_url}}",
        "content": [
            "{{evolution}} {{form}} {{pkmn_name}}{{gender}} {{costume}} Raid Ends: {{end_time}} ({{end_time_left}} left)",
            "**Perfect CP:** {{perfect_cp}} / :white_sun_rain_cloud: {{perfect_cp_boosted}}",
            "**Worst CP:** {{worst_cp}} / :white_sun_rain_cloud: {{worst_cp_boosted}}",
            "**Types:** {{types_emoji}} | **Level:** {{lvl}} | **Team:** {{team_emoji}}",
            "**Moveset:** {{moveset}}",
            "**Weaknesses:** {{weaknesses_emoji}}",
            "{{#if is_ex}}{{ex_emoji}} Gym!",
            "{{/if}}{{#if power_up_level}}**Power Level**",
            "Level: {{power_up_level}} | Points: {{power_up_points}}",
            "Time Left: {{power_up_end_time_left}}",
            "{{/if}}**[Google]({{gmaps_url}}) | [Apple]({{applemaps_url}}) | [Waze]({{wazemaps_url}}) | [Scanner]({{scanmaps_url}})**"
        ],
        "iconUrl": "{{pkmn_img_url}}",
        "title": "{{geofence}}: {{gym_name}}",
        "url": "{{gmaps_url}}",
        "username": "{{form}} {{pkmn_name}}{{gender}} {{costume}} Raid",
        "imageUrl": "{{tilemaps_url}}",
        "footer": {
            "text": "{{guild_name}} {{date_time}}",
            "iconUrl": "{{guild_img_url}}"
        }
    },
    "eggs": {
        "avatarUrl": "{{pkmn_img_url}}",
        "content": [
            "Hatches: {{start_time}} ({{start_time_left}})",
            "**Ends:** {{end_time}} ({{end_time_left}} left)",
            "**Team:** {{team_emoji}}",
            "{{#if is_ex}}{{ex_emoji}} Gym!",
            "{{/if}}{{#if power_up_level}}**Power Level**",
            "Level: {{power_up_level}} | Points: {{power_up_points}}",
            "Time Left: {{power_up_end_time_left}}",
            "{{/if}}**[Google]({{gmaps_url}}) | [Apple]({{applemaps_url}}) | [Waze]({{wazemaps_url}}) | [Scanner]({{scanmaps_url}})**"
        ],
        "iconUrl": "{{pkmn_img_url}}",
        "title": "{{geofence}}: {{gym_name}}",
        "url": "{{gmaps_url}}",
        "username": "Level {{lvl}} Egg",
        "imageUrl": "{{tilemaps_url}}",
        "footer": {
            "text": "{{guild_name}} {{date_time}}",
            "iconUrl": "{{guild_img_url}}"
        }
    },
    "pokestops": {
        "avatarUrl": "{{pokestop_url}}",
        "content": [
            "{{#if has_lure}}**Lure Expires** {{lure_expire_time}} ({{lure_expire_time_left}} left)",
            "**Lure Type:** {{lure_type}}",
            "{{/if}}{{#if power_up_level}}**Power Level**",
            "Level: {{power_up_level}} | Points: {{power_up_points}}",
            "Time Left: {{power_up_end_time_left}}",
            "{{/if}}**[Google]({{gmaps_url}}) | [Apple]({{applemaps_url}}) | [Waze]({{wazemaps_url}}) | [Scanner]({{scanmaps_url}})**"
        ],
        "iconUrl": "{{pokestop_url}}",
        "title": "{{geofence}}: {{pokestop_name}}",
        "url": "{{gmaps_url}}",
        "username": "{{pokestop_name}}",
        "imageUrl": "{{tilemaps_url}}",
        "footer": {
            "text": "{{guild_name}} {{date_time}}",
            "iconUrl": "{{guild_img_url}}"
        }
    },
    "quests": {
        "avatarUrl": "{{quest_reward_img_url}}",
        "content": [
            "**Quest:** {{quest_task}}",
            "{{#if has_quest_conditions}}**Condition(s):** {{quest_conditions}}",
            "{{/if}}**Reward:** {{quest_reward}}",
            "{{#if is_ar}}**AR Quest Required!**",
            "{{/if}}",
            "**[Google]({{gmaps_url}}) | [Apple]({{applemaps_url}}) | [Waze]({{wazemaps_url}}) | [Scanner]({{scanmaps_url}})**"
        ],
        "iconUrl": "{{pokestop_url}}",
        "title": "{{geofence}}: {{pokestop_name}}",
        "url": "{{gmaps_url}}",
        "username": "{{quest_task}}",
        "imageUrl": "{{tilemaps_url}}",
        "footer": {
            "text": "{{guild_name}} {{date_time}}",
            "iconUrl": "{{guild_img_url}}"
        }
    },
    "lures": {
        "avatarUrl": "{{lure_img_url}}",
        "content": [
            "{{#if has_lure}}**Lure Expires** {{lure_expire_time}} ({{lure_expire_time_left}} left)",
            "**Lure Type:** {{lure_type}}",
            "{{/if}}{{#if power_up_level}}**Power Level**",
            "Level: {{power_up_level}} | Points: {{power_up_points}}",
            "Time Left: {{power_up_end_time_left}}",
            "{{/if}}**[Google]({{gmaps_url}}) | [Apple]({{applemaps_url}}) | [Waze]({{wazemaps_url}}) | [Scanner]({{scanmaps_url}})**"
        ],
        "iconUrl": "{{pokestop_url}}",
        "title": "{{geofence}}: {{pokestop_name}}",
        "url": "{{gmaps_url}}",
        "username": "{{pokestop_name}}",
        "imageUrl": "{{tilemaps_url}}",
        "footer": {
            "text": "{{guild_name}} {{date_time}}",
            "iconUrl": "{{guild_img_url}}"
        }
    },
    "invasions": {
        "avatarUrl": "{{invasion_img_url}}",
        "content": [
            "{{#if has_invasion}}**Expires:** {{invasion_expire_time}} ({{invasion_expire_time_left}} left)",
            "**Type:** {{grunt_type_emoji}} | **Gender:** {{grunt_gender}}",
            "**Encounter Reward Chance:**",
            "{{#each invasion_encounters}}",
            "{{chance}} - {{pokemon}}",
            "{{/each}}",
            "{{/if}}**[Google]({{gmaps_url}}) | [Apple]({{applemaps_url}}) | [Waze]({{wazemaps_url}}) | [Scanner]({{scanmaps_url}})**"
        ],
        "iconUrl": "{{pokestop_url}}",
        "title": "{{geofence}}: {{pokestop_name}}",
        "url": "{{gmaps_url}}",
        "username": "{{pokestop_name}}",
        "imageUrl": "{{tilemaps_url}}",
        "footer": {
            "text": "{{guild_name}} {{date_time}}",
            "iconUrl": "{{guild_img_url}}"
        }
    },
    "nests": {
        "avatarUrl": "{{pkmn_img_url}}",
        "content": [
            "**Pokemon:** {{pkmn_name}}",
            "**Average Spawns:** {{avg_spawns}}/h | **Types:** {{types_emojis}}",
            "**[Google]({{gmaps_url}}) | [Apple]({{applemaps_url}}) | [Waze]({{wazemaps_url}}) | [Scanner]({{scanmaps_url}})**"
        ],
        "iconUrl": "{{pkmn_img_url}}",
        "title": "{{geofence}}: {{nest_name}}",
        "url": "{{gmaps_url}}",
        "username": "",
        "imageUrl": "{{tilemaps_url}}",
        "footer": {
            "text": "{{guild_name}} {{date_time}}",
            "iconUrl": "{{guild_img_url}}"
        }
    },
    "weather": {
        "avatarUrl": "{{weather_img_url}}",
        "content": [
            "**Weather Condition:** {{weather_emoji}} {{weather_condition}}",
            "**Weather Cell ID:** #{{id}}"
        ],
        "iconUrl": "{{weather_img_url}}",
        "title": "{{geofence}}",
        "url": "{{gmaps_url}}",
        "username": "Weather",
        "imageUrl": "{{tilemaps_url}}",
        "footer": {
            "text": "{{guild_name}} {{date_time}}",
            "iconUrl": "{{guild_img_url}}"
        }
    }
}
```