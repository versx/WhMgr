namespace WhMgr.Alarms.Alerts
{
    using System;
    using System.Collections.Generic;

    public class AlertMessage : Dictionary<AlertMessageType, AlertMessageSettings>
    {
        public static readonly AlertMessage Defaults = new AlertMessage
        {
            {
                AlertMessageType.Pokemon, new AlertMessageSettings
                {
                    AvatarUrl = "",
                    Content = "<pkmn_name> <form><gender> <iv> (<atk_iv>/<def_iv>/<sta_iv>) L<lvl><br>**Despawn:** <despawn_time> (<time_left> left)<br>**Details:** CP: <cp> IV: <iv> LV: <lvl><br>**Types:** <types_emoji> | **Size:** <size><br>**Moveset:** <moveset><br><#near_pokestop>**Near Pokestop:** [<pokestop_name>](<pokestop_url>)<br></near_pokestop><#is_ditto>**Catch Pokemon:** <original_pkmn_name><br></is_ditto>**[[Google Maps](<gmaps_url>)] [[Apple Maps](<applemaps_url>)]**",
                    IconUrl = "",
                    Title = "<geofence>",
                    Url = "<gmaps_url>",
                    Username = "<form> <pkmn_name><gender>"
                }
            },
            {
                AlertMessageType.PokemonMissingStats, new AlertMessageSettings
                {
                    AvatarUrl = "",
                    Content = "<pkmn_name> <form><gender><br>**Despawn:** <despawn_time> (<time_left> left)<despawn_time_verified><br>**Types:** <types_emoji><br><#near_pokestop>**Near Pokestop:** [<pokestop_name>](<pokestop_url>)<br></near_pokestop>**[[Google Maps](<gmaps_url>)] [[Apple Maps](<applemaps_url>)]**",
                    IconUrl = "",
                    Title = "<geofence>",
                    Url = "<gmaps_url>",
                    Username = "<form> <pkmn_name><gender>"
                }
            },
            {
                AlertMessageType.Gyms, new AlertMessageSettings
                {
                    AvatarUrl = "",
                    Content = "",
                    IconUrl = "",
                    Title = "<geofence>: <gym_name>",
                    Url = "<gmaps_url>",
                    Username = ""
                }
            },
            {
                AlertMessageType.Raids, new AlertMessageSettings
                {
                    AvatarUrl = "", //TODO: Raid AvatarUrl <pkmn>
                    //Content = "<pkmn_name> Raid Ends: <end_time><br>**Started:** <start_time><br>**Ends:** <end_time> (<end_time_left> left)<br>**Perfect CP:** <perfect_cp> / :white_sun_rain_cloud: <perfect_cp_boosted><br>**Worst CP:** <worst_cp> / :white_sun_rain_cloud: <worst_cp_boosted><br>**Types:** <types_emoji> | **Level:** <lvl><br>**Moveset:** <moveset><br>**Weaknesses:** <weaknesses_emoji><br>**Team:** <team_emoji><br>**[[Google Maps](<gmaps_url>)] [[Apple Maps](<applemaps_url>)]**",
                    Content = "<pkmn_name> Raid Ends: <end_time> (<end_time_left> left)<br>**Perfect CP:** <perfect_cp> / :white_sun_rain_cloud: <perfect_cp_boosted><br>**Worst CP:** <worst_cp> / :white_sun_rain_cloud: <worst_cp_boosted><br>**Types:** <types_emoji> | **Level:** <lvl> | **Team:** <team_emoji><br>**Moveset:** <moveset><br>**Weaknesses:** <weaknesses_emoji><br><#is_ex><ex_emoji> Gym!<br></is_ex>**[[Google Maps](<gmaps_url>)] [[Apple Maps](<applemaps_url>)]**",
                    IconUrl = "", //TODO: Raid IconUrl <pkmn>
                    Title = "<geofence>: <gym_name>",
                    Url = "<gmaps_url>",
                    Username = "<pkmn_form> <pkmn_name> Raid"
                }
            },
            {
                AlertMessageType.Eggs, new AlertMessageSettings
                {
                    AvatarUrl = "", //TODO: Egg AvatarUrl
                    Content = "Hatches: <start_time> (<start_time_left>)<br>**Ends:** <end_time> (<end_time_left> left)<br>**Team:** <team_emoji><br><#is_ex><ex_emoji> Gym!<br></is_ex>**[[Google Maps](<gmaps_url>)] [[Apple Maps](<applemaps_url>)]**", //TODO: Maybe Expire_time_left
                    IconUrl = "", //TODO: Egg IconUrl
                    Title = "<geofence>: <gym_name>",
                    Url = "<gmaps_url>",
                    Username = "Level <raid_lvl> Egg"
                }
            },
            {
                AlertMessageType.Pokestops, new AlertMessageSettings
                {
                    AvatarUrl = "",
                    Content = "<#has_lure>**Lured Until:** <lure_expire_time> (<lure_expire_time_left> left)<br>**Lure Type:** <lure_type><br></has_lure><#has_invasion>**Expires:** <invasion_expire_time> (<invasion_expire_time_left> left)<br>**Type:** <grunt_type_emoji> | **Gender:** <grunt_gender><br><invasion_encounters><br></has_invasion>**[[Google Maps](<gmaps_url>)] [[Apple Maps](<applemaps_url>)]**",
                    IconUrl = "",
                    Title = "<geofence>: <pokestop_name>",
                    Url = "<gmaps_url>",
                    Username = "<pokestop_name>"
                }
            },
            {
                AlertMessageType.Quests, new AlertMessageSettings
                {
                    AvatarUrl = "<quest_reward>", //TODO: QuestReward AvatorUrl
                    Content = "**Quest:** <quest_task><br><#has_quest_conditions>**Condition(s):** <quest_conditions><br></has_quest_conditions>**Reward:** <quest_reward><br>**[[Google Maps](<gmaps_url>)] [[Apple Maps](<applemaps_url>)]**",
                    IconUrl = "<quest_reward>", //TODO: QuestReward IconUrl
                    Title = "<geofence>: <pokestop_name>",
                    Url = "<gmaps_url>",
                    Username = "<quest_task>"
                }
            },
            {
                AlertMessageType.Invasions, new AlertMessageSettings
                {
                    AvatarUrl = "", //TODO: Invasions IconUrl
                    Content = "**Expires:** <invasion_expire_time> (<invasion_expire_time_left> left)<br>**Type:** <grunt_type_emoji> | **Gender:** <grunt_gender><br><invasion_encounters>**[[Google Maps](<gmaps_url>)] [[Apple Maps](<applemaps_url>)]**",
                    IconUrl = "<pokestop_url>",
                    Title = "<geofence>: <pokestop_name>",
                    Url = "<gmaps_url>",
                    Username = "<pokestop_name>"
                }
            },
            {
                AlertMessageType.Lures, new AlertMessageSettings
                {
                    AvatarUrl = "", //TODO: Lures IconUrl
                    Content = "**Lured Until:** <lure_expire_time> (<lure_expire_time_left> left)<br>**Lure Type:** <lure_type><br>**[[Google Maps](<gmaps_url>)] [[Apple Maps](<applemaps_url>)]**",
                    IconUrl = "<pokestop_url>",
                    Title = "<geofence>: <pokestop_name>",
                    Url = "<gmaps_url>",
                    Username = "<pokestop_name>"
                }
            }
        };
    }
}