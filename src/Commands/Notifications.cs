namespace WhMgr.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;

    using WhMgr.Data;
    using WhMgr.Data.Subscriptions.Models;
    using WhMgr.Diagnostics;
    using WhMgr.Extensions;
    using WhMgr.Net.Models;

    public class Notifications
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger();

        private readonly Dependencies _dep;

        public Notifications(Dependencies dep)
        {
            _dep = dep;
        }

        [
            Command("info"),
            Description("Shows your current Pokemon and Raid boss notification subscriptions.")
        ]
        public async Task InfoAsync(CommandContext ctx,
            [Description("Discord user mention string.")] string mention = "")
        {
            if (!_dep.WhConfig.EnableSubscriptions)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            if (string.IsNullOrEmpty(mention))
            {
                await SendUserSubscriptionSettings(ctx.Client, ctx.User, ctx.User);
                return;
            }

            if (!ctx.User.Id.IsModeratorOrHigher(_dep.WhConfig))
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

            await SendUserSubscriptionSettings(ctx.Client, ctx.User, user);
        }

        [
            Command("enable"),
            Aliases("disable"),
            Description("Enables or disables all of your Pokemon and Raid notification subscriptions at once.")
        ]
        public async Task EnableDisableAsync(CommandContext ctx)
        {
            if (!_dep.WhConfig.EnableSubscriptions)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            if (!_dep.SubscriptionProcessor.Manager.UserExists(ctx.User.Id))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed(_dep.Language.Translate("MSG_USER_NOT_SUBSCRIBED").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();

            var cmd = ctx.Message.Content.TrimStart('.', ' ');
            var enabled = cmd.ToLower().Contains("enable");
            if (_dep.SubscriptionProcessor.Manager.Set(ctx.User.Id, enabled))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed($"{ctx.User.Username} has **{cmd}d** Pokemon, Raid, and Quest notifications.");
            }

            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
        }

        #region Pokeme / Pokemenot

        [
            Command("pokeme"),
            Description("Subscribe to Pokemon notifications based on the pokedex number or name, minimum IV stats, or minimum level.")
        ]
        public async Task PokeMeAsync(CommandContext ctx,
            [Description("Pokemon name or id to subscribe to Pokemon spawn notifications.")] string poke,
            [Description("Minimum IV to receive notifications for, use 0 to disregard IV.")] int iv = 0,
            [Description("Minimum level to receive notifications for, use 0 to disregard level.")] int lvl = 0,
            [Description("Specific gender the Pokemon must be, use * to disregard gender.")] string gender = "*")
        {
            if (!_dep.WhConfig.EnableSubscriptions)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            var isSupporter = ctx.Client.IsSupporterOrHigher(ctx.User.Id, _dep.WhConfig);
            if (!isSupporter)
            {
                await ctx.DonateUnlockFeaturesMessage();
                return;
            }

            if (lvl > 0 || gender != "*")
            {
                if (!ctx.Client.IsSupporterOrHigher(ctx.User.Id, _dep.WhConfig))
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondEmbed($"{ctx.User.Username} The minimum level and gender type parameters are only available to Supporter members, please consider donating to unlock this feature.", DiscordColor.Red);
                    return;
                }
            }

            //if (!int.TryParse(cpArg, out int cp))
            //{
            //    await message.RespondAsync($"'{cpArg}' is not a valid value for CP.");
            //    return;
            //}

            if (iv < 0 || iv > 100)
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed($"{ctx.User.Username} {iv} must be within the range of `0-100`.", DiscordColor.Red);
                return;
            }

            if (gender != "*" && gender != "m" && gender != "f")
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed($"{ctx.User.Username} {gender} is not a valid gender.", DiscordColor.Red);
                return;
            }

            if (lvl < 0 || lvl > 35)
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed($"{ctx.User.Username} {lvl} must be within the range of `0-35`.", DiscordColor.Red);
                return;
            }

            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.User.Id);

            try
            {
                if (string.Compare(poke, Strings.All, true) == 0)
                {
                    if (!isSupporter && subscription.Pokemon.Count >= Strings.MaxPokemonSubscriptions)
                    {
                        await ctx.TriggerTypingAsync();
                        await ctx.RespondEmbed($"{ctx.User.Username} non-supporter members have a limited Pokemon notification amount of {Strings.MaxPokemonSubscriptions}, thus you may not use the 'all' parameter. Please narrow down your Pokemon notification subscriptions to be more specific and try again.", DiscordColor.Red);
                        return;
                    }

                    if (iv < 90)
                    {
                        await ctx.TriggerTypingAsync();
                        await ctx.RespondEmbed($"{ctx.User.Username} may not subscribe to **all** Pokemon with a minimum IV less than 90, please set something higher.", DiscordColor.Red);
                        return;
                    }

                    await ctx.TriggerTypingAsync();
                    for (int i = 1; i < 495; i++)
                    {
                        /*
                        if (i == 132 && !isSupporter)
                        {
                            await ctx.TriggerTypingAsync();
                            await ctx.RespondEmbed($"{ctx.User.Username} Ditto has been skipped since he is only available to Supporters. Please consider donating to lift this restriction.", DiscordColor.Red);
                            continue;
                        }
                        */

                        if (!_dep.SubscriptionProcessor.Manager.UserExists(ctx.User.Id))
                        {
                            _dep.SubscriptionProcessor.Manager.AddPokemon(ctx.User.Id, i, (i == 201 ? 0 : iv), lvl, gender);
                            continue;
                        }

                        //User has already subscribed before, check if their new requested sub already exists.
                        if (!subscription.Pokemon.Exists(x => x.PokemonId == i))
                        {
                            //Always ignore the user's input for Unown and set it to 0 by default.
                            subscription.Pokemon.Add(new PokemonSubscription { PokemonId = i, MinimumIV = (i == 201 ? 0 : iv), MinimumLevel = lvl, Gender = gender });
                            continue;
                        }

                        //Check if minimum IV value is different from value in database, if not add it to the already subscribed list.
                        var subscribedPokemon = subscription.Pokemon.Find(x => x.PokemonId == i);
                        if (iv != subscribedPokemon.MinimumIV ||
                            lvl != subscribedPokemon.MinimumLevel ||
                            gender != subscribedPokemon.Gender)
                        {
                            subscribedPokemon.MinimumIV = (i == 201 ? 0 : iv);
                            subscribedPokemon.MinimumLevel = lvl;
                            subscribedPokemon.Gender = gender;
                        }
                    }

                    _dep.SubscriptionProcessor.Manager.Save(subscription);

                    await ctx.TriggerTypingAsync();
                    await ctx.RespondEmbed($"{ctx.User.Username} subscribed to **all** Pokemon notifications with a minimum IV of {iv}%{(lvl > 0 ? $" and a minimum level of {lvl}" : null)}{(gender == "*" ? null : $" and only '{gender}' gender types")}.");
                    _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.Debug($"POKEME FAILED-----------------------------------");
                _logger.Error(ex);
            }
            var alreadySubscribed = new List<string>();
            var subscribed = new List<string>();
            var isModOrHigher = ctx.User.Id.IsModeratorOrHigher(_dep.WhConfig);
            var validation = poke.Replace(" ", "").Split(',').ValidatePokemon();
            var db = Database.Instance;

            if (validation.Valid.Count == 0)
            {
                await ctx.RespondEmbed($"{ctx.User.Username}#{ctx.User.Discriminator} Invalid Pokemon: `{poke}`");
                return;
            }

            foreach (var arg in poke.Replace(" ", "").Split(','))
            {
                if (!int.TryParse(arg, out int pokeId))
                {
                    pokeId = arg.PokemonIdFromName();
                    if (pokeId == 0)
                    {
                        await ctx.TriggerTypingAsync();
                        await ctx.RespondEmbed($"{ctx.User.Username} failed to lookup Pokemon by name and pokedex id {arg}.", DiscordColor.Red);
                        continue;
                    }
                }

                //Check if common type pokemon e.g. Pidgey, Ratatta, Spinarak 'they are beneath him and he refuses to discuss them further'
                if (IsCommonPokemon(pokeId) && iv < Strings.CommonTypeMinimumIV && !isModOrHigher)
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondEmbed($"{ctx.User.Username} {db.Pokemon[pokeId].Name} is a common type Pokemon and cannot be subscribed to for notifications unless the IV is set to at least {Strings.CommonTypeMinimumIV}% or higher.", DiscordColor.Red);
                    continue;
                }

                if (!db.Pokemon.ContainsKey(pokeId))
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondEmbed($"{ctx.User.Username} {pokeId} is not a valid Pokemon id.", DiscordColor.Red);
                    continue;
                }

                var pokemon = db.Pokemon[pokeId];

                if (!_dep.SubscriptionProcessor.Manager.UserExists(ctx.User.Id))
                {
                    _dep.SubscriptionProcessor.Manager.AddPokemon(ctx.User.Id, pokeId, (pokeId == 201 ? 0 : iv), lvl, gender);
                    subscribed.Add(pokemon.Name);
                    continue;
                }

                //User has already subscribed before, check if their new requested sub already exists.
                if (!subscription.Pokemon.Exists(x => x.PokemonId == pokeId))
                {
                    if (!isSupporter && subscription.Pokemon.Count >= Strings.MaxPokemonSubscriptions)
                    {
                        await ctx.TriggerTypingAsync();
                        await ctx.RespondEmbed($"{ctx.User.Username} non-supporter members have a limited notification amount of {Strings.MaxPokemonSubscriptions} different Pokemon, please consider donating to lift this to every Pokemon. Otherwise you will need to remove some subscriptions in order to subscribe to new Pokemon.", DiscordColor.Red);
                        return;
                    }

                    _dep.SubscriptionProcessor.Manager.AddPokemon(ctx.User.Id, pokeId, (pokeId == 201 ? 0 : iv), lvl, gender);
                    subscribed.Add(pokemon.Name);
                    continue;
                }
                else
                {
                    //Check if minimum IV value is different from value in database, if not add it to the already subscribed list.
                    var subscribedPokemon = subscription.Pokemon.Find(x => x.PokemonId == pokeId);
                    if (iv != subscribedPokemon.MinimumIV ||
                        lvl != subscribedPokemon.MinimumLevel ||
                        gender != subscribedPokemon.Gender)
                    {
                        subscribedPokemon.MinimumIV = iv;
                        subscribedPokemon.MinimumLevel = lvl;
                        subscribedPokemon.Gender = gender;
                        subscribed.Add(pokemon.Name);

                        _dep.SubscriptionProcessor.Manager.Save(subscription);
                    }
                    else
                    {
                        alreadySubscribed.Add(pokemon.Name);
                    }
                }
            }

            await ctx.TriggerTypingAsync();
            if (subscribed.Count == 0 && alreadySubscribed.Count == 0)
            {
                await ctx.RespondEmbed($"{ctx.User.Username} I don't recognize any of the Pokemon you specified.");
                return;
            }

            await ctx.RespondEmbed
            (
                (subscribed.Count > 0
                    ? $"{ctx.User.Username} has subscribed to **{string.Join("**, **", subscribed)}** notifications with a minimum IV of {iv}%{(lvl > 0 ? $" and a minimum level of {lvl}" : null)}{(gender == "*" ? null : $" and only '{gender}' gender types")}."
                    : string.Empty) +
                (alreadySubscribed.Count > 0
                    ? $"\r\n{ctx.User.Username} is already subscribed to **{string.Join("**, **", alreadySubscribed)}** notifications with a minimum IV of {iv}%{(lvl > 0 ? $" and a minimum level of {lvl}" : null)}{(gender == "*" ? null : $" and only '{gender}' gender types")}."
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
            if (!_dep.WhConfig.EnableSubscriptions)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            if (!_dep.SubscriptionProcessor.Manager.UserExists(ctx.User.Id))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed($"{ctx.User.Username} is not subscribed to any Pokemon notifications.", DiscordColor.Red);
                return;
            }

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.User.Id);
            if (string.Compare(poke, Strings.All, true) == 0)
            {
                var confirm = await ctx.Confirm($"{ctx.User.Username} are you sure you want to remove **all** {subscription.Pokemon.Count.ToString("N0")} of your Pokemon subscriptions? Please reply back with `y` or `yes` to confirm.");
                if (!confirm)
                    return;

                await ctx.TriggerTypingAsync();
                if (!_dep.SubscriptionProcessor.Manager.RemoveAllPokemon(ctx.User.Id))
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondEmbed($"Could not remove all Pokemon subscriptions for {ctx.User.Username}.", DiscordColor.Red);
                    return;
                }

                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed($"{ctx.User.Username} has unsubscribed from **all** Pokemon notifications.");
                _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
                return;
            }

            var validation = poke.Replace(" ", "").Split(',').ValidatePokemon();
            if (validation.Valid != null && validation.Valid.Count > 0)
            {
                var pokemonNames = validation.Valid.Select(x => Database.Instance.Pokemon[x].Name);
                var result = _dep.SubscriptionProcessor.Manager.RemovePokemon(ctx.User.Id, validation.Valid);
                if (!result)
                {
                    await ctx.RespondEmbed($"{ctx.User.Username} Could not remove {string.Join(", ", pokemonNames)} Pokemon subscriptions.", DiscordColor.Red);
                    return;
                }

                var msg = $"{ctx.User.Username} has unsubscribed from **{string.Join("**, **", pokemonNames)}** Pokemon notifications.";
                if (validation.Invalid != null && validation.Invalid.Count > 0)
                {
                    msg += $"\r\n{string.Join(", ", validation.Invalid)} are not a valid Pokemon.";
                }

                await ctx.RespondEmbed(msg);
            }
            else
            {
                await ctx.RespondEmbed($"{ctx.User.Username} An error occurred while trying to remove your Pokemon subscriptions.", DiscordColor.Red);
            }

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
            [Description("City to send the notification if the raid appears in otherwise if null all will be sent.")] string city = null)
        {
            if (!_dep.WhConfig.EnableSubscriptions)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            var isSupporter = ctx.Client.IsSupporterOrHigher(ctx.User.Id, _dep.WhConfig);
            if (!isSupporter)
            {
                await ctx.DonateUnlockFeaturesMessage();
                return;
            }

            if (string.Compare(city, Strings.All, true) != 0 && !string.IsNullOrEmpty(city))
            {
                if (_dep.WhConfig.CityRoles.Find(x => string.Compare(x.ToLower(), city.ToLower(), true) == 0) == null)
                {
                    await ctx.RespondEmbed($"{ctx.User.Username} {city} is not a valid city role. To see a list of valid city roles type the command `.cities` or `.feeds`.", DiscordColor.Red);
                    return;
                }
            }

            if (string.Compare(poke, Strings.All, true) == 0)
            {
                if (!isSupporter)
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondEmbed($"{ctx.User.Username} Non-supporter members have a limited raid boss notification amount of {Strings.MaxRaidSubscriptions}, thus you may not use the 'all' parameter. Please narrow down your raid boss notification subscriptions to be more specific and try again.", DiscordColor.Red);
                    return;
                }

                await ctx.TriggerTypingAsync();
                for (var i = 1; i < 493; i++)
                {
                    var pokemon = Database.Instance.Pokemon[i];
                    var result = _dep.SubscriptionProcessor.Manager.AddRaid(ctx.User.Id, i, string.IsNullOrEmpty(city) ? _dep.WhConfig.CityRoles : new List<string> { city });
                }

                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed($"{ctx.User.Username} Subscribed to **all** raid boss notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}.");
                _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
                return;
            }

            //Check if user is not a Supporter and if they've exceeded their maximum raid subscriptions.
            //var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.User.Id);
            //if (!isSupporter && subscription.Raids.Count >= Strings.MaxRaidSubscriptions)
            //{
            //    await ctx.TriggerTypingAsync();
            //    await ctx.RespondEmbed($"{ctx.User.Username} Non-supporter members have a limited notification amount of {Strings.MaxRaidSubscriptions} different raid bosses, please consider donating to lift this to every raid Pokemon. Otherwise you will need to remove some subscriptions in order to subscribe to new raid Pokemon.", DiscordColor.Red);
            //    return;
            //}

            var validation = poke.Replace(" ", "").Split(',').ValidatePokemon();
            if (validation.Valid != null && validation.Valid.Count > 0)
            {
                var result = _dep.SubscriptionProcessor.Manager.AddRaid(
                    ctx.User.Id,
                    validation.Valid,
                    string.IsNullOrEmpty(city)
                        ? _dep.WhConfig.CityRoles
                        : new List<string> { city });

                var pokemonNames = validation.Valid.Select(x => Database.Instance.Pokemon[x].Name);
                var msg = $"{ctx.User.Username} has subscribed to **{string.Join("**, **", pokemonNames)}** raid notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}.";
                if (validation.Invalid != null && validation.Invalid.Count > 0)
                {
                    msg += $"\r\n{string.Join(", ", validation.Invalid)} are not valid raid boss Pokemon.";
                }

                await ctx.RespondEmbed(msg);
            }
            else
            {
                await ctx.RespondEmbed($"{ctx.User.Username} An error occurred while trying to add {poke} to your raid subscriptions.", DiscordColor.Red);
            }

            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
        }

        [
            Command("raidmenot"),
            Description("Unsubscribe from one or more or even all subscribed raid boss notifications by pokedex number or name.")
        ]
        public async Task RaidMeNotAsync(CommandContext ctx,
            [Description("Pokemon name or id to unsubscribe from raid notifications.")] string poke,
            [Description("City to remove the quest notifications from otherwise if null all will be sent.")] string city = null)
        {
            if (!_dep.WhConfig.EnableSubscriptions)
            {
                await ctx.RespondEmbed(string.Format(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED"), ctx.User.Username), DiscordColor.Red);
                return;
            }

            if (string.Compare(city, Strings.All, true) != 0 && !string.IsNullOrEmpty(city))
            {
                if (_dep.WhConfig.CityRoles.Find(x => string.Compare(x.ToLower(), city.ToLower(), true) == 0) == null)
                {
                    await ctx.RespondEmbed($"{ctx.User.Username} {city} is not a valid city role. To see a list of valid city roles type the command `.cities` or `.feeds`.", DiscordColor.Red);
                    return;
                }
            }

            if (!_dep.SubscriptionProcessor.Manager.UserExists(ctx.User.Id))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed($"{ctx.User.Username} is not subscribed to any raid notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}.", DiscordColor.Red);
                return;
            }

            var notSubscribed = new List<string>();
            var unsubscribed = new List<string>();

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.User.Id);

            if (string.Compare(poke, Strings.All, true) == 0)
            {
                var result = await ctx.Confirm($"{ctx.User.Username} are you sure you want to remove **all** {subscription.Pokemon.Count.ToString("N0")} of your raid boss subscriptions? Please reply back with `y` or `yes` to confirm.");
                if (!result)
                    return;

                await ctx.TriggerTypingAsync();
                if (!_dep.SubscriptionProcessor.Manager.RemoveAllRaids(ctx.User.Id))
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondEmbed($"{ctx.User.Username} Could not remove all raid boss subscriptions.", DiscordColor.Red);
                    return;
                }

                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed($"{ctx.User.Username} has unsubscribed from **all** raid boss notifications!");
                _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
                return;
            }

            var validation = poke.Replace(" ", "").Split(',').ValidatePokemon();
            if (validation.Valid != null && validation.Valid.Count > 0)
            {
                var result = _dep.SubscriptionProcessor.Manager.RemoveRaid(
                    ctx.User.Id, 
                    validation.Valid, 
                    string.IsNullOrEmpty(city) 
                        ? _dep.WhConfig.CityRoles 
                        : new List<string> { city });

                var pokemonNames = validation.Valid.Select(x => Database.Instance.Pokemon[x].Name);
                var msg = $"{ctx.User.Username} has unsubscribed from **{string.Join("**, **", pokemonNames)}** raid notifications{(string.IsNullOrEmpty(city) ? " from **all** cities" : $" from city **{city}**")}.";
                if (validation.Invalid != null && validation.Invalid.Count > 0)
                {
                    msg += $"\r\n{string.Join(", ", validation.Invalid)} are not valid raid boss Pokemon.";
                }

                await ctx.RespondEmbed(msg);
            }
            else
            {
                await ctx.RespondEmbed($"{ctx.User.Username} An error occurred while trying to remove your raid subscriptions.", DiscordColor.Red);
            }

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
            if (!_dep.WhConfig.EnableSubscriptions)
            {
                await ctx.RespondEmbed(string.Format(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED"), ctx.User.Username), DiscordColor.Red);
                return;
            }

            if (string.Compare(city, Strings.All, true) != 0 && !string.IsNullOrEmpty(city))
            {
                if (_dep.WhConfig.CityRoles.Find(x => string.Compare(x.ToLower(), city.ToLower(), true) == 0) == null)
                {
                    await ctx.RespondEmbed($"{ctx.User.Username} {city} is not a valid city role. To see a list of valid city roles type `.cities` or `.feeds`.", DiscordColor.Red);
                    return;
                }
            }

            var isSupporter = ctx.Client.IsSupporterOrHigher(ctx.User.Id, _dep.WhConfig);
            if (!isSupporter)
            {
                await ctx.DonateUnlockFeaturesMessage();
                return;
            }

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.User.Id);
            if (!isSupporter && subscription.Quests.Count >= Strings.MaxQuestSubscriptions)
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed($"{ctx.User.Username} Non-supporter members have a limited notification amount of {Strings.MaxQuestSubscriptions} different field research quests, please consider donating to lift this to every field research quest. Otherwise you will need to remove some subscriptions in order to subscribe to new field research quests.", DiscordColor.Red);
                return;
            }

            await ctx.TriggerTypingAsync();
            var result = _dep.SubscriptionProcessor.Manager.AddQuest(ctx.User.Id, rewardKeyword, string.IsNullOrEmpty(city) ? _dep.WhConfig.CityRoles : new List<string> { city });
            if (result)
            {
                await ctx.RespondEmbed($"{ctx.User.Username} has subscribed to **{rewardKeyword}** quest notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}.");
            }
            else
            {
                await ctx.RespondEmbed($"{ctx.User.Username} is already subscribed to **{rewardKeyword}** quest notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}.", DiscordColor.Red);
            }

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
            if (!_dep.WhConfig.EnableSubscriptions)
            {
                await ctx.RespondEmbed(string.Format(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED"), ctx.User.Username), DiscordColor.Red);
                return;
            }

            if (string.Compare(city, Strings.All, true) != 0 && !string.IsNullOrEmpty(city))
            {
                if (_dep.WhConfig.CityRoles.Find(x => string.Compare(x.ToLower(), city.ToLower(), true) == 0) == null)
                {
                    await ctx.RespondEmbed($"{ctx.User.Username} {city} is not a valid city role. To see a list of valid city roles type the command `.cities` or `.feeds`.", DiscordColor.Red);
                    return;
                }
            }

            if (!_dep.SubscriptionProcessor.Manager.UserExists(ctx.User.Id))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed($"{ctx.User.Username} is not subscribed to any quest notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}.", DiscordColor.Red);
                return;
            }

            var notSubscribed = new List<string>();
            var unsubscribed = new List<string>();

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.User.Id);
            await ctx.TriggerTypingAsync();

            if (string.Compare(rewardKeyword, Strings.All, true) == 0)
            {
                var removeAllResult = await ctx.Confirm($"{ctx.User.Mention} are you sure you want to remove **all** {subscription.Quests.Count.ToString("N0")} of your field research quest subscriptions? Please reply back with `y` or `yes` to confirm.");
                if (!removeAllResult)
                    return;

                if (!_dep.SubscriptionProcessor.Manager.RemoveAllQuests(ctx.User.Id))
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondEmbed($"{ctx.User.Username} Failed to remove all quest subscriptions.", DiscordColor.Red);
                    return;
                }

                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed($"{ctx.User.Username} has unsubscribed from **all** quest notifications.");
                _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
                return;
            }

            var result = _dep.SubscriptionProcessor.Manager.RemoveQuest(
                    ctx.User.Id,
                    rewardKeyword,
                    string.IsNullOrEmpty(city)
                        ? _dep.WhConfig.CityRoles
                        : new List<string> { city });

            if (result)
            {
                await ctx.RespondEmbed($"{ctx.User.Username} has unsubscribed from **{rewardKeyword}** quest notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}.");
            }
            else
            {
                await ctx.RespondEmbed($"{ctx.User.Username} is not subscribed to **{rewardKeyword}** quest notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}.", DiscordColor.Red);
            }

            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
        }

        #endregion

        [
            Command("quests"),
            Description("Display a list of your field research quests for the day.")
        ]
        public async Task QuestsAsync(CommandContext ctx,
            [Description("Filter by reward or leave empty for all.")] string reward,
            [Description("City")] string city)
        {
            if (!_dep.WhConfig.EnableSubscriptions)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            if (!_dep.SubscriptionProcessor.Manager.UserExists(ctx.User.Id))
            {
                await ctx.RespondEmbed(_dep.Language.Translate("MSG_USER_NOT_SUBSCRIBED").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.User.Id);
            if (subscription.Quests.Count == 0)
            {
                await ctx.RespondEmbed($"{ctx.User.Username} does not have any field research quest subscriptions.");
                return;
            }

            /**
             * .quests - Return all quests based on their subscriptions.
             * .quests reward - Return all quests based on keyword.
             */

            //TODO: Check date
            var rewardKeywords = subscription.Quests.Select(x => x.RewardKeyword).ToList();
            var quests = _dep.SubscriptionProcessor.Manager.GetQuests(
                string.IsNullOrEmpty(reward)
                ? rewardKeywords
                : new List<string> { reward });

            quests = quests.Where(x => x.QuestTimestamp.FromUnix().Date == DateTime.Now.Date).ToList();
            /*
            var eb = new DiscordEmbedBuilder
            {
                Title = string.IsNullOrEmpty(reward) ? "All Field Research Quests" : $"{reward} Field Research Quests",
                Color = DiscordColor.Orange,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"versx | {DateTime.Now}",
                    IconUrl = ctx.Guild?.IconUrl
                }
            };
            */

            var grouped = quests.GroupBy(x => GetGeofence(x.Latitude, x.Longitude)?.Name).ToList();
            var pagesData = (string.IsNullOrEmpty(reward) ? "All Field Research Quests" : $"{reward} Field Research Quests") + "\r\n";
            for (var i = 0; i < Math.Min(25, grouped.Count); i++)
            {
                if (string.Compare(grouped[i].Key, city, true) != 0)
                    continue;

                var rewardsWithCities = grouped[i].Select(x => $"• [{x.Name}]({string.Format(Strings.GoogleMaps, x.Latitude, x.Longitude)})");
                var value = string.Join(Environment.NewLine, rewardsWithCities);

                _logger.Debug($"REWARD: {grouped[i].Key} => {value}");
                pagesData += $"{grouped[i].Key}\r\n{value}";
                //eb.AddField(grouped[i].Key, value.Substring(0, Math.Min(1024, value.Length)), true);
            }
            var pages = _dep.Interactivity.GeneratePagesInEmbeds(pagesData);
            var dm = await ctx.Client.CreateDmAsync(ctx.User);
            await _dep.Interactivity.SendPaginatedMessage(dm, ctx.User, pages, System.Threading.Timeout.InfiniteTimeSpan);

            //await ctx.Client.SendDirectMessage(ctx.User, eb);
        }

        [
            Command("set-distance"),
            Description("Set the distance and location you'd like to receive raid notifications.")
        ]
        public async Task SetDistanceAsync(CommandContext ctx,
            [Description("Maximum distance in meters between the set coordinates.")] int distance,
            [Description("Coordinates in `34.00,-117.00` format."), RemainingText] string coordinates)
        {
            if (!_dep.WhConfig.EnableSubscriptions)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            if (!_dep.SubscriptionProcessor.Manager.UserExists(ctx.User.Id))
            {
                await ctx.RespondEmbed(_dep.Language.Translate("MSG_USER_NOT_SUBSCRIBED").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            var parts = coordinates.Replace(" ", null).Split(',');
            if (!double.TryParse(parts[0], out var lat) || !double.TryParse(parts[1], out var lng))
            {
                await ctx.RespondEmbed($"{ctx.User.Mention} Could not parse {coordinates} as valid coordinates.", DiscordColor.Red);
                return;
            }

            if (!_dep.SubscriptionProcessor.Manager.SetDistance(ctx.User.Id, distance, lat, lng))
            {
                await ctx.RespondEmbed($"{ctx.User.Mention} Could not update database, please try again later.", DiscordColor.Red);
                return;
            }

            await ctx.RespondEmbed($"{ctx.User.Mention} Raid notifications within a {distance} meter radius of location {lat},{lng}.");

            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
        }

        #region Gymme / Gymmenot

        [
            Command("gymme"),
            Description("Add raid notifications for specific gyms.")
        ]
        public async Task GymMeAsync(CommandContext ctx,
            [Description("Gym name to subscribed to."), RemainingText] string gymName)
        {
            if (!_dep.WhConfig.EnableSubscriptions)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            if (!_dep.SubscriptionProcessor.Manager.AddGym(ctx.User.Id, gymName))
            {
                await ctx.RespondEmbed($"{ctx.User.Mention} Could not add gym subscription '{gymName}' to your list of gyms to receive notifications from.", DiscordColor.Red);
                return;
            }

            await ctx.RespondEmbed($"{ctx.User.Mention} Added gym subscription '{gymName}' to your list of gyms to receive notifications from.");

            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
        }

        [
            Command("gymmenot"),
            Description("Remove raid notifications for specific gyms.")
        ]
        public async Task GymMeNotAsync(CommandContext ctx,
            [Description("Gym name to unsubscribed from."), RemainingText] string gymName)
        {
            if (!_dep.WhConfig.EnableSubscriptions)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            if (string.Compare(Strings.All, gymName, true) == 0)
            {
                if (!_dep.SubscriptionProcessor.Manager.RemoveAllGyms(ctx.User.Id))
                {
                    await ctx.RespondEmbed($"{ctx.User.Mention} Could not remove all gym subscriptions from your list of gyms to receive notifications from.", DiscordColor.Red);
                    return;
                }

                await ctx.RespondEmbed($"{ctx.User.Mention} Removed all gym subscriptions from your list of gyms to receive notifications from.");
                _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
                return;
            }

            if (!_dep.SubscriptionProcessor.Manager.RemoveGym(ctx.User.Id, gymName))
            {
                await ctx.RespondEmbed($"{ctx.User.Mention} Could not remove gym subscription '{gymName}' from your list of gyms to receive notifications from.", DiscordColor.Red);
                return;
            }

            await ctx.RespondEmbed($"{ctx.User.Mention} Removed gym subscription '{gymName}' from your list of gyms to receive notifications from.");

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
            if (!_dep.WhConfig.EnableSubscriptions)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            var isSupporter = ctx.Client.IsSupporterOrHigher(ctx.User.Id, _dep.WhConfig);
            if (!isSupporter)
            {
                await ctx.DonateUnlockFeaturesMessage();
                return;
            }

            if (string.Compare(city, Strings.All, true) != 0 && !string.IsNullOrEmpty(city))
            {
                if (_dep.WhConfig.CityRoles.Find(x => string.Compare(x.ToLower(), city.ToLower(), true) == 0) == null)
                {
                    await ctx.RespondEmbed($"{ctx.User.Username} {city} is not a valid city role. To see a list of valid city roles type the command `.cities` or `.feeds`.", DiscordColor.Red);
                    return;
                }
            }

            if (!invasionType.Contains("-"))
            {
                await ctx.RespondEmbed($"{ctx.User.Username} Please specify a gender. i.e. `.invme fire-m` or `.invme water-f ontario`");
                return;
            }

            var parts = invasionType.Split('-');
            var type = parts[0];
            var gender = parts[1];
            var pkmnType = GetPokemonTypeFromString(type);
            var pokemonGender = (gender.ToLower().Contains("male") || gender.ToLower()[0] == 'm') ? PokemonGender.Male : PokemonGender.Female;
            var gruntType = TeamRocketInvasion.GruntTypeToTrInvasion(pkmnType, pokemonGender);

            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.User.Id);
            //if (!isSupporter && subscription.Quests.Count >= Strings.MaxQuestSubscriptions)
            //{
            //    await ctx.TriggerTypingAsync();
            //    await ctx.RespondEmbed($"{ctx.User.Username} Non-supporter members have a limited notification amount of {Strings.MaxQuestSubscriptions} different field research quests, please consider donating to lift this to every field research quest. Otherwise you will need to remove some subscriptions in order to subscribe to new field research quests.", DiscordColor.Red);
            //    return;
            //}

            await ctx.TriggerTypingAsync();
            var result = _dep.SubscriptionProcessor.Manager.AddInvasion(ctx.User.Id, gruntType, string.IsNullOrEmpty(city) ? _dep.WhConfig.CityRoles : new List<string> { city });
            if (result)
            {
                await ctx.RespondEmbed($"{ctx.User.Username} has subscribed to **{(pkmnType == PokemonType.None ? "Tier II" : pkmnType.ToString())} {pokemonGender}** Team Rocket invasion notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}.");
            }
            else
            {
                await ctx.RespondEmbed($"{ctx.User.Username} is already subscribed to **{(pkmnType == PokemonType.None ? "Tier II" : pkmnType.ToString())} {pokemonGender}** Team Rocket invasion notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}.", DiscordColor.Red);
            }

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
            if (!_dep.WhConfig.EnableSubscriptions)
            {
                await ctx.RespondEmbed(string.Format(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED"), ctx.User.Username), DiscordColor.Red);
                return;
            }

            var isSupporter = ctx.Client.IsSupporterOrHigher(ctx.User.Id, _dep.WhConfig);
            if (!isSupporter)
            {
                await ctx.DonateUnlockFeaturesMessage();
                return;
            }

            if (string.Compare(city, Strings.All, true) != 0 && !string.IsNullOrEmpty(city))
            {
                if (_dep.WhConfig.CityRoles.Find(x => string.Compare(x.ToLower(), city.ToLower(), true) == 0) == null)
                {
                    await ctx.RespondEmbed($"{ctx.User.Username} {city} is not a valid city role. To see a list of valid city roles type the command `.cities` or `.feeds`.", DiscordColor.Red);
                    return;
                }
            }

            if (!_dep.SubscriptionProcessor.Manager.UserExists(ctx.User.Id))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed($"{ctx.User.Username} is not subscribed to any Team Rocket invasion notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}.", DiscordColor.Red);
                return;
            }

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.User.Id);

            if (string.Compare(invasionType, Strings.All, true) == 0)
            {
                var removeAllResult = await ctx.Confirm($"{ctx.User.Mention} are you sure you want to remove **all** {subscription.Invasions.Count.ToString("N0")} of your Team Rocket invasion subscriptions? Please reply back with `y` or `yes` to confirm.");
                if (!removeAllResult)
                    return;

                if (!_dep.SubscriptionProcessor.Manager.RemoveAllInvasions(ctx.User.Id))
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondEmbed($"{ctx.User.Username} Failed to remove all Team Rocket invasion subscriptions.", DiscordColor.Red);
                    return;
                }

                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed($"{ctx.User.Username} has unsubscribed from **all** Team Rocket invasion notifications.");
                _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
                return;
            }

            if (!invasionType.Contains("-"))
            {
                await ctx.RespondEmbed($"{ctx.User.Username} Please specify a gender. i.e. `.invmenot fire-m` or `.invmenot water-f ontario`");
                return;
            }

            var parts = invasionType.Split('-');
            var type = parts[0];
            var gender = parts[1];
            var pkmnType = GetPokemonTypeFromString(type);
            var pokemonGender = (gender.ToLower().Contains("male") || gender.ToLower()[0] == 'm') ? PokemonGender.Male : PokemonGender.Female;
            var gruntType = TeamRocketInvasion.GruntTypeToTrInvasion(pkmnType, pokemonGender);

            await ctx.TriggerTypingAsync();

            var result = _dep.SubscriptionProcessor.Manager.RemoveInvasion(
                    ctx.User.Id,
                    gruntType,
                    string.IsNullOrEmpty(city)
                        ? _dep.WhConfig.CityRoles
                        : new List<string> { city });

            if (result)
            {
                await ctx.RespondEmbed($"{ctx.User.Username} has unsubscribed from **{(pkmnType == PokemonType.None ? "Tier II" : pkmnType.ToString())} {gender}** Team Rocket invasion notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}.");
            }
            else
            {
                await ctx.RespondEmbed($"{ctx.User.Username} is not subscribed to **{(pkmnType == PokemonType.None ? "Tier II" : pkmnType.ToString())} {gender}** Team Rocket invasion notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}.", DiscordColor.Red);
            }

            _dep.SubscriptionProcessor.Manager.ReloadSubscriptions();
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

        //[
        //    Command("history"),
        //    Aliases("h")
        //]
        //public async Task HistoryAsync(CommandContext ctx)
        //{
        //    var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.User.Id);
        //    var eb = new DiscordEmbedBuilder
        //    {
        //        Title = $"{ctx.User.Username}#{ctx.User.Discriminator} Notification History for {DateTime.Now.ToLongDateString()}",
        //        Color = DiscordColor.Blurple,
        //        Footer = new DiscordEmbedBuilder.EmbedFooter
        //        {
        //            Text = $"versx | {DateTime.Now}",
        //            IconUrl = ctx.Guild?.IconUrl
        //        }
        //    };

        //    var pkmnStats = string.Join(Environment.NewLine, subscription.PokemonStatistics.Where(x => x.Date.Date == DateTime.Now.Date).Select(x => $"{x.Date.ToShortTimeString()}: {Database.Instance.Pokemon[(int)x.PokemonId].Name} {x.IV} IV {x.CP} CP"));
        //    var raidStats = string.Join(Environment.NewLine, subscription.RaidStatistics.Where(x => x.Date.Date == DateTime.Now.Date).Select(x => $"{x.Date.ToShortTimeString()}: {Database.Instance.Pokemon[(int)x.PokemonId].Name}"));
        //    var questStats = string.Join(Environment.NewLine, subscription.QuestStatistics.Where(x => x.Date.Date == DateTime.Now.Date).Select(x => $"{x.Date.ToShortTimeString()}: {x.Reward}"));

        //    var interactivity = _dep.Interactivity;
        //    if (interactivity == null)
        //    {
        //        _logger.Warn("Failed to get 'InteractivityModel'.");
        //        return;
        //    }

        //    var timeout = System.Threading.Timeout.InfiniteTimeSpan;
        //    var msg = $"**Pokemon Notifications**\r\n{(string.IsNullOrEmpty(pkmnStats) ? "None" : pkmnStats)}\r\n\r\n" +
        //              $"**Raid Notifications**\r\n{(string.IsNullOrEmpty(raidStats) ? "None" : raidStats)}\r\n\r\n" +
        //              $"**Quest Notifications**\r\n{(string.IsNullOrEmpty(questStats) ? "None" : questStats)}\r\n";
        //    await interactivity.SendPaginatedMessage(ctx.Channel, ctx.User, interactivity.GeneratePagesInEmbeds(msg), timeout, TimeoutBehaviour.Ignore);

        //    //eb.AddField("Pokemon Notifications", string.IsNullOrEmpty(pkmnStats) ? "None" : pkmnStats, true);
        //    //eb.AddField("Raid Notifications", string.IsNullOrEmpty(raidStats) ? "None" : raidStats, true);
        //    //eb.AddField("Quest Notifications", string.IsNullOrEmpty(questStats) ? "None" : questStats, true);

        //    //await ctx.RespondAsync(embed: eb);
        //}

        [
            Command("stats"),
            Description("Notification statistics for alarms and subscriptions of Pokemon, Raids, and Quests.")
        ]
        public async Task StatsAsync(CommandContext ctx)
        {
            var stats = Statistics.Instance;
            var eb = new DiscordEmbedBuilder
            {
                Title = $"{DateTime.Now.ToLongDateString()} Notification Statistics",
                Color = DiscordColor.Blurple,
                ThumbnailUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQdNi3XTIwl8tkN_D6laRdexk0fXJ-fMr0C_s4ju-bXw2kcDSRI"
            };
            eb.AddField("Pokemon", stats.PokemonSent.ToString("N0"), true);
            eb.AddField("Pokemon Subscriptions", stats.SubscriptionPokemonSent.ToString("N0"), true);
            eb.AddField("Raids", stats.RaidsSent.ToString("N0"), true);
            eb.AddField("Raid Subscriptions", stats.SubscriptionRaidsSent.ToString("N0"), true);
            eb.AddField("Quests", stats.QuestsSent.ToString("N0"), true);
            eb.AddField("Quest Subscriptions", stats.SubscriptionQuestsSent.ToString("N0"), true);

            var pkmnMsg = string.Join(Environment.NewLine, stats.Top25Pokemon.Select(x => $"{Database.Instance.Pokemon[x.Key].Name}: {x.Value.ToString("N0")}"));
            var raidMsg = string.Join(Environment.NewLine, stats.Top25Raids.Select(x => $"{Database.Instance.Pokemon[x.Key].Name}: {x.Value.ToString("N0")}"));

            eb.AddField("Top 25 Pokemon Stats", pkmnMsg.Substring(0, Math.Min(pkmnMsg.Length, 1500)) + "\r\n...", true);
            eb.AddField("Top 25 Raid Stats", raidMsg.Substring(0, Math.Min(raidMsg.Length, 1500)) + "\r\n...", true);

            var hundos = string.Join(Environment.NewLine, stats.Hundos.Select(x => $"{x.Key}: {Database.Instance.Pokemon[x.Value.Id].Name} {x.Value.IV} IV {x.Value.CP} CP"));
            eb.AddField("Recent 100% Spawns", string.IsNullOrEmpty(hundos) ? "None" : hundos);

            eb.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"versx | {DateTime.Now}",
                IconUrl = ctx.Guild?.IconUrl
            };
            await ctx.RespondAsync(embed: eb);
        }

        #region Private Methods

        private async Task SendUserSubscriptionSettings(DiscordClient client, DiscordUser receiver, DiscordUser user)
        {
            var messages = await BuildUserSubscriptionSettings(client, user);
            for (var i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                message = message.Length > 2000 ? message.Substring(0, Math.Min(message.Length, 1500)) : message;
                var eb = new DiscordEmbedBuilder
                {
                    Title = $"**{user.Username} Notification Settings (Page: {i + 1}/{messages.Count}):**\r\n",
                    Description = message,
                    Color = DiscordColor.CornflowerBlue,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"versx | {DateTime.Now}"
                    }
                };
                await client.SendDirectMessage(receiver, eb.Build());
            }
        }

        private async Task<List<string>> BuildUserSubscriptionSettings(DiscordClient client, DiscordUser user)
        {
            var author = user.Id;
            var isSubbed = _dep.SubscriptionProcessor.Manager.UserExists(author);
            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(user.Id);
            var hasPokemon = isSubbed && subscription?.Pokemon.Count > 0;
            var hasRaids = isSubbed && subscription?.Raids.Count > 0;
            var hasQuests = isSubbed && subscription?.Quests.Count > 0;
            var hasInvasions = isSubbed && subscription?.Invasions.Count > 0;
            var messages = new List<string>();
            var isSupporter = client.IsSupporterOrHigher(author, _dep.WhConfig);

            if (hasPokemon)
            {
                var member = await client.GetMemberById(_dep.WhConfig.GuildId, author);
                if (member == null)
                {
                    var error = $"Failed to get discord member from id {author}.";
                    _logger.Error(error);
                    return new List<string> { error };
                }

                var feeds = member.Roles.Select(x => x.Name).Where(x => _dep.WhConfig.CityRoles.Contains(x)).ToList();
                feeds.Sort();

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

                var msg = $"Enabled: **{(subscription.Enabled ? "Yes" : "No")}**\r\n";
                msg += $"Feed Zones: ```{string.Join(", ", feeds)}```\r\n";
                msg += $"Pokemon Subscriptions: ({pokemon.Count}/{(isSupporter ? "∞" : Strings.MaxPokemonSubscriptions.ToString())} used)\r\n";
                msg += "```";

                if (exceedsLimits)
                {
                    msg += $"Default: {defaultIV}% ({defaultCount.ToString("N0")} unlisted)\r\n";
                }

                foreach (var sub in results)
                {
                    if (sub.IV == defaultIV && exceedsLimits) 
                        continue;

                    foreach (var poke in sub.Pokes)
                    {
                        var pkmn = Database.Instance.Pokemon[poke.PokemonId];
                        msg += $"{poke.PokemonId}: {pkmn.Name} {poke.MinimumIV}%+{(poke.MinimumLevel > 0 ? $", L{poke.MinimumLevel}+" : null)}\r\n";
                    }
                }
                msg += "```" + Environment.NewLine + Environment.NewLine;
                messages.Add(msg);
            }

            var msg2 = string.Empty;
            if (hasRaids)
            {
                msg2 += $"Raid Subscriptions: ({subscription.Raids.Count.ToString("N0")}/{(isSupporter ? "∞" : Strings.MaxRaidSubscriptions.ToString())} used)\r\n";
                msg2 += "```";
                msg2 += string.Join(Environment.NewLine, GetRaidSubscriptionNames(author));
                msg2 += "```" + Environment.NewLine + Environment.NewLine;
            }

            if (hasQuests)
            {
                msg2 += $"Quest Subscriptions: ({subscription.Quests.Count.ToString("N0")}/{(isSupporter ? "∞" : Strings.MaxQuestSubscriptions.ToString())} used)\r\n";
                //msg += $"Alert Time: {(subscription.AlertTime.HasValue ? subscription.AlertTime.Value.ToString("hh:mm:ss") : "Not set")}\r\n";
                msg2 += "```";
                msg2 += string.Join(Environment.NewLine, GetQuestSubscriptionNames(author));
                msg2 += "```";
            }

            if (hasInvasions)
            {
                msg2 += $"Invasion Subscriptions: ({subscription.Invasions.Count.ToString("N0")}/{(isSupporter ? "" : Strings.MaxInvasionSubscriptions.ToString())} used)\r\n";
                msg2 += "```";
                msg2 += string.Join(Environment.NewLine, GetInvasionSubscriptionNames(author));
                msg2 += "```";
            }

            if (!string.IsNullOrEmpty(msg2))
            {
                messages.Add(msg2);
            }

            if (messages.Count == 0)
            {
                messages.Add($"**{user.Mention}** is not subscribed to any Pokemon or Raid notifications.");
            }

            return messages;
        }

        private List<string> GetPokemonSubscriptionNames(ulong userId)
        {
            var list = new List<string>();
            if (!_dep.SubscriptionProcessor.Manager.UserExists(userId))
                return list;

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(userId);
            var subscribedPokemon = subscription.Pokemon;
            subscribedPokemon.Sort((x, y) => x.PokemonId.CompareTo(y.PokemonId));

            foreach (var poke in subscribedPokemon)
            {
                if (!Database.Instance.Pokemon.ContainsKey(poke.PokemonId))
                    continue;

                var pokemon = Database.Instance.Pokemon[poke.PokemonId];
                if (pokemon == null)
                    continue;

                list.Add(pokemon.Name);
            }

            return list;
        }

        private List<string> GetRaidSubscriptionNames(ulong userId)
        {
            var list = new List<string>();
            if (!_dep.SubscriptionProcessor.Manager.UserExists(userId))
                return list;

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(userId);
            var subscribedRaids = subscription.Raids;
            subscribedRaids.Sort((x, y) => x.PokemonId.CompareTo(y.PokemonId));
            var cityRoles = _dep.WhConfig.CityRoles.Select(x => x.ToLower());

            var results = subscribedRaids.GroupBy(x => x.PokemonId, (key, g) => new { PokemonId = key, Cities = g.ToList() });
            foreach (var raid in results)
            {
                if (!Database.Instance.Pokemon.ContainsKey(raid.PokemonId))
                    continue;

                var pokemon = Database.Instance.Pokemon[raid.PokemonId];
                if (pokemon == null)
                    continue;

                var isAllCities = cityRoles.ScrambledEquals(raid.Cities.Select(x => x.City).ToList(), StringComparer.Create(System.Globalization.CultureInfo.CurrentCulture, true));
                list.Add($"{pokemon.Name} (From: {(isAllCities ? "All Areas" : string.Join(", ", raid.Cities.Select(x => x.City)))})");
            }

            return list;
        }

        private List<string> GetQuestSubscriptionNames(ulong userId)
        {
            var list = new List<string>();
            if (!_dep.SubscriptionProcessor.Manager.UserExists(userId))
                return list;

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(userId);
            var subscribedQuests = subscription.Quests;
            subscribedQuests.Sort((x, y) => string.Compare(x.RewardKeyword.ToLower(), y.RewardKeyword.ToLower(), true));
            var cityRoles = _dep.WhConfig.CityRoles.Select(x => x.ToLower());

            var results = subscribedQuests.GroupBy(p => p.RewardKeyword, (key, g) => new { Reward = key, Cities = g.ToList() });
            foreach (var quest in results)
            {
                var isAllCities = cityRoles.ScrambledEquals(quest.Cities.Select(x => x.City.ToLower()).ToList(), StringComparer.Create(System.Globalization.CultureInfo.CurrentCulture, true));
                list.Add($"{quest.Reward} (From: {(isAllCities ? "All Areas" : string.Join(", ", quest.Cities.Select(x => x.City)))})");
            }

            return list;
        }

        private List<string> GetInvasionSubscriptionNames(ulong userId)
        {
            var list = new List<string>();
            if (!_dep.SubscriptionProcessor.Manager.UserExists(userId))
                return list;

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(userId);
            var subscribedInvasions = subscription.Invasions;
            subscribedInvasions.Sort((x, y) => string.Compare(x.GruntType.ToString().ToLower(), y.GruntType.ToString().ToLower(), true));
            var cityRoles = _dep.WhConfig.CityRoles.Select(x => x.ToLower());

            var results = subscribedInvasions.GroupBy(p => p.GruntType, (key, g) => new { Grunt = key, Cities = g.ToList() });
            foreach (var invasion in results)
            {
                var isAllCities = cityRoles.ScrambledEquals(invasion.Cities.Select(x => x.City.ToLower()).ToList(), StringComparer.Create(System.Globalization.CultureInfo.CurrentCulture, true));
                list.Add($"{invasion.Grunt} (From: {(isAllCities ? "All Areas" : string.Join(", ", invasion.Cities.Select(x => x.City)))})");
            }

            return list;
        }

        private Geofence.GeofenceItem GetGeofence(double latitude, double longitude)
        {
            var loc = _dep.Whm.GeofenceService.GetGeofence(_dep.Whm.Geofences.Select(x => x.Value).ToList(), new Geofence.Location(latitude, longitude));
            return loc;
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
                63, //Abra
                66, //Machop
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
                133, //Eevee
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
                185, //Sudowoodo
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
                220, //Swinub
                223, //Remoraid
                228, //Houndour
                231, //Phanpy
                252, //Treecko
                255, //Torchic
                258, //Mudkip
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
                459 //Snover

            };
            return commonPokemon.Contains(pokeId);
        }

        #endregion

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
    }
}