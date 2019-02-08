namespace WhMgr
{
    using System;

    using DSharpPlus;
    using DSharpPlus.Entities;

    using ServiceStack.OrmLite;

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

        public DiscordEmbed BuildPokemonMessage(PokemonData pokemon, string city)
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

            var eb = new DiscordEmbedBuilder
            {
                Title = string.IsNullOrEmpty(city) ? _lang.Translate("EMBED_DIRECTIONS") /*"DIRECTIONS"*/ : city,
                Url = string.Format(Strings.GoogleMaps, pokemon.Latitude, pokemon.Longitude),
                ImageUrl = string.Format(Strings.GoogleMapsStaticImage, pokemon.Latitude, pokemon.Longitude),// + $"&key={_whConfig.GmapsKey}",
                ThumbnailUrl = pokemon.Id.GetPokemonImage(pokemon.Gender, pokemon.FormId.ToString()),//string.Format(Strings.PokemonImage, pokemon.Id, Convert.ToInt32(pokemon.Gender)),//Convert.ToInt32(string.IsNullOrEmpty(pokemon.FormId) ? "0" : pokemon.FormId)),
                Color = pokemon.IV.BuildColor()
            };

            var form = pokemon.Id.GetPokemonForm(pokemon.FormId.ToString());
            var pkmnName = pkmn.Name;//$"{(string.IsNullOrEmpty(form) ? null : form + "-")}{pkmn.Name}";
            if (pokemon.IsMissingStats)
            {
                eb.Description = _lang.Translate("EMBED_POKEMON_TITLE_WITHOUT_DETAILS").FormatText(pkmnName, form, pokemon.Gender.GetPokemonGenderIcon(), pokemon.DespawnTime.ToLongTimeString(), pokemon.SecondsLeft.ToReadableStringNoSeconds()) + "\r\n"; //$"{pkmn.Name} {form}{pokemon.Gender.GetPokemonGenderIcon()} Despawn: {pokemon.DespawnTime.ToLongTimeString()} ({pokemon.SecondsLeft.ToReadableStringNoSeconds()} left)\r\n\r\n";
            }
            else
            {
                eb.Description = _lang.Translate("EMBED_POKEMON_TITLE").FormatText(pkmnName, form, pokemon.Gender.GetPokemonGenderIcon(), pokemon.IV, pokemon.Level, pokemon.DespawnTime.ToLongTimeString(), pokemon.SecondsLeft.ToReadableStringNoSeconds()) + "\r\n";
                eb.Description += _lang.Translate("EMBED_POKEMON_DETAILS").FormatText(pokemon.CP, pokemon.IV, pokemon.Level) + "\r\n";
                eb.Description += _lang.Translate("EMBED_POKEMON_STATS").FormatText(pokemon.Attack, pokemon.Defense, pokemon.Stamina) + "\r\n";
                //eb.Description = $"{pkmn.Name} {form}{pokemon.Gender.GetPokemonGenderIcon()} {pokemon.IV} L{pokemon.Level} Despawn: {pokemon.DespawnTime.ToLongTimeString()} ({pokemon.SecondsLeft.ToReadableStringNoSeconds()} left)\r\n\r\n";
                //eb.Description += $"**Details:** CP: {pokemon.CP} IV: {pokemon.IV} LV: {pokemon.Level}\r\n";
                //eb.Description += $"**IV Stats:** Atk: {pokemon.Attack}/Def: {pokemon.Defense}/Sta: {pokemon.Stamina}\r\n";
            }

            if (!string.IsNullOrEmpty(form))
            {
                eb.Description += _lang.Translate("EMBED_POKEMON_FORM").FormatText(form) + "\r\n";
                //eb.Description += $"**Form:** {form}\r\n";
            }

            //if (int.TryParse(pokemon.Level, out var lvl) && lvl >= 30)
            //{
            //    eb.Description += _lang.Translate("EMBED_POKEMON_WEATHER_BOOSTED") + "\r\n";
            //    //eb.Description += $":white_sun_rain_cloud: Boosted\r\n";
            //}

            //var maxCp = db.MaxCpAtLevel(pokemon.Id, 40);
            //var maxWildCp = db.MaxCpAtLevel(pokemon.Id, 35);
            //eb.Description += $"**Max Wild CP:** {maxWildCp}, **Max CP:** {maxCp} \r\n";

            if (Strings.WeatherEmojis.ContainsKey(pokemon.Weather))
            {
                eb.Description += _lang.Translate("EMBED_POKEMON_WEATHER").FormatText(Strings.WeatherEmojis[pokemon.Weather]) + (pkmn.IsWeatherBoosted(pokemon.Weather) ? " (Boosted)" : null) + "\r\n";
                //eb.Description += $"**Weather:** {Strings.WeatherEmojis[pokemon.Weather]}\r\n";
            }

            if (pkmn.Types != null)
            {
                eb.Description += _lang.Translate("EMBED_TYPES").FormatText(pkmn.Types.GetTypeEmojiIcons(_client, _whConfig.GuildId)) + "\r\n";
                //eb.Description += $"**Types:** {GetTypeEmojiIcons(pkmn.Types)}\r\n";
            }

            if (pokemon.Size.HasValue)
            {
                eb.Description += _lang.Translate("EMBED_POKEMON_SIZE").FormatText(pokemon.Size.ToString()) + "\r\n";
                //eb.Description += $"**Size:** {size}\r\n";
            }

            int.TryParse(pokemon.FastMove, out var fastMoveId);
            if (db.Movesets.ContainsKey(fastMoveId))
            {
                var fastMove = db.Movesets[fastMoveId];
                eb.Description += _lang.Translate("EMBED_MOVE_FAST").FormatText(fastMove.Name) + "\r\n";
                //eb.Description += $"**Fast Move:** {fastMove.Name}\r\n";
            }

            int.TryParse(pokemon.ChargeMove, out var chargeMoveId);
            if (db.Movesets.ContainsKey(chargeMoveId))
            {
                var chargeMove = db.Movesets[chargeMoveId];
                eb.Description += _lang.Translate("EMBED_MOVE_CHARGE").FormatText(chargeMove.Name) + "\r\n";
                //eb.Description += $"**Charge Move:** {chargeMove.Name}\r\n";
            }

            if (!string.IsNullOrEmpty(pokemon.PokestopId))
            {
                var pokestop = GetPokestopById(pokemon.PokestopId);
                if (pokestop != null && !string.IsNullOrEmpty(pokestop?.Name))
                {
                    eb.Description += $"**Near Pokestop:** [{pokestop.Name}]({pokestop.Url})\r\n";
                }
            }

            eb.Description += _lang.Translate("EMBED_LOCATION").FormatText(Math.Round(pokemon.Latitude, 5), Math.Round(pokemon.Longitude, 5)) + "\r\n";
            //eb.Description += $"**Location:** {Math.Round(pokemon.Latitude, 5)},{Math.Round(pokemon.Longitude, 5)}\r\n";
            //eb.Description += $"**Address:** {Utils.GetGoogleAddress(pokemon.Latitude, pokemon.Longitude, _whConfig.GmapsKey)?.Address}\r\n";
            eb.Description += _lang.Translate("EMBED_GMAPS").FormatText(string.Format(Strings.GoogleMaps, pokemon.Latitude, pokemon.Longitude)) + " " + _lang.Translate("EMBED_APPLEMAPS").FormatText(string.Format(Strings.AppleMaps, pokemon.Latitude, pokemon.Longitude)) + "\r\n";
            //eb.Description += $"**[Google Maps Link]({string.Format(Strings.GoogleMaps, pokemon.Latitude, pokemon.Longitude)})**";
            eb.ImageUrl = string.Format(Strings.GoogleMapsStaticImage, pokemon.Latitude, pokemon.Longitude);// + $"&key={_whConfig.GmapsKey}";
            eb.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"versx | {DateTime.Now}",
                IconUrl = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId]?.IconUrl : string.Empty
            };
            var embed = eb.Build();

            return embed;
        }

        public DiscordEmbed BuildRaidMessage(RaidData raid, string city)
        {
            //_logger.Trace($"EmbedBuilder::BuildRaidMessage [Raid={raid.PokemonId}, City={city}]");

            var db = Database.Instance;
            var pkmn = db.Pokemon[raid.PokemonId];
            if (pkmn == null)
            {
                _logger.Error($"Failed to lookup Raid Pokemon '{raid.PokemonId}' in database.");
                return null;
            }

            var pkmnImage = raid.IsEgg ? string.Format(Strings.EggImage, raid.Level) : raid.PokemonId.GetPokemonImage(PokemonGender.Unset, raid.Form.ToString()); //string.Format(Strings.PokemonImage, raid.PokemonId, 0);
            var eb = new DiscordEmbedBuilder
            {
                Title = string.IsNullOrEmpty(city) ? _lang.Translate("EMBED_DIRECTIONS") /*"DIRECTIONS"*/ : $"{city}: {raid.GymName}",
                Url = string.Format(Strings.GoogleMaps, raid.Latitude, raid.Longitude),
                ImageUrl = string.Format(Strings.GoogleMapsStaticImage, raid.Latitude, raid.Longitude),// + $"&key={_whConfig.GmapsKey}",
                ThumbnailUrl = pkmnImage,
                Color = Convert.ToInt32(raid.Level).BuildRaidColor()
            };

            if (raid.IsEgg)
            {
                eb.Description = _lang.Translate("EMBED_EGG_HATCHES").FormatText(raid.StartTime.ToLongTimeString(), DateTime.Now.GetTimeRemaining(raid.StartTime).ToReadableStringNoSeconds()) + "\r\n";
                eb.Description += _lang.Translate("EMBED_EGG_ENDS").FormatText(raid.EndTime.ToLongTimeString(), DateTime.Now.GetTimeRemaining(raid.EndTime).ToReadableStringNoSeconds()) + "\r\n";
                //eb.Description += $"Hatches: {raid.StartTime.ToLongTimeString()} ({DateTime.Now.GetTimeRemaining(raid.StartTime).ToReadableStringNoSeconds()} left)\r\n";
                //eb.Description += $"**Ends:** {raid.EndTime.ToLongTimeString()} ({DateTime.Now.GetTimeRemaining(raid.EndTime).ToReadableStringNoSeconds()} left)\r\n";
            }
            else
            {
                var form = raid.PokemonId.GetPokemonForm(raid.Form.ToString());
                var pkmnName = $"{(string.IsNullOrEmpty(form) ? null : $"{form}-")}{pkmn.Name}";//$"{(string.IsNullOrEmpty(form) ? null : form + "-")}{pkmn.Name}";
                eb.Description += _lang.Translate("EMBED_RAID_ENDS").FormatText(pkmnName, raid.EndTime.ToLongTimeString()) + "\r\n";
                eb.Description += _lang.Translate("EMBED_RAID_STARTED").FormatText(raid.StartTime.ToLongTimeString()) + "\r\n";
                eb.Description += _lang.Translate("EMBED_RAID_ENDS_WITH_TIME_LEFT").FormatText(raid.EndTime.ToLongTimeString(), raid.EndTime.GetTimeRemaining().ToReadableStringNoSeconds()) + "\r\n";

                if (raid.Form > 0)
                {
                    //var form = raid.PokemonId.GetPokemonForm(raid.Form.ToString());
                    eb.Description += _lang.Translate("EMBED_POKEMON_FORM").FormatText(form) + "\r\n";
                }
                //eb.Description += $"{pkmn.Name} Raid Ends: {raid.EndTime.ToLongTimeString()}\r\n\r\n";
                //eb.Description += $"**Started:** {raid.StartTime.ToLongTimeString()}\r\n";
                //eb.Description += $"**Ends:** {raid.EndTime.ToLongTimeString()} ({raid.EndTime.GetTimeRemaining().ToReadableStringNoSeconds()} left)\r\n";

                var perfectRange = raid.PokemonId.MaxCpAtLevel(20);
                var boostedRange = raid.PokemonId.MaxCpAtLevel(25);
                eb.Description += _lang.Translate("EMBED_RAID_PERFECT_CP").FormatText(perfectRange, boostedRange) + "\r\n";
                //eb.Description += $"**Perfect CP:** {perfectRange} / :white_sun_rain_cloud: {boostedRange}\r\n";

                eb.Description += $"**Level:** {raid.Level}\r\n";

                if (pkmn.Types != null)
                {
                    eb.Description += _lang.Translate("EMBED_TYPES").FormatText(pkmn.Types.GetTypeEmojiIcons(_client, _whConfig.GuildId)) + "\r\n";
                    //eb.Description += $"**Types:** {GetTypeEmojiIcons(pkmn.Types)}\r\n";
                }

                if (db.Movesets.ContainsKey(raid.FastMove))
                {
                    var fastMove = db.Movesets[raid.FastMove];
                    eb.Description += _lang.Translate("EMBED_MOVE_FAST").FormatText(fastMove.Name) + "\r\n";
                    //eb.Description += $"**Fast Move:** {fastMove.Name}\r\n";
                }

                if (db.Movesets.ContainsKey(raid.ChargeMove))
                {
                    var chargeMove = db.Movesets[raid.ChargeMove];
                    eb.Description += _lang.Translate("EMBED_MOVE_CHARGE").FormatText(chargeMove.Name) + "\r\n";
                    //eb.Description += $"**Charge Move:** {chargeMove.Name}\r\n";
                }

                var weaknessesEmojis = pkmn.Types.GetWeaknessEmojiIcons(_client, _whConfig.GuildId);
                if (!string.IsNullOrEmpty(weaknessesEmojis))
                {
                    eb.Description += _lang.Translate("EMBED_RAID_WEAKNESSES").FormatText(weaknessesEmojis) + "\r\n";
                    //eb.Description += $"**Weaknesses:** {weaknessesEmojis}\r\n";
                }
            }

            if (raid.IsExclusive || raid.SponsorId)
            {
                var exEmojiId = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId].GetEmojiId(_lang.Translate("EMOJI_EX")) : 0;
                var exEmoji = exEmojiId > 0 ? $"<:ex:{exEmojiId}>" : "EX";
                eb.Description += _lang.Translate("EMBED_RAID_EX_GYM").FormatText(exEmoji) + "\r\n";
                //eb.Description += $"{exEmoji} **Gym!**\r\n";
            }

            if (_client.Guilds.ContainsKey(_whConfig.GuildId))
            {
                var teamEmojiId = _client.Guilds[_whConfig.GuildId].GetEmojiId(raid.Team.ToString().ToLower());
                var teamEmoji = teamEmojiId > 0 ? $"<:{raid.Team.ToString().ToLower()}:{teamEmojiId}>" : raid.Team.ToString();
                eb.Description += _lang.Translate("EMBED_TEAM").FormatText(teamEmoji) + "\r\n";
            }

            eb.Description += _lang.Translate("EMBED_LOCATION").FormatText(Math.Round(raid.Latitude, 5), Math.Round(raid.Longitude, 5)) + "\r\n";
            //eb.Description += $"**Team:** {teamEmoji}\r\n";
            //eb.Description += $"**Location:** {Math.Round(raid.Latitude, 5)},{Math.Round(raid.Longitude, 5)}\r\n";
            //eb.Description += $"**Address:** {Utils.GetGoogleAddress(raid.Latitude, raid.Longitude, _whConfig.GmapsKey)?.Address}\r\n";
            eb.Description += _lang.Translate("EMBED_GMAPS").FormatText(string.Format(Strings.GoogleMaps, raid.Latitude, raid.Longitude)) + " " + _lang.Translate("EMBED_APPLEMAPS").FormatText(string.Format(Strings.AppleMaps, raid.Latitude, raid.Longitude)) + "\r\n";
            //eb.Description += $"**[Google Maps Link]({string.Format(Strings.GoogleMaps, raid.Latitude, raid.Longitude)})**";
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
                ImageUrl = string.Format(Strings.GoogleMapsStaticImage, quest.Latitude, quest.Longitude),//+ $"&key={_whConfig.GmapsKey}",
                ThumbnailUrl = quest.PokestopUrl,//quest.GetIconUrl(),
                Color = DiscordColor.Orange
            };

            eb.Description = _lang.Translate("EMBED_QUEST_QUEST").FormatText(quest.GetMessage()) + "\r\n";
            //eb.Description += $"**Quest:** {quest.GetMessage()}\r\n";
            if (quest.Conditions != null && quest.Conditions.Count > 0)
            {
                var condition = quest.Conditions[0];
                eb.Description += _lang.Translate("EMBED_QUEST_CONDITION").FormatText(quest.GetConditionName()) + "\r\n";
                //eb.Description += $"**Condition:** {quest.GetConditionName()}\r\n";
            }
            eb.Description += _lang.Translate("EMBED_QUEST_REWARD").FormatText(quest.GetRewardString()) + "\r\n";
            eb.Description += _lang.Translate("EMBED_TIME_REMAINING").FormatText(quest.TimeLeft.ToReadableStringNoSeconds()) + "\r\n";
            eb.Description += _lang.Translate("EMBED_LOCATION").FormatText(Math.Round(quest.Latitude, 5), Math.Round(quest.Longitude, 5)) + "\r\n";
            //eb.Description += $"**Reward:** {quest.GetRewardString()}\r\n";
            //eb.Description += $"**Time Remaining:** {quest.TimeLeft.ToReadableStringNoSeconds()}\r\n";
            //eb.Description += $"**Location:** {quest.Latitude},{quest.Longitude}\r\n";
            //eb.Description += $"**Address:** {Utils.GetGoogleAddress(quest.Latitude, quest.Longitude, _whConfig.GmapsKey)?.Address}\r\n";
            //eb.Description += $"**[[Click Here To View Pokestop]({quest.PokestopUrl})]**\r\n";
            eb.Description += _lang.Translate("EMBED_GMAPS").FormatText(string.Format(Strings.GoogleMaps, quest.Latitude, quest.Longitude)) + " " + _lang.Translate("EMBED_APPLEMAPS").FormatText(string.Format(Strings.AppleMaps, quest.Latitude, quest.Longitude)) + "\r\n";
            //eb.Description += $"**[Google Maps Link]({gmapsUrl})**\r\n";
            eb.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"versx | {DateTime.Now}",
                IconUrl = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId]?.IconUrl : string.Empty
            };

            return eb.Build();
        }

        public DiscordEmbed BuildPokestopMessage(PokestopData pokestop, string city)
        {
            _logger.Trace($"EmbedBuilder::BuildPokestopMessage [Pokestop={pokestop.PokestopId}, City={city}]");

            return null;
        }

        public DiscordEmbed BuildGymMessage(GymData gym, string city)
        {
            _logger.Trace($"EmbedBuilder::BuildGymMessage [Gym={gym.GymId}, City={city}]");

            return null;
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
                using (var db = DataAccessLayer.CreateFactory(_whConfig.ScannerConnectionString))
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
    }
}