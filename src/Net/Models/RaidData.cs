namespace WhMgr.Net.Models
{
    using System;
    using System.Collections.Generic;

    using DSharpPlus;
    using DSharpPlus.Entities;
    using Newtonsoft.Json;

    using WhMgr.Alarms.Alerts;
    using WhMgr.Alarms.Models;
    using WhMgr.Configuration;
    using WhMgr.Data;
    using WhMgr.Extensions;
    using WhMgr.Utilities;

    public sealed class RaidData
    {
        public const string WebHookHeader = "raid";

        #region Properties

        [JsonProperty("gym_id")]
        public string GymId { get; set; }

        [JsonProperty("gym_name")]
        public string GymName { get; set; }

        [JsonProperty("gym_url")]
        public string GymUrl { get; set; }

        [JsonProperty("pokemon_id")]
        public int PokemonId { get; set; }

        [JsonProperty("team_id")]
        public PokemonTeam Team { get; set; } = PokemonTeam.Neutral;

        [JsonProperty("level")]
        public string Level { get; set; }

        [JsonProperty("cp")]
        public string CP { get; set; }

        [JsonProperty("move_1")]
        public int FastMove { get; set; }

        [JsonProperty("move_2")]
        public int ChargeMove { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("start")]
        public long Start { get; set; }

        [JsonProperty("end")]
        public long End { get; set; }

        [JsonProperty("ex_raid_eligible")]
        public bool IsExEligible { get; set; }

        [JsonProperty("sponsor_id")]
        public bool SponsorId { get; set; }

        [JsonProperty("form")]
        public int Form { get; set; }

        [JsonProperty("gender")]
        public PokemonGender Gender { get; set; }

        [JsonIgnore]
        public DateTime StartTime { get; private set; }

        [JsonIgnore]
        public DateTime EndTime { get; private set; }

        [JsonIgnore]
        public bool IsEgg => PokemonId == 0;

        [JsonIgnore]
        public List<PokemonType> Weaknesses
        {
            get
            {
                if (Database.Instance.Pokemon.ContainsKey(PokemonId) && !IsEgg)
                {
                    var list = new List<PokemonType>();
                    Database.Instance.Pokemon[PokemonId].Types.ForEach(x => x.GetWeaknesses().ForEach(y => list.Add(y)));
                    return list;
                }

                return null;
            }
        }

        [JsonIgnore]
        public bool IsMissingStats => FastMove == 0;

        #endregion

        public RaidData()
        {
            SetTimes();
        }

        public void SetTimes()
        {
            StartTime = Start.FromUnix();
            //if (TimeZoneInfo.Local.IsDaylightSavingTime(StartTime))
            //{
            //    StartTime = StartTime.AddHours(1); //DST
            //}

            EndTime = End.FromUnix();
            //if (TimeZoneInfo.Local.IsDaylightSavingTime(EndTime))
            //{
            //    EndTime = EndTime.AddHours(1); //DST
            //}
        }

        public DiscordEmbed GenerateRaidMessage(DiscordClient client, WhConfig whConfig, AlarmObject alarm, string city, string pokemonRaidImageUrl)
        {
            var alertType = PokemonId > 0 ? AlertMessageType.Raids : AlertMessageType.Eggs;
            var alert = alarm?.Alerts[alertType] ?? AlertMessage.Defaults[alertType];
            var properties = GetProperties(client, whConfig, city);
            var img = IsEgg ? string.Format(whConfig.Urls.EggImage, Level) : PokemonId.GetPokemonImage(pokemonRaidImageUrl, PokemonGender.Unset, Form);
            var eb = new DiscordEmbedBuilder
            {
                Title = DynamicReplacementEngine.ReplaceText(alert.Title, properties),
                Url = DynamicReplacementEngine.ReplaceText(alert.Url, properties),
                ImageUrl = properties["tilemaps_url"],
                ThumbnailUrl = img,
                Description = DynamicReplacementEngine.ReplaceText(alert.Content, properties),
                Color = Level.BuildRaidColor(),
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"versx | {DateTime.Now}",
                    IconUrl = client.Guilds.ContainsKey(whConfig.GuildId) ? client.Guilds[whConfig.GuildId]?.IconUrl : string.Empty
                }
            };
            return eb.Build();
        }

        private IReadOnlyDictionary<string, string> GetProperties(DiscordClient client, WhConfig whConfig, string city)
        {
            var pkmnInfo = Database.Instance.Pokemon[PokemonId];
            var form = PokemonId.GetPokemonForm(Form.ToString());
            var gender = Gender.GetPokemonGenderIcon();
            var level = Level;
            //var weather = raid.Weather?.ToString();
            //var weatherEmoji = string.Empty;
            //if (raid.Weather.HasValue && Strings.WeatherEmojis.ContainsKey(raid.Weather.Value) && raid.Weather != WeatherType.None)
            //{
            //    var isWeatherBoosted = pkmnInfo.IsWeatherBoosted(raid.Weather.Value);
            //    var isWeatherBoostedText = isWeatherBoosted ? " (Boosted)" : null;
            //    weatherEmoji = Strings.WeatherEmojis[raid.Weather.Value] + isWeatherBoostedText;
            //}
            var move1 = string.Empty;
            var move2 = string.Empty;
            if (Database.Instance.Movesets.ContainsKey(FastMove))
            {
                move1 = Database.Instance.Movesets[FastMove].Name;
            }
            if (Database.Instance.Movesets.ContainsKey(ChargeMove))
            {
                move2 = Database.Instance.Movesets[ChargeMove].Name;
            }
            var type1 = pkmnInfo?.Types?[0];
            var type2 = pkmnInfo?.Types?.Count > 1 ? pkmnInfo?.Types?[1] : PokemonType.None;
            var type1Emoji = pkmnInfo?.Types?[0].GetTypeEmojiIcons(client, whConfig.GuildId);
            var type2Emoji = pkmnInfo?.Types?.Count > 1 ? pkmnInfo?.Types?[1].GetTypeEmojiIcons(client, whConfig.GuildId) : string.Empty;
            var typeEmojis = $"{type1Emoji} {type2Emoji}";
            var weaknesses = Weaknesses == null ? string.Empty : string.Join(", ", Weaknesses);
            var weaknessesEmoji = Weaknesses.GetWeaknessEmojiIcons(client, whConfig.GuildId);
            var weaknessesEmojiFormatted = weaknessesEmoji;
            var perfectRange = PokemonId.MaxCpAtLevel(20);
            var boostedRange = PokemonId.MaxCpAtLevel(25);
            var worstRange = PokemonId.MinCpAtLevel(20);
            var worstBoosted = PokemonId.MinCpAtLevel(25);
            var exEmojiId = client.Guilds.ContainsKey(whConfig.GuildId) ? client.Guilds[whConfig.GuildId].GetEmojiId("ex") : 0;
            var exEmoji = exEmojiId > 0 ? $"<:ex:{exEmojiId}>" : "EX";
            var teamEmojiId = client.Guilds[whConfig.GuildId].GetEmojiId(Team.ToString().ToLower());
            var teamEmoji = teamEmojiId > 0 ? $"<:{Team.ToString().ToLower()}:{teamEmojiId}>" : Team.ToString();

            var gmapsLink = string.Format(Strings.GoogleMaps, Latitude, Longitude);
            var appleMapsLink = string.Format(Strings.AppleMaps, Latitude, Longitude);
            var staticMapLink = string.Format(whConfig.Urls.StaticMap, Latitude, Longitude);
            var gmapsLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? gmapsLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, gmapsLink);
            var appleMapsLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? appleMapsLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, appleMapsLink);
            var gmapsStaticMapLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? staticMapLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, staticMapLink);

            const string defaultMissingValue = "?";
            var dict = new Dictionary<string, string>
            {
                //Raid boss properties
                { "pkmn_id", PokemonId.ToString() },
                { "pkmn_name", pkmnInfo.Name },
                { "form", form },
                { "form_id", Form.ToString() },
                { "form_id_3", Form.ToString("D3") },
                { "is_egg", Convert.ToString(IsEgg) },
                { "is_ex", Convert.ToString(IsExEligible) },
                { "ex_emoji", exEmoji },
                { "team", Team.ToString() },
                { "team_emoji", teamEmoji },
                { "cp", CP ?? defaultMissingValue },
                { "lvl", level ?? defaultMissingValue },
                { "gender", gender ?? defaultMissingValue },
                { "move_1", move1 ?? defaultMissingValue },
                { "move_2", move2 ?? defaultMissingValue },
                { "moveset", $"{move1}/{move2}" },
                { "type_1", type1?.ToString() ?? defaultMissingValue },
                { "type_2", type2?.ToString() ?? defaultMissingValue },
                { "type_1_emoji", type1Emoji },
                { "type_2_emoji", type2Emoji },
                { "types", $"{type1}/{type2}" },
                { "types_emoji", typeEmojis },
                { "weaknesses", weaknesses },
                { "weaknesses_emoji", weaknessesEmojiFormatted },
                { "perfect_cp", perfectRange.ToString() },
                { "perfect_cp_boosted", boostedRange.ToString() },
                { "worst_cp", worstRange.ToString() },
                { "worst_cp_boosted", worstBoosted.ToString() },

                //Time properties
                { "start_time", StartTime.ToLongTimeString() },
                { "start_time_left", DateTime.Now.GetTimeRemaining(StartTime).ToReadableStringNoSeconds() },
                { "end_time", EndTime.ToLongTimeString() },
                { "end_time_left", EndTime.GetTimeRemaining().ToReadableStringNoSeconds() },

                //Location properties
                { "geofence", city ?? defaultMissingValue },
                { "lat", Latitude.ToString() },
                { "lng", Longitude.ToString() },
                { "lat_5", Math.Round(Latitude, 5).ToString() },
                { "lng_5", Math.Round(Longitude, 5).ToString() },

                //Location links
                { "tilemaps_url", gmapsStaticMapLink },
                { "gmaps_url", gmapsLocationLink },
                { "applemaps_url", appleMapsLocationLink },

                //Gym properties
                { "gym_id", GymId },
                { "gym_name", GymName },
                { "gym_url", GymUrl },

                //Misc properties
                { "br", "\r\n" }
            };
            return dict;
        }
    }
}