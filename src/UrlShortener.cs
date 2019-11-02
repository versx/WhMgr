namespace WhMgr
{
    using Newtonsoft.Json;

    //    public class EmbedBuilder
    //    {
    //        #region Variables

    //        private static readonly IEventLogger _logger = EventLogger.GetLogger();

    //        private readonly DiscordClient _client;
    //        private readonly WhConfig _whConfig;
    //        private readonly Translator _lang;

    //        #endregion

    //        #region Constructor

    //        public EmbedBuilder(DiscordClient client, WhConfig config, Translator lang)
    //        {
    //            _logger.Trace($"EmbedBuilder::EmbedBuilder");

    //            _client = client;
    //            _whConfig = config;
    //            _lang = lang;
    //        }

    //        #endregion

    //        #region Public Methods

    //        public DiscordEmbed BuildPokemonMessage(PokemonData pokemon, string city, string pokemonImageUrl)
    //        {
    //            //_logger.Trace($"EmbedBuilder::BuildPokemonMessage [Pokemon={pokemon.Id}, City={city}]");

    //            var db = Database.Instance;
    //            if (!db.Pokemon.ContainsKey(pokemon.Id))
    //                return null;

    //            var pkmn = db.Pokemon[pokemon.Id];
    //            if (pkmn == null)
    //            {
    //                _logger.Error($"Failed to lookup Pokemon '{pokemon.Id}' in database.");
    //                return null;
    //            }

    //            var gmapsLink = string.Format(Strings.GoogleMaps, pokemon.Latitude, pokemon.Longitude);
    //            var appleMapsLink = string.Format(Strings.AppleMaps, pokemon.Latitude, pokemon.Longitude);
    //            var staticMapLink = string.Format(_whConfig.Urls.StaticMap, pokemon.Latitude, pokemon.Longitude);
    //            var gmapsLocationLink = string.IsNullOrEmpty(_whConfig.ShortUrlApiUrl) ? gmapsLink : NetUtil.CreateShortUrl(_whConfig.ShortUrlApiUrl, gmapsLink);
    //            var appleMapsLocationLink = string.IsNullOrEmpty(_whConfig.ShortUrlApiUrl) ? appleMapsLink : NetUtil.CreateShortUrl(_whConfig.ShortUrlApiUrl, appleMapsLink);
    //            var gmapsStaticMapLink = string.IsNullOrEmpty(_whConfig.ShortUrlApiUrl) ? staticMapLink : NetUtil.CreateShortUrl(_whConfig.ShortUrlApiUrl, staticMapLink);

    //            var eb = new DiscordEmbedBuilder
    //            {
    //                Title = string.IsNullOrEmpty(city) ? _lang.Translate("EMBED_DIRECTIONS") : city,
    //                Url = gmapsLocationLink,
    //                ImageUrl = gmapsStaticMapLink,
    //                ThumbnailUrl = pokemon.Id.GetPokemonImage(pokemonImageUrl, pokemon.Gender, pokemon.FormId, pokemon.Costume),
    //                Color = pokemon.IV.BuildColor()
    //            };

    //            var form = pokemon.Id.GetPokemonForm(pokemon.FormId.ToString());
    //            var costume = pokemon.Id.GetCostume(pokemon.Costume.ToString());
    //            var pkmnName = pkmn.Name;
    //            if (pokemon.IsMissingStats)
    //            {
    //                eb.Description = _lang.Translate("EMBED_POKEMON_TITLE_WITHOUT_DETAILS").FormatText(
    //                    pkmnName, form, pokemon.Gender.GetPokemonGenderIconValue()) + "\r\n";
    //                eb.Description += _lang.Translate("EMBED_POKEMON_DESPAWN").FormatText(
    //                    pokemon.DespawnTime.ToLongTimeString(), pokemon.SecondsLeft.ToReadableString(true)) + (pokemon.DisappearTimeVerified ? "" : "~") + "\r\n";
    //            }
    //            else
    //            {
    //                eb.Description = _lang.Translate("EMBED_POKEMON_TITLE").FormatText(
    //                    pkmnName, form, pokemon.Gender.GetPokemonGenderIconValue(), pokemon.IV, pokemon.Attack, pokemon.Defense, pokemon.Stamina, pokemon.Level) + "\r\n";
    //                eb.Description += _lang.Translate("EMBED_POKEMON_DESPAWN").FormatText
    //                    (pokemon.DespawnTime.ToLongTimeString(), pokemon.SecondsLeft.ToReadableString(true)) + (pokemon.DisappearTimeVerified ? "" : "~") + "\r\n";
    //                eb.Description += _lang.Translate("EMBED_POKEMON_DETAILS").FormatText(
    //                    pokemon.CP, pokemon.IV, pokemon.Level) + "\r\n";
    //            }

    //            if (!string.IsNullOrEmpty(form))
    //            {
    //                eb.Description += _lang.Translate("EMBED_POKEMON_FORM").FormatText(form) + "\r\n";
    //            }

    //            if (!string.IsNullOrEmpty(costume))
    //            {
    //                eb.Description += $"**Costume:** {costume}\r\n";
    //            }

    //            //if (int.TryParse(pokemon.Level, out var lvl) && lvl >= 30)
    //            //{
    //            //    eb.Description += _lang.Translate("EMBED_POKEMON_WEATHER_BOOSTED") + "\r\n";
    //            //    //eb.Description += $":white_sun_rain_cloud: Boosted\r\n";
    //            //}

    //            //var maxCp = db.MaxCpAtLevel(pokemon.Id, 40);
    //            //var maxWildCp = db.MaxCpAtLevel(pokemon.Id, 35);
    //            //eb.Description += $"**Max Wild CP:** {maxWildCp}, **Max CP:** {maxCp} \r\n";

    //            if (pokemon.OriginalPokemonId > 0 && db.Pokemon.ContainsKey(pokemon.OriginalPokemonId))
    //            {
    //                var catchPkmn = db.Pokemon[pokemon.OriginalPokemonId];
    //                eb.Description += $"**Catch Pokemon:** {catchPkmn.Name}\r\n";
    //            }

    //            if (pokemon.Weather.HasValue && Strings.WeatherEmojis.ContainsKey(pokemon.Weather.Value) && pokemon.Weather != WeatherType.None)
    //            {
    //                var isWeatherBoosted = pkmn.IsWeatherBoosted(pokemon.Weather.Value);
    //                var isWeatherBoostedText = isWeatherBoosted ? " (Boosted)" : null;
    //                eb.Description += _lang.Translate("EMBED_POKEMON_WEATHER").FormatText(Strings.WeatherEmojis[pokemon.Weather.Value]) + isWeatherBoostedText + "\r\n";
    //            }

    //            var hasTypes = pkmn.Types != null;
    //            var typeEmojis = pkmn.Types.GetTypeEmojiIcons(_client, _whConfig.GuildId);
    //            if (hasTypes)
    //            {
    //                eb.Description += _lang.Translate("EMBED_TYPES").FormatText(typeEmojis);
    //            }

    //            if (pokemon.Size.HasValue)
    //            {
    //                eb.Description += " | " + _lang.Translate("EMBED_POKEMON_SIZE").FormatText(pokemon.Size.ToString()) + "\r\n";
    //            }
    //            else
    //            {
    //                eb.Description += "\r\n";
    //            }

    //            if (int.TryParse(pokemon.FastMove, out var fastMoveId) && db.Movesets.ContainsKey(fastMoveId))
    //            {
    //                var fastMove = db.Movesets[fastMoveId];
    //                eb.Description += _lang.Translate("EMBED_MOVE_FAST").FormatText(fastMove.Name) + "/";
    //            }

    //            if (int.TryParse(pokemon.ChargeMove, out var chargeMoveId) && db.Movesets.ContainsKey(chargeMoveId))
    //            {
    //                var chargeMove = db.Movesets[chargeMoveId];
    //                eb.Description += chargeMove.Name + "\r\n";//_lang.Translate("EMBED_MOVE_CHARGE").FormatText(chargeMove.Name) + "\r\n";
    //            }

    //            if (!string.IsNullOrEmpty(pokemon.PokestopId))
    //            {
    //                var pokestop = pokemon.GetPokestopById(_whConfig.ScannerConnectionString, pokemon.PokestopId);
    //                if (pokestop != null && !string.IsNullOrEmpty(pokestop?.Name))
    //                {
    //                    eb.Description += $"**Near Pokestop:** [{pokestop.Name}]({pokestop.Url})\r\n";
    //                }
    //            }


    //            if (pokemon.MatchesGreatLeague || pokemon.MatchesUltraLeague)
    //            {
    //                eb.Description += "\r\n__**PvP Stats Ranking**__\r\n";
    //            }

    //            if (pokemon.MatchesGreatLeague)
    //            {
    //                var greatLeague = pokemon.GreatLeagueStats;
    //                if (greatLeague != null)
    //                {
    //                    greatLeague = greatLeague.Where(x => x.Rank <= 25).ToList();
    //                    greatLeague.Sort((x, y) => x.Rank.CompareTo(y.Rank));
    //                    if (greatLeague.Count > 0)
    //                    {
    //                        eb.Description += "**Great League:**\r\n";
    //                        foreach (var stat in greatLeague)
    //                        {
    //                            eb.Description += $"**Rank:** #{stat.Rank}\t**Level:** {stat.Level}\t**CP:** {stat.CP}\r\n";
    //                        }
    //                    }
    //                }
    //            }

    //            if (pokemon.MatchesUltraLeague)
    //            {
    //                var ultraLeague = pokemon.UltraLeagueStats;
    //                if (ultraLeague != null)
    //                {
    //                    ultraLeague = ultraLeague.Where(x => x.Rank <= 25).ToList();
    //                    ultraLeague.Sort((x, y) => x.Rank.CompareTo(y.Rank));
    //                    if (ultraLeague.Count > 0)
    //                    {
    //                        eb.Description += "**Ultra League:**\r\n";
    //                        foreach (var stat in ultraLeague)
    //                        {
    //                            eb.Description += $"**Rank:** #{stat.Rank}\t**Level:** {stat.Level}\t**CP:** {stat.CP}\r\n";
    //                        }
    //                    }
    //                }
    //            }

    //            //eb.Description += _lang.Translate("EMBED_LOCATION").FormatText(Math.Round(pokemon.Latitude, 5), Math.Round(pokemon.Longitude, 5)) + "\r\n";
    //            eb.Description += _lang.Translate("EMBED_GMAPS").FormatText(gmapsLocationLink) + " " + _lang.Translate("EMBED_APPLEMAPS").FormatText(appleMapsLocationLink) + "\r\n";
    //            eb.Footer = new DiscordEmbedBuilder.EmbedFooter
    //            {
    //                Text = $"versx | {DateTime.Now}",
    //                IconUrl = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId]?.IconUrl : string.Empty
    //            };
    //            var embed = eb.Build();

    //            return embed;
    //        }

    //        public DiscordEmbed BuildRaidMessage(RaidData raid, string city, string pokemonRaidImageUrl)
    //        {
    //            //_logger.Trace($"EmbedBuilder::BuildRaidMessage [Raid={raid.PokemonId}, City={city}]");

    //            var db = Database.Instance;
    //            var pkmn = db.Pokemon[raid.PokemonId];
    //            if (pkmn == null)
    //            {
    //                _logger.Error($"Failed to lookup Raid Pokemon '{raid.PokemonId}' in database.");
    //                return null;
    //            }

    //            var pkmnImage = raid.IsEgg ? string.Format(_whConfig.Urls.EggImage, raid.Level) : raid.PokemonId.GetPokemonImage(pokemonRaidImageUrl, PokemonGender.Unset, raid.Form);
    //            var eb = new DiscordEmbedBuilder
    //            {
    //                Title = string.IsNullOrEmpty(city) ? _lang.Translate("EMBED_DIRECTIONS") : $"{city}: {raid.GymName}",
    //                Url = string.Format(Strings.GoogleMaps, raid.Latitude, raid.Longitude),
    //                ImageUrl = string.Format(_whConfig.Urls.StaticMap, raid.Latitude, raid.Longitude),
    //                ThumbnailUrl = pkmnImage,
    //                Color = Convert.ToInt32(raid.Level).BuildRaidColor()
    //            };

    //            if (raid.IsEgg)
    //            {
    //                eb.Description = _lang.Translate("EMBED_EGG_HATCHES").FormatText(raid.StartTime.ToLongTimeString(), DateTime.Now.GetTimeRemaining(raid.StartTime).ToReadableStringNoSeconds()) + "\r\n";
    //                eb.Description += _lang.Translate("EMBED_EGG_ENDS").FormatText(raid.EndTime.ToLongTimeString(), DateTime.Now.GetTimeRemaining(raid.EndTime).ToReadableStringNoSeconds()) + "\r\n";
    //            }
    //            else
    //            {
    //                var form = raid.PokemonId.GetPokemonForm(raid.Form.ToString());
    //                var pkmnName = $"{(string.IsNullOrEmpty(form) ? null : $"{form}-")}{pkmn.Name}";
    //                eb.Description += _lang.Translate("EMBED_RAID_ENDS").FormatText(pkmnName, raid.EndTime.ToLongTimeString()) + "\r\n";
    //                eb.Description += _lang.Translate("EMBED_RAID_STARTED").FormatText(raid.StartTime.ToLongTimeString()) + "\r\n";
    //                eb.Description += _lang.Translate("EMBED_RAID_ENDS_WITH_TIME_LEFT").FormatText(raid.EndTime.ToLongTimeString(), raid.EndTime.GetTimeRemaining().ToReadableStringNoSeconds()) + "\r\n";

    //                if (raid.Form > 0)
    //                {
    //                    eb.Description += _lang.Translate("EMBED_POKEMON_FORM").FormatText(form) + "\r\n";
    //                }

    //                var perfectRange = raid.PokemonId.MaxCpAtLevel(20);
    //                var boostedRange = raid.PokemonId.MaxCpAtLevel(25);
    //                var worstRange = raid.PokemonId.MinCpAtLevel(20);
    //                var worstBoosted = raid.PokemonId.MinCpAtLevel(25);
    //                eb.Description += _lang.Translate("EMBED_RAID_PERFECT_CP").FormatText(perfectRange, boostedRange) + "\r\n";
    //                eb.Description += _lang.Translate("EMBED_RAID_WORST_CP").FormatText(worstRange, worstBoosted) + "\r\n";

    //                if (pkmn.Types != null)
    //                {
    //                    eb.Description += _lang.Translate("EMBED_TYPES").FormatText(pkmn.Types.GetTypeEmojiIcons(_client, _whConfig.GuildId)) + $" | **Level:** {raid.Level}\r\n";
    //                }
    //                else
    //                {
    //                    eb.Description += $"**Level:** {raid.Level}\r\n";
    //                }

    //                if (db.Movesets.ContainsKey(raid.FastMove))
    //                {
    //                    var fastMove = db.Movesets[raid.FastMove];
    //                    eb.Description += _lang.Translate("EMBED_MOVE_FAST").FormatText(fastMove.Name) + "/";
    //                }

    //                if (db.Movesets.ContainsKey(raid.ChargeMove))
    //                {
    //                    var chargeMove = db.Movesets[raid.ChargeMove];
    //                    eb.Description += chargeMove.Name + "\r\n";//_lang.Translate("EMBED_MOVE_CHARGE").FormatText(chargeMove.Name) + "\r\n";
    //                }

    //                var weaknessesEmojis = pkmn.Types.GetWeaknessEmojiIcons(_client, _whConfig.GuildId);
    //                if (!string.IsNullOrEmpty(weaknessesEmojis))
    //                {
    //                    eb.Description += _lang.Translate("EMBED_RAID_WEAKNESSES").FormatText(weaknessesEmojis) + "\r\n";
    //                }
    //            }

    //            if (raid.IsExEligible)
    //            {
    //                var exEmojiId = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId].GetEmojiId(_lang.Translate("EMOJI_EX")) : 0;
    //                var exEmoji = exEmojiId > 0 ? $"<:ex:{exEmojiId}>" : "EX";
    //                eb.Description += _lang.Translate("EMBED_RAID_EX_GYM").FormatText(exEmoji) + "\r\n";
    //            }

    //            if (_client.Guilds.ContainsKey(_whConfig.GuildId))
    //            {
    //                var teamEmojiId = _client.Guilds[_whConfig.GuildId].GetEmojiId(raid.Team.ToString().ToLower());
    //                var teamEmoji = teamEmojiId > 0 ? $"<:{raid.Team.ToString().ToLower()}:{teamEmojiId}>" : raid.Team.ToString();
    //                eb.Description += _lang.Translate("EMBED_TEAM").FormatText(teamEmoji) + "\r\n";
    //            }

    //            eb.Description += _lang.Translate("EMBED_LOCATION").FormatText(Math.Round(raid.Latitude, 5), Math.Round(raid.Longitude, 5)) + "\r\n";
    //            eb.Description += _lang.Translate("EMBED_GMAPS").FormatText(string.Format(Strings.GoogleMaps, raid.Latitude, raid.Longitude)) + " " + _lang.Translate("EMBED_APPLEMAPS").FormatText(string.Format(Strings.AppleMaps, raid.Latitude, raid.Longitude)) + "\r\n";
    //            eb.Footer = new DiscordEmbedBuilder.EmbedFooter
    //            {
    //                Text = $"versx | {DateTime.Now}",
    //                IconUrl = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId]?.IconUrl : string.Empty
    //            };
    //            var embed = eb.Build();

    //            return embed;
    //        }

    //        public DiscordEmbed BuildQuestMessage(QuestData quest, string city)
    //        {
    //            //_logger.Trace($"EmbedBuilder::BuildQuestMessage [Quest={quest.PokestopId}, City={city}]");

    //            var gmapsUrl = string.Format(Strings.GoogleMaps, quest.Latitude, quest.Longitude);
    //            var eb = new DiscordEmbedBuilder
    //            {
    //                Title = $"{city}: {(string.IsNullOrEmpty(quest.PokestopName) ? _lang.Translate("UNKNOWN_POKESTOP") : quest.PokestopName)}",
    //                Url = gmapsUrl,
    //                ImageUrl = string.Format(_whConfig.Urls.StaticMap, quest.Latitude, quest.Longitude),
    //                ThumbnailUrl = quest.PokestopUrl,//quest.GetIconUrl(),
    //                Color = DiscordColor.Orange
    //            };

    //            eb.Description = _lang.Translate("EMBED_QUEST_QUEST").FormatText(quest.GetQuestMessage()) + "\r\n";
    //            if (quest.Conditions != null && quest.Conditions.Count > 0)
    //            {
    //                eb.Description += _lang.Translate("EMBED_QUEST_CONDITION").FormatText(quest.GetConditions()) + "\r\n";
    //            }
    //            eb.Description += _lang.Translate("EMBED_QUEST_REWARD").FormatText(quest.GetReward()) + "\r\n";
    //            eb.Description += _lang.Translate("EMBED_LOCATION").FormatText(Math.Round(quest.Latitude, 5), Math.Round(quest.Longitude, 5)) + "\r\n";
    //            eb.Description += _lang.Translate("EMBED_GMAPS").FormatText(string.Format(Strings.GoogleMaps, quest.Latitude, quest.Longitude)) + " " + _lang.Translate("EMBED_APPLEMAPS").FormatText(string.Format(Strings.AppleMaps, quest.Latitude, quest.Longitude)) + "\r\n";
    //            eb.Footer = new DiscordEmbedBuilder.EmbedFooter
    //            {
    //                Text = $"versx | {DateTime.Now}",
    //                IconUrl = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId]?.IconUrl : string.Empty
    //            };

    //            return eb.Build();
    //        }

    //        public DiscordEmbed BuildPokestopMessage(PokestopData pokestop, string city)
    //        {
    //            //_logger.Trace($"EmbedBuilder::BuildPokestopMessage [Pokestop={pokestop.PokestopId}, City={city}]");

    //            var gmapsUrl = string.Format(Strings.GoogleMaps, pokestop.Latitude, pokestop.Longitude);
    //            var eb = new DiscordEmbedBuilder
    //            {
    //                Title = $"{city}: {(string.IsNullOrEmpty(pokestop.Name) ? _lang.Translate("UNKNOWN_POKESTOP") : pokestop.Name)}",
    //                Url = gmapsUrl,
    //                ImageUrl = string.Format(_whConfig.Urls.StaticMap, pokestop.Latitude, pokestop.Longitude),
    //                ThumbnailUrl = pokestop.Url,
    //                Color = pokestop.HasInvasion ? DiscordColor.Red : pokestop.HasLure ?
    //                    (pokestop.LureType == PokestopLureType.Normal ? DiscordColor.HotPink
    //                    : pokestop.LureType == PokestopLureType.Glacial ? DiscordColor.CornflowerBlue
    //                    : pokestop.LureType == PokestopLureType.Mossy ? DiscordColor.SapGreen
    //                    : pokestop.LureType == PokestopLureType.Magnetic ? DiscordColor.Gray
    //                    : DiscordColor.CornflowerBlue) : DiscordColor.CornflowerBlue
    //            };

    //            if (pokestop.HasLure)
    //            {
    //                eb.Description += $"**Lured Until:** {pokestop.LureExpireTime.ToLongTimeString()} ({pokestop.LureExpireTime.GetTimeRemaining().ToReadableStringNoSeconds()})\r\n";
    //                if (pokestop.LureType != PokestopLureType.None)
    //                {
    //                    eb.Description += $"**Lure Type:** {pokestop.LureType}\r\n";
    //                }
    //            }
    //            if (pokestop.HasInvasion)
    //            {
    //                var invasion = new TeamRocketInvasion(pokestop.GruntType);
    //                var typeEmoji = invasion.Type == PokemonType.None ? "Tier II" : invasion.Type.GetTypeEmojiIcons(_client, _whConfig.GuildId);
    //                eb.Description += $"**Expires:** {pokestop.InvasionExpireTime.ToLongTimeString()} ({pokestop.InvasionExpireTime.GetTimeRemaining().ToReadableStringNoSeconds()})\r\n";
    //                eb.Description += $"**Type:** {typeEmoji} | **Gender:** {invasion.Gender.ToString()}\r\n";

    //                if (invasion.HasEncounter)
    //                {
    //                    eb.Description += $"**Encounter Reward Chance:**\r\n" + invasion.GetPossibleInvasionEncounters();
    //                }
    //            }

    //            eb.Description += _lang.Translate("EMBED_LOCATION").FormatText(Math.Round(pokestop.Latitude, 5), Math.Round(pokestop.Longitude, 5)) + "\r\n";
    //            eb.Description += _lang.Translate("EMBED_GMAPS").FormatText(string.Format(Strings.GoogleMaps, pokestop.Latitude, pokestop.Longitude)) + " " + _lang.Translate("EMBED_APPLEMAPS").FormatText(string.Format(Strings.AppleMaps, pokestop.Latitude, pokestop.Longitude)) + "\r\n";
    //            eb.Footer = new DiscordEmbedBuilder.EmbedFooter
    //            {
    //                Text = $"versx | {DateTime.Now}",
    //                IconUrl = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId]?.IconUrl : string.Empty
    //            };

    //            return eb.Build();
    //        }

    //        public DiscordEmbed BuildGymMessage(GymData gym, string city)
    //        {
    //            _logger.Trace($"EmbedBuilder::BuildGymMessage [Gym={gym.GymId}, City={city}]");

    //            var gmapsUrl = string.Format(Strings.GoogleMaps, gym.Latitude, gym.Longitude);
    //            var eb = new DiscordEmbedBuilder
    //            {
    //                Title = $"{city}: {(string.IsNullOrEmpty(gym.GymName) ? _lang.Translate("UNKNOWN_GYM") : gym.GymName)}",
    //                Url = gmapsUrl,
    //                ImageUrl = string.Format(_whConfig.Urls.StaticMap, gym.Latitude, gym.Longitude),
    //                ThumbnailUrl = gym.Url,
    //                Color = gym.Team == PokemonTeam.Mystic ? DiscordColor.Blue : 
    //                        gym.Team == PokemonTeam.Valor ? DiscordColor.Red : 
    //                        gym.Team == PokemonTeam.Instinct ? DiscordColor.Yellow : 
    //                        DiscordColor.Gray
    //            };

    //            return eb.Build();
    //        }

    //        public DiscordEmbed BuildGymDetailsMessage(GymDetailsData gymDetails, string city)
    //        {
    //            _logger.Trace($"EmbedBuilder::BuildGymDetailsMessage [Gym={gymDetails.GymId}, City={city}]");

    //            return null;
    //        }

    //        #endregion
    //    }

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