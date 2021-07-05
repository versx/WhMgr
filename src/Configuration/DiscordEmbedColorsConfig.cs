namespace WhMgr.Configuration
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class DiscordEmbedColorsConfig
    {
        [JsonPropertyName("pokemon")]
        public DiscordEmbedColorPokemon Pokemon { get; set; }

        [JsonPropertyName("raids")]
        public DiscordEmbedColorRaids Raids { get; set; }

        [JsonPropertyName("pokestops")]
        public DiscordEmbedColorPokestop Pokestops { get; set; }

        [JsonPropertyName("weather")]
        public DiscordEmbedColorWeather Weather { get; set; }

        public DiscordEmbedColorsConfig()
        {
            Pokemon = new DiscordEmbedColorPokemon
            {
                IV = new List<DiscordEmbedColorPokemonIV>
                {
                    new DiscordEmbedColorPokemonIV { Minimum = 0, Maximum = 0, Color = "#ffffff" },
                    new DiscordEmbedColorPokemonIV { Minimum = 1, Maximum = 89, Color = "#ffff00" },
                    new DiscordEmbedColorPokemonIV { Minimum = 90, Maximum = 99, Color = "#ffa500" },
                    new DiscordEmbedColorPokemonIV { Minimum = 100, Maximum = 100, Color = "#00ff00" },
                },
                PvP = new List<DiscordEmbedColorPokemonPvP>
                {
                    new DiscordEmbedColorPokemonPvP { Minimum = 1, Maximum = 3, Color = "#000080" },
                    new DiscordEmbedColorPokemonPvP { Minimum = 4, Maximum = 25, Color = "#800080" },
                    new DiscordEmbedColorPokemonPvP { Minimum = 26, Maximum = 100, Color = "#aa2299" },
                }
            };
            Raids = new DiscordEmbedColorRaids();
            Pokestops = new DiscordEmbedColorPokestop();
            Weather = new DiscordEmbedColorWeather();
        }
    }

    public class DiscordEmbedColorPokemon
    {
        [JsonPropertyName("iv")]
        public List<DiscordEmbedColorPokemonIV> IV { get; set; }

        [JsonPropertyName("pvp")]
        public List<DiscordEmbedColorPokemonPvP> PvP { get; set; }

        public DiscordEmbedColorPokemon()
        {
            IV = new List<DiscordEmbedColorPokemonIV>();
            PvP = new List<DiscordEmbedColorPokemonPvP>();
        }
    }

    public class DiscordEmbedColorPokemonIV
    {
        [JsonPropertyName("min")]
        public int Minimum { get; set; }

        [JsonPropertyName("max")]
        public int Maximum { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        public DiscordEmbedColorPokemonIV()
        {
            Minimum = 0;
            Maximum = 100;
            Color = "#ffffff";
        }
    }

    public class DiscordEmbedColorPokemonPvP
    {
        [JsonPropertyName("min")]
        public int Minimum { get; set; }

        [JsonPropertyName("max")]
        public int Maximum { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        public DiscordEmbedColorPokemonPvP()
        {
            Minimum = 0;
            Maximum = 100;
            Color = "#aa2299";
        }
    }

    public class DiscordEmbedColorRaids
    {
        [JsonPropertyName("1")]
        public string Level1 { get; set; }

        [JsonPropertyName("2")]
        public string Level2 { get; set; }

        [JsonPropertyName("3")]
        public string Level3 { get; set; }

        [JsonPropertyName("4")]
        public string Level4 { get; set; }

        [JsonPropertyName("5")]
        public string Level5 { get; set; }

        [JsonPropertyName("6")]
        public string Level6 { get; set; }

        [JsonPropertyName("ex")]
        public string Ex { get; set; }

        public DiscordEmbedColorRaids()
        {
            Level1 = "#ff69b4";
            Level2 = "#ff69b4";
            Level3 = "#ffff00";
            Level4 = "#ffff00";
            Level5 = "#800080";
            Level6 = "#a52a2a";
            Ex = "#2c2f33";
        }
    }

    public class DiscordEmbedColorPokestop
    {
        [JsonPropertyName("quests")]
        public string Quests { get; set; }

        [JsonPropertyName("lures")]
        public DiscordEmbedColorPokestopLures Lures { get; set; }

        [JsonPropertyName("invasions")]
        public string Invasions { get; set; }

        public DiscordEmbedColorPokestop()
        {
            Quests = "#ffa500";
            Lures = new DiscordEmbedColorPokestopLures();
            Invasions = "#ff0000";
        }
    }

    public class DiscordEmbedColorPokestopLures
    {
        [JsonPropertyName("normal")]
        public string Normal { get; set; }

        [JsonPropertyName("glacial")]
        public string Glacial { get; set; }

        [JsonPropertyName("mossy")]
        public string Mossy { get; set; }

        [JsonPropertyName("magnetic")]
        public string Magnetic { get; set; }

        [JsonPropertyName("rainy")]
        public string Rainy { get; set; }

        public DiscordEmbedColorPokestopLures()
        {
            Normal = "#ff69b4";
            Glacial = "#6495ed";
            Mossy = "#507d2a";
            Magnetic = "#808080";
            Rainy = "#1da7de";
        }
    }

    public class DiscordEmbedColorWeather
    {
        [JsonPropertyName("clear")]
        public string Clear { get; set; }

        [JsonPropertyName("cloudy")]
        public string Cloudy { get; set; }

        [JsonPropertyName("fog")]
        public string Fog { get; set; }

        [JsonPropertyName("partlyCloudy")]
        public string PartlyCloudy { get; set; }

        [JsonPropertyName("rain")]
        public string Rain { get; set; }

        [JsonPropertyName("snow")]
        public string Snow { get; set; }

        [JsonPropertyName("windy")]
        public string Windy { get; set; }

        public DiscordEmbedColorWeather()
        {
            Clear = "#ffff00";
            Cloudy = "#99aab5";
            Fog = "#9a9a9a";
            PartlyCloudy = "#808080";
            Rain = "#0000ff";
            Snow = "#ffffff";
            Windy = "#800080";
        }
    }
}