namespace WhMgr.Net.Models
{
    using System;

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

                return Convert.ToString((sta + atk + def) * 100 / 45) + "%";
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

        [JsonProperty("disappear_time")]
        public long DisappearTime { get; set; }

        [JsonProperty("seconds_until_despawn")]
        public int SecondsUntilDespawn { get; set; }

        [JsonIgnore]
        public DateTime DespawnTime { get; private set; }

        [JsonIgnore]
        public TimeSpan SecondsLeft { get; private set; }

        [JsonProperty("form")]
        public string FormId { get; set; }

        [JsonIgnore]
        public bool IsMissingStats
        {
            get
            {
                return Attack == "?" ||
                       Defense == "?" ||
                       Stamina == "?" ||
                       Level == "?" ||
                       string.IsNullOrEmpty(Level);
            }
        }

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
        }
    }
}