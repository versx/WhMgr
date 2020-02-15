namespace WhMgr.Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;

    using Newtonsoft.Json;

    using WhMgr.Data;
    using WhMgr.Data.Subscriptions.Models;
    using WhMgr.Diagnostics;
    using WhMgr.Extensions;
    using WhMgr.Net.Models;
    using WhMgr.Utilities;

    public class Notifications
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger("NOTIFICATIONS");

        private readonly Dependencies _dep;

        public Notifications(Dependencies dep)
        {
            _dep = dep;
        }

        #region General

        [
            Command("info"),
            Description("Shows your current Pokemon and Raid boss notification subscriptions.")
        ]
        public async Task InfoAsync(CommandContext ctx,
            [Description("Discord user mention string.")] string mention = "")
        {
            if (!await CanExecute(ctx))
                return;

            if (string.IsNullOrEmpty(mention))
            {
                await SendUserSubscriptionSettings(ctx.Client, ctx.User, ctx.User, ctx.Guild.Id);
                return;
            }

            if (!ctx.User.Id.IsModeratorOrHigher(ctx.Guild.Id, _dep.WhConfig))
            {
                await ctx.RespondEmbed(_dep.Language.Translate("MSG_NOT_MODERATOR_OR_HIGHER").FormatText(ctx.User.Mention), DiscordColor.Red);
                return;
            }

            var userId = ConvertMentionToUserId(mention);
            if (userId <= 0)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("MSG_INVALID_USER_MENTION").FormatText(ctx.User.Mention, mention), DiscordColor.Red);
                return;
            }

            var user = await ctx.Client.GetUserAsync(userId);
            if (user == null)
            {
                _logger.Warn($"Failed to get Discord user with id {userId}.");
                return;
            }

            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();

            await SendUserSubscriptionSettings(ctx.Client, ctx.User, user, ctx.Guild.Id);
        }

        [
            Command("enable"),
            Aliases("disable"),
            Description("Enables or disables all of your Pokemon and Raid notification subscriptions at once.")
        ]
        public async Task EnableDisableAsync(CommandContext ctx)
        {
            if (!await CanExecute(ctx))
                return;

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.Guild.Id, ctx.User.Id);
            if (subscription == null)
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed(_dep.Language.Translate("MSG_USER_NOT_SUBSCRIBED").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            var cmd = ctx.Message.Content.TrimStart('.', ' ');
            subscription.Enabled = cmd.ToLower().Contains("enable");
            subscription.Save();

            await ctx.TriggerTypingAsync();
            await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_ENABLE_DISABLE").FormatText(ctx.User.Username, cmd));
            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
        }

        [
            Command("set-distance"),
            Description("Set the distance and location you'd like to receive raid notifications.")
        ]
        public async Task SetDistanceAsync(CommandContext ctx,
            [Description("Maximum distance in meters between the set coordinates.")] int distance,
            [Description("Coordinates in `34.00,-117.00` format."), RemainingText] string coordinates)
        {
            if (!await CanExecute(ctx))
                return;

            var parts = coordinates.Replace(" ", null).Split(',');
            if (!double.TryParse(parts[0], out var lat) || !double.TryParse(parts[1], out var lng))
            {
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_COORDINATES").FormatText(ctx.User.Username, coordinates), DiscordColor.Red);
                return;
            }

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.Guild.Id, ctx.User.Id);
            if (subscription == null)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("MSG_USER_NOT_SUBSCRIBED").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            subscription.DistanceM = distance;
            subscription.Latitude = lat;
            subscription.Longitude = lng;
            subscription.Save();

            await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_DISTANCE_SET").FormatText(ctx.User.Username, distance, lat, lng));
            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
        }

        [
            Command("expire"),
            Aliases("expires"),
            Description("")
        ]
        public async Task GetExpireDateAsync(CommandContext ctx)
        {
            if (!await CanExecute(ctx))
                return;

            var guildId = ctx.Guild?.Id ?? 0;
            if (guildId == 0)
                return;

            var message = BuildExpirationMessage(guildId, ctx.User);
            await ctx.Client.SendDirectMessage(ctx.User, message);
        }

        [
            Command("expire-admin"),
            Description(""),
            RequireOwner
        ]
        public async Task GetExpireAdminAsync(CommandContext ctx,
            [Description("Discord user id to check expire date for")] string userId)
        {
            if (!await CanExecute(ctx))
                return;

            if (!ulong.TryParse(userId, out var realUserId))
            {
                await ctx.RespondEmbed(_dep.Language.Translate("ERROR_PARSING_USER_ID").FormatText(ctx.User.Username, userId), DiscordColor.Red);
                return;
            }

            var guildId = ctx.Guild?.Id ?? _dep.WhConfig.Servers[ctx.Guild.Id].GuildId;
            var user = await ctx.Client.GetUserAsync(realUserId);
            var message = BuildExpirationMessage(guildId, user);
            await ctx.Client.SendDirectMessage(ctx.User, message);
        }

        #endregion

        #region Pokeme / Pokemenot

        [
            Command("pokeme"),
            Description("Subscribe to Pokemon notifications based on the pokedex number or name, minimum IV stats, or minimum level.")
        ]
        public async Task PokeMeAsync(CommandContext ctx,
            [Description("Comma delimited list of Pokemon name(s) and/or Pokedex IDs to subscribe to Pokemon spawn notifications.")] string poke,
            [Description("Minimum IV to receive notifications for, use 0 to disregard IV.")] string iv = "0",
            [Description("Minimum level to receive notifications for, use 0 to disregard level.")] int lvl = 0,
            [Description("Specific gender the Pokemon must be, use * to disregard gender.")] string gender = "*")
        {
            if (!await CanExecute(ctx))
                return;

            //if (!int.TryParse(cpArg, out int cp))
            //{
            //    await message.RespondEmbed($"'{cpArg}' is not a valid value for CP.", DiscordColor.Red);
            //    return;
            //}

            var attack = 0;
            var defense = 0;
            var stamina = 0;
            var realIV = 0;
            if (iv.Contains("-"))
            {
                var split = iv.Split('-');
                if (split.Length != 3)
                {
                    await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_IV_VALUES").FormatText(ctx.User.Username, iv), DiscordColor.Red);
                    return;
                }
                if (!int.TryParse(split[0], out attack))
                {
                    await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_ATTACK_VALUE").FormatText(ctx.User.Username, split[0]), DiscordColor.Red);
                    return;
                }
                if (!int.TryParse(split[1], out defense))
                {
                    await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_DEFENSE_VALUE").FormatText(ctx.User.Username, split[1]), DiscordColor.Red);
                    return;
                }
                if (!int.TryParse(split[2], out stamina))
                {
                    await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_STAMINA_VALUE").FormatText(ctx.User.Username, split[2]), DiscordColor.Red);
                    return;
                }
            }
            else
            {
                if (!int.TryParse(iv, out realIV) || realIV < 0 || realIV > 100)
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_IV_RANGE").FormatText(ctx.User.Username, iv), DiscordColor.Red);
                    return;
                }
            }

            if (gender != "*" && gender != "m" && gender != "f")
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_GENDER").FormatText(ctx.User.Username, gender), DiscordColor.Red);
                return;
            }

            if (lvl < 0 || lvl > 35)
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_LEVEL").FormatText(ctx.User.Username, lvl), DiscordColor.Red);
                return;
            }

            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.Guild.Id, ctx.User.Id);

            try
            {
                if (string.Compare(poke, Strings.All, true) == 0)
                {
                    if (realIV < 90)
                    {
                        await ctx.TriggerTypingAsync();
                        await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_MINIMUM_IV").FormatText(ctx.User.Username), DiscordColor.Red);
                        return;
                    }

                    await ctx.TriggerTypingAsync();
                    for (int i = 1; i < Strings.MaxPokemonIds; i++)
                    {
                        var subPkmn = subscription.Pokemon.FirstOrDefault(x => x.PokemonId == i);
                        //Always ignore the user's input for Unown, Azelf, Mesprit, or Uxie and set it to 0 by default.
                        var minIV = IsRarePokemon(i) ? 0 : realIV;
                        var minLvl = IsRarePokemon(i) ? 0 : lvl;
                        if (subPkmn == null)
                        {
                            //Does not exist, create.
                            subscription.Pokemon.Add(new PokemonSubscription
                            {
                                GuildId = ctx.Guild.Id,
                                UserId = ctx.User.Id,
                                PokemonId = i,
                                Form = null,
                                MinimumIV = minIV,
                                MinimumLevel = minLvl,
                                Gender = gender
                            });
                            continue;
                        }

                        //Exists, check if anything changed.
                        if (realIV != subPkmn.MinimumIV ||
                            lvl != subPkmn.MinimumLevel ||
                            gender != subPkmn.Gender)
                        {
                            subPkmn.MinimumIV = minIV;
                            subPkmn.MinimumLevel = minLvl;
                            subPkmn.Gender = gender;
                        }
                    }

                    subscription.Save();

                    await ctx.TriggerTypingAsync();
                    await ctx.RespondEmbed($"{ctx.User.Username} subscribed to **all** Pokemon notifications with a minimum IV of {iv}%{(lvl > 0 ? $" and a minimum level of {lvl}" : null)}{(gender == "*" ? null : $" and only '{gender}' gender types")}.");
                    _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.Debug($"[ERROR] POKEME FAILED-----------------------------------");
                _logger.Error(ex);
            }

            var alreadySubscribed = new List<string>();
            var subscribed = new List<string>();
            var isModOrHigher = ctx.User.Id.IsModeratorOrHigher(ctx.Guild.Id, _dep.WhConfig);
            var validation = ValidatePokemonList(poke);
            if (validation == null || validation.Valid.Count == 0)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_POKEMON_IDS_OR_NAMES").FormatText(ctx.User.Username, string.Join(", ", validation.Invalid)), DiscordColor.Red);
                return;
            }

            var keys = validation.Valid.Keys.ToList();
            for (var i = 0; i < keys.Count; i++)
            {
                var pokemonId = keys[i];
                var form = validation.Valid[pokemonId];

                if (!MasterFile.Instance.Pokedex.ContainsKey(pokemonId))
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_POKEMON_ID").FormatText(ctx.User.Username, pokemonId), DiscordColor.Red);
                    continue;
                }

                var pokemon = MasterFile.Instance.Pokedex[pokemonId];
                var name = string.IsNullOrEmpty(form) ? pokemon.Name : pokemon.Name + "-" + form;

                //Check if common type pokemon e.g. Pidgey, Ratatta, Spinarak 'they are beneath him and he refuses to discuss them further'
                if (IsCommonPokemon(pokemonId) && realIV < Strings.CommonTypeMinimumIV && !isModOrHigher)
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_COMMON_TYPE_POKEMON").FormatText(ctx.User.Username, pokemon.Name, Strings.CommonTypeMinimumIV), DiscordColor.Red);
                    continue;
                }

                var subPkmn = subscription.Pokemon.FirstOrDefault(x => x.PokemonId == pokemonId && string.Compare(x.Form, form, true) == 0);
                //Always ignore the user's input for Unown and set it to 0 by default.
                var minIV = IsRarePokemon(pokemonId) ? 0 : realIV;
                var minLvl = IsRarePokemon(pokemonId) ? 0 : lvl;
                if (subPkmn == null)
                {
                    //Does not exist, create.
                    subscription.Pokemon.Add(new PokemonSubscription
                    {
                        GuildId = ctx.Guild.Id,
                        UserId = ctx.User.Id,
                        PokemonId = pokemonId,
                        Form = form,
                        MinimumIV = minIV,
                        MinimumLevel = minLvl,
                        Gender = gender
                    });
                    subscribed.Add(name);
                    continue;
                }

                //Exists, check if anything changed.
                if (realIV != subPkmn.MinimumIV ||
                    string.Compare(form, subPkmn.Form, true) != 0 ||
                    lvl != subPkmn.MinimumLevel ||
                    gender != subPkmn.Gender ||
                    attack != subPkmn.Attack ||
                    defense != subPkmn.Defense ||
                    stamina != subPkmn.Stamina)
                {
                    subPkmn.Form = form;
                    subPkmn.MinimumIV = realIV;
                    subPkmn.MinimumLevel = lvl;
                    subPkmn.Gender = gender;
                    subPkmn.Attack = attack;
                    subPkmn.Defense = defense;
                    subPkmn.Stamina = stamina;
                    subscribed.Add(name);
                    continue;
                }

                //Already subscribed to the same Pokemon and form
                alreadySubscribed.Add(name);
            }

            subscription.Save();
            await ctx.TriggerTypingAsync();
            if (subscribed.Count == 0 && alreadySubscribed.Count == 0)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_POKEMON_SPECIFIED").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            await ctx.RespondEmbed
            (
                (subscribed.Count > 0
                    ? $"{ctx.User.Username} has subscribed to **{string.Join("**, **", subscribed)}** notifications with a{(attack > 0 || defense > 0 || stamina > 0 ? $"n IV value of {attack}/{defense}/{stamina}" : $" minimum IV of {iv}%")}{(lvl > 0 ? $" and a minimum level of {lvl}" : null)}{(gender == "*" ? null : $" and only '{gender}' gender types")}."
                    : string.Empty) +
                (alreadySubscribed.Count > 0
                    ? $"\r\n{ctx.User.Username} is already subscribed to **{string.Join("**, **", alreadySubscribed)}** notifications with a{(attack > 0 || defense > 0 || stamina > 0 ? $"n IV value of {attack}/{defense}/{stamina}" : $" minimum IV of {iv}%")}{(lvl > 0 ? $" and a minimum level of {lvl}" : null)}{(gender == "*" ? null : $" and only '{gender}' gender types")}."
                    : string.Empty)
            );

            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
        }

        [
            Command("pokemenot"),
            Description("Unsubscribe from one or more or even all subscribed Pokemon notifications by pokedex number or name.")
        ]
        public async Task PokeMeNotAsync(CommandContext ctx,
            [Description("Pokemon name or id to unsubscribe from Pokemon spawn notifications.")] string poke)
        {
            if (!await CanExecute(ctx))
                return;

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.Guild.Id, ctx.User.Id);
            if (subscription == null || subscription?.Pokemon.Count == 0)
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_NO_POKEMON_SUBSCRIPTIONS").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            if (string.Compare(poke, Strings.All, true) == 0)
            {
                var confirm = await ctx.Confirm(_dep.Language.Translate("NOTIFY_CONFIRM_REMOVE_ALL_POKEMON_SUBSCRIPTIONS").FormatText(ctx.User.Username, subscription.Pokemon.Count.ToString("N0")));
                if (!confirm)
                    return;

                subscription.Pokemon.ForEach(x => x.Id.Remove<PokemonSubscription>());
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_SUCCESS_REMOVE_ALL_POKEMON_SUBSCRIPTIONS").FormatText(ctx.User.Username));
                _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
                return;
            }

            var validation = ValidatePokemonList(poke);
            if (validation.Valid == null || validation.Valid.Count == 0)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_POKEMON_IDS_OR_NAMES").FormatText(ctx.User.Username, string.Join(", ", validation.Invalid)), DiscordColor.Red);
                return;
            }

            //subscription.Pokemon
            //    .Where(x =>
            //           validation.Valid.ContainsKey(x.PokemonId) &&
            //           string.Compare(validation.Valid[x.PokemonId], x.Form, true) == 0)?
            //    .ToList()?
            //    .ForEach(x => x.Id.Remove<PokemonSubscription>()
            //);

            var pokemonNames = validation.Valid.Select(x => MasterFile.Instance.Pokedex[x.Key].Name + (string.IsNullOrEmpty(x.Value) ? string.Empty : "-" + x.Value));
            var error = false;
            var keys = validation.Valid.Keys.ToList();
            for (var i = 0; i < keys.Count; i++)
            {
                var pokemonId = keys[i];
                var form = validation.Valid[pokemonId];
                var pkmnSub = subscription.Pokemon.FirstOrDefault(x => x.PokemonId == pokemonId && string.Compare(x.Form, form, true) == 0);
                if (pkmnSub == null)
                    continue;

                var result = pkmnSub.Id.Remove<PokemonSubscription>();
                if (!result)
                {
                    error = true;
                    //TODO: Collect list of failed.
                }
            }

            if (error)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("FAILED_POKEMON_SUBSCRIPTIONS_UNSUBSCRIBE").FormatText(ctx.User.Username, string.Join(", ", pokemonNames)), DiscordColor.Red);
                return;
            }

            await ctx.RespondEmbed(_dep.Language.Translate("SUCCESS_POKEMON_SUBSCRIPTIONS_UNSUBSCRIBE").FormatText(ctx.User.Username, string.Join("**, **", pokemonNames)));
            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
        }

        #endregion

        #region Raidme / Raidmenot

        [
            Command("raidme"),
            Description("Subscribe to raid boss notifications based on the pokedex number or name.")
        ]
        public async Task RaidMeAsync(CommandContext ctx,
            [Description("Pokemon name or id to subscribe to raid notifications.")] string poke,
            [Description("City to send the notification if the raid appears in otherwise if null all will be sent."), RemainingText] string city = null)
        {
            if (!await CanExecute(ctx))
                return;

            //Remove any spaces from city names
            if (!string.IsNullOrEmpty(city) && city.Contains(" "))
            {
                city = city.Replace(" ", "");
            }

            if (string.Compare(city, Strings.All, true) != 0 && !string.IsNullOrEmpty(city))
            {
                if (_dep.WhConfig.Servers[ctx.Guild.Id].CityRoles.Find(x => string.Compare(x.ToLower(), city.ToLower(), true) == 0) == null)
                {
                    await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_CITY_ROLE").FormatText(ctx.User.Username, city), DiscordColor.Red);
                    return;
                }
            }

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.Guild.Id, ctx.User.Id);
            var validation = ValidatePokemonList(poke);
            if (validation.Valid == null || validation.Valid.Count == 0)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_POKEMON_IDS_OR_NAMES").FormatText(ctx.User.Username, string.Join(", ", validation.Invalid)), DiscordColor.Red);
                return;
            }

            var keys = validation.Valid.Keys.ToList();
            for (var i = 0; i < keys.Count; i++)
            {
                var pokemonId = keys[i];
                var form = validation.Valid[pokemonId];
                var cities = string.IsNullOrEmpty(city)
                    ? _dep.WhConfig.Servers[ctx.Guild.Id].CityRoles
                    : new List<string> { city };
                foreach (var area in cities)
                {
                    var subRaid = subscription.Raids.FirstOrDefault(x => x.PokemonId == pokemonId &&
                                                                         string.Compare(x.Form, form, true) == 0 &&
                                                                         string.Compare(x.City, area, true) == 0);
                    if (subRaid != null)
                        continue; //Already exists

                    subscription.Raids.Add(new RaidSubscription
                    {
                        GuildId = ctx.Guild.Id,
                        UserId = ctx.User.Id,
                        PokemonId = pokemonId,
                        Form = form,
                        City = area
                    });
                }
            }
            subscription.Save();

            var pokemonNames = validation.Valid.Select(x => MasterFile.Instance.Pokedex[x.Key].Name + (string.IsNullOrEmpty(x.Value) ? string.Empty : "-" + x.Value));
            await ctx.RespondEmbed(_dep.Language.Translate("SUCCESS_RAID_SUBSCRIPTIONS_SUBSCRIBE").FormatText(
                ctx.User.Username,
                string.Join("**, **", pokemonNames),
                string.IsNullOrEmpty(city) ?
                    _dep.Language.Translate("SUBSCRIPTIONS_FROM_ALL_CITIES") :
                    _dep.Language.Translate("SUBSCRIPTIONS_FROM_CITY").FormatText(city))
            );
            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
        }

        [
            Command("raidmenot"),
            Description("Unsubscribe from one or more or even all subscribed raid boss notifications by pokedex number or name.")
        ]
        public async Task RaidMeNotAsync(CommandContext ctx,
            [Description("Pokemon name or id to unsubscribe from raid notifications.")] string poke,
            [Description("City to remove the quest notifications from otherwise if null all will be sent."), RemainingText] string city = null)
        {
            if (!await CanExecute(ctx))
                return;

            //Remove any spaces from city names
            if (!string.IsNullOrEmpty(city) && city.Contains(" "))
            {
                city = city.Replace(" ", "");
            }

            if (string.Compare(city, Strings.All, true) != 0 && !string.IsNullOrEmpty(city))
            {
                if (_dep.WhConfig.Servers[ctx.Guild.Id].CityRoles.Find(x => string.Compare(x.ToLower(), city.ToLower(), true) == 0) == null)
                {
                    await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_CITY_ROLE").FormatText(ctx.User.Username, city), DiscordColor.Red);
                    return;
                }
            }

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.Guild.Id, ctx.User.Id);
            if (subscription == null || subscription?.Raids.Count == 0)
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed(_dep.Language.Translate("ERROR_NO_RAID_SUBSCRIPTIONS").FormatText(ctx.User.Username, string.IsNullOrEmpty(city) ?
                    _dep.Language.Translate("SUBSCRIPTIONS_FROM_ALL_CITIES") :
                    _dep.Language.Translate("SUBSCRIPTIONS_FROM_CITY").FormatText(city)),
                    DiscordColor.Red
                );
            }

            if (string.Compare(poke, Strings.All, true) == 0)
            {
                var result = await ctx.Confirm(_dep.Language.Translate("NOTIFY_CONFIRM_REMOVE_ALL_RAID_SUBSCRIPTIONS").FormatText(ctx.User.Username, subscription.Raids.Count.ToString("N0")));
                if (!result)
                    return;

                subscription.Raids.ForEach(x => x.Id.Remove<RaidSubscription>());

                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_SUCCESS_REMOVE_ALL_RAID_SUBSCRIPTIONS").FormatText(ctx.User.Username));
                _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
                return;
            }

            var validation = ValidatePokemonList(poke);
            if (validation.Valid == null || validation.Valid.Count == 0)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_POKEMON_IDS_OR_NAMES").FormatText(ctx.User.Username, string.Join(", ", validation.Invalid)), DiscordColor.Red);
                return;
            }

            //var notSubscribed = new List<string>();
            //var unsubscribed = new List<string>();
            foreach (var item in validation.Valid)
            {
                var pokemonId = item.Key;
                var form = item.Value;
                var cities = string.IsNullOrEmpty(city)
                    ? _dep.WhConfig.Servers[ctx.Guild.Id].CityRoles
                    : new List<string> { city };
                foreach (var area in cities)
                {
                    var subRaid = subscription.Raids.FirstOrDefault(x => x.PokemonId == pokemonId &&
                                                                         string.Compare(x.Form, form, true) == 0 &&
                                                                         string.Compare(x.City, area, true) == 0);
                    if (subRaid == null)
                        continue; //Already removed

                    if (!subRaid.Id.Remove<RaidSubscription>())
                    {
                        _logger.Error($"Unable to remove raid subscription for user id {subRaid.UserId} from guild id {subRaid.GuildId}");
                    }
                }
            }

            var pokemonNames = validation.Valid.Select(x => MasterFile.Instance.Pokedex[x.Key].Name + (string.IsNullOrEmpty(x.Value) ? string.Empty : "-" + x.Value));
            await ctx.RespondEmbed(_dep.Language.Translate("SUCCESS_RAID_SUBSCRIPTIONS_UNSUBSCRIBE").FormatText(
                ctx.User.Username,
                string.Join("**, **", pokemonNames),
                string.IsNullOrEmpty(city) ?
                    _dep.Language.Translate("SUBSCRIPTIONS_FROM_ALL_CITIES") :
                    _dep.Language.Translate("SUBSCRIPTIONS_FROM_CITY").FormatText(city))
            );
            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
        }

        #endregion

        #region Questme / Questmenot

        [
            Command("questme"),
            Description("Subscribe to quest notifications based on the reward keyword.")
        ]
        public async Task QuestMeAsync(CommandContext ctx,
            [Description("Reward keyword to use to find field research. Example: Spinda, 1200 stardust, candy")] string rewardKeyword,
            [Description("City to send the notification if the quest appears in otherwise if null all will be sent.")] string city = null)
        {
            if (!await CanExecute(ctx))
                return;

            if (string.Compare(city, Strings.All, true) != 0 && !string.IsNullOrEmpty(city))
            {
                if (_dep.WhConfig.Servers[ctx.Guild.Id].CityRoles.Find(x => string.Compare(x.ToLower(), city.ToLower(), true) == 0) == null)
                {
                    await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_CITY_ROLE").FormatText(ctx.User.Username, city), DiscordColor.Red);
                    return;
                }
            }

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.Guild.Id, ctx.User.Id);
            var cities = string.IsNullOrEmpty(city)
                ? _dep.WhConfig.Servers[ctx.Guild.Id].CityRoles
                : new List<string> { city };

            foreach (var area in cities)
            {
                var subQuest = subscription.Quests.FirstOrDefault(x => string.Compare(x.RewardKeyword, rewardKeyword, true) == 0 &&
                                                                       string.Compare(x.City, area, true) == 0);
                if (subQuest != null)
                    continue; //Already exists

                subscription.Quests.Add(new QuestSubscription
                {
                    GuildId = ctx.Guild.Id,
                    UserId = ctx.User.Id,
                    RewardKeyword = rewardKeyword,
                    City = area
                });
            }

            subscription.Save();
            //await ctx.RespondEmbed($"{ctx.User.Username} is already subscribed to **{rewardKeyword}** quest notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}.", DiscordColor.Red);
            await ctx.RespondEmbed(_dep.Language.Translate("SUCCESS_QUEST_SUBSCRIPTIONS_SUBSCRIBE").FormatText(
                ctx.User.Username,
                rewardKeyword,
                string.IsNullOrEmpty(city) ?
                    _dep.Language.Translate("SUBSCRIPTIONS_FROM_ALL_CITIES") :
                    _dep.Language.Translate("SUBSCRIPTIONS_FROM_CITY").FormatText(city))
            );
            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
        }

        [
            Command("questmenot"),
            Description("Unsubscribe from one or all subscribed field research quest notifications by reward keyword.")
        ]
        public async Task QuestMeNotAsync(CommandContext ctx,
            [Description("Reward keyword to remove from field research quest subscriptions. Example: Spinda, 1200 stardust, candy")] string rewardKeyword,
            [Description("City to remove the quest notifications from otherwise if null all will be sent.")] string city = null)
        {
            if (!await CanExecute(ctx))
                return;

            if (string.Compare(city, Strings.All, true) != 0 && !string.IsNullOrEmpty(city))
            {
                if (_dep.WhConfig.Servers[ctx.Guild.Id].CityRoles.Find(x => string.Compare(x.ToLower(), city.ToLower(), true) == 0) == null)
                {
                    await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_CITY_ROLE").FormatText(ctx.User.Username, city), DiscordColor.Red);
                    return;
                }
            }

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.Guild.Id, ctx.User.Id);
            if (subscription == null || subscription?.Quests.Count == 0)
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed(_dep.Language.Translate("ERROR_NO_QUEST_SUBSCRIPTIONS").FormatText(
                    ctx.User.Username,
                    rewardKeyword,
                    string.IsNullOrEmpty(city) ?
                        _dep.Language.Translate("SUBSCRIPTIONS_FROM_ALL_CITIES") :
                        _dep.Language.Translate("SUBSCRIPTIONS_FROM_CITY").FormatText(city)),
                    DiscordColor.Red
                );
                return;
            }

            var notSubscribed = new List<string>();
            var unsubscribed = new List<string>();
            await ctx.TriggerTypingAsync();

            if (string.Compare(rewardKeyword, Strings.All, true) == 0)
            {
                var removeAllResult = await ctx.Confirm(_dep.Language.Translate("NOTIFY_CONFIRM_REMOVE_ALL_QUEST_SUBSCRIPTIONS").FormatText(ctx.User.Username, subscription.Quests.Count.ToString("N0")));
                if (!removeAllResult)
                    return;

                subscription.Quests.ForEach(x => x.Id.Remove<QuestSubscription>());
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_CONFIRM_SUCCESS_ALL_QUEST_SUBSCRIPTIONS").FormatText(ctx.User.Username));
                _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
                return;
            }

            var cities = string.IsNullOrEmpty(city)
                        ? _dep.WhConfig.Servers[ctx.Guild.Id].CityRoles.Select(x => x.ToLower())
                        : new List<string> { city.ToLower() };

            subscription.Quests
                .Where(x =>
                       string.Compare(x.RewardKeyword, rewardKeyword, true) == 0 &&
                       cities.Contains(x.City.ToLower()))?
                .ToList()?
                .ForEach(x => x.Id.Remove<QuestSubscription>());
            subscription.Save();

            //await ctx.RespondEmbed($"{ctx.User.Username} is not subscribed to **{rewardKeyword}** quest notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}.", DiscordColor.Red);
            await ctx.RespondEmbed(_dep.Language.Translate("SUCCESS_QUEST_SUBSCRIPTIONS_UNSUBSCRIBE").FormatText(
                ctx.User.Username,
                rewardKeyword,
                string.IsNullOrEmpty(city) ?
                    _dep.Language.Translate("SUBSCRIPTIONS_FROM_ALL_CITIES") :
                    _dep.Language.Translate("SUBSCRIPTIONS_FROM_CITY").FormatText(city))
            );
            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
        }

        #endregion

        #region Gymme / Gymmenot

        [
            Command("gymme"),
            Description("Add raid notifications for specific gyms.")
        ]
        public async Task GymMeAsync(CommandContext ctx,
            [Description("Gym name to subscribed to."), RemainingText] string gymName)
        {
            if (!await CanExecute(ctx))
                return;

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.Guild.Id, ctx.User.Id);
            var subGym = subscription.Gyms.FirstOrDefault(x => string.Compare(x.Name, gymName, true) == 0);
            if (subGym != null)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_GYM_SUBSCRIPTION_EXISTS").FormatText(ctx.User.Username, gymName), DiscordColor.Red);
                return;
            }

            subscription.Gyms.Add(new GymSubscription
            {
                GuildId = ctx.Guild.Id,
                UserId = ctx.User.Id,
                Name = gymName
            });
            subscription.Save();

            await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_GYM_SUBSCRIPTION_ADDED").FormatText(ctx.User.Username, gymName));
            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
        }

        [
            Command("gymmenot"),
            Description("Remove raid notifications for specific gyms.")
        ]
        public async Task GymMeNotAsync(CommandContext ctx,
            [Description("Gym name to unsubscribed from."), RemainingText] string gymName)
        {
            if (!await CanExecute(ctx))
                return;

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.Guild.Id, ctx.User.Id);
            if (string.Compare(Strings.All, gymName, true) == 0)
            {
                var result = await ctx.Confirm(_dep.Language.Translate("NOTIFY_CONFIRM_REMOVE_ALL_GYM_SUBSCRIPTIONS").FormatText(ctx.User.Username, subscription.Gyms.Count.ToString("N0")));
                if (!result)
                    return;

                subscription.Gyms.ForEach(x => x.Id.Remove<GymSubscription>());
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_SUCCESS_REMOVE_ALL_GYM_SUBSCRIPTIONS").FormatText(ctx.User.Username));
                _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
                return;
            }

            subscription.Gyms
                .Where(x => string.Compare(x.Name, gymName, true) == 0)?
                .ToList()?
                .ForEach(x => x.Id.Remove<GymSubscription>());
            await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_GYM_SUBSCRIPTION_REMOVED").FormatText(ctx.User.Username, gymName));
            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
        }

        #endregion

        #region Invme / Invmenot

        [
            Command("invme"),
            Description("Subscribe to Team Rocket invasion notifications based on the grunt type and gender.")
        ]
        public async Task InvMeAsync(CommandContext ctx,
            [Description("Invasion type i.e. `fire-m` to add.")] string invasionType,
            [Description("City to send the notification if the raid appears in otherwise if null all will be sent.")] string city = null)
        {
            if (!await CanExecute(ctx))
                return;

            if (string.Compare(city, Strings.All, true) != 0 && !string.IsNullOrEmpty(city))
            {
                if (_dep.WhConfig.Servers[ctx.Guild.Id].CityRoles.Find(x => string.Compare(x.ToLower(), city.ToLower(), true) == 0) == null)
                {
                    await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_CITY_ROLE").FormatText(ctx.User.Username, city), DiscordColor.Red);
                    return;
                }
            }

            if (!invasionType.Contains("-"))
            {
                var invType = invasionType.ToLower();
                if (!(invType.Contains("gio") || invType.Contains("clif") || invType.Contains("arlo") || invType.Contains("sierra") || invType.Contains("decoy")))
                {
                    await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_INVASION_GENDER").FormatText(ctx.User.Username), DiscordColor.Red);
                    return;
                }
            }

            var parts = invasionType.Split('-');
            var type = parts[0];
            var gender = parts.Length > 1 ? parts[1] : "m";
            var pokemonGender = (gender.ToLower().Contains("male") || gender.ToLower()[0] == 'm') ? PokemonGender.Male : PokemonGender.Female;
            var leaderType = GetLeaderGruntType(type, pokemonGender);
            var pkmnType = GetPokemonTypeFromString(type);
            var gruntType = leaderType == InvasionGruntType.Unset ?
                TeamRocketInvasion.GruntTypeToTrInvasion(pkmnType, pokemonGender) :
                leaderType;
            var leaderString = PokestopData.GetGruntLeaderString(gruntType);

            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.Guild.Id, ctx.User.Id);
            var cities = string.IsNullOrEmpty(city)
                ? _dep.WhConfig.Servers[ctx.Guild.Id].CityRoles
                : new List<string> { city };

            foreach (var area in cities)
            {
                var subInvasion = subscription.Invasions.FirstOrDefault(x => x.GruntType == gruntType &&
                                                                             string.Compare(x.City, area, true) == 0);
                if (subInvasion != null)
                    continue; //Already exists

                subscription.Invasions.Add(new InvasionSubscription
                {
                    GuildId = ctx.Guild.Id,
                    UserId = ctx.User.Id,
                    GruntType = gruntType,
                    City = area
                });
            }

            subscription.Save();
            await ctx.RespondEmbed(_dep.Language.Translate("SUCCESS_INVASION_SUBSCRIPTIONS_SUBSCRIBE").FormatText(
                ctx.User.Username,
                pkmnType == PokemonType.None ? leaderString : Convert.ToString(pkmnType),
                leaderString != "Tier II" ? string.Empty : Convert.ToString(pokemonGender),
                string.IsNullOrEmpty(city) ?
                    _dep.Language.Translate("SUBSCRIPTIONS_FROM_ALL_CITIES") :
                    _dep.Language.Translate("SUBSCRIPTIONS_FROM_CITY").FormatText(city))
            );
            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
        }

        [
            Command("invmenot"),
            Description("Unsubscribe from one or all subscribed field research quest notifications by reward keyword.")
        ]
        public async Task InvMeNotAsync(CommandContext ctx,
            [Description("Invasion type i.e. `water-f` to remove.")] string invasionType,
            [Description("City to send the notification if the raid appears in otherwise if null all will be sent.")] string city = null)
        {
            if (!await CanExecute(ctx))
                return;

            if (string.Compare(city, Strings.All, true) != 0 && !string.IsNullOrEmpty(city))
            {
                if (_dep.WhConfig.Servers[ctx.Guild.Id].CityRoles.Find(x => string.Compare(x.ToLower(), city.ToLower(), true) == 0) == null)
                {
                    await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_CITY_ROLE").FormatText(ctx.User.Username, city), DiscordColor.Red);
                    return;
                }
            }

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.Guild.Id, ctx.User.Id);
            if (subscription == null || subscription?.Invasions.Count == 0)
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed(_dep.Language.Translate("ERROR_NO_INVASION_SUBSCRIPTIONS").FormatText(
                    ctx.User.Username,
                    string.IsNullOrEmpty(city) ?
                        _dep.Language.Translate("SUBSCRIPTIONS_FROM_ALL_CITIES") :
                        _dep.Language.Translate("SUBSCRIPTIONS_FROM_CITY").FormatText(city)),
                        DiscordColor.Red
                );
                return;
            }

            if (string.Compare(invasionType, Strings.All, true) == 0)
            {
                var removeAllResult = await ctx.Confirm(_dep.Language.Translate("NOTIFY_CONFIRM_REMOVE_ALL_INVASION_SUBSCRIPTIONS").FormatText(ctx.User.Username, subscription.Invasions.Count.ToString("N0")));
                if (!removeAllResult)
                    return;

                subscription.Invasions.ForEach(x => x.Id.Remove<InvasionSubscription>());
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_SUCCESS_REMOVE_ALL_INVASION_SUBSCRIPTIONS").FormatText(ctx.User.Username));
                _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
                return;
            }

            if (!invasionType.Contains("-"))
            {
                var invType = invasionType.ToLower();
                if (!(invType.Contains("gio") || invType.Contains("clif") || invType.Contains("arlo") || invType.Contains("sierra") || invType.Contains("decoy")))
                {
                    await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_INVASION_GENDER").FormatText(ctx.User.Username), DiscordColor.Red);
                    return;
                }
            }

            var parts = invasionType.Split('-');
            var type = parts[0];
            var gender = parts.Length > 1 ? parts[1] : "m";
            var pokemonGender = (gender.ToLower().Contains("male") || gender.ToLower()[0] == 'm') ? PokemonGender.Male : PokemonGender.Female;
            var leaderType = GetLeaderGruntType(type, pokemonGender);
            var pkmnType = GetPokemonTypeFromString(type);
            var gruntType = leaderType == InvasionGruntType.Unset ?
                TeamRocketInvasion.GruntTypeToTrInvasion(pkmnType, pokemonGender) :
                leaderType;
            var leaderString = PokestopData.GetGruntLeaderString(gruntType);

            var cities = string.IsNullOrEmpty(city)
                        ? _dep.WhConfig.Servers[ctx.Guild.Id].CityRoles
                        : new List<string> { city };

            subscription.Invasions
                .Where(x =>
                       x.GruntType == gruntType &&
                       cities.Contains(x.City.ToLower()))
                .ToList()
                .ForEach(x => x.Id.Remove<InvasionSubscription>());

            //await ctx.RespondEmbed($"{ctx.User.Username} is not subscribed to **{(pkmnType == PokemonType.None ? leaderString : Convert.ToString(pkmnType))} {(leaderString != "Tier II" ? string.Empty : Convert.ToString(pokemonGender))}** Team Rocket invasion notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}.", DiscordColor.Red);
            await ctx.RespondEmbed(_dep.Language.Translate("SUCCESS_INVASION_SUBSCRIPTIONS_UNSUBSCRIBE").FormatText(
                ctx.User.Username,
                pkmnType == PokemonType.None ? leaderString : Convert.ToString(pkmnType),
                leaderString != "Tier II" ? string.Empty : Convert.ToString(pokemonGender),
                string.IsNullOrEmpty(city) ?
                    _dep.Language.Translate("SUBSCRIPTIONS_FROM_ALL_CITIES") :
                    _dep.Language.Translate("SUBSCRIPTIONS_FROM_CITY").FormatText(city))
            );

            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
        }

        private InvasionGruntType GetLeaderGruntType(string leaderGruntType, PokemonGender gender = PokemonGender.Unset)
        {
            var type = leaderGruntType.ToLower();
            if (type.Contains("gio") || type.Contains("giovanni"))
                return InvasionGruntType.Giovanni;
            else if (type.Contains("arlo"))
                return InvasionGruntType.ExecutiveArlo;
            else if (type.Contains("clif"))
                return InvasionGruntType.ExecutiveCliff;
            else if (type.Contains("sierra"))
                return InvasionGruntType.ExecutiveSierra;
            else if (type.Contains("decoy"))
                return gender == PokemonGender.Male ?
                    InvasionGruntType.DecoyMale :
                    InvasionGruntType.DecoyFemale;

            return InvasionGruntType.Unset;
        }

        private PokemonType GetPokemonTypeFromString(string pokemonType)
        {
            var type = pokemonType.ToLower();
            if (type.Contains("bug"))
                return PokemonType.Bug;
            else if (type.Contains("dark"))
                return PokemonType.Dark;
            else if (type.Contains("dragon"))
                return PokemonType.Dragon;
            else if (type.Contains("electric"))
                return PokemonType.Electric;
            else if (type.Contains("fairy"))
                return PokemonType.Fairy;
            else if (type.Contains("fighting") || type.Contains("fight"))
                return PokemonType.Fighting;
            else if (type.Contains("fire"))
                return PokemonType.Fire;
            else if (type.Contains("flying") || type.Contains("fly"))
                return PokemonType.Flying;
            else if (type.Contains("ghost"))
                return PokemonType.Ghost;
            else if (type.Contains("grass"))
                return PokemonType.Grass;
            else if (type.Contains("ground"))
                return PokemonType.Ground;
            else if (type.Contains("ice"))
                return PokemonType.Ice;
            else if (type.Contains("tierii") || type.Contains("none") || type.Contains("tier2") || type.Contains("t2"))
                return PokemonType.None;
            else if (type.Contains("normal"))
                return PokemonType.Normal;
            else if (type.Contains("poison"))
                return PokemonType.Poison;
            else if (type.Contains("psychic"))
                return PokemonType.Psychic;
            else if (type.Contains("rock"))
                return PokemonType.Rock;
            else if (type.Contains("steel"))
                return PokemonType.Steel;
            else if (type.Contains("water"))
                return PokemonType.Water;
            else
                return PokemonType.None;
        }
        #endregion

        #region Pvpme / Pvpmenot

        [
            Command("pvpme"),
            Description("")
        ]
        public async Task PvpMeAsync(CommandContext ctx,
            [Description("Comma delimited list of Pokemon name(s) and/or Pokedex IDs to subscribe to Pokemon spawn notifications.")] string poke,
            [Description("PvP league")] string league,
            [Description("Minimum PvP ranking.")] int minimumRank = 5,
            [Description("Minimum PvP rank percentage.")] double minimumPercent = 0.0)
        {
            if (!await CanExecute(ctx))
                return;

            var pvpLeague = string.Compare(league, "great", true) == 0 ?
                PvPLeague.Great :
                string.Compare(league, "ultra", true) == 0 ?
                    PvPLeague.Ultra :
                    string.Compare(league, "master", true) == 0 ?
                        PvPLeague.Master :
                        PvPLeague.Other;

            if (pvpLeague == PvPLeague.Other)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_PVP_LEAGUE").FormatText(ctx.User.Username, league), DiscordColor.Red);
                return;
            }

            //You may only subscribe to the top 100 or higher rank.
            if (minimumRank < 0 || minimumRank > 100)
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_PVP_RANK_RANGE").FormatText(ctx.User.Username, minimumRank), DiscordColor.Red);
                return;
            }

            if (minimumPercent < 0 || minimumPercent > 100)
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_PVP_RANK_RANGE").FormatText(ctx.User.Username, minimumPercent), DiscordColor.Red);
                return;
            }

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.Guild.Id, ctx.User.Id);
            var alreadySubscribed = new List<string>();
            var subscribed = new List<string>();
            var validation = ValidatePokemonList(poke);
            if (validation == null || validation.Valid.Count == 0)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_POKEMON_IDS_OR_NAMES").FormatText(ctx.User.Username, string.Join(", ", validation.Invalid)), DiscordColor.Red);
                return;
            }

            var keys = validation.Valid.Keys.ToList();
            for (var i = 0; i < keys.Count; i++)
            {
                var pokemonId = keys[i];
                var form = validation.Valid[pokemonId];

                if (!MasterFile.Instance.Pokedex.ContainsKey(pokemonId))
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_POKEMON_ID").FormatText(ctx.User.Username, pokemonId), DiscordColor.Red);
                    continue;
                }

                var pokemon = MasterFile.Instance.Pokedex[pokemonId];
                var name = string.IsNullOrEmpty(form) ? pokemon.Name : pokemon.Name + "-" + form;
                var subPkmn = subscription.PvP.FirstOrDefault(x => x.PokemonId == pokemonId &&
                                                                   string.Compare(x.Form, form, true) == 0 &&
                                                                   x.League == pvpLeague);
                if (subPkmn == null)
                {
                    //Does not exist, create.
                    subscription.PvP.Add(new PvPSubscription
                    {
                        GuildId = ctx.Guild.Id,
                        UserId = ctx.User.Id,
                        PokemonId = pokemonId,
                        Form = form,
                        League = pvpLeague,
                        MinimumRank = minimumRank,
                        MinimumPercent = minimumPercent
                    });
                    subscribed.Add(name);
                    continue;
                }

                //Exists, check if anything changed.
                if (minimumRank != subPkmn.MinimumRank || minimumPercent != subPkmn.MinimumPercent)
                {
                    subPkmn.MinimumRank = minimumRank;
                    subPkmn.MinimumPercent = minimumPercent;
                    subscribed.Add(name);
                    continue;
                }

                //Already subscribed to the same Pokemon and form
                alreadySubscribed.Add(name);
            }

            var result = subscription.Save();

            await ctx.TriggerTypingAsync();
            if (subscribed.Count == 0 && alreadySubscribed.Count == 0)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_POKEMON_SPECIFIED").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            await ctx.RespondEmbed
            (
                (subscribed.Count > 0
                    ? $"{ctx.User.Username} has subscribed to **{string.Join("**, **", subscribed)}** notifications with a minimum {pvpLeague} League PvP ranking of {minimumRank} or higher and a minimum ranking percentage of {minimumPercent}%."
                    : string.Empty) +
                (alreadySubscribed.Count > 0
                    ? $"\r\n{ctx.User.Username} is already subscribed to **{string.Join("**, **", alreadySubscribed)}** notifications with a minimum {pvpLeague} League PvP ranking of '{minimumRank}' or higher and a minimum ranking percentage of {minimumPercent}%."
                    : string.Empty)
            );
            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
        }

        [
            Command("pvpmenot"),
            Description("")
        ]
        public async Task PvpMeNotAsync(CommandContext ctx,
            [Description("Comma delimited list of Pokemon name(s) and/or Pokedex IDs to subscribe to Pokemon spawn notifications.")] string poke,
            [Description("PvP league")] string league)
        {
            if (!await CanExecute(ctx))
                return;

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.Guild.Id, ctx.User.Id);
            if (subscription == null || subscription?.Pokemon?.Count == 0)
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_NO_POKEMON_SUBSCRIPTIONS").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            var pvpLeague = string.Compare(league, "great", true) == 0 ?
                PvPLeague.Great :
                string.Compare(league, "ultra", true) == 0 ?
                    PvPLeague.Ultra :
                    string.Compare(league, "master", true) == 0 ?
                        PvPLeague.Master :
                        PvPLeague.Other;

            if (pvpLeague == PvPLeague.Other)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_PVP_LEAGUE").FormatText(ctx.User.Username, league), DiscordColor.Red);
                return;
            }

            if (string.Compare(poke, Strings.All, true) == 0)
            {
                var confirm = await ctx.Confirm(_dep.Language.Translate("NOTIFY_CONFIRM_REMOVE_ALL_PVP_SUBSCRIPTIONS").FormatText(ctx.User.Username, subscription.PvP.Count(x => x.League == pvpLeague).ToString("N0"), pvpLeague));
                if (!confirm)
                    return;

                subscription.PvP
                    .Where(x => x.League == pvpLeague)?
                    .ToList()?
                    .ForEach(x => x.Id.Remove<PvPSubscription>());

                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_SUCCESS_REMOVE_ALL_PVP_SUBSCRIPTIONS").FormatText(ctx.User.Username, pvpLeague));
                _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
                return;
            }

            var validation = ValidatePokemonList(poke);
            if (validation.Valid == null || validation.Valid.Count == 0)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_POKEMON_IDS_OR_NAMES").FormatText(ctx.User.Username, string.Join(", ", validation.Invalid)), DiscordColor.Red);
                return;
            }

            var pokemonNames = validation.Valid.Select(x => MasterFile.Instance.Pokedex[x.Key].Name + (string.IsNullOrEmpty(x.Value) ? string.Empty : "-" + x.Value));
            subscription.PvP
                .Where(x =>
                    validation.Valid.ContainsKey(x.PokemonId) &&
                    string.Compare(x.Form, validation.Valid[x.PokemonId], true) == 0 &&
                    x.League == pvpLeague)?
                .ToList()?
                .ForEach(x => x.Id.Remove<PvPSubscription>());

            await ctx.RespondEmbed(_dep.Language.Translate("SUCCESS_PVP_SUBSCRIPTIONS_UNSUBSCRIBE").FormatText(ctx.User.Username, string.Join("**, **", pokemonNames), pvpLeague));
            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
        }

        #endregion

        #region Import / Export

        [
            Command("import"),
            Description("Import your saved notification subscription settings for Pokemon, Raids, Quests, Pokestops, and Gyms.")
        ]
        public async Task ImportAsync(CommandContext ctx)
        {
            if (!await CanExecute(ctx))
                return;

            await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_IMPORT_UPLOAD_FILE").FormatText(ctx.User.Username));
            var xc = await _dep.Interactivity.WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && x.Attachments.Count > 0, TimeSpan.FromSeconds(180));
            if (xc == null)
                return;

            var attachment = xc.Message.Attachments[0];
            if (attachment == null)
                return;

            var data = NetUtil.Get(attachment.Url);
            if (string.IsNullOrEmpty(data))
            {
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_IMPORT_INVALID_ATTACHMENT").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            var oldSubscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.Guild.Id, ctx.User.Id);
            if (oldSubscription != null)
            {
                var result = Data.Subscriptions.SubscriptionManager.RemoveAllUserSubscriptions(ctx.Guild.Id, ctx.User.Id);
                if (!result)
                {
                    _logger.Error($"Failed to clear old user subscriptions for {ctx.User.Username} ({ctx.User.Id}) in guild {ctx.Guild?.Name} ({ctx.Guild?.Id}) before importing.");
                }
            }

            var subscription = JsonConvert.DeserializeObject<SubscriptionObject>(data);
            if (subscription == null)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_IMPORT_MALFORMED_DATA").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }
            subscription.Save();
            await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_IMPORT_SUCCESS").FormatText(ctx.User.Username));
        }

        [
            Command("export"),
            Description("Export your current notification subscription settings for Pokemon, Raids, Quests, Pokestops, and Gyms.")
        ]
        public async Task ExportAsync(CommandContext ctx)
        {
            if (!await CanExecute(ctx))
                return;

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.Guild.Id, ctx.User.Id);
            if (subscription == null)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_EXPORT_NO_SUBSCRIPTIONS").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            var json = JsonConvert.SerializeObject(subscription, Formatting.Indented);
            var tmpFile = Path.Combine(Path.GetTempPath(), $"{ctx.Guild?.Name}_{ctx.User.Username}_subscriptions_{DateTime.Now.ToString("yyyy-MM-dd")}.json");
            File.WriteAllText(tmpFile, json);

            await ctx.RespondWithFileAsync(tmpFile, _dep.Language.Translate("NOTIFY_EXPORT_SUCCESS").FormatText(ctx.User.Username));
        }

        #endregion

        #region Icon Style

        [
            Command("icons"),
            Description("List all available icon styles.")
        ]
        public async Task IconsAsync(CommandContext ctx)
        {
            if (!await CanExecute(ctx))
                return;

            var description = "**Available Icon Styles:**\r\n" +
                    $"- {string.Join($"{Environment.NewLine}- ", _dep.WhConfig.IconStyles.Keys)}" +
                    Environment.NewLine +
                    Environment.NewLine +
                    $"*Type `{_dep.WhConfig.Servers[ctx.Guild.Id].CommandPrefix}set-icons iconStyle` to use that icon style when receiving notifications from {Strings.BotName}.*";
            var eb = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Green,
                Description = description,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"{ctx.Guild?.Name} | {DateTime.Now}",
                    IconUrl = ctx.Guild?.IconUrl
                }
            };

            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(embed: eb.Build());
        }

        [
            Command("set-icons"),
            Description("Set the icon style to use when receiving notifications via direct message.")
        ]
        public async Task SetIconAsync(CommandContext ctx,
            [Description("Icon style to use.")] string iconStyle = "Default")
        {
            if (!await CanExecute(ctx))
                return;

            if (!_dep.WhConfig.IconStyles.Select(x => x.Key.ToLower()).Contains(iconStyle.ToLower()))
            {
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_ICON_STYLE").FormatText(ctx.User.Username, _dep.WhConfig.Servers[ctx.Guild.Id].CommandPrefix), DiscordColor.Red);
                return;
            }

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.Guild.Id, ctx.User.Id);
            if (subscription == null)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("MSG_USER_NOT_SUBSCRIBED").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            subscription.IconStyle = iconStyle;
            subscription.Save();

            await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_ICON_STYLE_CHANGE").FormatText(ctx.User.Username, iconStyle));
            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
        }

        #endregion

        [
            Command("stats"),
            Description("Notification statistics for alarms and subscriptions of Pokemon, Raids, and Quests.")
        ]
        public async Task StatsAsync(CommandContext ctx)
        {
            var stats = Statistics.Instance;
            var eb = new DiscordEmbedBuilder
            {
                Title = $"{DateTime.Now.ToLongDateString()} Statistics",
                Color = DiscordColor.Blurple,
                ThumbnailUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQdNi3XTIwl8tkN_D6laRdexk0fXJ-fMr0C_s4ju-bXw2kcDSRI"
            };

            var sb = new StringBuilder();
            sb.AppendLine($"__**Pokemon**__");
            sb.AppendLine($"Alarms Sent: {stats.PokemonAlarmsSent.ToString("N0")}");
            sb.AppendLine($"Total Received: {stats.TotalReceivedPokemon.ToString("N0")}");
            sb.AppendLine($"With IV Stats: {stats.TotalReceivedPokemonWithStats.ToString("N0")}");
            sb.AppendLine($"Missing IV Stats: {stats.TotalReceivedPokemonMissingStats.ToString("N0")}");
            sb.AppendLine($"Subscriptions Sent: {stats.SubscriptionPokemonSent.ToString("N0")}");
            sb.AppendLine();
            sb.AppendLine("__**Raids**__");
            sb.AppendLine($"Egg Alarms Sent: {stats.EggAlarmsSent.ToString("N0")}");
            sb.AppendLine($"Raids Alarms Sent: {stats.RaidAlarmsSent.ToString("N0")}");
            sb.AppendLine($"Total Eggs Received: {stats.TotalReceivedRaids.ToString("N0")}");
            sb.AppendLine($"Total Raids Received: {stats.TotalReceivedRaids.ToString("N0")}");
            sb.AppendLine($"Raid Subscriptions Sent: {stats.SubscriptionRaidsSent.ToString("N0")}");
            sb.AppendLine();
            sb.AppendLine($"__**Quests**__");
            sb.AppendLine($"Alarms Sent: {stats.QuestAlarmsSent.ToString("N0")}");
            sb.AppendLine($"Total Received: {stats.TotalReceivedQuests.ToString("N0")}");
            sb.AppendLine($"Subscriptions Sent: {stats.SubscriptionQuestsSent.ToString("N0")}");
            sb.AppendLine();
            sb.AppendLine($"__**Invasions**__");
            sb.AppendLine($"Alarms Sent: {stats.InvasionAlarmsSent.ToString("N0")}");
            sb.AppendLine($"Total Received: {stats.TotalReceivedInvasions.ToString("N0")}");
            sb.AppendLine($"Subscriptions Sent: {stats.SubscriptionInvasionsSent.ToString("N0")}");
            sb.AppendLine();
            sb.AppendLine($"__**Lures**__");
            sb.AppendLine($"Alarms Sent: {stats.LureAlarmsSent.ToString("N0")}");
            sb.AppendLine($"Total Received: {stats.TotalReceivedLures.ToString("N0")}");
            sb.AppendLine();
            sb.AppendLine($"__**Gyms**__");
            sb.AppendLine($"Alarms Sent: {stats.GymAlarmsSent.ToString("N0")}");
            sb.AppendLine($"Total Received: {stats.TotalReceivedGyms.ToString("N0")}");
            sb.AppendLine();
            sb.AppendLine($"__**Weather**__");
            sb.AppendLine($"Alarms Sent: {stats.WeatherAlarmsSent.ToString("N0")}");
            sb.AppendLine($"Total Received: {stats.TotalReceivedWeathers.ToString("N0")}");
            sb.AppendLine();
            //var hundos = string.Join(Environment.NewLine, stats.Hundos.Select(x => $"{x.Key}: {MasterFile.Instance.Pokedex[x.Value.Id].Name} {x.Value.IV} IV {x.Value.CP} CP"));
            //sb.AppendLine($"**Recent 100% Spawns**");
            //sb.AppendLine(string.IsNullOrEmpty(hundos) ? "None" : hundos);

            eb.Description = sb.ToString();
            eb.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"{(ctx.Guild?.Name ?? Strings.Creator)} | {DateTime.Now}",
                IconUrl = ctx.Guild?.IconUrl
            };
            await ctx.RespondAsync(embed: eb);
        }

        #region Private Methods

        private async Task SendUserSubscriptionSettings(DiscordClient client, DiscordUser receiver, DiscordUser user, ulong guildId)
        {
            var messages = await BuildUserSubscriptionSettings(client, user, guildId);
            for (var i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                message = message.Length > 2000 ? message.Substring(0, Math.Min(message.Length, 1500)) : message;
                var eb = new DiscordEmbedBuilder
                {
                    Title = _dep.Language.Translate("NOTIFY_SETTINGS_EMBED_TITLE").FormatText(user.Username, i + 1, messages.Count),
                    Description = message,
                    Color = DiscordColor.CornflowerBlue,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"{Strings.Creator} | {DateTime.Now}"
                    }
                };
                await client.SendDirectMessage(receiver, eb.Build());
            }
        }

        private async Task<List<string>> BuildUserSubscriptionSettings(DiscordClient client, DiscordUser user, ulong guildId)
        {
            var member = await client.GetMemberById(_dep.WhConfig.Servers[guildId].GuildId, user.Id);
            if (member == null)
            {
                var error = $"Failed to get discord member from id {user.Id}.";
                _logger.Error(error);
                return new List<string> { error };
            }

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(guildId, user.Id);
            //var isSubbed = _dep.SubscriptionProcessor.Manager.UserExists(guildId, user.Id);
            var isSubbed = subscription?.Pokemon.Count > 0 || subscription?.Raids.Count > 0 || subscription?.Quests.Count > 0 || subscription?.Invasions.Count > 0 || subscription?.Gyms.Count > 0;
            var hasPokemon = isSubbed && subscription?.Pokemon.Count > 0;
            var hasPvP = isSubbed && subscription?.PvP.Count > 0;
            var hasRaids = isSubbed && subscription?.Raids.Count > 0;
            var hasQuests = isSubbed && subscription?.Quests.Count > 0;
            var hasInvasions = isSubbed && subscription?.Invasions.Count > 0;
            var messages = new List<string>();
            var isSupporter = client.IsSupporterOrHigher(user.Id, guildId, _dep.WhConfig);

            var feeds = member?.Roles?.Select(x => x.Name).Where(x => _dep.WhConfig.Servers[guildId].CityRoles.Contains(x))?.ToList();
            if (feeds == null)
                return messages;
            feeds.Sort();

            var sb = new StringBuilder();
            sb.AppendLine(_dep.Language.Translate("NOTIFY_SETTINGS_EMBED_ENABLED").FormatText(subscription.Enabled ? "Yes" : "No"));
            sb.AppendLine(_dep.Language.Translate("NOTIFY_SETTINGS_EMBED_CITIES").FormatText(string.Join(", ", feeds)));
            sb.AppendLine(_dep.Language.Translate("NOTIFY_SETTINGS_EMBED_ICON_STYLE").FormatText(subscription.IconStyle));
            sb.AppendLine(_dep.Language.Translate("NOTIFY_SETTINGS_EMBED_DISTANCE").FormatText(subscription.DistanceM == 0 ?
                _dep.Language.Translate("NOTIFY_SETTINGS_EMBED_DISTANCE_NOT_SET") :
                _dep.Language.Translate("NOTIFY_SETTINGS_EMBED_DISTANCE_KM").FormatText(subscription.DistanceM)));

            if (hasPokemon)
            {
                var pokemon = subscription.Pokemon;
                pokemon.Sort((x, y) => x.PokemonId.CompareTo(y.PokemonId));

                var exceedsLimits = pokemon.Count > Strings.MaxPokemonDisplayed;
                var defaultIV = 0;
                var defaultCount = 0;
                var results = pokemon.GroupBy(p => p.MinimumIV, (key, g) => new { IV = key, Pokes = g.ToList() });
                foreach (var result in results)
                {
                    if (result.Pokes.Count > defaultIV)
                    {
                        defaultIV = result.IV;
                        defaultCount = result.Pokes.Count;
                    }
                }

                sb.AppendLine(_dep.Language.Translate("NOTIFY_SETTINGS_EMBED_POKEMON").FormatText(pokemon.Count, isSupporter ? "∞" : Strings.MaxPokemonSubscriptions.ToString("N0")));
                sb.Append("```");

                if (exceedsLimits)
                {
                    sb.AppendLine(_dep.Language.Translate("NOTIFY_SETTINGS_EMBED_POKEMON_DEFAULT_UNLISTED").FormatText(defaultIV, defaultCount.ToString("N0")));
                }

                foreach (var sub in results)
                {
                    if (sub.IV == defaultIV && exceedsLimits)
                        continue;

                    foreach (var poke in sub.Pokes)
                    {
                        if (!MasterFile.Instance.Pokedex.ContainsKey(poke.PokemonId))
                            continue;

                        var pkmn = MasterFile.Instance.Pokedex[poke.PokemonId];
                        var form = string.IsNullOrEmpty(poke.Form) ? string.Empty : $" ({poke.Form})";
                        sb.AppendLine($"{poke.PokemonId}: {pkmn.Name}{form} {(poke.HasStats ? $"{poke.Attack}/{poke.Defense}/{poke.Stamina}" : poke.MinimumIV + "%+")}{(poke.MinimumLevel > 0 ? $", L{poke.MinimumLevel}+" : null)}{(poke.Gender == "*" ? null : $", Gender: {poke.Gender}")}");
                    }
                }
                sb.Append("```");
                sb.AppendLine();
                sb.AppendLine();
                messages.Add(sb.ToString());
            }

            var sb2 = new StringBuilder();
            if (hasPvP)
            {
                sb2.AppendLine(_dep.Language.Translate("NOTIFY_SETTINGS_EMBED_PVP").FormatText(subscription.PvP.Count.ToString("N0"), isSupporter ? "∞" : Strings.MaxPvPSubscriptions.ToString("N0")));
                sb2.Append("```");
                sb2.Append(string.Join(Environment.NewLine, GetPvPSubscriptionNames(guildId, user.Id)));
                sb2.Append("```");
                sb2.AppendLine();
                sb2.AppendLine();
            }

            if (hasRaids)
            {
                sb2.AppendLine(_dep.Language.Translate("NOTIFY_SETTINGS_EMBED_RAIDS").FormatText(subscription.Raids.Count.ToString("N0"), isSupporter ? "∞" : Strings.MaxRaidSubscriptions.ToString("N0")));
                sb2.Append("```");
                sb2.Append(string.Join(Environment.NewLine, GetRaidSubscriptionNames(guildId, user.Id)));
                sb2.Append("```");
                sb2.AppendLine();
                sb2.AppendLine();
            }

            if (hasQuests)
            {
                sb2.AppendLine(_dep.Language.Translate("NOTIFY_SETTINGS_EMBED_QUESTS").FormatText(subscription.Quests.Count.ToString("N0"), isSupporter ? "∞" : Strings.MaxQuestSubscriptions.ToString("N0")));
                //msg += $"Alert Time: {(subscription.AlertTime.HasValue ? subscription.AlertTime.Value.ToString("hh:mm:ss") : "Not set")}\r\n";
                sb2.Append("```");
                sb2.Append(string.Join(Environment.NewLine, GetQuestSubscriptionNames(guildId, user.Id)));
                sb2.Append("```");
                sb2.AppendLine();
                sb2.AppendLine();
            }

            if (hasInvasions)
            {
                sb2.AppendLine(_dep.Language.Translate("NOTIFY_SETTINGS_EMBED_INVASIONS").FormatText(subscription.Invasions.Count.ToString("N0"), isSupporter ? "∞" : Strings.MaxInvasionSubscriptions.ToString("N0")));
                sb2.Append("```");
                sb2.Append(string.Join(Environment.NewLine, GetInvasionSubscriptionNames(guildId, user.Id)));
                sb2.Append("```");
                sb2.AppendLine();
                sb2.AppendLine();
            }

            if (sb2.Length > 0)
            {
                messages.Add(sb2.ToString());
            }

            return messages;
        }

        //private List<string> GetPokemonSubscriptionNames(ulong userId)
        //{
        //    var list = new List<string>();
        //    if (!_dep.SubscriptionProcessor.Manager.UserExists(userId))
        //        return list;

        //    var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(userId);
        //    var subscribedPokemon = subscription.Pokemon;
        //    subscribedPokemon.Sort((x, y) => x.PokemonId.CompareTo(y.PokemonId));

        //    foreach (var poke in subscribedPokemon)
        //    {
        //        if (!MasterFile.Instance.Pokedex.ContainsKey(poke.PokemonId))
        //            continue;

        //        var pokemon = MasterFile.Instance.Pokedex[poke.PokemonId];
        //        if (pokemon == null)
        //            continue;

        //        list.Add(pokemon.Name);
        //    }

        //    return list;
        //}

        private List<string> GetPvPSubscriptionNames(ulong guildId, ulong userId)
        {
            var list = new List<string>();
            //if (!_dep.SubscriptionProcessor.Manager.UserExists(guildId, userId))
            //    return list;

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(guildId, userId);
            var subscribedPvP = subscription.PvP;
            subscribedPvP.Sort((x, y) => x.PokemonId.CompareTo(y.PokemonId));
            foreach (var pvp in subscribedPvP)
            {
                if (!MasterFile.Instance.Pokedex.ContainsKey(pvp.PokemonId))
                    continue;

                var pokemon = MasterFile.Instance.Pokedex[pvp.PokemonId];
                if (pokemon == null)
                    continue;

                list.Add($"{pvp.PokemonId}: {pokemon.Name} {(string.IsNullOrEmpty(pvp.Form) ? string.Empty : $"Form: {pvp.Form} ")}({pvp.League} League Rank: 1-{pvp.MinimumRank} Percent: {pvp.MinimumPercent}%+)");
            }

            return list;
        }

        private List<string> GetRaidSubscriptionNames(ulong guildId, ulong userId)
        {
            var list = new List<string>();
            //if (!_dep.SubscriptionProcessor.Manager.UserExists(guildId, userId))
            //    return list;

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(guildId, userId);
            var subscribedRaids = subscription.Raids;
            subscribedRaids.Sort((x, y) => x.PokemonId.CompareTo(y.PokemonId));
            var cityRoles = _dep.WhConfig.Servers[guildId].CityRoles.Select(x => x.ToLower());

            var results = subscribedRaids.GroupBy(x => x.PokemonId, (key, g) => new { PokemonId = key, Cities = g.ToList() });
            foreach (var raid in results)
            {
                if (!MasterFile.Instance.Pokedex.ContainsKey(raid.PokemonId))
                    continue;

                var pokemon = MasterFile.Instance.Pokedex[raid.PokemonId];
                if (pokemon == null)
                    continue;

                var isAllCities = cityRoles.ScrambledEquals(raid.Cities.Select(x => x.City).ToList(), StringComparer.Create(System.Globalization.CultureInfo.CurrentCulture, true));
                list.Add(_dep.Language.Translate("NOTIFY_FROM").FormatText(pokemon.Name, isAllCities ? _dep.Language.Translate("ALL_AREAS") : string.Join(", ", raid.Cities.Select(x => x.City))));
            }

            return list;
        }

        private List<string> GetQuestSubscriptionNames(ulong guildId, ulong userId)
        {
            var list = new List<string>();
            //if (!_dep.SubscriptionProcessor.Manager.UserExists(guildId, userId))
            //    return list;

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(guildId, userId);
            var subscribedQuests = subscription.Quests;
            subscribedQuests.Sort((x, y) => string.Compare(x.RewardKeyword.ToLower(), y.RewardKeyword.ToLower(), true));
            var cityRoles = _dep.WhConfig.Servers[guildId].CityRoles.Select(x => x.ToLower());

            var results = subscribedQuests.GroupBy(p => p.RewardKeyword, (key, g) => new { Reward = key, Cities = g.ToList() });
            foreach (var quest in results)
            {
                var isAllCities = cityRoles.ScrambledEquals(quest.Cities.Select(x => x.City.ToLower()).ToList(), StringComparer.Create(System.Globalization.CultureInfo.CurrentCulture, true));
                list.Add(_dep.Language.Translate("NOTIFY_FROM").FormatText(quest.Reward, isAllCities ? _dep.Language.Translate("ALL_AREAS") : string.Join(", ", quest.Cities.Select(x => x.City))));
            }

            return list;
        }

        private List<string> GetInvasionSubscriptionNames(ulong guildId, ulong userId)
        {
            var list = new List<string>();
            //if (!_dep.SubscriptionProcessor.Manager.UserExists(guildId, userId))
            //    return list;

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(guildId, userId);
            var subscribedInvasions = subscription.Invasions;
            subscribedInvasions.Sort((x, y) => string.Compare(x.GruntType.ToString().ToLower(), y.GruntType.ToString().ToLower(), true));
            var cityRoles = _dep.WhConfig.Servers[guildId].CityRoles.Select(x => x.ToLower());

            var results = subscribedInvasions.GroupBy(p => p.GruntType, (key, g) => new { Grunt = key, Cities = g.ToList() });
            foreach (var invasion in results)
            {
                var isAllCities = cityRoles.ScrambledEquals(invasion.Cities.Select(x => x.City.ToLower()).ToList(), StringComparer.Create(System.Globalization.CultureInfo.CurrentCulture, true));
                list.Add(_dep.Language.Translate("NOTIFY_FROM").FormatText(invasion.Grunt, isAllCities ? _dep.Language.Translate("ALL_AREAS") : string.Join(", ", invasion.Cities.Select(x => x.City))));
            }

            return list;
        }

        //TODO: Add common Pokemon list to external file
        private bool IsCommonPokemon(int pokeId)
        {
            var commonPokemon = new List<int>
            {
                1, //Bulbasaur
                4, //Charmander
                7, //Squirtle
                10, //Caterpie
                13, //Weedle
                16, //Pidgey
                17, //Pidgeotto
                19, //Rattata
                20, //Raticate
                21, //Spearow
                23, //Ekans
                25, //Pikachu
                27, //Sandshrew
                29, //Nidoran Female
                32, //Nidoran Male
                35, //Clefairy
                37, //Vulpix
                39, //Jigglypuff
                41, //Zubat
                43, //Oddish
                46, //Paras
                48, //Venonat
                50, //Diglett
                52, //Meowth
                54, //Psyduck
                56, //Mankey
                58, //Growlithe
                60, //Poliwag
                //63, //Abra
                //66, //Machop
                69, //Bellsprout
                72, //Tentacool
                74, //Geodude
                77, //Ponyta
                79, //Slowpoke
                81, //Magnemite
                84, //Doduo
                86, //Seel
                90, //Shellder
                92, //Gastly
                96, //Drowzee
                98, //Krabby
                100, //Voltorb
                102, //Exeggcute
                104, //Cubone
                109, //Koffing
                111, //Ryhorn
                116, //Horsea
                118, //Goldeen
                120, //Staryu
                127, //Pinsir
                128, //Tauros
                129, //Magikarp
                //133, //Eevee
                138, //Omanyte
                140, //Kabuto
                152, //Chikorita
                155, //Cyndaquil
                158, //Totodile
                161, //Sentret
                163, //Hoothoot
                165, //Ledyba
                167, //Spinarak
                170, //Chinchou
                177, //Natu
                179, //Mareep
                183, //Marill
                //185, //Sudowoodo
                187, //Hoppip
                //190, //Aipom
                191, //Sunkern
                193, //Yanma
                194, //Wooper
                198, //Murkrow
                200, //Misdreavus
                204, //Pineco
                206, //Dunsparce
                207, //Gligar
                209, //Snubbull
                213, //Shuckle
                215, //Sneasel
                216, //Teddiursa
                218, //Slugma
                //220, //Swinub
                223, //Remoraid
                228, //Houndour
                231, //Phanpy
                252, //Treecko
                //255, //Torchic
                //258, //Mudkip
                261, //Poochyena
                263, //Zigzagoon
                265, //Wurmple
                273, //Seedot
                276, //Taillow
                //293, //Whismur
                296, //Makuhita
                299, //Nosepass
                300, //Skitty
                302, //Sableye
                304, //Aron
                307, //Meditite
                309, //Electrike
                311, //Plusle
                312, //Minun
                314, //Illumise
                315, //Roselia
                316, //Gulpin
                318, //Carvanha
                320, //Wailmer
                322, //Numel
                325, //Spoink
                328, //Trapinch
                331, //Cacnea
                333, //Swablu
                336, //Seviper
                339, //Barboach
                341, //Corphish
                343, //Baltoy
                345, //Lileep
                347, //Anorith
                351, //Castform
                353, //Shuppet
                355, //Duskull
                //361, //Snorunt
                363, //Spheal
                //370, //Luvdisc
                387, //Turtwig
                390, //Chimchar
                396, //Starly
                399, //Bidoof
                401, //Kricketot
                418, //Buizel
                //425, //Drifloon
                427, //Buneary
                434, //Stunky
                459, //Snover
                504, //Patrat
                506, //Lillipup
                509, //Purrloin
                519, //Pidove

            };
            return commonPokemon.Contains(pokeId);
        }

        private bool IsRarePokemon(int pokeId)
        {
            var rarePokemon = new List<int>
            {
                201, //Unown
                480, //Uxie
                481, //Mesprit
                482 //Azelf
            };
            return rarePokemon.Contains(pokeId);
        }

        private List<string> GetListFromRange(int startRange, int endRange)
        {
            var list = new List<string>();
            for (; startRange <= endRange; startRange++)
            {
                list.Add(startRange.ToString());
            }
            return list;
        }

        private DiscordEmbedBuilder BuildExpirationMessage(ulong guildId, DiscordUser user)
        {
            var customerData = _dep.Stripe.GetCustomerData(guildId, user.Id);
            if (!customerData.ExpireDate.HasValue)
            {
                return null;
            }
            var expires = customerData.ExpireDate.Value;
            var remaining = expires.GetTimeRemaining();
            return new DiscordEmbedBuilder
            {
                Title = $"{user.Username}#{user.Discriminator} ({user.Id}) Subscription Expires",
                Description = $"Your subscription will expire in {remaining.ToReadableStringNoSeconds()} on {customerData.ExpireDate}\r\n\r\nTo cancel your subscription type `cancel` in the #become-a-donor channel."
            };
        }

        private static ulong ConvertMentionToUserId(string mention)
        {
            //<@201909896357216256>
            //mention = Utils.GetBetween(mention, "<", ">");
            mention = mention.Replace("<", null);
            mention = mention.Replace(">", null);
            mention = mention.Replace("@", null);
            mention = mention.Replace("!", null);

            return ulong.TryParse(mention, out ulong result) ? result : 0;
        }

        private PokemonValidation ValidatePokemonList(string pokemonList)
        {
            if (string.IsNullOrEmpty(pokemonList))
                return null;

            pokemonList = pokemonList.Replace(" ", "");

            PokemonValidation validation;
            if (pokemonList.Contains("-") && int.TryParse(pokemonList.Split('-')[0], out var startRange) && int.TryParse(pokemonList.Split('-')[1], out var endRange))
            {
                //If `poke` param is a range
                var range = GetListFromRange(startRange, endRange);
                validation = range.ValidatePokemon();
            }
            else if (Strings.PokemonGenerationRanges.Select(x => "gen" + x.Key).ToList().Contains(pokemonList))
            {
                //If `poke` is pokemon generation
                if (!int.TryParse(pokemonList.Replace("gen", ""), out var gen) || !Strings.PokemonGenerationRanges.ContainsKey(gen))
                {
                    var keys = Strings.PokemonGenerationRanges.Keys.ToList();
                    var minValue = keys[0];
                    var maxValue = keys[keys.Count - 1];
                    return null;
                }

                var genRange = Strings.PokemonGenerationRanges[gen];
                var range = GetListFromRange(genRange.Start, genRange.End);
                validation = range.ValidatePokemon();
            }
            else
            {
                //If `poke` param is a list
                validation = pokemonList.Replace(" ", "").Split(',').ValidatePokemon();
            }

            return validation;
        }

        private async Task<bool> CanExecute(CommandContext ctx)
        {
            if (!await ctx.Message.IsDirectMessageSupported())
                return false;

            if (!_dep.WhConfig.Servers.ContainsKey(ctx.Guild.Id))
                return false;

            if (!_dep.WhConfig.Servers[ctx.Guild.Id].EnableSubscriptions)
            {
                await ctx.RespondEmbed(string.Format(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED"), ctx.User.Username), DiscordColor.Red);
                return false;
            }

            var isSupporter = ctx.Client.IsSupporterOrHigher(ctx.User.Id, ctx.Guild.Id, _dep.WhConfig);
            if (!isSupporter)
            {
                await ctx.DonateUnlockFeaturesMessage();
                return false;
            }

            return true;
        }

        #endregion
    }
}