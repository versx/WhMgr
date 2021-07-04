namespace WhMgr.Services.Alarms.Embeds
{
    using System.Collections.Generic;

    // TODO: Update values
    public class EmbedMessage : Dictionary<EmbedMessageType, EmbedMessageSettings>
    {
        public static readonly EmbedMessage Defaults = new()
        {
            {
                EmbedMessageType.Pokemon, new EmbedMessageSettings
                {
                    AvatarUrl = "{{pkmn_img_url}}",
                    ContentList = new List<string>
                    {
                        "{{pkmn_name}} {{form}}{{gender}} {{iv}} ({{atk_iv}}/{{def_iv}}/{{sta_iv}}) L{{lvl}}",
                        "**Despawn:** {{despawn_time}} ({{time_left}} left)",
                        "**Details:** CP: {{cp}} IV: {{iv}} LV: {{lvl}}",
                        "**Size:** {{size}} | {{types_emoji}}{{#if has_weather}} | {{weather_emoji}}{{#if is_weather_boosted}} (Boosted){{/if}}{{/if}}",
                        "**Moveset:** {{moveset}}",
                        "{{#if near_pokestop}}**Near Pokestop:** [{{pokestop_name}}]({{pokestop_url}})",
                        "{{/if}}{{#if is_ditto}}**Catch Pokemon:** {{original_pkmn_name}}",
                        "{{/if}}{{#if has_capture_rates}}{{capture_1_emoji}} {{capture_1}}% {{capture_2_emoji}} {{capture_2}}% {{capture_3_emoji}} {{capture_3}}%",
                        "{{/if}}{{#if is_event}}Go Fest Spawn",
                        "{{/if}}{{#if is_pvp}}",
                        "{{#if is_great}}{{great_league_emoji}}**Great League**",
                        "{{#each great_league}}Rank {{rank}} {{name}} {{cp}}CP @ L{{level}} {{percentage}}%",
                        "{{/each}}",
                        "{{/if}}{{#if is_ultra}}{{ultra_league_emoji}}**Ultra League**",
                        "{{#each ultra_league}}Rank {{rank}} {{name}} {{cp}}CP @ L{{level}} {{percentage}}%",
                        "{{/each}}",
                        "{{/if}}{{/if}}**[Google]({{gmaps_url}}) | [Apple]({{applemaps_url}}) | [Waze]({{wazemaps_url}}) | [Scanner]({{scanmaps_url}})**",
                    },
                    IconUrl = "{{pkmn_img_url}}",
                    Title = "{{geofence}}",
                    Url = "{{gmaps_url}}",
                    Username = "{{form}} {{pkmn_name}}{{gender}}",
                    ImageUrl = "{{tilemaps_url}}",
                    Footer = new EmbedMessageFooter
                    {
                        Text = "{{guild_name}} | {{date_time}}",
                        IconUrl = "{{guild_img_url}}"
                    }
                }
            },
            {
                EmbedMessageType.PokemonMissingStats, new EmbedMessageSettings
                {
                    AvatarUrl = "{{pkmn_img_url}}",
                    ContentList = new List<string>
                    {
                        "{{pkmn_name}} {{form}}{{gender}}",
                        "**Despawn:** {{despawn_time}} ({{time_left}} left){{despawn_time_verified}}",
                        "**Types:** {{types_emoji}}",
                        "{{#if near_pokestop}}**Near Pokestop:** [{{pokestop_name}}]({{pokestop_url}})",
                        "{{/if}}**[Google]({{gmaps_url}}) | [Apple]({{applemaps_url}}) | [Waze]({{wazemaps_url}}) | [Scanner]({{scanmaps_url}})**",
                    },
                    IconUrl = "{{pkmn_img_url}}",
                    Title = "{{geofence}}",
                    Url = "{{gmaps_url}}",
                    Username = "{{form}} {{pkmn_name}}{{gender}}",
                    ImageUrl = "{{tilemaps_url}}",
                    Footer = new EmbedMessageFooter
                    {
                        Text = "{{guild_name}} | {{date_time}}",
                        IconUrl = "{{guild_img_url}}"
                    }
                }
            },
            {
                EmbedMessageType.Gyms, new EmbedMessageSettings
                {
                    AvatarUrl = "{{gym_url}}",
                    ContentList = new List<string>
                    {
                        "{{#if team_changed}}Gym changed from {{old_gym_team_emoji}} {{old_gym_team}} to {{gym_team_emoji}} {{gym_team}}",
                        "{{/if}}{{#if in_battle}}Gym is under attack!",
                        "{{/if}}**Slots Available:** {{slots_available}}",
                        "{{#if is_ex}}{{ex_gym_emoji}} Gym!{{/if}}**[Google]({{gmaps_url}}) | [Apple]({{applemaps_url}}) | [Waze]({{wazemaps_url}}) | [Scanner]({{scanmaps_url}})**",
                    },
                    IconUrl = "{{gym_url}}",
                    Title = "{{geofence}}: {{gym_name}}",
                    Url = "{{gmaps_url}}",
                    Username = "{{gym_name}}",
                    ImageUrl = "{{tilemaps_url}}",
                    Footer = new EmbedMessageFooter
                    {
                        Text = "{{guild_name}} | {{date_time}}",
                        IconUrl = "{{guild_img_url}}"
                    }
                }
            },
            {
                EmbedMessageType.Raids, new EmbedMessageSettings
                {
                    AvatarUrl = "{{pkmn_img_url}}",
                    ContentList = new List<string>
                    {
                        "{{evolution}} {{form}} {{pkmn_name}}{{gender}} {{costume}} Raid Ends: {{end_time}} ({{end_time_left}} left)",
                        "**Perfect CP:** {{perfect_cp}} / :white_sun_rain_cloud: {{perfect_cp_boosted}}",
                        "**Worst CP:** {{worst_cp}} / :white_sun_rain_cloud: {{worst_cp_boosted}}",
                        "**Types:** {{types_emoji}} | **Level:** {{lvl}} | **Team:** {{team_emoji}}",
                        "**Moveset:** {{moveset}}",
                        "**Weaknesses:** {{weaknesses_emoji}}",
                        "{{#if is_ex}}{{ex_emoji}} Gym!",
                        "{{/if}}**[Google]({{gmaps_url}}) | [Apple]({{applemaps_url}}) | [Waze]({{wazemaps_url}}) | [Scanner]({{scanmaps_url}})**",
                    },
                    IconUrl = "{{pkmn_img_url}}",
                    Title = "{{geofence}}: {{gym_name}}",
                    Url = "{{gmaps_url}}",
                    Username = "{{form}} {{pkmn_name}}{{gender}} {{costume}} Raid",
                    ImageUrl = "{{tilemaps_url}}",
                    Footer = new EmbedMessageFooter
                    {
                        Text = "{{guild_name}} | {{date_time}}",
                        IconUrl = "{{guild_img_url}}"
                    }
                }
            },
            {
                EmbedMessageType.Eggs, new EmbedMessageSettings
                {
                    AvatarUrl = "{{pkmn_img_url}}",
                    ContentList = new List<string>
                    {
                        "Hatches: {{start_time}} ({{start_time_left}})",
                        "**Ends:** {{end_time}} ({{end_time_left}} left)",
                        "**Team:** {{team_emoji}}",
                        "{{#if is_ex}}{{ex_emoji}} Gym!",
                        "{{/if**[Google]({{gmaps_url}}) | [Apple]({{applemaps_url}}) | [Waze]({{wazemaps_url}}) | [Scanner]({{scanmaps_url}})**",
                    },
                    IconUrl = "{{pkmn_img_url}}",
                    Title = "{{geofence}}: {{gym_name}}",
                    Url = "{{gmaps_url}}",
                    Username = "Level {{lvl}} Egg",
                    ImageUrl = "{{tilemaps_url}}",
                    Footer = new EmbedMessageFooter
                    {
                        Text = "{{guild_name}} | {{date_time}}",
                        IconUrl = "{{guild_img_url}}"
                    }
                }
            },
            {
                EmbedMessageType.Pokestops, new EmbedMessageSettings
                {
                    AvatarUrl = "{{pokestop_url}}",
                    ContentList = new List<string>
                    {
                        "{{#if has_lure}}**Lure Expires** {{lure_expire_time}} ({{lure_expire_time_left}} left)",
                        "**Lure Type:** {{lure_type}}",
                        "{{/if}}{{#if has_invasion}}**Expires:** {{invasion_expire_time}} ({{invasion_expire_time_left}} left)",
                        "**Type:** {{grunt_type_emoji}} | **Gender:** {{grunt_gender}}",
                        "**Encounter Reward Chance:**",
                        "{{#each invasion_encounters}}",
                        "{{chance}} - {{pokemon}}",
                        "{{/each}}",
                        "{{/if}}**[Google]({{gmaps_url}}) | [Apple]({{applemaps_url}}) | [Waze]({{wazemaps_url}}) | [Scanner]({{scanmaps_url}})**",
                    },
                    IconUrl = "{{pokestop_url}}",
                    Title = "{{geofence}}: {{pokestop_name}}",
                    Url = "{{gmaps_url}}",
                    Username = "{{pokestop_name}}",
                    ImageUrl = "{{tilemaps_url}}",
                    Footer = new EmbedMessageFooter
                    {
                        Text = "{{guild_name}} | {{date_time}}",
                        IconUrl = "{{guild_img_url}}"
                    }
                }
            },
            {
                EmbedMessageType.Quests, new EmbedMessageSettings
                {
                    AvatarUrl = "{{quest_reward_img_url}}",
                    ContentList = new List<string>
                    {
                        "**Quest:** {{quest_task}}",
                        "{{#if has_quest_conditions}}**Condition(s):** {{quest_conditions}}",
                        "{{/if}}**Reward:** {{quest_reward}}",
                        "**[Google]({{gmaps_url}}) | [Apple]({{applemaps_url}}) | [Waze]({{wazemaps_url}}) | [Scanner]({{scanmaps_url}})**",
                    },
                    IconUrl = "{{pokestop_url}}",
                    Title = "{{geofence}}: {{pokestop_name}}",
                    Url = "{{gmaps_url}}",
                    Username = "{{quest_task}}",
                    ImageUrl = "{{tilemaps_url}}",
                    Footer = new EmbedMessageFooter
                    {
                        Text = "{{guild_name}} | {{date_time}}",
                        IconUrl = "{{guild_img_url}}"
                    }
                }
            },
            {
                EmbedMessageType.Invasions, new EmbedMessageSettings
                {
                    AvatarUrl = "{{invasion_img_url}}",
                    ContentList = new List<string>
                    {
                        "{{#if has_invasion}}**Expires:** {{invasion_expire_time}} ({{invasion_expire_time_left}} left)",
                        "**Type:** {{grunt_type_emoji}} | **Gender:** {{grunt_gender}}",
                        "**Encounter Reward Chance:**",
                        "{{#each invasion_encounters}}",
                        "{{chance}} - {{pokemon}}",
                        "{{/each}}",
                        "{{/if}}**[Google]({{gmaps_url}}) | [Apple]({{applemaps_url}}) | [Waze]({{wazemaps_url}}) | [Scanner]({{scanmaps_url}})**",
                    },
                    IconUrl = "{{pokestop_url}}",
                    Title = "{{geofence}}: {{pokestop_name}}",
                    Url = "{{gmaps_url}}",
                    Username = "{{pokestop_name}}",
                    ImageUrl = "{{tilemaps_url}}",
                    Footer = new EmbedMessageFooter
                    {
                        Text = "{{guild_name}} | {{date_time}}",
                        IconUrl = "{{guild_img_url}}"
                    }
                }
            },
            {
                EmbedMessageType.Lures, new EmbedMessageSettings
                {
                    AvatarUrl = "{{lure_img_url}}",
                    ContentList = new List<string>
                    {
                        "{{#if has_lure}}**Lure Expires:** {{lure_expire_time}} ({{lure_expire_time_left}} left)",
                        "**Lure Type:** {{lure_type}}",
                        "{{/if}}**[Google]({{gmaps_url}}) | [Apple]({{applemaps_url}}) | [Waze]({{wazemaps_url}}) | [Scanner]({{scanmaps_url}})**",
                    },
                    IconUrl = "{{pokestop_url}}",
                    Title = "{{geofence}}: {{pokestop_name}}",
                    Url = "{{gmaps_url}}",
                    Username = "{{pokestop_name}}",
                    ImageUrl = "{{tilemaps_url}}",
                    Footer = new EmbedMessageFooter
                    {
                        Text = "{{guild_name}} | {{date_time}}",
                        IconUrl = "{{guild_img_url}}"
                    }
                }
            },
            {
                EmbedMessageType.Nests, new EmbedMessageSettings
                {
                    AvatarUrl = "{{pkmn_img_url}}",
                    ContentList = new List<string>
                    {
                        "**Pokemon:** {{pkmn_name}}",
                        "**Average Spawns:** {{avg_spawns}}/h | **Types:** {{types_emojis}}",
                        "**[Google]({{gmaps_url}}) | [Apple]({{applemaps_url}}) | [Waze]({{wazemaps_url}}) | [Scanner]({{scanmaps_url}})**",
                    },
                    IconUrl = "{{pkmn_img_url}}",
                    Title = "{{geofence}}: {{nest_name}}",
                    Url = "{{gmaps_url}}",
                    Username = "",
                    ImageUrl = "{{tilemaps_url}}",
                    Footer = new EmbedMessageFooter
                    {
                        Text = "{{guild_name}} | {{date_time}}",
                        IconUrl = "{{guild_img_url}}"
                    }
                }
            },
            {
                EmbedMessageType.Weather, new EmbedMessageSettings
                {
                    AvatarUrl = "{{weather_img_url}}",
                    ContentList = new List<string>
                    {
                        "**Weather Condition:** {{weather_emoji}} {{weather_condition}}",
                        "**Weather Cell ID:** #{{id}}",
                    },
                    IconUrl = "{{weather_img_url}}",
                    Title = "{{geofence}}",
                    Url = "{{gmaps_url}}",
                    Username = "Weather",
                    ImageUrl = "{{tilemaps_url}}",
                    Footer = new EmbedMessageFooter
                    {
                        Text = "{{guild_name}} | {{date_time}}",
                        IconUrl = "{{guild_img_url}}"
                    }
                }
            }
        };
    }
}