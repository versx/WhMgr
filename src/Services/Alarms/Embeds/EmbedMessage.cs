namespace WhMgr.Services.Alarms.Embeds
{
    using System.Collections.Generic;

    public class EmbedMessage : Dictionary<EmbedMessageType, EmbedMessageSettings>
    {
        public static readonly EmbedMessage Defaults = new()
        {
            {
                EmbedMessageType.Pokemon, new EmbedMessageSettings
                {
                    AvatarUrl = "<pkmn_img_url>",
                    Content = "<pkmn_name> <form><gender> <iv> (<atk_iv>/<def_iv>/<sta_iv>) L<lvl><br>**Despawn:** <despawn_time> (<time_left> left)<br>**Details:** CP: <cp> IV: <iv> LV: <lvl><br><types_emoji> | **Size:** <size><#has_weather> | <weather_emoji><#is_weather_boosted> (Boosted)</is_weather_boosted></has_weather><br>**Moveset:** <moveset><br><#near_pokestop>**Near Pokestop:** [<pokestop_name>](<pokestop_url>)<br></near_pokestop><#is_ditto>**Catch Pokemon:** <original_pkmn_name><br></is_ditto><#has_capture_rates><capture_1_emoji> <capture_1>% <capture_2_emoji> <capture_2>% <capture_3_emoji> <capture_3>%<br></has_capture_rates><#is_event>Go Fest Spawn<br></is_event><#is_pvp><br><pvp_stats></is_pvp>**[[Google](<gmaps_url>)] [[Apple](<applemaps_url>)] [[Waze](<wazemaps_url>)] [[Scanner](<scanmaps_url>)]**",
                    IconUrl = "<pkmn_img_url>",
                    Title = "<geofence>",
                    Url = "<gmaps_url>",
                    Username = "<form> <pkmn_name><gender>",
                    ImageUrl = "<tilemaps_url>",
                    Footer = new EmbedMessageFooter
                    {
                        Text = "<guild_name> | <date_time>",
                        IconUrl = "<guild_img_url>"
                    }
                }
            },
            {
                EmbedMessageType.PokemonMissingStats, new EmbedMessageSettings
                {
                    AvatarUrl = "<pkmn_img_url>",
                    Content = "<pkmn_name> <form><gender><br>**Despawn:** <despawn_time> (<time_left> left)<despawn_time_verified><br>**Types:** <types_emoji><br><#near_pokestop>**Near Pokestop:** [<pokestop_name>](<pokestop_url>)<br></near_pokestop>**[[Google](<gmaps_url>)] [[Apple](<applemaps_url>)] [[Waze](<wazemaps_url>)] [[Scanner](<scanmaps_url>)]**",
                    IconUrl = "<pkmn_img_url>",
                    Title = "<geofence>",
                    Url = "<gmaps_url>",
                    Username = "<form> <pkmn_name><gender>",
                    ImageUrl = "<tilemaps_url>",
                    Footer = new EmbedMessageFooter
                    {
                        Text = "<guild_name> | <date_time>",
                        IconUrl = "<guild_img_url>"
                    }
                }
            },
            {
                EmbedMessageType.Gyms, new EmbedMessageSettings
                {
                    AvatarUrl = "<gym_url>",
                    Content = "<#team_changed>Gym changed from <old_gym_team_emoji> <old_gym_team> to <gym_team_emoji> <gym_team><br></team_changed><#in_battle>Gym is under attack!<br></in_battle>**Slots Available:** <slots_available><br><#is_ex><ex_gym_emoji> Gym!</is_ex>**[[Google](<gmaps_url>)] [[Apple](<applemaps_url>)] [[Waze](<wazemaps_url>)] [[Scanner](<scanmaps_url>)]**",
                    IconUrl = "<gym_url>",
                    Title = "<geofence>: <gym_name>",
                    Url = "<gmaps_url>",
                    Username = "<gym_name>",
                    ImageUrl = "<tilemaps_url>",
                    Footer = new EmbedMessageFooter
                    {
                        Text = "<guild_name> | <date_time>",
                        IconUrl = "<guild_img_url>"
                    }
                }
            },
            {
                EmbedMessageType.Raids, new EmbedMessageSettings
                {
                    AvatarUrl = "<pkmn_img_url>",
                    Content = "<evolution> <form> <pkmn_name><gender> <costume> Raid Ends: <end_time> (<end_time_left> left)<br>**Perfect CP:** <perfect_cp> / :white_sun_rain_cloud: <perfect_cp_boosted><br>**Worst CP:** <worst_cp> / :white_sun_rain_cloud: <worst_cp_boosted><br>**Types:** <types_emoji> | **Level:** <lvl> | **Team:** <team_emoji><br>**Moveset:** <moveset><br>**Weaknesses:** <weaknesses_emoji><br><#is_ex><ex_emoji> Gym!<br></is_ex>**[[Google](<gmaps_url>)] [[Apple](<applemaps_url>)] [[Waze](<wazemaps_url>)] [[Scanner](<scanmaps_url>)]**",
                    IconUrl = "<pkmn_img_url>",
                    Title = "<geofence>: <gym_name>",
                    Url = "<gmaps_url>",
                    Username = "<form> <pkmn_name><gender> <costume> Raid",
                    ImageUrl = "<tilemaps_url>",
                    Footer = new EmbedMessageFooter
                    {
                        Text = "<guild_name> | <date_time>",
                        IconUrl = "<guild_img_url>"
                    }
                }
            },
            {
                EmbedMessageType.Eggs, new EmbedMessageSettings
                {
                    AvatarUrl = "<pkmn_img_url>",
                    Content = "Hatches: <start_time> (<start_time_left>)<br>**Ends:** <end_time> (<end_time_left> left)<br>**Team:** <team_emoji><br><#is_ex><ex_emoji> Gym!<br></is_ex>**[[Google](<gmaps_url>)] [[Apple](<applemaps_url>)] [[Waze](<wazemaps_url>)] [[Scanner](<scanmaps_url>)]**",
                    IconUrl = "<pkmn_img_url>",
                    Title = "<geofence>: <gym_name>",
                    Url = "<gmaps_url>",
                    Username = "Level <lvl> Egg",
                    ImageUrl = "<tilemaps_url>",
                    Footer = new EmbedMessageFooter
                    {
                        Text = "<guild_name> | <date_time>",
                        IconUrl = "<guild_img_url>"
                    }
                }
            },
            {
                EmbedMessageType.Pokestops, new EmbedMessageSettings
                {
                    AvatarUrl = "<pokestop_url>",
                    Content = "<#has_lure>**Lure Expires** <lure_expire_time> (<lure_expire_time_left> left)<br>**Lure Type:** <lure_type><br></has_lure><#has_invasion>**Expires:** <invasion_expire_time> (<invasion_expire_time_left> left)<br>**Type:** <grunt_type_emoji> | **Gender:** <grunt_gender><br><invasion_encounters><br></has_invasion>**[[Google](<gmaps_url>)] [[Apple](<applemaps_url>)] [[Waze](<wazemaps_url>)] [[Scanner](<scanmaps_url>)]**",
                    IconUrl = "<pokestop_url>",
                    Title = "<geofence>: <pokestop_name>",
                    Url = "<gmaps_url>",
                    Username = "<pokestop_name>",
                    ImageUrl = "<tilemaps_url>",
                    Footer = new EmbedMessageFooter
                    {
                        Text = "<guild_name> | <date_time>",
                        IconUrl = "<guild_img_url>"
                    }
                }
            },
            {
                EmbedMessageType.Quests, new EmbedMessageSettings
                {
                    AvatarUrl = "<quest_reward_img_url>",
                    Content = "**Quest:** <quest_task><br><#has_quest_conditions>**Condition(s):** <quest_conditions><br></has_quest_conditions>**Reward:** <quest_reward><br>**[[Google](<gmaps_url>)] [[Apple](<applemaps_url>)] [[Waze](<wazemaps_url>)] [[Scanner](<scanmaps_url>)]**",
                    IconUrl = "<pokestop_url>",
                    Title = "<geofence>: <pokestop_name>",
                    Url = "<gmaps_url>",
                    Username = "<quest_task>",
                    ImageUrl = "<tilemaps_url>",
                    Footer = new EmbedMessageFooter
                    {
                        Text = "<guild_name> | <date_time>",
                        IconUrl = "<guild_img_url>"
                    }
                }
            },
            {
                EmbedMessageType.Invasions, new EmbedMessageSettings
                {
                    AvatarUrl = "<invasion_img_url>",
                    Content = "<#has_invasion>**Expires:** <invasion_expire_time> (<invasion_expire_time_left> left)<br>**Type:** <grunt_type_emoji> | **Gender:** <grunt_gender><br><invasion_encounters><br></has_invasion>**[[Google](<gmaps_url>)] [[Apple](<applemaps_url>)] [[Waze](<wazemaps_url>)] [[Scanner](<scanmaps_url>)]**",
                    IconUrl = "<pokestop_url>",
                    Title = "<geofence>: <pokestop_name>",
                    Url = "<gmaps_url>",
                    Username = "<pokestop_name>",
                    ImageUrl = "<tilemaps_url>",
                    Footer = new EmbedMessageFooter
                    {
                        Text = "<guild_name> | <date_time>",
                        IconUrl = "<guild_img_url>"
                    }
                }
            },
            {
                EmbedMessageType.Lures, new EmbedMessageSettings
                {
                    AvatarUrl = "<lure_img_url>",
                    Content = "<#has_lure>**Lure Expires:** <lure_expire_time> (<lure_expire_time_left> left)<br>**Lure Type:** <lure_type><br></has_lure>**[[Google](<gmaps_url>)] [[Apple](<applemaps_url>)] [[Waze](<wazemaps_url>)] [[Scanner](<scanmaps_url>)]**",
                    IconUrl = "<pokestop_url>",
                    Title = "<geofence>: <pokestop_name>",
                    Url = "<gmaps_url>",
                    Username = "<pokestop_name>",
                    ImageUrl = "<tilemaps_url>",
                    Footer = new EmbedMessageFooter
                    {
                        Text = "<guild_name> | <date_time>",
                        IconUrl = "<guild_img_url>"
                    }
                }
            },
            {
                EmbedMessageType.Nests, new EmbedMessageSettings
                {
                    AvatarUrl = "<pkmn_img_url>",
                    Content = "**Pokemon:** <pkmn_name><br>**Average Spawns:** <avg_spawns>/h | **Types:** <types_emojis><br>**[[Google](<gmaps_url>)] [[Apple](<applemaps_url>)] [[Waze](<wazemaps_url>)] [[Scanner](<scanmaps_url>)]**",
                    IconUrl = "<pkmn_img_url>",
                    Title = "<geofence>: <nest_name>",
                    Url = "<gmaps_url>",
                    Username = "",
                    ImageUrl = "<tilemaps_url>",
                    Footer = new EmbedMessageFooter
                    {
                        Text = "<guild_name> | <date_time>",
                        IconUrl = "<guild_img_url>"
                    }
                }
            },
            {
                EmbedMessageType.Weather, new EmbedMessageSettings
                {
                    AvatarUrl = "<weather_img_url>",
                    Content = "**Weather Condition:** <weather_condition><br>**Weather Cell ID:** #<id>",
                    IconUrl = "<weather_img_url>",
                    Title = "<geofence>",
                    Url = "<gmaps_url>",
                    Username = "Weather",
                    ImageUrl = "<tilemaps_url>",
                    Footer = new EmbedMessageFooter
                    {
                        Text = "<guild_name> | <date_time>",
                        IconUrl = "<guild_img_url>"
                    }
                }
            }
        };
    }
}