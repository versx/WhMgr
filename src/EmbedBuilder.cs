namespace WhMgr
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web;

    using DSharpPlus;
    using DSharpPlus.Entities;

    using Newtonsoft.Json;

    using ServiceStack.OrmLite;

    using WhMgr.Alarms.Alerts;
    using WhMgr.Alarms.Models;
    using WhMgr.Configuration;
    using WhMgr.Data;
    using WhMgr.Data.Models;
    using WhMgr.Diagnostics;
    using WhMgr.Extensions;
    using WhMgr.Localization;
    using WhMgr.Net.Models;

    public class EmbedBuilder
    {
        #region Variables

        private static readonly IEventLogger _logger = EventLogger.GetLogger();

        private readonly DiscordClient _client;
        private readonly WhConfig _whConfig;
        private readonly Translator _lang;

        #endregion

        #region Constructor

        public EmbedBuilder(DiscordClient client, WhConfig config, Translator lang)
        {
            _logger.Trace($"EmbedBuilder::EmbedBuilder");

            _client = client;
            _whConfig = config;
            _lang = lang;
        }

        #endregion

        #region Public Methods

        public DiscordEmbed BuildPokemonMessage(PokemonData pokemon, string city, string pokemonImageUrl)
        {
            //_logger.Trace($"EmbedBuilder::BuildPokemonMessage [Pokemon={pokemon.Id}, City={city}]");

            var db = Database.Instance;
            if (!db.Pokemon.ContainsKey(pokemon.Id))
                return null;

            var pkmn = db.Pokemon[pokemon.Id];
            if (pkmn == null)
            {
                _logger.Error($"Failed to lookup Pokemon '{pokemon.Id}' in database.");
                return null;
            }

            var gmapsLink = string.Format(Strings.GoogleMaps, pokemon.Latitude, pokemon.Longitude);
            var appleMapsLink = string.Format(Strings.AppleMaps, pokemon.Latitude, pokemon.Longitude);
            var staticMapLink = string.Format(_whConfig.Urls.StaticMap, pokemon.Latitude, pokemon.Longitude);
            var gmapsLocationLink = string.IsNullOrEmpty(_whConfig.ShortUrlApiUrl) ? gmapsLink : CreateShortUrl(_whConfig.ShortUrlApiUrl, gmapsLink);
            var appleMapsLocationLink = string.IsNullOrEmpty(_whConfig.ShortUrlApiUrl) ? appleMapsLink : CreateShortUrl(_whConfig.ShortUrlApiUrl, appleMapsLink);
            var gmapsStaticMapLink = string.IsNullOrEmpty(_whConfig.ShortUrlApiUrl) ? staticMapLink : CreateShortUrl(_whConfig.ShortUrlApiUrl, staticMapLink);

            var eb = new DiscordEmbedBuilder
            {
                Title = string.IsNullOrEmpty(city) ? _lang.Translate("EMBED_DIRECTIONS") : city,
                Url = gmapsLocationLink,
                ImageUrl = gmapsStaticMapLink,
                ThumbnailUrl = pokemon.Id.GetPokemonImage(pokemonImageUrl, pokemon.Gender, pokemon.FormId, pokemon.Costume),
                Color = pokemon.IV.BuildColor()
            };

            var form = pokemon.Id.GetPokemonForm(pokemon.FormId.ToString());
            var costume = pokemon.Id.GetCostume(pokemon.Costume.ToString());
            var pkmnName = pkmn.Name;
            if (pokemon.IsMissingStats)
            {
                eb.Description = _lang.Translate("EMBED_POKEMON_TITLE_WITHOUT_DETAILS").FormatText(
                    pkmnName, form, pokemon.Gender.GetPokemonGenderIconValue()) + "\r\n";
                eb.Description += _lang.Translate("EMBED_POKEMON_DESPAWN").FormatText(
                    pokemon.DespawnTime.ToLongTimeString(), pokemon.SecondsLeft.ToReadableString(true)) + (pokemon.DisappearTimeVerified ? "" : "~") + "\r\n";
            }
            else
            {
                eb.Description = _lang.Translate("EMBED_POKEMON_TITLE").FormatText(
                    pkmnName, form, pokemon.Gender.GetPokemonGenderIconValue(), pokemon.IV, pokemon.Attack, pokemon.Defense, pokemon.Stamina, pokemon.Level) + "\r\n";
                eb.Description += _lang.Translate("EMBED_POKEMON_DESPAWN").FormatText
                    (pokemon.DespawnTime.ToLongTimeString(), pokemon.SecondsLeft.ToReadableString(true)) + (pokemon.DisappearTimeVerified ? "" : "~") + "\r\n";
                eb.Description += _lang.Translate("EMBED_POKEMON_DETAILS").FormatText(
                    pokemon.CP, pokemon.IV, pokemon.Level) + "\r\n";
            }

            if (!string.IsNullOrEmpty(form))
            {
                eb.Description += _lang.Translate("EMBED_POKEMON_FORM").FormatText(form) + "\r\n";
            }

            if (!string.IsNullOrEmpty(costume))
            {
                eb.Description += $"**Costume:** {costume}\r\n";
            }

            //if (int.TryParse(pokemon.Level, out var lvl) && lvl >= 30)
            //{
            //    eb.Description += _lang.Translate("EMBED_POKEMON_WEATHER_BOOSTED") + "\r\n";
            //    //eb.Description += $":white_sun_rain_cloud: Boosted\r\n";
            //}

            //var maxCp = db.MaxCpAtLevel(pokemon.Id, 40);
            //var maxWildCp = db.MaxCpAtLevel(pokemon.Id, 35);
            //eb.Description += $"**Max Wild CP:** {maxWildCp}, **Max CP:** {maxCp} \r\n";

            if (pokemon.OriginalPokemonId > 0 && db.Pokemon.ContainsKey(pokemon.OriginalPokemonId))
            {
                var catchPkmn = db.Pokemon[pokemon.OriginalPokemonId];
                eb.Description += $"**Catch Pokemon:** {catchPkmn.Name}\r\n";
            }

            if (pokemon.Weather.HasValue && Strings.WeatherEmojis.ContainsKey(pokemon.Weather.Value) && pokemon.Weather != WeatherType.None)
            {
                var isWeatherBoosted = pkmn.IsWeatherBoosted(pokemon.Weather.Value);
                var isWeatherBoostedText = isWeatherBoosted ? " (Boosted)" : null;
                eb.Description += _lang.Translate("EMBED_POKEMON_WEATHER").FormatText(Strings.WeatherEmojis[pokemon.Weather.Value]) + isWeatherBoostedText + "\r\n";
            }

            var hasTypes = pkmn.Types != null;
            var typeEmojis = pkmn.Types.GetTypeEmojiIcons(_client, _whConfig.GuildId);
            if (hasTypes)
            {
                eb.Description += _lang.Translate("EMBED_TYPES").FormatText(typeEmojis);
            }

            if (pokemon.Size.HasValue)
            {
                eb.Description += " | " + _lang.Translate("EMBED_POKEMON_SIZE").FormatText(pokemon.Size.ToString()) + "\r\n";
            }
            else
            {
                eb.Description += "\r\n";
            }

            if (int.TryParse(pokemon.FastMove, out var fastMoveId) && db.Movesets.ContainsKey(fastMoveId))
            {
                var fastMove = db.Movesets[fastMoveId];
                eb.Description += _lang.Translate("EMBED_MOVE_FAST").FormatText(fastMove.Name) + "/";
            }

            if (int.TryParse(pokemon.ChargeMove, out var chargeMoveId) && db.Movesets.ContainsKey(chargeMoveId))
            {
                var chargeMove = db.Movesets[chargeMoveId];
                eb.Description += chargeMove.Name + "\r\n";//_lang.Translate("EMBED_MOVE_CHARGE").FormatText(chargeMove.Name) + "\r\n";
            }

            if (!string.IsNullOrEmpty(pokemon.PokestopId))
            {
                var pokestop = GetPokestopById(pokemon.PokestopId);
                if (pokestop != null && !string.IsNullOrEmpty(pokestop?.Name))
                {
                    eb.Description += $"**Near Pokestop:** [{pokestop.Name}]({pokestop.Url})\r\n";
                }
            }


            if (pokemon.MatchesGreatLeague || pokemon.MatchesUltraLeague)
            {
                eb.Description += "\r\n__**PvP Stats Ranking**__\r\n";
            }

            if (pokemon.MatchesGreatLeague)
            {
                var greatLeague = pokemon.GreatLeagueStats;
                if (greatLeague != null)
                {
                    greatLeague = greatLeague.Where(x => x.Rank <= 25).ToList();
                    greatLeague.Sort((x, y) => x.Rank.CompareTo(y.Rank));
                    if (greatLeague.Count > 0)
                    {
                        eb.Description += "**Great League:**\r\n";
                        foreach (var stat in greatLeague)
                        {
                            eb.Description += $"**Rank:** #{stat.Rank}\t**Level:** {stat.Level}\t**CP:** {stat.CP}\r\n";
                        }
                    }
                }
            }

            if (pokemon.MatchesUltraLeague)
            {
                var ultraLeague = pokemon.UltraLeagueStats;
                if (ultraLeague != null)
                {
                    ultraLeague = ultraLeague.Where(x => x.Rank <= 25).ToList();
                    ultraLeague.Sort((x, y) => x.Rank.CompareTo(y.Rank));
                    if (ultraLeague.Count > 0)
                    {
                        eb.Description += "**Ultra League:**\r\n";
                        foreach (var stat in ultraLeague)
                        {
                            eb.Description += $"**Rank:** #{stat.Rank}\t**Level:** {stat.Level}\t**CP:** {stat.CP}\r\n";
                        }
                    }
                }
            }

            //eb.Description += _lang.Translate("EMBED_LOCATION").FormatText(Math.Round(pokemon.Latitude, 5), Math.Round(pokemon.Longitude, 5)) + "\r\n";
            eb.Description += _lang.Translate("EMBED_GMAPS").FormatText(gmapsLocationLink) + " " + _lang.Translate("EMBED_APPLEMAPS").FormatText(appleMapsLocationLink) + "\r\n";
            eb.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"versx | {DateTime.Now}",
                IconUrl = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId]?.IconUrl : string.Empty
            };
            var embed = eb.Build();

            return embed;
        }

        public DiscordEmbed BuildRaidMessage(RaidData raid, string city, string pokemonRaidImageUrl)
        {
            //_logger.Trace($"EmbedBuilder::BuildRaidMessage [Raid={raid.PokemonId}, City={city}]");

            var db = Database.Instance;
            var pkmn = db.Pokemon[raid.PokemonId];
            if (pkmn == null)
            {
                _logger.Error($"Failed to lookup Raid Pokemon '{raid.PokemonId}' in database.");
                return null;
            }

            var pkmnImage = raid.IsEgg ? string.Format(_whConfig.Urls.EggImage, raid.Level) : raid.PokemonId.GetPokemonImage(pokemonRaidImageUrl, PokemonGender.Unset, raid.Form);
            var eb = new DiscordEmbedBuilder
            {
                Title = string.IsNullOrEmpty(city) ? _lang.Translate("EMBED_DIRECTIONS") : $"{city}: {raid.GymName}",
                Url = string.Format(Strings.GoogleMaps, raid.Latitude, raid.Longitude),
                ImageUrl = string.Format(_whConfig.Urls.StaticMap, raid.Latitude, raid.Longitude),
                ThumbnailUrl = pkmnImage,
                Color = Convert.ToInt32(raid.Level).BuildRaidColor()
            };

            if (raid.IsEgg)
            {
                eb.Description = _lang.Translate("EMBED_EGG_HATCHES").FormatText(raid.StartTime.ToLongTimeString(), DateTime.Now.GetTimeRemaining(raid.StartTime).ToReadableStringNoSeconds()) + "\r\n";
                eb.Description += _lang.Translate("EMBED_EGG_ENDS").FormatText(raid.EndTime.ToLongTimeString(), DateTime.Now.GetTimeRemaining(raid.EndTime).ToReadableStringNoSeconds()) + "\r\n";
            }
            else
            {
                var form = raid.PokemonId.GetPokemonForm(raid.Form.ToString());
                var pkmnName = $"{(string.IsNullOrEmpty(form) ? null : $"{form}-")}{pkmn.Name}";
                eb.Description += _lang.Translate("EMBED_RAID_ENDS").FormatText(pkmnName, raid.EndTime.ToLongTimeString()) + "\r\n";
                eb.Description += _lang.Translate("EMBED_RAID_STARTED").FormatText(raid.StartTime.ToLongTimeString()) + "\r\n";
                eb.Description += _lang.Translate("EMBED_RAID_ENDS_WITH_TIME_LEFT").FormatText(raid.EndTime.ToLongTimeString(), raid.EndTime.GetTimeRemaining().ToReadableStringNoSeconds()) + "\r\n";

                if (raid.Form > 0)
                {
                    eb.Description += _lang.Translate("EMBED_POKEMON_FORM").FormatText(form) + "\r\n";
                }

                var perfectRange = raid.PokemonId.MaxCpAtLevel(20);
                var boostedRange = raid.PokemonId.MaxCpAtLevel(25);
                var worstRange = raid.PokemonId.MinCpAtLevel(20);
                var worstBoosted = raid.PokemonId.MinCpAtLevel(25);
                eb.Description += _lang.Translate("EMBED_RAID_PERFECT_CP").FormatText(perfectRange, boostedRange) + "\r\n";
                eb.Description += _lang.Translate("EMBED_RAID_WORST_CP").FormatText(worstRange, worstBoosted) + "\r\n";

                if (pkmn.Types != null)
                {
                    eb.Description += _lang.Translate("EMBED_TYPES").FormatText(pkmn.Types.GetTypeEmojiIcons(_client, _whConfig.GuildId)) + $" | **Level:** {raid.Level}\r\n";
                }
                else
                {
                    eb.Description += $"**Level:** {raid.Level}\r\n";
                }

                if (db.Movesets.ContainsKey(raid.FastMove))
                {
                    var fastMove = db.Movesets[raid.FastMove];
                    eb.Description += _lang.Translate("EMBED_MOVE_FAST").FormatText(fastMove.Name) + "/";
                }

                if (db.Movesets.ContainsKey(raid.ChargeMove))
                {
                    var chargeMove = db.Movesets[raid.ChargeMove];
                    eb.Description += chargeMove.Name + "\r\n";//_lang.Translate("EMBED_MOVE_CHARGE").FormatText(chargeMove.Name) + "\r\n";
                }

                var weaknessesEmojis = pkmn.Types.GetWeaknessEmojiIcons(_client, _whConfig.GuildId);
                if (!string.IsNullOrEmpty(weaknessesEmojis))
                {
                    eb.Description += _lang.Translate("EMBED_RAID_WEAKNESSES").FormatText(weaknessesEmojis) + "\r\n";
                }
            }

            if (raid.IsExEligible)
            {
                var exEmojiId = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId].GetEmojiId(_lang.Translate("EMOJI_EX")) : 0;
                var exEmoji = exEmojiId > 0 ? $"<:ex:{exEmojiId}>" : "EX";
                eb.Description += _lang.Translate("EMBED_RAID_EX_GYM").FormatText(exEmoji) + "\r\n";
            }

            if (_client.Guilds.ContainsKey(_whConfig.GuildId))
            {
                var teamEmojiId = _client.Guilds[_whConfig.GuildId].GetEmojiId(raid.Team.ToString().ToLower());
                var teamEmoji = teamEmojiId > 0 ? $"<:{raid.Team.ToString().ToLower()}:{teamEmojiId}>" : raid.Team.ToString();
                eb.Description += _lang.Translate("EMBED_TEAM").FormatText(teamEmoji) + "\r\n";
            }

            eb.Description += _lang.Translate("EMBED_LOCATION").FormatText(Math.Round(raid.Latitude, 5), Math.Round(raid.Longitude, 5)) + "\r\n";
            eb.Description += _lang.Translate("EMBED_GMAPS").FormatText(string.Format(Strings.GoogleMaps, raid.Latitude, raid.Longitude)) + " " + _lang.Translate("EMBED_APPLEMAPS").FormatText(string.Format(Strings.AppleMaps, raid.Latitude, raid.Longitude)) + "\r\n";
            eb.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"versx | {DateTime.Now}",
                IconUrl = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId]?.IconUrl : string.Empty
            };
            var embed = eb.Build();

            return embed;
        }

        public DiscordEmbed BuildQuestMessage(QuestData quest, string city)
        {
            //_logger.Trace($"EmbedBuilder::BuildQuestMessage [Quest={quest.PokestopId}, City={city}]");

            var gmapsUrl = string.Format(Strings.GoogleMaps, quest.Latitude, quest.Longitude);
            var eb = new DiscordEmbedBuilder
            {
                Title = $"{city}: {(string.IsNullOrEmpty(quest.PokestopName) ? _lang.Translate("UNKNOWN_POKESTOP") : quest.PokestopName)}",
                Url = gmapsUrl,
                ImageUrl = string.Format(_whConfig.Urls.StaticMap, quest.Latitude, quest.Longitude),
                ThumbnailUrl = quest.PokestopUrl,//quest.GetIconUrl(),
                Color = DiscordColor.Orange
            };

            eb.Description = _lang.Translate("EMBED_QUEST_QUEST").FormatText(quest.GetQuestMessage()) + "\r\n";
            if (quest.Conditions != null && quest.Conditions.Count > 0)
            {
                eb.Description += _lang.Translate("EMBED_QUEST_CONDITION").FormatText(quest.GetConditions()) + "\r\n";
            }
            eb.Description += _lang.Translate("EMBED_QUEST_REWARD").FormatText(quest.GetReward()) + "\r\n";
            eb.Description += _lang.Translate("EMBED_LOCATION").FormatText(Math.Round(quest.Latitude, 5), Math.Round(quest.Longitude, 5)) + "\r\n";
            eb.Description += _lang.Translate("EMBED_GMAPS").FormatText(string.Format(Strings.GoogleMaps, quest.Latitude, quest.Longitude)) + " " + _lang.Translate("EMBED_APPLEMAPS").FormatText(string.Format(Strings.AppleMaps, quest.Latitude, quest.Longitude)) + "\r\n";
            eb.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"versx | {DateTime.Now}",
                IconUrl = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId]?.IconUrl : string.Empty
            };

            return eb.Build();
        }

        public DiscordEmbed BuildPokestopMessage(PokestopData pokestop, string city)
        {
            //_logger.Trace($"EmbedBuilder::BuildPokestopMessage [Pokestop={pokestop.PokestopId}, City={city}]");

            var gmapsUrl = string.Format(Strings.GoogleMaps, pokestop.Latitude, pokestop.Longitude);
            var eb = new DiscordEmbedBuilder
            {
                Title = $"{city}: {(string.IsNullOrEmpty(pokestop.Name) ? _lang.Translate("UNKNOWN_POKESTOP") : pokestop.Name)}",
                Url = gmapsUrl,
                ImageUrl = string.Format(_whConfig.Urls.StaticMap, pokestop.Latitude, pokestop.Longitude),
                ThumbnailUrl = pokestop.Url,
                Color = pokestop.HasInvasion ? DiscordColor.Red : pokestop.HasLure ?
                    (pokestop.LureType == PokestopLureType.Normal ? DiscordColor.HotPink
                    : pokestop.LureType == PokestopLureType.Glacial ? DiscordColor.CornflowerBlue
                    : pokestop.LureType == PokestopLureType.Mossy ? DiscordColor.SapGreen
                    : pokestop.LureType == PokestopLureType.Magnetic ? DiscordColor.Gray
                    : DiscordColor.CornflowerBlue) : DiscordColor.CornflowerBlue
            };

            if (pokestop.HasLure)
            {
                eb.Description += $"**Lured Until:** {pokestop.LureExpireTime.ToLongTimeString()} ({pokestop.LureExpireTime.GetTimeRemaining().ToReadableStringNoSeconds()})\r\n";
                if (pokestop.LureType != PokestopLureType.None)
                {
                    eb.Description += $"**Lure Type:** {pokestop.LureType}\r\n";
                }
            }
            if (pokestop.HasInvasion)
            {
                var invasion = new TeamRocketInvasion(pokestop.GruntType);
                var typeEmoji = invasion.Type == PokemonType.None ? "Tier II" : invasion.Type.GetTypeEmojiIcons(_client, _whConfig.GuildId);
                eb.Description += $"**Expires:** {pokestop.InvasionExpireTime.ToLongTimeString()} ({pokestop.InvasionExpireTime.GetTimeRemaining().ToReadableStringNoSeconds()})\r\n";
                eb.Description += $"**Type:** {typeEmoji} | **Gender:** {invasion.Gender.ToString()}\r\n";

                if (invasion.HasEncounter)
                {
                    eb.Description += $"**Encounter Reward Chance:**\r\n" + GetPossibleInvasionEncounters(invasion);
                }
            }

            eb.Description += _lang.Translate("EMBED_LOCATION").FormatText(Math.Round(pokestop.Latitude, 5), Math.Round(pokestop.Longitude, 5)) + "\r\n";
            eb.Description += _lang.Translate("EMBED_GMAPS").FormatText(string.Format(Strings.GoogleMaps, pokestop.Latitude, pokestop.Longitude)) + " " + _lang.Translate("EMBED_APPLEMAPS").FormatText(string.Format(Strings.AppleMaps, pokestop.Latitude, pokestop.Longitude)) + "\r\n";
            eb.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"versx | {DateTime.Now}",
                IconUrl = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId]?.IconUrl : string.Empty
            };

            return eb.Build();
        }

        public DiscordEmbed BuildGymMessage(GymData gym, string city)
        {
            _logger.Trace($"EmbedBuilder::BuildGymMessage [Gym={gym.GymId}, City={city}]");

            var gmapsUrl = string.Format(Strings.GoogleMaps, gym.Latitude, gym.Longitude);
            var eb = new DiscordEmbedBuilder
            {
                Title = $"{city}: {(string.IsNullOrEmpty(gym.GymName) ? _lang.Translate("UNKNOWN_GYM") : gym.GymName)}",
                Url = gmapsUrl,
                ImageUrl = string.Format(_whConfig.Urls.StaticMap, gym.Latitude, gym.Longitude),
                ThumbnailUrl = gym.Url,
                Color = gym.Team == PokemonTeam.Mystic ? DiscordColor.Blue : 
                        gym.Team == PokemonTeam.Valor ? DiscordColor.Red : 
                        gym.Team == PokemonTeam.Instinct ? DiscordColor.Yellow : 
                        DiscordColor.Gray
            };

            return eb.Build();
        }

        public DiscordEmbed BuildGymDetailsMessage(GymDetailsData gymDetails, string city)
        {
            _logger.Trace($"EmbedBuilder::BuildGymDetailsMessage [Gym={gymDetails.GymId}, City={city}]");

            return null;
        }

        #endregion

        private Pokestop GetPokestopById(string pokestopId)
        {
            if (string.IsNullOrEmpty(_whConfig.ScannerConnectionString))
                return null;

            try
            {
                using (var db = DataAccessLayer.CreateFactory(_whConfig.ScannerConnectionString).Open())
                {
                    var pokestop = db.LoadSingleById<Pokestop>(pokestopId);
                    return pokestop;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return null;
        }

        private IReadOnlyDictionary<string, string> GetProperties(PokemonData pkmn, string city)
        {
            var pkmnInfo = Database.Instance.Pokemon[pkmn.Id];
            var form = pkmn.Id.GetPokemonForm(pkmn.FormId.ToString());
            var costume = pkmn.Id.GetCostume(pkmn.Costume.ToString());
            var gender = pkmn.Gender.GetPokemonGenderIcon();
            var level = pkmn.Level;
            var size = pkmn.Size?.ToString();
            var weather = pkmn.Weather?.ToString();
            var weatherEmoji = string.Empty;
            if (pkmn.Weather.HasValue && Strings.WeatherEmojis.ContainsKey(pkmn.Weather.Value) && pkmn.Weather != WeatherType.None)
            {
                var isWeatherBoosted = pkmnInfo.IsWeatherBoosted(pkmn.Weather.Value);
                var isWeatherBoostedText = isWeatherBoosted ? " (Boosted)" : null;
                weatherEmoji = Strings.WeatherEmojis[pkmn.Weather.Value] + isWeatherBoostedText;
            }
            var move1 = string.Empty;
            var move2 = string.Empty;
            if (int.TryParse(pkmn.FastMove, out var fastMoveId) && Database.Instance.Movesets.ContainsKey(fastMoveId))
            {
                move1 = Database.Instance.Movesets[fastMoveId].Name;
            }
            if (int.TryParse(pkmn.ChargeMove, out var chargeMoveId) && Database.Instance.Movesets.ContainsKey(chargeMoveId))
            {
                move2 = Database.Instance.Movesets[chargeMoveId].Name;
            }
            var type1 = pkmnInfo?.Types?[0];
            var type2 = pkmnInfo?.Types?.Count > 1 ? pkmnInfo.Types?[1] : PokemonType.None;
            var type1Emoji = pkmnInfo?.Types?[0].GetTypeEmojiIcons(_client, _whConfig.GuildId);
            var type2Emoji = pkmnInfo?.Types?.Count > 1 ? pkmnInfo?.Types?[1].GetTypeEmojiIcons(_client, _whConfig.GuildId) : string.Empty;
            var typeEmojis = $"{type1Emoji} {type2Emoji}";
            var gmapsLink = string.Format(Strings.GoogleMaps, pkmn.Latitude, pkmn.Longitude);
            var appleMapsLink = string.Format(Strings.AppleMaps, pkmn.Latitude, pkmn.Longitude);
            var staticMapLink = string.Format(_whConfig.Urls.StaticMap, pkmn.Latitude, pkmn.Longitude);
            var gmapsLocationLink = string.IsNullOrEmpty(_whConfig.ShortUrlApiUrl) ? gmapsLink : CreateShortUrl(_whConfig.ShortUrlApiUrl, gmapsLink);
            var appleMapsLocationLink = string.IsNullOrEmpty(_whConfig.ShortUrlApiUrl) ? appleMapsLink : CreateShortUrl(_whConfig.ShortUrlApiUrl, appleMapsLink);
            var gmapsStaticMapLink = string.IsNullOrEmpty(_whConfig.ShortUrlApiUrl) ? staticMapLink : CreateShortUrl(_whConfig.ShortUrlApiUrl, staticMapLink);
            var pokestop = GetPokestopById(pkmn.PokestopId);

            const string defaultMissingValue = "?";
            var dict = new Dictionary<string, string>
            {
                //Main properties
                { "pkmn_id", pkmn.Id.ToString() },
                { "pkmn_name", pkmnInfo.Name },
                { "form", form },
                { "form_id", pkmn.FormId.ToString() },
                { "form_id_3", pkmn.FormId.ToString("D3") },
                { "costume_id_3", pkmn.Costume.ToString("D3") },
                { "cp", pkmn.CP ?? defaultMissingValue },
                { "lvl", level ?? defaultMissingValue },
                { "gender", gender ?? defaultMissingValue },
                { "size", size ?? defaultMissingValue },
                { "move_1", move1 ?? defaultMissingValue },
                { "move_2", move2 ?? defaultMissingValue },
                { "moveset", $"{move1}/{move2}" },
                { "type_1", type1?.ToString() ?? defaultMissingValue },
                { "type_2", type2?.ToString() ?? defaultMissingValue },
                { "type_1_emoji", type1Emoji },
                { "type_2_emoji", type2Emoji },
                { "types", $"{type1}/{type2}" },
                { "types_emoji", typeEmojis },
                { "atk_iv", pkmn.Attack ?? defaultMissingValue },
                { "def_iv", pkmn.Defense ?? defaultMissingValue },
                { "sta_iv", pkmn.Stamina ?? defaultMissingValue },
                { "iv", pkmn.IV ?? defaultMissingValue },
                { "iv_rnd", pkmn.IVRounded ?? defaultMissingValue },

                //Other properties
                { "costume", costume ?? defaultMissingValue },
                { "height", pkmn.Height ?? defaultMissingValue },
                { "weight", pkmn.Weight ?? defaultMissingValue },
                { "is_ditto", pkmn.IsDitto ? "Yes" : "No" },
                { "original_pkmn_id", pkmn.OriginalPokemonId.ToString() },
                { "weather", weather ?? defaultMissingValue },
                { "weather_emoji", weatherEmoji ?? defaultMissingValue },
                { "username", pkmn.Username ?? defaultMissingValue },
                { "spawnpoint_id", pkmn.SpawnpointId ?? defaultMissingValue },
                { "encounter_id", pkmn.EncounterId ?? defaultMissingValue },

                //Time properties
                { "despawn_time", pkmn.DespawnTime.ToString("hh:mm:ss tt") },
                { "despawn_time_verified", pkmn.DisappearTimeVerified ? "Yes" : "No" },
                { "time_left", pkmn.SecondsLeft.ToReadableString(true) ?? defaultMissingValue },

                //Location properties
                { "geofence", city ?? defaultMissingValue },
                { "lat", pkmn.Latitude.ToString() },
                { "lng", pkmn.Longitude.ToString() },
                { "lat_5", Math.Round(pkmn.Latitude, 5).ToString() },
                { "lng_5", Math.Round(pkmn.Longitude, 5).ToString() },

                //Location links
                { "tilemaps_url", gmapsStaticMapLink },
                { "gmaps_url", gmapsLocationLink },
                { "applemaps_url", appleMapsLocationLink },

                //Pokestop properties
                { "pokestop_id", pkmn.PokestopId ?? "?" },
                { "pokestop_name", pokestop?.Name ?? "?" },
                { "pokestop_url", pokestop?.Url ?? "?" },

                //Misc properties
                { "br", "\r\n" }
            };
            return dict;
        }

        private IReadOnlyDictionary<string, string> GetProperties(RaidData raid, string city)
        {
            var pkmnInfo = Database.Instance.Pokemon[raid.PokemonId];
            var form = raid.PokemonId.GetPokemonForm(raid.Form.ToString());
            var gender = raid.Gender.GetPokemonGenderIcon();
            var level = raid.Level;
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
            if (Database.Instance.Movesets.ContainsKey(raid.FastMove))
            {
                move1 = Database.Instance.Movesets[raid.FastMove].Name;
            }
            if (Database.Instance.Movesets.ContainsKey(raid.ChargeMove))
            {
                move2 = Database.Instance.Movesets[raid.ChargeMove].Name;
            }
            var type1 = pkmnInfo?.Types?[0];
            var type2 = pkmnInfo?.Types?.Count > 1 ? pkmnInfo.Types?[1] : PokemonType.None;
            var type1Emoji = pkmnInfo?.Types?[0].GetTypeEmojiIcons(_client, _whConfig.GuildId);
            var type2Emoji = pkmnInfo?.Types?.Count > 1 ? pkmnInfo?.Types?[1].GetTypeEmojiIcons(_client, _whConfig.GuildId) : string.Empty;
            var typeEmojis = $"{type1Emoji} {type2Emoji}";
            var weaknesses = string.Join(", ", raid.Weaknesses);
            var weaknessesEmoji = raid.Weaknesses.GetWeaknessEmojiIcons(_client, _whConfig.GuildId);
            var weaknessesEmojiFormatted = weaknessesEmoji.Split(' ').Length > 6 ? System.Text.RegularExpressions.Regex.Replace(weaknessesEmoji, "(.{" + 6 + "})", "$1" + Environment.NewLine) : weaknessesEmoji;
            var perfectRange = raid.PokemonId.MaxCpAtLevel(20);
            var boostedRange = raid.PokemonId.MaxCpAtLevel(25);
            var worstRange = raid.PokemonId.MinCpAtLevel(20);
            var worstBoosted = raid.PokemonId.MinCpAtLevel(25);
            var exEmojiId = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId].GetEmojiId(_lang.Translate("EMOJI_EX")) : 0;
            var exEmoji = exEmojiId > 0 ? $"<:ex:{exEmojiId}>" : "EX";
            var teamEmojiId = _client.Guilds[_whConfig.GuildId].GetEmojiId(raid.Team.ToString().ToLower());
            var teamEmoji = teamEmojiId > 0 ? $"<:{raid.Team.ToString().ToLower()}:{teamEmojiId}>" : raid.Team.ToString();

            var gmapsLink = string.Format(Strings.GoogleMaps, raid.Latitude, raid.Longitude);
            var appleMapsLink = string.Format(Strings.AppleMaps, raid.Latitude, raid.Longitude);
            var staticMapLink = string.Format(_whConfig.Urls.StaticMap, raid.Latitude, raid.Longitude);
            var gmapsLocationLink = string.IsNullOrEmpty(_whConfig.ShortUrlApiUrl) ? gmapsLink : CreateShortUrl(_whConfig.ShortUrlApiUrl, gmapsLink);
            var appleMapsLocationLink = string.IsNullOrEmpty(_whConfig.ShortUrlApiUrl) ? appleMapsLink : CreateShortUrl(_whConfig.ShortUrlApiUrl, appleMapsLink);
            var gmapsStaticMapLink = string.IsNullOrEmpty(_whConfig.ShortUrlApiUrl) ? staticMapLink : CreateShortUrl(_whConfig.ShortUrlApiUrl, staticMapLink);

            const string defaultMissingValue = "?";
            var dict = new Dictionary<string, string>
            {
                //Raid boss properties
                { "pkmn_id", raid.PokemonId.ToString() },
                { "pkmn_name", pkmnInfo.Name },
                { "form", form },
                { "form_id", raid.Form.ToString() },
                { "form_id_3", raid.Form.ToString("D3") },
                { "is_egg", raid.IsEgg ? "Yes": "No" },
                { "is_ex", raid.IsExEligible ? "Yes" : "No" },
                { "ex_emoji", exEmoji },
                { "team", raid.Team.ToString() },
                { "team_emoji", teamEmoji },
                { "cp", raid.CP ?? defaultMissingValue },
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
                { "start_time", raid.StartTime.ToLongTimeString() },
                { "start_time_left", DateTime.Now.GetTimeRemaining(raid.StartTime).ToReadableStringNoSeconds() },
                { "end_time", raid.EndTime.ToLongTimeString() },
                { "end_time_left", raid.EndTime.GetTimeRemaining().ToReadableStringNoSeconds() },

                //Location properties
                { "geofence", city ?? defaultMissingValue },
                { "lat", raid.Latitude.ToString() },
                { "lng", raid.Longitude.ToString() },
                { "lat_5", Math.Round(raid.Latitude, 5).ToString() },
                { "lng_5", Math.Round(raid.Longitude, 5).ToString() },

                //Location links
                { "tilemaps_url", gmapsStaticMapLink },
                { "gmaps_url", gmapsLocationLink },
                { "applemaps_url", appleMapsLocationLink },

                //Gym properties
                { "gym_id", raid.GymId },
                { "gym_name", raid.GymName },
                { "gym_url", raid.GymUrl },

                //Misc properties
                { "br", "\r\n" }
            };
            return dict;
        }

        private IReadOnlyDictionary<string, string> GetProperties(QuestData quest, string city)
        {
            var questMessage = quest.GetQuestMessage();
            var questConditions = quest.GetConditions();
            var questReward = quest.GetReward();
            var gmapsLink = string.Format(Strings.GoogleMaps, quest.Latitude, quest.Longitude);
            var appleMapsLink = string.Format(Strings.AppleMaps, quest.Latitude, quest.Longitude);
            var staticMapLink = string.Format(_whConfig.Urls.StaticMap, quest.Latitude, quest.Longitude);
            var gmapsLocationLink = string.IsNullOrEmpty(_whConfig.ShortUrlApiUrl) ? gmapsLink : CreateShortUrl(_whConfig.ShortUrlApiUrl, gmapsLink);
            var appleMapsLocationLink = string.IsNullOrEmpty(_whConfig.ShortUrlApiUrl) ? appleMapsLink : CreateShortUrl(_whConfig.ShortUrlApiUrl, appleMapsLink);
            var gmapsStaticMapLink = string.IsNullOrEmpty(_whConfig.ShortUrlApiUrl) ? staticMapLink : CreateShortUrl(_whConfig.ShortUrlApiUrl, staticMapLink);

            const string defaultMissingValue = "?";
            var dict = new Dictionary<string, string>
            {
                //Main properties
                { "quest_task", questMessage },
                { "quest_conditions", questConditions },
                { "quest_reward", questReward },
                { "is_ditto", quest.IsDitto ? "Yes" : "No" },
                { "is_shiny", quest.IsShiny ? "Yes" : "No" },

                //Location properties
                { "geofence", city ?? defaultMissingValue },
                { "lat", quest.Latitude.ToString() },
                { "lng", quest.Longitude.ToString() },
                { "lat_5", Math.Round(quest.Latitude, 5).ToString() },
                { "lng_5", Math.Round(quest.Longitude, 5).ToString() },

                //Location links
                { "tilemaps_url", gmapsStaticMapLink },
                { "gmaps_url", gmapsLocationLink },
                { "applemaps_url", appleMapsLocationLink },

                //Pokestop properties
                { "pokestop_id", quest.PokestopId ?? "?" },
                { "pokestop_name", quest.PokestopName ?? "?" },
                { "pokestop_url", quest.PokestopUrl ?? "?" },

                //Misc properties
                { "br", "\r\n" }
            };
            return dict;
        }

        private IReadOnlyDictionary<string, string> GetProperties(PokestopData pokestop, string city)
        {
            var gmapsLink = string.Format(Strings.GoogleMaps, pokestop.Latitude, pokestop.Longitude);
            var appleMapsLink = string.Format(Strings.AppleMaps, pokestop.Latitude, pokestop.Longitude);
            var staticMapLink = string.Format(_whConfig.Urls.StaticMap, pokestop.Latitude, pokestop.Longitude);
            var gmapsLocationLink = string.IsNullOrEmpty(_whConfig.ShortUrlApiUrl) ? gmapsLink : CreateShortUrl(_whConfig.ShortUrlApiUrl, gmapsLink);
            var appleMapsLocationLink = string.IsNullOrEmpty(_whConfig.ShortUrlApiUrl) ? appleMapsLink : CreateShortUrl(_whConfig.ShortUrlApiUrl, appleMapsLink);
            var gmapsStaticMapLink = string.IsNullOrEmpty(_whConfig.ShortUrlApiUrl) ? staticMapLink : CreateShortUrl(_whConfig.ShortUrlApiUrl, staticMapLink);
            var invasion = new TeamRocketInvasion(pokestop.GruntType);
            var invasionEncounters = GetPossibleInvasionEncounters(invasion);

            const string defaultMissingValue = "?";
            var dict = new Dictionary<string, string>
            {
                //Main properties
                { "has_lure", pokestop.HasLure ? "Yes" : "No" },
                { "lure_type", pokestop.LureType.ToString() },
                { "lure_expire_time", pokestop.LureExpireTime.ToLongTimeString() },
                { "lure_expire_time_left", pokestop.LureExpireTime.GetTimeRemaining().ToReadableStringNoSeconds() },
                { "has_invasion", pokestop.HasInvasion ? "Yes" : "No" },
                { "grunt_type", invasion.Type == PokemonType.None ? "Tier II" : invasion?.Type.ToString() },
                { "grunt_type_emoji", invasion.Type == PokemonType.None ? "Tier II" : invasion.Type.GetTypeEmojiIcons(_client, _whConfig.GuildId) },
                { "grunt_gender", invasion.Gender.ToString() },
                { "invasion_expire_time", pokestop.InvasionExpireTime.ToLongTimeString() },
                { "invasion_expire_time_left", pokestop.InvasionExpireTime.GetTimeRemaining().ToReadableStringNoSeconds() },
                { "invasion_encounters", $"**Encounter Reward Chance:**\r\n" + invasionEncounters },

                //Location properties
                { "geofence", city ?? defaultMissingValue },
                { "lat", pokestop.Latitude.ToString() },
                { "lng", pokestop.Longitude.ToString() },
                { "lat_5", Math.Round(pokestop.Latitude, 5).ToString() },
                { "lng_5", Math.Round(pokestop.Longitude, 5).ToString() },

                //Location links
                { "tilemaps_url", gmapsStaticMapLink },
                { "gmaps_url", gmapsLocationLink },
                { "applemaps_url", appleMapsLocationLink },

                //Pokestop properties
                { "pokestop_id", pokestop.PokestopId ?? "?" },
                { "pokestop_name", pokestop.Name ?? "?" },
                { "pokestop_url", pokestop.Url ?? "?" },

                //Misc properties
                { "br", "\r\n" }
            };
            return dict;
        }

        private static string CreateShortUrl(string baseApiUrl, string url)
        {
            try
            {
                var apiUrl = baseApiUrl + "&action=shorturl&url=" + HttpUtility.UrlEncode(url) + "&format=json";
                using (var wc = new WebClient())
                {
                    wc.Proxy = null;
                    var json = wc.DownloadString(apiUrl);
                    var obj = JsonConvert.DeserializeObject<UrlShortener>(json);
                    return obj.ShortUrl;
                }
            }
            catch (Exception)
            {
                return url;
            }
        }

        private static string GetPossibleInvasionEncounters(TeamRocketInvasion invasion)
        {
            var first = string.Join(", ", invasion.Encounters.First.Select(x => Database.Instance.Pokemon[x].Name));
            var second = string.Join(", ", invasion.Encounters.Second.Select(x => Database.Instance.Pokemon[x].Name));
            //var third = string.Join(", ", invasion.Encounters.Third.Select(x => Database.Instance.Pokemon[x].Name));
            var msg = string.Empty;
            if (invasion.SecondReward)
            {
                //85%/15% Rate
                msg += $"85% - {first}\r\n";
                msg += $"15% - {second}\r\n";
            }
            else
            {
                //100% Rate
                msg += $"100% - {first}\r\n";
            }
            return msg;
        }

        public DiscordEmbed GeneratePokemonMessage(PokemonData pkmn, AlarmObject alarm, string city, string pokemonImageUrl)
        {
            //If IV has value then use alarmText if not null otherwise use default. If no stats use default missing stats alarmText
            //TODO: Add to alarm content and contentMissingInfo
            var alertMessageType = pkmn.IsMissingStats ? AlertMessageType.PokemonMissingStats : AlertMessageType.Pokemon;
            var alertMessage = alarm.Alerts[alertMessageType] ?? AlertMessage.Defaults[alertMessageType];
            var properties = GetProperties(pkmn, city);
            var eb = new DiscordEmbedBuilder
            {
                Title = DynamicReplacementEngine.ReplaceText(alertMessage.Title, properties),
                Url = DynamicReplacementEngine.ReplaceText(alertMessage.Url, properties),
                ImageUrl = properties["tilemaps_url"],
                ThumbnailUrl = pkmn.Id.GetPokemonImage(pokemonImageUrl, pkmn.Gender, pkmn.FormId, pkmn.Costume),
                Description = DynamicReplacementEngine.ReplaceText(alertMessage.Content, properties),
                Color = pkmn.IV.BuildColor(),
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"versx | {DateTime.Now}",
                    IconUrl = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId]?.IconUrl : string.Empty
                }
            };
            return eb.Build();
        }

        public DiscordEmbed GenerateRaidMessage(RaidData raid, AlarmObject alarm, string city, string pokemonRaidImageUrl)
        {
            var alertType = raid.PokemonId > 0 ? AlertMessageType.Raids : AlertMessageType.Eggs;
            var alert = alarm.Alerts[alertType] ?? AlertMessage.Defaults[alertType];
            var properties = GetProperties(raid, city);
            var img = raid.IsEgg ? string.Format(_whConfig.Urls.EggImage, raid.Level) : raid.PokemonId.GetPokemonImage(pokemonRaidImageUrl, PokemonGender.Unset, raid.Form);
            var eb = new DiscordEmbedBuilder
            {
                Title = DynamicReplacementEngine.ReplaceText(alert.Title, properties),
                Url = DynamicReplacementEngine.ReplaceText(alert.Url, properties),
                ImageUrl = properties["tilemaps_url"],
                ThumbnailUrl = img,
                Description = DynamicReplacementEngine.ReplaceText(alert.Content, properties),
                Color = raid.Level.BuildRaidColor(),
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"versx | {DateTime.Now}",
                    IconUrl = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId]?.IconUrl : string.Empty
                }
            };
            return eb.Build();
        }

        public DiscordEmbed GenerateQuestMessage(QuestData quest, AlarmObject alarm, string city)
        {
            var alertType = AlertMessageType.Quests;
            var alert = alarm.Alerts[alertType] ?? AlertMessage.Defaults[alertType];
            var properties = GetProperties(quest, city);
            var eb = new DiscordEmbedBuilder
            {
                Title = DynamicReplacementEngine.ReplaceText(alert.Title, properties),
                Url = DynamicReplacementEngine.ReplaceText(alert.Url, properties),
                ImageUrl = properties["tilemaps_url"],
                ThumbnailUrl = quest.PokestopUrl,
                Description = DynamicReplacementEngine.ReplaceText(alert.Content, properties),
                Color = DiscordColor.Orange
            };
            return eb.Build();
        }

        public DiscordEmbed GeneratePokestopMessage(PokestopData pokestop, AlarmObject alarm, string city)
        {
            var alertType = pokestop.HasInvasion ? AlertMessageType.Invasions : pokestop.HasLure ? AlertMessageType.Lures : AlertMessageType.Pokestops;
            var alert = alarm.Alerts[alertType] ?? AlertMessage.Defaults[alertType];
            var properties = GetProperties(pokestop, city);
            var eb = new DiscordEmbedBuilder
            {
                Title = DynamicReplacementEngine.ReplaceText(alert.Title, properties),
                Url = DynamicReplacementEngine.ReplaceText(alert.Url, properties),
                ImageUrl = properties["tilemaps_url"],
                ThumbnailUrl = pokestop.Url,
                Description = DynamicReplacementEngine.ReplaceText(alert.Content, properties),
                Color = pokestop.HasInvasion ? DiscordColor.Red : pokestop.HasLure ?
                    (pokestop.LureType == PokestopLureType.Normal ? DiscordColor.HotPink
                    : pokestop.LureType == PokestopLureType.Glacial ? DiscordColor.CornflowerBlue
                    : pokestop.LureType == PokestopLureType.Mossy ? DiscordColor.SapGreen
                    : pokestop.LureType == PokestopLureType.Magnetic ? DiscordColor.Gray
                    : DiscordColor.CornflowerBlue) : DiscordColor.CornflowerBlue
            };
            return eb.Build();
        }
    }

    public class UrlShortener
    {
        /*
        {
  "url": {
    "keyword":"1",
	"url":"https://www.google.com/maps?q=34.1351088673568,-118.051129828759",
	"title":"Google Maps",
	"date":"2019-05-25 04:48:55",
	"ip":"172.89.225.76"
  },
  "status":"success",
  "message":"https://www.google.com/maps?q=34.1351088673568,-118.051[...] added to database",
  "title":"Google Maps",
  "shorturl":"https://site.com/u/1",
  "statusCode":200
}*/
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("shorturl")]
        public string ShortUrl { get; set; }

        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }
    }
}

/*
'monsters': {
    'username': "<mon_name>",
    'content': "",
    'icon_url': get_image_url(
        "regular/monsters/<mon_id_3>_<form_id_3>.png"),
    'avatar_url': get_image_url(
        "regular/monsters/<mon_id_3>_<form_id_3>.png"),
    'title': "A wild <mon_name> has appeared!",
    'url': "<gmaps>",
    'body': "Available until <24h_time> (<time_left>)."
},

alert = {
    'webhook_url': settings.pop('webhook_url', self.__webhook_url),
    'username': settings.pop('username', default['username']),
    'avatar_url': settings.pop('avatar_url', default['avatar_url']),
    'disable_embed': parse_boolean(
        settings.pop('disable_embed', self.__disable_embed)),
    'content': settings.pop('content', default['content']),
    'icon_url': settings.pop('icon_url', default['icon_url']),
    'title': settings.pop('title', default['title']),
    'url': settings.pop('url', default['url']),
    'body': settings.pop('body', default['body']),
    'map': get_static_map_url(
        settings.pop('map', self.__map), self.__static_map_key)
}

def replace(string, pkinfo):
    for key in pkinfo:
        s = s.replace("<{}>".format(key), str(pkinfo[key]))
    return s
 */
