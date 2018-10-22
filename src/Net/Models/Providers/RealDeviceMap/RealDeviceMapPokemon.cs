namespace WhMgr.Net.Models.Providers
{
    using System;

    using Newtonsoft.Json;
    using WhMgr.Extensions;

    public class RealDeviceMapPokemon : IMapProviderPokemon
    {
        public const string WebHookHeader = "pokemon";

        #region Properties

        [JsonProperty("pokemon_id")]
        public int Id { get; set; }

        [JsonProperty("cp")]
        public string CP { get; set; }

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

        #endregion

        #region Constructor

        public RealDeviceMapPokemon()
        {
            SetDespawnTime();
        }

        #endregion

        public void SetDespawnTime()
        {
            var disappear = DisappearTime.FromUnix();
            var secondsLeft = disappear.Subtract(DateTime.Now);
            DespawnTime = disappear;
            SecondsLeft = secondsLeft;
        }
    }
}