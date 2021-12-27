namespace WhMgr.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class TwilioConfig
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("accountSid")]
        public string AccountSid { get; set; }

        [JsonPropertyName("authToken")]
        public string AuthToken { get; set; }

        [JsonPropertyName("from")]
        public string FromNumber { get; set; }

        [JsonPropertyName("userIds")]
        public List<ulong> UserIds { get; set; }

        [JsonPropertyName("roleIds")]
        public List<ulong> RoleIds { get; set; }

        [JsonPropertyName("pokemonIds")]
        public List<uint> PokemonIds { get; set; }

        [JsonPropertyName("minIV")]
        public int MinimumIV { get; set; }

        public TwilioConfig()
        {
            UserIds = new List<ulong>();
            PokemonIds = new List<uint>
            {
                201, // Unown
                480, // Uxie
                481, // Mesprite
                482, // Azelf
                443, // Gible
                444, // Gabite
                445, // Garchomp
                633, // Deino
                634, // Zweilous
                635, // Hydreigon
                610, // Axew
                611, // Fraxure
                612, // Haxorus
            };
            MinimumIV = 100;
        }
    }
}
