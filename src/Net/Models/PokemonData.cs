namespace WhMgr.Net.Models
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    using WhMgr.Extensions;

    public sealed class PokemonData
    {
        public const string WebHookHeader = "pokemon";

        #region Properties

        [JsonProperty("pokemon_id")]
        public int Id { get; set; }

        [JsonProperty("cp")]
        public string CP { get; set; }

        [JsonIgnore]
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

        [JsonIgnore]
        public string IVRounded
        {
            get
            {
                if (!int.TryParse(Stamina, out int sta) ||
                    !int.TryParse(Attack, out int atk) ||
                    !int.TryParse(Defense, out int def))
                {
                    return "?";
                }

                return Math.Round((double)(sta + atk + def) * 100 / 45) + "%";
            }
        }

        [JsonProperty("individual_stamina")]
        public string Stamina { get; set; }

        [JsonProperty("individual_attack")]
        public string Attack { get; set; }

        [JsonProperty("individual_defense")]
        public string Defense { get; set; }

        [JsonProperty("gender")]
        public PokemonGender Gender { get; set; }

        [JsonProperty("costume")]
        public int Costume { get; set; }

        [JsonProperty("pokemon_level")]
        public string Level { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("move_1")]
        public string FastMove { get; set; }

        [JsonProperty("move_2")]
        public string ChargeMove { get; set; }

        [JsonProperty("height")]
        public string Height { get; set; }

        [JsonProperty("weight")]
        public string Weight { get; set; }

        [JsonProperty("encounter_id")]
        public string EncounterId { get; set; }

        [JsonProperty("spawnpoint_id")]
        public string SpawnpointId { get; set; }

        [JsonProperty("disappear_time")]
        public long DisappearTime { get; set; }

        [JsonProperty("disappear_time_verified")]
        public bool DisappearTimeVerified { get; set; }

        //[JsonProperty("seconds_until_despawn")]
        //public int SecondsUntilDespawn { get; set; }

        [JsonProperty("first_seen")]
        public long FirstSeen { get; set; }

        [JsonProperty("last_modified_time")]
        public long LastModified { get; set; }

        [JsonProperty("pokestop_id")]
        public string PokestopId { get; set; }

        [JsonProperty("weather")]
        public WeatherType Weather { get; set; }

        [JsonIgnore]
        public DateTime DespawnTime { get; private set; }

        [JsonIgnore]
        public TimeSpan SecondsLeft { get; private set; }

        [JsonIgnore]
        public DateTime FirstSeenTime { get; set; }

        [JsonIgnore]
        public DateTime LastModifiedTime { get; set; }

        [JsonProperty("form")]
        public int FormId { get; set; }

        [JsonIgnore]
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

        [JsonIgnore]
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
            if (TimeZoneInfo.Local.IsDaylightSavingTime(DespawnTime))
            {
                DespawnTime = DespawnTime.AddHours(1); //DST
            }
            SecondsLeft = DespawnTime.Subtract(DateTime.Now);

            FirstSeenTime = FirstSeen.FromUnix();
            if (TimeZoneInfo.Local.IsDaylightSavingTime(FirstSeenTime))
            {
                FirstSeenTime = FirstSeenTime.AddHours(1); //DST
            }

            LastModifiedTime = LastModified.FromUnix();
            if (TimeZoneInfo.Local.IsDaylightSavingTime(LastModifiedTime))
            {
                LastModifiedTime = LastModifiedTime.AddHours(1);
            }
        }
    }
}