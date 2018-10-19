namespace T
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using T.Configuration;
    using T.Data;
    using T.Diagnostics;
    using T.Extensions;

    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;

    public class Bot
    {
        #region Variables

        private readonly DiscordClient _client;
        private readonly WebHookManager _whm;
        private readonly WhConfig _whConfig;
        private readonly IEventLogger _logger;

        #endregion

        #region Constructor

        public Bot(WhConfig whConfig)
        {
            var name = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName;
            _logger = EventLogger.GetLogger(name);

            _whm = new WebHookManager(whConfig.WebHookPort);
            _whm.PokemonAlarmTriggered += Whm_PokemonAlarmTriggered;
            _whm.RaidAlarmTriggered += Whm_RaidAlarmTriggered;

            _logger.Info("WebHookManager is running...");

            _whConfig = whConfig;
            _client = new DiscordClient(new DiscordConfiguration
            {
                AutomaticGuildSync = true,
                AutoReconnect = true,
                EnableCompression = true,
                Token = _whConfig.Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true
            });
        }

        #endregion

        #region Public Methods

        public void Start()
        {
            _logger.Info($"Connecting to Discord...");
            _client.Ready += Client_Ready;
            _client.MessageCreated += Client_MessageCreated;
            _client.ConnectAsync();
        }

        #endregion

        #region Discord Events

        private async Task Client_Ready(ReadyEventArgs e)
        {
            _logger.Info($"Connected.");

            await Task.CompletedTask;
        }

        private async Task Client_MessageCreated(MessageCreateEventArgs e)
        {
            if (e.Author.IsBot)
                return;

            if (e.Author.Id == _whConfig.OwnerId)
            {
            //    if (e.Message.Content == "!stats")
            //    {
            //        var stats = await GetUserStatistics(e.Message);
            //        using (var sw = new System.IO.StreamWriter("user_statistics.txt"))
            //        {
            //            foreach (var stat in stats)
            //            {
            //                //await e.Message.RespondAsync();
            //                sw.WriteLine($"{stat.Value.Username} ({stat.Value.UserId}) {stat.Value.MessagesTotal.ToString()} Messages, Last Message {stat.Value.LastMessagePosted.ToLocalTime().ToString()}{Environment.NewLine}");
            //            }
            //        }
            //    }
            }

            await Task.CompletedTask;
        }

        #endregion

        public class DiscordUserStatistics
        {
            public ulong UserId { get; set; }

            public string Username { get; set; }

            public DateTimeOffset LastMessagePosted { get; set; }

            public long MessagesTotal { get; set; }

            public bool IsSupporter { get; set; }

            public bool IsModerator { get; set; }

            public bool IsOwner { get; set; }

            public DiscordUserStatistics(ulong userId, string username, DateTimeOffset lastMessagePosted, bool isSupporter = false, bool isModerator = false, bool isOwner = false)
            {
                UserId = userId;
                Username = username;
                LastMessagePosted = lastMessagePosted;
                IsSupporter = isSupporter;
                IsModerator = isModerator;
                IsOwner = isOwner;
                MessagesTotal = 0;
            }
        }
        private async Task<Dictionary<ulong, DiscordUserStatistics>> GetUserStatistics(DiscordMessage e)
        {
            var guild = e.Channel.Guild;//await _client.GetGuildAsync(_whConfig.GuidId);
            var members = guild.Members;
            var channels = guild.Channels.Where(x => x.Type == ChannelType.Text).ToList();
            var list = new Dictionary<ulong, DiscordUserStatistics>();

            for (int chId = 0; chId < channels.Count; chId++)
            {
                try
                {
                    var count = 100;
                    var juneMsg = 457421223987511296u;
                    var messages = await channels[chId].GetMessagesAsync(count, juneMsg);
                    for (int msgId = 0; msgId < messages.Count; msgId++)
                    {
                        try
                        {
                            for (int usrId = 0; usrId < members.Count; usrId++)
                            {
                                try
                                {
                                    if (messages[msgId].Author.Id == members[usrId].Id)
                                    {
                                        if (list.ContainsKey(messages[msgId].Author.Id))
                                        {
                                            var creationTime = messages[msgId].CreationTimestamp.ToLocalTime();
                                            var lastMessagePosted = list[messages[msgId].Author.Id].LastMessagePosted.ToLocalTime();
                                            if (creationTime > lastMessagePosted)
                                            {
                                                list[messages[msgId].Author.Id].LastMessagePosted =creationTime;
                                            }
                                            list[messages[msgId].Author.Id].MessagesTotal++;
                                        }
                                        else
                                        {
                                            var isSupporter = false;
                                            var isModerator = false;
                                            var isOwner = members[usrId].Id == _whConfig.OwnerId;
                                            list.Add(messages[msgId].Author.Id, new DiscordUserStatistics(messages[msgId].Author.Id, messages[msgId].Author.Username, messages[msgId].CreationTimestamp.ToLocalTime(), isSupporter, isModerator, isOwner));
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(ex);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            return list;
        }

        private async Task RemoveUserRoles(DiscordMessage e)
        {
            var roles = new string[]
            {
                "Rancho",
                "RanchoRaids",
                "Upland",
                "UplandRaids",
                "Ontario",
                "OntarioRaids",
                "Chino",
                "ChinoRaids",
                "Claremont",
                "ClaremontRaids",
                "Pomona",
                "PomonaRaids",
                "Montclair",
                "MontclairRaids",
                "EastLA",
                "EastLARaids",
                "Whittier",
                "WhittierRaids",
                "100iv",
                "90iv",
                "lunatone",
                "Nests",
                "Families",
                "Quests"
            };
            var members = e.Channel.Guild.Members;
            for (var usrId = 0; usrId < members.Count; usrId++)
            {
                var member = members[usrId];
                var roleIds = member.Roles.Select(x => x.Id).ToList();

                // Skip supporters
                if (roleIds.Contains(_whConfig.SupporterRoleId))
                    continue;

                foreach (var cityRole in roles)
                {
                    var roleNames = member.Roles.Select(x => x.Name).ToList();
                    if (roleNames.Contains(cityRole))
                    {
                        var role = member.Roles.FirstOrDefault(x => string.Compare(x.Name, cityRole, true) == 0);
                        await member.RevokeRoleAsync(role, "Roles refreshed.");
                    }
                }
            }
        }

        #region WebHookManager Events

        private async void Whm_PokemonAlarmTriggered(object sender, PokemonAlarmTriggeredEventArgs e)
        {
            if (!_whConfig.Enabled)
                return;

            _logger.Info($"Pokemon Found [Alarm: {e.Alarm.Name}, Pokemon: {e.Pokemon.Id}, Despawn: {e.Pokemon.DespawnTime}");

            var wh = _whm.WebHooks[e.Alarm.Name];//WebHookManager.GetWebHookData(e.Alarm.Webhook);
            if (wh == null)
            {
                _logger.Error($"Failed to parse webhook data from {e.Alarm.Name} {e.Alarm.Webhook}.");
                return;
            }

            //var guild = await _client.GetGuildAsync(wh.GuildId);
            //if (guild == null)
            //{
            //    _logger.Error($"Failed to parse guild from id {wh.GuildId}.");
            //    return;
            //}

            //var channel = guild.GetChannel(wh.ChannelId);
            //if (channel == null)
            //{
            //    _logger.Error($"Failed to parse channel from id {wh.ChannelId}.");
            //    return;
            //}

            var form = e.Pokemon.Id.GetPokemonForm(e.Pokemon.FormId);
            var pkmn = Database.Instance.Pokemon[e.Pokemon.Id];
            var pkmnImage = string.Format(Strings.PokemonImage, e.Pokemon.Id, Convert.ToInt32(string.IsNullOrEmpty(e.Pokemon.FormId) ? "0" : e.Pokemon.FormId));
            var eb = new DiscordEmbedBuilder
            {
                Title = e.Alarm.Geofence.Name == null || string.IsNullOrEmpty(e.Alarm.Geofence.Name) ? "DIRECTIONS" : e.Alarm.Geofence.Name,
                //Description = $"{pkmn.Name}{pokemon.Gender.GetPokemonGenderIcon()} {pokemon.CP}CP {pokemon.IV} Despawn: {pokemon.DespawnTime.ToLongTimeString()}",
                Url = string.Format(Strings.GoogleMaps, e.Pokemon.Latitude, e.Pokemon.Longitude),
                ImageUrl = string.Format(Strings.GoogleMapsStaticImage, e.Pokemon.Latitude, e.Pokemon.Longitude),
                ThumbnailUrl = pkmnImage,
                Color = DiscordColor.CornflowerBlue //DiscordHelpers.BuildColor(e.Pokemon.IV)
            };

            if (e.Pokemon.IV == "?" || e.Pokemon.Level == "?" || string.IsNullOrEmpty(e.Pokemon.Level))
            {
                eb.Description = $"{pkmn.Name} {form}{e.Pokemon.Gender.GetPokemonGenderIcon()} Despawn: {e.Pokemon.DespawnTime.ToLongTimeString()}\r\n";
            }
            else
            {
                eb.Description = $"{pkmn.Name} {form}{e.Pokemon.Gender.GetPokemonGenderIcon()} {e.Pokemon.IV} L{e.Pokemon.Level} Despawn: {e.Pokemon.DespawnTime.ToLongTimeString()}\r\n\r\n";
                eb.Description += $"**Details:** CP: {e.Pokemon.CP} IV: {e.Pokemon.IV} LV: {e.Pokemon.Level}\r\n";
            }
            eb.Description += $"**Despawn:** {e.Pokemon.DespawnTime.ToLongTimeString()} ({e.Pokemon.SecondsLeft.ToReadableStringNoSeconds()} left)\r\n";
            if (e.Pokemon.Attack != "?" && e.Pokemon.Defense != "?" && e.Pokemon.Stamina != "?" && e.Pokemon.Level != "?")
            {
                eb.Description += $"**IV Stats:** Atk: {e.Pokemon.Attack}/Def: {e.Pokemon.Defense}/Sta: {e.Pokemon.Stamina}\r\n";
            }

            if (!string.IsNullOrEmpty(form))
            {
                eb.Description += $"**Form:** {form}\r\n";
            }

            if (int.TryParse(e.Pokemon.Level, out int lvl) && lvl >= 30)
            {
                eb.Description += $":white_sun_rain_cloud: Boosted\r\n";
            }

            //var maxCp = _db.MaxCpAtLevel(e.Pokemon.Id, 40);
            //var maxWildCp = _db.MaxCpAtLevel(e.Pokemon.Id, 35);
            //eb.Description += $"**Max Wild CP:** {maxWildCp}, **Max CP:** {maxCp} \r\n";

            //if (pkmn.Types.Count > 0)
            //{
            //    var types = new List<string>();
            //    pkmn.Types.ForEach(x =>
            //    {
            //        if (Strings.TypeEmojis.ContainsKey(x.Type.ToLower()))
            //        {
            //            types.Add($"{Strings.TypeEmojis[x.Type.ToLower()]} {x.Type}");
            //        }
            //    });
            //    eb.Description += $"**Types:** {string.Join("/", types)}\r\n";
            //}

            //if (float.TryParse(e.Pokemon.Height, out float height) && float.TryParse(e.Pokemon.Weight, out float weight))
            //{
            //    var size = _db.GetSize(e.Pokemon.Id, height, weight);
            //    eb.Description += $"**Size:** {size}\r\n";
            //}

            //var fastMove = _db.Movesets.ContainsKey(e.Pokemon.FastMove) ? _db.Movesets[e.Pokemon.FastMove] : null;
            //if (fastMove != null)
            //{
            //    //var fastMoveIcon = Strings.TypeEmojis.ContainsKey(fastMove.Type.ToLower()) ? Strings.TypeEmojis[fastMove.Type.ToLower()] : fastMove.Type;
            //    eb.Description += $"**Fast Move:** {fastMove.Name} ({fastMove.Type})\r\n";
            //}

            //var chargeMove = _db.Movesets.ContainsKey(e.Pokemon.ChargeMove) ? _db.Movesets[e.Pokemon.ChargeMove] : null;
            //if (chargeMove != null)
            //{
            //    //var chargeMoveIcon = Strings.TypeEmojis.ContainsKey(chargeMove.Type.ToLower()) ? Strings.TypeEmojis[chargeMove.Type.ToLower()] : chargeMove.Type;
            //    eb.Description += $"**Charge Move:** {chargeMove.Name} ({chargeMove.Type})\r\n";
            //}

            eb.Description += $"**Location:** {Math.Round(e.Pokemon.Latitude, 5)},{Math.Round(e.Pokemon.Longitude, 5)}";
            eb.ImageUrl = string.Format(Strings.GoogleMapsStaticImage, e.Pokemon.Latitude, e.Pokemon.Longitude) + $"&key={_whConfig.GmapsKey}";
            eb.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"versx | {DateTime.Now}"
            };

            //await channel.SendMessageAsync(string.Empty, false, eb);
            var whData = await _client.GetWebhookWithTokenAsync(wh.Id, wh.Token);
            await whData.ExecuteAsync(string.Empty, pkmn.Name, pkmnImage, false, new List<DiscordEmbed> { eb });
        }

        private async void Whm_RaidAlarmTriggered(object sender, RaidAlarmTriggeredEventArgs e)
        {
            if (!_whConfig.Enabled)
                return;

            _logger.Info($"Raid Found [Alarm: {e.Alarm.Name}, Raid: {e.Raid.PokemonId}, Level: {e.Raid.Level}, StartTime: {e.Raid.StartTime}]");

            var wh = _whm.WebHooks[e.Alarm.Name];//WebHookManager.GetWebHookData(e.Alarm.Webhook);
            if (wh == null)
            {
                _logger.Error($"Failed to parse webhook data from {e.Alarm.Name} {e.Alarm.Webhook}.");
                return;
            }

            //var guild = await _client.GetGuildAsync(wh.GuildId);
            //if (guild == null)
            //{
            //    _logger.Error($"Failed to parse guild from id {wh.GuildId}.");
            //    return;
            //}

            //var channel = guild.GetChannel(wh.ChannelId);
            //if (channel == null)
            //{
            //    _logger.Error($"Failed to parse channel from id {wh.ChannelId}.");
            //    return;
            //}

            var pkmn = Database.Instance.Pokemon[e.Raid.PokemonId] ?? new Data.Models.PokemonModel { Name = "Egg" };
            var pkmnImage = string.Format(Strings.PokemonImage, e.Raid.PokemonId, 0);
            var eb = new DiscordEmbedBuilder
            {
                Title = e.Alarm.Geofence.Name == null || string.IsNullOrEmpty(e.Alarm.Geofence.Name) ? "DIRECTIONS" : e.Alarm.Geofence.Name,
                //Description = $"{pkmn.Name} raid available until {raid.EndTime.ToLongTimeString()}!",
                Url = string.Format(Strings.GoogleMaps, e.Raid.Latitude, e.Raid.Longitude),
                ImageUrl = string.Format(Strings.GoogleMapsStaticImage, e.Raid.Latitude, e.Raid.Longitude),
                ThumbnailUrl = pkmnImage,
                Color = DiscordColor.Red//DiscordHelpers.BuildRaidColor(Convert.ToInt32(e.Raid.Level))
            };

            eb.Description += $"{pkmn.Name} Raid Ends: {e.Raid.EndTime.ToLongTimeString()}\r\n";
            eb.Description = $"{e.Raid.GymName}\r\n\r\n";
            eb.Description += $"**Starts:** {e.Raid.StartTime.ToLongTimeString()}\r\n";
            eb.Description += $"**Ends:** {e.Raid.EndTime.ToLongTimeString()} ({e.Raid.EndTime.GetTimeRemaining().ToReadableStringNoSeconds()} left)\r\n";

            //var perfectRange = _db.GetPokemonCpRange(raid.PokemonId, 20);
            //var boostedRange = _db.GetPokemonCpRange(raid.PokemonId, 25);
            //eb.Description += $"**Perfect CP:** {perfectRange.Best} / :white_sun_rain_cloud: {boostedRange.Best}\r\n";

            //if (pkmn.Types.Count > 0)
            //{
            //    var types = new List<string>();
            //    pkmn.Types.ForEach(x =>
            //    {
            //        if (Strings.TypeEmojis.ContainsKey(x.Type.ToLower()))
            //        {
            //            types.Add(Strings.TypeEmojis[x.Type.ToLower()] + " " + x.Type);
            //        }
            //    });
            //    eb.Description += $"**Types:** {string.Join("/", types)}\r\n";
            //}

            //TODO: Moveset information.
            var fastMove = Database.Instance.Movesets.ContainsKey(Convert.ToInt32(e.Raid.FastMove ?? "0")) ? Database.Instance.Movesets[Convert.ToInt32(e.Raid.FastMove ?? "0")] : null;
            if (fastMove != null)
            {
                //eb.Description += $"**Fast Move:** {Strings.TypeEmojis[fastMove.Type.ToLower()]} {fastMove.Name}\r\n";
                eb.Description += $"**Fast Move:** {fastMove.Name}\r\n";
            }

            var chargeMove = Database.Instance.Movesets.ContainsKey(Convert.ToInt32(e.Raid.ChargeMove ?? "0")) ? Database.Instance.Movesets[Convert.ToInt32(e.Raid.ChargeMove ?? "0")] : null;
            if (chargeMove != null)
            {
                //eb.Description += $"**Charge Move:** {Strings.TypeEmojis[chargeMove.Type.ToLower()]} {chargeMove.Name}\r\n";
                eb.Description += $"**Charge Move:** {chargeMove.Name}\r\n";
            }

            //var strengths = new List<string>();
            //var weaknesses = new List<string>();
            //foreach (var type in pkmn.Types)
            //{
            //    foreach (var strength in PokemonExtensions.GetStrengths(type.Type))
            //    {
            //        if (!strengths.Contains(strength))
            //        {
            //            strengths.Add(strength);
            //        }
            //    }
            //    foreach (var weakness in PokemonExtensions.GetWeaknesses(type.Type))
            //    {
            //        if (!weaknesses.Contains(weakness))
            //        {
            //            weaknesses.Add(weakness);
            //        }
            //    }
            //}

            //if (strengths.Count > 0)
            //{
            //    eb.Description += $"**Strong Against:** {string.Join(", ", strengths)}\r\n";
            //}

            //if (weaknesses.Count > 0)
            //{
            //    eb.Description += $"**Weaknesses:** {string.Join(", ", weaknesses)}\r\n";
            //}

            eb.Description += $"**Location:** {Math.Round(e.Raid.Latitude, 5)},{Math.Round(e.Raid.Longitude, 5)}";
            eb.ImageUrl = string.Format(Strings.GoogleMapsStaticImage, e.Raid.Latitude, e.Raid.Longitude) + $"&key={_whConfig.GmapsKey}";
            eb.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"versx | {DateTime.Now}"
            };

            //await channel.SendMessageAsync(string.Empty, false, eb);
            var whData = await _client.GetWebhookWithTokenAsync(wh.Id, wh.Token);
            await whData.ExecuteAsync(string.Empty, pkmn.Name, pkmnImage, false, new List<DiscordEmbed> { eb });

        }

        #endregion
    }
}