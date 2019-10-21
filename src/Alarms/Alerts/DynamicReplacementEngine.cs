namespace WhMgr.Alarms.Alerts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class DynamicReplacementEngine
    {
        //public const string DefaultPokemonAlarmText = "<pkmn_name> <form> (<atk_iv>/<def_iv>/<sta_iv>) L<lvl><br>**Despawn:** <despawn_time> (<time_left>)<br>**Details:** CP: <cp> IV: <iv> LV: <lvl><br>**Types:** <types_emoji> | **Size:** <size><br>**Moveset:** <moveset><br>**[[Google Maps](<gmaps_link>)] [[Apple Maps](<applemaps_link>)]**";
        //public const string DefaultPokemonMissingStatsAlarmText = "<pkmn_name> <form><br>**Despawn:** <despawn_time> (<time_left>)<br>**Types:** <types_emoji> | **Size:** <size><br>**[[Google Maps](<gmaps_link>)] [[Apple Maps](<applemaps_link>)]**";

        public static string ReplaceText(string alarmText, IReadOnlyDictionary<string, string> pkmnInfo)
        {
            var placeHolder = alarmText;
            var keys = pkmnInfo.Keys.ToList();
            for (var i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                placeHolder = placeHolder.Replace($"<{key}>", pkmnInfo[key]);
            }
            return placeHolder;
        }
    }
}