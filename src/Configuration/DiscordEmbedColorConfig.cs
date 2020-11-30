namespace WhMgr.Configuration
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class DiscordEmbedColorConfig
    {
        [JsonProperty("pokemon")]
        public DiscordEmbedColorPokemon Pokemon { get; set; }

        [JsonProperty("raids")]
        public DiscordEmbedColorRaids Raids { get; set; }

        [JsonProperty("pokestops")]
        public DiscordEmbedColorPokestop Pokestops { get; set; }

        [JsonProperty("weather")]
        public DiscordEmbedColorWeather Weather { get; set; }

        public DiscordEmbedColorConfig()
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
        [JsonProperty("iv")]
        public List<DiscordEmbedColorPokemonIV> IV { get; set; }

        [JsonProperty("pvp")]
        public List<DiscordEmbedColorPokemonPvP> PvP { get; set; }

        public DiscordEmbedColorPokemon()
        {
            IV = new List<DiscordEmbedColorPokemonIV>();
            PvP = new List<DiscordEmbedColorPokemonPvP>();
        }
    }

    public class DiscordEmbedColorPokemonIV
    {
        [JsonProperty("min")]
        public int Minimum { get; set; }

        [JsonProperty("max")]
        public int Maximum { get; set; }

        [JsonProperty("color")]
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
        [JsonProperty("min")]
        public int Minimum { get; set; }

        [JsonProperty("max")]
        public int Maximum { get; set; }

        [JsonProperty("color")]
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
        [JsonProperty("1")]
        public string Level1 { get; set; }

        [JsonProperty("2")]
        public string Level2 { get; set; }

        [JsonProperty("3")]
        public string Level3 { get; set; }

        [JsonProperty("4")]
        public string Level4 { get; set; }

        [JsonProperty("5")]
        public string Level5 { get; set; }

        [JsonProperty("6")]
        public string Level6 { get; set; }

        [JsonProperty("ex")]
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
        [JsonProperty("quests")]
        public string Quests { get; set; }

        [JsonProperty("lures")]
        public DiscordEmbedColorPokestopLures Lures { get; set; }

        [JsonProperty("invasions")]
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
        [JsonProperty("normal")]
        public string Normal { get; set; }

        [JsonProperty("glacial")]
        public string Glacial { get; set; }

        [JsonProperty("mossy")]
        public string Mossy { get; set; }

        [JsonProperty("magnetic")]
        public string Magnetic { get; set; }

        public DiscordEmbedColorPokestopLures()
        {
            Normal = "#ff69b4";
            Glacial = "#6495ed";
            Mossy = "#507d2a";
            Magnetic = "#808080";
        }
    }

    public class DiscordEmbedColorWeather
    {
        [JsonProperty("clear")]
        public string Clear { get; set; }

        [JsonProperty("cloudy")]
        public string Cloudy { get; set; }

        [JsonProperty("fog")]
        public string Fog { get; set; }

        [JsonProperty("partlyCloudy")]
        public string PartlyCloudy { get; set; }

        [JsonProperty("rain")]
        public string Rain { get; set; }

        [JsonProperty("snow")]
        public string Snow { get; set; }

        [JsonProperty("windy")]
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