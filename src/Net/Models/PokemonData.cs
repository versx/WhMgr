namespace WhMgr.Net.Models
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    using ServiceStack.DataAnnotations;

    using WhMgr.Data;
    using WhMgr.Extensions;

    [Alias("pokemon")]
    public sealed class PokemonData
    {
        public const string WebHookHeader = "pokemon";

        //TODO: Add ditto disguises to external file
        private static readonly List<int> DittoDisguises = new List<int> { 13, 46, 48, 163, 165, 167, 187, 223, 273, 293, 300, 316, 322, 399 };

        #region Properties

        [
            JsonProperty("pokemon_id"),
            Alias("pokemon_id")
        ]
        public int Id { get; set; }

        [
            JsonProperty("cp"),
            Alias("cp")
        ]
        public string CP { get; set; }

        [
            JsonIgnore,
            Ignore
        ]
        public string IV
        {
            get
            {
                if (!int.TryParse(Stamina, out int sta) ||
                    !int.TryParse(Attack, out int atk) ||
                    !int.TryParse(Defense, out int def))
                {
                    return "?";
                }

                return Math.Round((sta + atk + def) * 100.0 / 45.0, 1) + "%";
            }
        }

        [
            JsonIgnore,
            Ignore
        ]
        public string IVRounded
        {
            get
            {
                /*
                var iv = GetIV(Attack, Defense, Stamina);
                return iv == -1 ? "?" : iv + "%";
                */
                if (!int.TryParse(Stamina, out int sta) ||
                    !int.TryParse(Attack, out int atk) ||
                    !int.TryParse(Defense, out int def))
                {
                    return "?";
                }

                return Math.Round((double)(sta + atk + def) * 100 / 45) + "%";
            }
        }

        [
            JsonProperty("individual_stamina"),
            Alias("sta_iv")
        ]
        public string Stamina { get; set; }

        [
            JsonProperty("individual_attack"),
            Alias("atk_iv")
        ]
        public string Attack { get; set; }

        [
            JsonProperty("individual_defense"),
            Alias("def_iv")
        ]
        public string Defense { get; set; }

        [
            JsonProperty("gender"),
            Alias("gender")
        ]
        public PokemonGender Gender { get; set; }

        [
            JsonProperty("costume"),
            Alias("costume")
        ]
        public int Costume { get; set; }

        [
            JsonProperty("pokemon_level"),
            Alias("level")
        ]
        public string Level { get; set; }

        [
            JsonProperty("latitude"),
            Alias("lat")
        ]
        public double Latitude { get; set; }

        [
            JsonProperty("longitude"),
            Alias("lon")
        ]
        public double Longitude { get; set; }

        [
            JsonProperty("move_1"),
            Alias("move_1")
        ]
        public string FastMove { get; set; }

        [
            JsonProperty("move_2"),
            Alias("move_2")
        ]
        public string ChargeMove { get; set; }

        [
            JsonProperty("height"),
            Alias("size")
        ]
        public string Height { get; set; }

        [
            JsonProperty("weight"),
            Alias("weight")
        ]
        public string Weight { get; set; }

        [
            JsonProperty("encounter_id"),
            Alias("id")
        ]
        public string EncounterId { get; set; }

        [
            JsonProperty("spawnpoint_id"),
            Alias("spawn_id")
        ]
        public string SpawnpointId { get; set; }

        [
            JsonProperty("disappear_time"),
            Alias("expire_timestamp")
        ]
        public long DisappearTime { get; set; }

        [
            JsonProperty("disappear_time_verified"),
            Alias("expire_timestamp_verified")
        ]
        public bool DisappearTimeVerified { get; set; }

        //[JsonProperty("seconds_until_despawn")]
        //public int SecondsUntilDespawn { get; set; }

        [
            JsonProperty("first_seen"),
            Alias("first_seen_timestamp")
        ]
        public long FirstSeen { get; set; }

        [
            JsonProperty("last_modified_time"),
            Alias("changed")
        ]
        public long LastModified { get; set; }

        [
            JsonProperty("pokestop_id"),
            Alias("pokestop_id")
        ]
        public string PokestopId { get; set; }

        [
            JsonProperty("weather"),
            Alias("weather")
        ]
        public WeatherType? Weather { get; set; }

        [
            JsonProperty("form"),
            Alias("form")
        ]
        public int FormId { get; set; }

        [
            JsonProperty("shiny"),
            Alias("shiny")
        ]
        public bool? Shiny { get; set; }

        [
            JsonProperty("username"),
            Alias("username")
        ]
        public string Username { get; set; }

        [
            JsonProperty("updated"),
            Alias("updated")
        ]
        public long Updated { get; set; }

        [
            JsonIgnore,
            Ignore
        ]
        public DateTime DespawnTime { get; private set; }

        [
            JsonIgnore,
            Ignore
        ]
        public TimeSpan SecondsLeft { get; private set; }

        [
            JsonIgnore,
            Ignore
        ]
        public DateTime FirstSeenTime { get; set; }

        [
            JsonIgnore,
            Ignore
        ]
        public DateTime LastModifiedTime { get; set; }

        [
            JsonIgnore,
            Ignore
        ]
        public DateTime UpdatedTime { get; set; }

        [
            JsonIgnore,
            Ignore
        ]
        public PokemonSize? Size
        {
            get
            {
                if (float.TryParse(Height, out var height) && float.TryParse(Weight, out var weight))
                {
                    return Id.GetSize(height, weight);
                }

                return null;
            }
        }

        [
            JsonIgnore,
            Ignore
        ]
        public bool IsDitto
        {
            get
            {
                //if (!int.TryParse(Level, out var level))
                //    return false;

                var isUnderLevel6 = IsUnderLevel(6);
                var isStatsUnder4 = IsStatsBelow4;
                //var isUnderLevel31 = IsUnderLevel(31);
                //var isAboveLevel30 = IsAboveLevel(30);
                //var isWindOrRain = Weather == WeatherType.Windy || Weather == WeatherType.Rain;
                //var isWindOrCloudy = Weather == WeatherType.Windy || Weather == WeatherType.Cloudy;
                //var isNotBoosted = Weather == WeatherType.None && isAboveLevel30;

                var check1 = Weather != WeatherType.None &&
                       isUnderLevel6 &&
                       isStatsUnder4 &&
                       DittoDisguises.Contains(Id);
                //var check2 = (Id == 193 && (isNotBoosted || (isWindOrRain && isUnderLevel6)));
                //var check3 = (Id == 193 && isWindOrRain && isUnderLevel31 && IsStatsBelow4);
                //var check4 = (Id == 41 && ((Weather == WeatherType.None && isAboveLevel30) || (isWindOrCloudy && isUnderLevel6)));
                //var check5 = (Id == 41 && isWindOrCloudy && isUnderLevel31 && IsStatsBelow4);
                //var check6 = (Id == 16 || Id == 163 || Id == 276) && Weather == WeatherType.Windy && (isUnderLevel6 || (isUnderLevel31 && IsStatsBelow4));

                return check1;// || check2 || check3 || check4 || check5 || check6;

                /*
                if pokemon_id == 193:
                    if (is_no_weather_boost and is_above_level_30) or ((is_windy or is_raining) and is_below_level_6):
                        make_ditto()
                        return
                    if (is_windy or is_raining) and is_below_level_31 and stat_below_4:
                        make_ditto()
                        return
                if pokemon_id == 41:
                    if (is_no_weather_boost and is_above_level_30) or ((is_windy or is_cloudy) and is_below_level_6):
                        make_ditto()
                    elif (is_windy or is_cloudy) and is_below_level_31 and stat_below_4:
                        make_ditto()
                if (pokemon_id == 16 or pokemon_id == 163 or pokemon_id == 276) and is_windy:
                    if is_below_level_6 or (is_below_level_31 and stat_below_4):
                        make_ditto()
                 */
            }
        }

		[
		    JsonIgnore,
			Ignore
		]
        public bool IsStatsBelow4
        {
            get
            {
                return int.TryParse(Attack, out var atk) &&
                       int.TryParse(Defense, out var def) &&
                       int.TryParse(Stamina, out var sta) &&
                       (atk < 4 || def < 4 || sta < 4);
            }
        }

        [
            JsonIgnore,
            Ignore
        ]
        public int OriginalPokemonId { get; set; }

		[
		    JsonIgnore,
			Ignore
		]
        public bool MatchesGreatLeague
        {
            get
            {
                if (!Database.Instance.PvPGreat.ContainsKey(Id))
                    return false;

                var greatPokemon = Database.Instance.PvPGreat[Id];
                return greatPokemon.Exists(x =>
                {
                    if (int.TryParse(Attack, out var atk) && atk == x.IVs.Attack &&
                        int.TryParse(Defense, out var def) && def == x.IVs.Defense &&
                        int.TryParse(Stamina, out var sta) && sta == x.IVs.Stamina)
                    {
                        return true;
                    }
                    return false;
                });
            }
        }

		[
		    JsonIgnore,
			Ignore
		]
        public bool MatchesUltraLeague
        {
            get
            {
                if (!Database.Instance.PvPUltra.ContainsKey(Id))
                    return false;

                var ultraPokemon = Database.Instance.PvPUltra[Id];
                return ultraPokemon.Exists(x =>
                {
                    if (int.TryParse(Attack, out var atk) && atk == x.IVs.Attack &&
                        int.TryParse(Defense, out var def) && def == x.IVs.Defense &&
                        int.TryParse(Stamina, out var sta) && sta == x.IVs.Stamina)
                    {
                        return true;
                    }
                    return false;
                });
            }
        }


        [
            JsonIgnore,
            Ignore
        ]
        public bool IsMissingStats => string.IsNullOrEmpty(Level);

        #endregion

        #region Constructor

        public PokemonData()
        {
            SetDespawnTime();
        }

        #endregion

        public void SetDespawnTime()
        {
            DespawnTime = DisappearTime.FromUnix();
            //if (TimeZoneInfo.Local.IsDaylightSavingTime(DespawnTime))
            //{
            //    DespawnTime = DespawnTime.AddHours(1); //DST
            //}
            SecondsLeft = DespawnTime.Subtract(DateTime.Now);

            FirstSeenTime = FirstSeen.FromUnix();
            //if (TimeZoneInfo.Local.IsDaylightSavingTime(FirstSeenTime))
            //{
            //    FirstSeenTime = FirstSeenTime.AddHours(1); //DST
            //}

            LastModifiedTime = LastModified.FromUnix();
            //if (TimeZoneInfo.Local.IsDaylightSavingTime(LastModifiedTime))
            //{
            //    LastModifiedTime = LastModifiedTime.AddHours(1);
            //}

            UpdatedTime = Updated.FromUnix();
            //if (TimeZoneInfo.Local.IsDaylightSavingTime(Updated))
            //{
            //    UpdatedTime = Updated.AddHours(1);
            //}
        }

        public bool IsUnderLevel(int targetLevel)
        {
            return int.TryParse(Level, out var level) && level < targetLevel;
        }

        public bool IsAboveLevel(int targetLevel)
        {
            return int.TryParse(Level, out var level) && level >= targetLevel;
        }

        public static double GetIV(string attack, string defense, string stamina)
        {
            if (!int.TryParse(attack, out int atk) ||
                !int.TryParse(defense, out int def) ||
                !int.TryParse(stamina, out int sta))
            {
                return -1;
            }

            return Math.Round((double)(sta + atk + def) * 100 / 45);
        }
    }
}