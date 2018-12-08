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
    using WhMgr.Data.Models;
    using WhMgr.Diagnostics;
    using WhMgr.Extensions;

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
                //await ctx.RespondAsync($"{ctx.User.Mention} Subscriptions are not enabled in the config.");
                await ctx.RespondEmbed(string.Format(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED"), ctx.User.Username));// $"{ctx.User.Username} Subscriptions are not enabled in the config.");
                return;
            }

            if (string.IsNullOrEmpty(mention))
            {
                await SendUserSubscriptionSettings(ctx.Client, ctx.User, ctx.User);
                return;
            }

            if (!ctx.User.Id.IsModeratorOrHigher(_dep.WhConfig))
            {
                await ctx.RespondEmbed(string.Format(_dep.Language.Translate("MSG_NOT_MODERATOR_OR_HIGHER"), ctx.User.Mention)); //$"{ctx.User.Mention} is not a moderator or higher thus you may not see other's subscription settings.");
                return;
            }

            var userId = ConvertMentionToUserId(mention);
            if (userId <= 0)
            {
                await ctx.RespondAsync(string.Format(_dep.Language.Translate("MSG_INVALID_USER_MENTION"), ctx.User.Mention, mention)); //$"{ctx.User.Mention} Failed to retrieve user with mention tag {mention}.");
                return;
            }

            var user = await ctx.Client.GetUserAsync(userId);
            if (user == null)
            {
                _logger.Warn($"Failed to get Discord user with id {userId}.");
                return;
            }

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
                //await ctx.RespondAsync($"{ctx.User.Mention} Subscriptions are not enabled in the config.");
                await ctx.RespondEmbed(string.Format(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED"), ctx.User.Username));// $"{ctx.User.Username} Subscriptions are not enabled in the config.");
                return;
            }

            if (!_dep.SubscriptionProcessor.Manager.UserExists(ctx.User.Id))
            {
                //await ctx.TriggerTypingAsync();
                //await ctx.RespondAsync($"{ctx.User.Mention} is not currently subscribed to any Pokemon or Raid notifications.");
                await ctx.RespondEmbed(_dep.Language.Translate("MSG_USER_NOT_SUBSCRIBED").FormatText(ctx.User.Username));
                return;
            }

            var cmd = ctx.Message.Content.TrimStart('.', ' ');
            var enabled = cmd.ToLower().Contains("enable");
            if (_dep.SubscriptionProcessor.Manager.Set(ctx.User.Id, enabled))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed($"{ctx.User.Mention} has **{cmd}d** Pokemon and Raid notifications.");
            }
        }

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
                //await ctx.RespondAsync($"{ctx.User.Mention} Subscriptions are not enabled in the config.");
                await ctx.RespondEmbed(string.Format(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED"), ctx.User.Username));
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
                    await ctx.RespondAsync($"{ctx.User.Mention} The minimum level and gender type parameters are only available to Supporter members, please consider donating to unlock this feature.");
                    return;
                }
            }

            //if (!int.TryParse(cpArg, out int cp))
            //{
            //    await message.RespondAsync($"'{cpArg}' is not a valid value for CP.");
            //    return;
            //}

            if (iv == 0)
            {
                //await message.RespondAsync($"{message.Author.Mention} you entered 0% for a minimum IV, are you s you want to do this?");
                //return;
            }

            if (iv < 0 || iv > 100)
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync($"{ctx.User.Mention} {iv} must be within the range of 0-100.");
                return;
            }

            if (gender != "*" && gender != "m" && gender != "f")
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync($"{ctx.User.Mention} {gender} is not a valid gender.");
                return;
            }

            if (lvl < 0 || lvl > 35)
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync($"{ctx.User.Mention} {lvl} must be within the range of 0-35.");
                return;
            }

            if (string.Compare(poke, Strings.All, true) == 0)
            {
                if (!isSupporter)
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondAsync($"{ctx.User.Mention} non-supporter members have a limited Pokemon notification amount of {Strings.MaxPokemonSubscriptions}, thus you may not use the 'all' parameter. Please narrow down your Pokemon notification subscriptions to be more specific and try again.");
                    return;
                }

                if (iv < 80)
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondAsync($"{ctx.User.Mention} may not subscribe to **all** Pokemon with a minimum IV less than 80, please set something higher.");
                    return;
                }

                var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.User.Id);

                await ctx.TriggerTypingAsync();
                for (int i = 1; i < 493; i++)
                {
                    if (i == 132 && !isSupporter)
                    {
                        await ctx.TriggerTypingAsync();
                        await ctx.RespondAsync($"{ctx.User.Mention} Ditto has been skipped since he is only available to Supporters. Please consider donating to lift this restriction.");
                        continue;
                    }

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
                await ctx.RespondAsync($"{ctx.User.Mention} subscribed to **all** Pokemon notifications with a minimum IV of {iv}%.");
                return;
            }

            var alreadySubscribed = new List<string>();
            var subscribed = new List<string>();
            var isModOrHigher = ctx.User.Id.IsModeratorOrHigher(_dep.WhConfig);
            foreach (var arg in poke.Replace(" ", "").Split(','))
            {
                if (!int.TryParse(arg, out int pokeId))
                {
                    pokeId = arg.PokemonIdFromName();
                    if (pokeId == 0)
                    {
                        await ctx.TriggerTypingAsync();
                        await ctx.RespondAsync($"{ctx.User.Mention} failed to lookup Pokemon by name and pokedex id {arg}.");
                        continue;
                    }
                }

                if (pokeId == 132 && !isSupporter)
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondAsync($"{ctx.User.Mention} Ditto is only available to Supporters, please consider donating to unlock this feature.");
                    continue;
                }

                //Check if common type pokemon e.g. Pidgey, Ratatta, Spinarak 'they are beneath him and he refuses to discuss them further'
                if (IsCommonPokemon(pokeId) && iv < Strings.CommonTypeMinimumIV && !isModOrHigher)
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondAsync($"{ctx.User.Mention} {Database.Instance.Pokemon[pokeId].Name} is a common type Pokemon and cannot be subscribed to for notifications unless the IV is set to at least {Strings.CommonTypeMinimumIV}% or higher.");
                    continue;
                }

                if (!Database.Instance.Pokemon.ContainsKey(pokeId))
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondAsync($"{ctx.User.Mention} {pokeId} is not a valid Pokemon id.");
                    continue;
                }

                var pokemon = Database.Instance.Pokemon[pokeId];

                if (!_dep.SubscriptionProcessor.Manager.UserExists(ctx.User.Id))
                {
                    _dep.SubscriptionProcessor.Manager.AddPokemon(ctx.User.Id, pokeId, (pokeId == 201 ? 0 : iv), lvl, gender);
                    subscribed.Add(pokemon.Name);
                    continue;
                }

                var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.User.Id);

                //User has already subscribed before, check if their new requested sub already exists.
                if (!subscription.Pokemon.Exists(x => x.PokemonId == pokeId))
                {
                    if (!isSupporter && subscription.Pokemon.Count >= Strings.MaxPokemonSubscriptions)
                    {
                        await ctx.TriggerTypingAsync();
                        await ctx.RespondAsync($"{ctx.User.Mention} non-supporter members have a limited notification amount of {Strings.MaxPokemonSubscriptions} different Pokemon, please consider donating to lift this to every Pokemon. Otherwise you will need to remove some subscriptions in order to subscribe to new Pokemon.");
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
            await ctx.RespondAsync
            (
                (subscribed.Count > 0
                    ? $"{ctx.User.Mention} has subscribed to **{string.Join("**, **", subscribed)}** notifications with a minimum IV of {iv}%{(lvl > 0 ? $" and a minimum level of {lvl}" : null)}{(gender == "*" ? null : $" and only '{gender}' gender types")}."
                    : string.Empty) +
                (alreadySubscribed.Count > 0
                    ? $"\r\n{ctx.User.Mention} is already subscribed to **{string.Join("**, **", alreadySubscribed)}** notifications with a minimum IV of {iv}%{(lvl > 0 ? $" and a minimum level of {lvl}" : null)}{(gender == "*" ? null : $" and only '{gender}' gender types")}."
                    : string.Empty)
            );
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
                //await ctx.RespondAsync($"{ctx.User.Mention} Subscriptions are not enabled in the config.");
                await ctx.RespondEmbed(string.Format(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED"), ctx.User.Username));// $"{ctx.User.Username} Subscriptions are not enabled in the config.");
                return;
            }

            if (!_dep.SubscriptionProcessor.Manager.UserExists(ctx.User.Id))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync($"{ctx.User.Mention} is not subscribed to any Pokemon notifications.");
                return;
            }

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.User.Id);

            if (string.Compare(poke, Strings.All, true) == 0)
            {
                var confirm = await ctx.Confirm($"{ctx.User.Mention} are you sure you want to remove **all** {subscription.Pokemon.Count.ToString("N0")} of your Pokemon subscriptions? Please reply back with `y` or `yes` to confirm.");
                if (!confirm)
                    return;

                await ctx.TriggerTypingAsync();
                if (!_dep.SubscriptionProcessor.Manager.RemoveAllPokemon(ctx.User.Id))
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondAsync($"Failed to remove all Pokemon subscriptions for {ctx.User.Mention}.");
                    return;
                }

                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync($"{ctx.User.Mention} has unsubscribed from **all** Pokemon notifications.");
                return;
            }

            var db = Database.Instance;

            var notSubscribed = new List<string>();
            var unsubscribed = new List<string>();
            foreach (var arg in poke.Replace(" ", "").Split(','))
            {
                var pokeId = arg.PokemonIdFromName();
                if (pokeId == 0)
                {
                    if (!int.TryParse(arg, out pokeId))
                    {
                        await ctx.TriggerTypingAsync();
                        await ctx.RespondAsync($"{ctx.User.Mention}, failed to lookup Pokemon by name and pokedex id using {arg}.");
                        return;
                    }
                }

                if (!db.Pokemon.ContainsKey(pokeId))
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondAsync($"{ctx.User.Mention}, pokedex number {pokeId} is not a valid Pokemon id.");
                    continue;
                }

                var pokemon = db.Pokemon[pokeId];
                var unsubscribePokemon = subscription.Pokemon.Find(x => x.PokemonId == pokeId);
                if (unsubscribePokemon != null)
                {
                    if (_dep.SubscriptionProcessor.Manager.RemovePokemon(ctx.User.Id, pokeId))
                    {
                        unsubscribed.Add(pokemon.Name);
                    }
                }
                else
                {
                    notSubscribed.Add(pokemon.Name);
                }
            }

            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync
            (
                (unsubscribed.Count > 0
                    ? $"{ctx.User.Mention} has unsubscribed from **{string.Join("**, **", unsubscribed)}** notifications."
                    : string.Empty) +
                (notSubscribed.Count > 0
                    ? $" {ctx.User.Mention} is not subscribed to **{string.Join("**, **", notSubscribed)}** notifications."
                    : string.Empty)
            );
        }

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
                //await ctx.RespondAsync($"{ctx.User.Mention} Subscriptions are not enabled in the config.");
                await ctx.RespondEmbed(string.Format(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED"), ctx.User.Username));// $"{ctx.User.Username} Subscriptions are not enabled in the config.");
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
                    await ctx.RespondAsync($"{ctx.User.Mention} Failed to find city role {city}. To see a list of valid city roles type the command `.cities` or `.feeds`.");
                    return;
                }
            }
            else
            {
                //Assign to all cities.
                city = string.Empty;
            }

            if (string.Compare(poke, Strings.All, true) == 0)
            {
                //var isSupporter = await ctx.Client.IsSupporterOrHigher(ctx.User.Id, _dep.Config);
                if (!isSupporter)
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondAsync($"{ctx.User.Mention} Non-supporter members have a limited raid boss notification amount of {Strings.MaxRaidSubscriptions}, thus you may not use the 'all' parameter. Please narrow down your raid boss notification subscriptions to be more specific and try again.");
                    return;
                }

                await ctx.TriggerTypingAsync();
                for (var i = 1; i < 493; i++)
                {
                    //if (!i.IsValidRaidBoss(_dep.Config.RaidBosses)) continue;
                    //if (!_dep.Db.IsValidRaidBoss(i))
                    //    continue;

                    var pokemon = Database.Instance.Pokemon[i];
                    if (string.IsNullOrEmpty(city))
                    {
                        for (var cty = 0; cty < _dep.WhConfig.CityRoles.Count; cty++)
                        {
                            if (!_dep.SubscriptionProcessor.Manager.AddRaid(ctx.User.Id, i, _dep.WhConfig.CityRoles[cty]))
                            {
                                _logger.Error($"Failed to add raid boss {i} in city {_dep.WhConfig.CityRoles[cty]} added to {ctx.User.Id} subscription list.");
                                continue;
                            }

                            _logger.Info($"Raid boss {i} in city {_dep.WhConfig.CityRoles[cty]} added to {ctx.User.Id} subscription list.");
                            //AddRaidBoss(ctx.User.Id, i, _dep.WhConfig.CityRoles[cty]);
                        }
                    }
                    else
                    {
                        //AddRaidBoss(ctx.User.Id, i, city);
                        if (!_dep.SubscriptionProcessor.Manager.AddRaid(ctx.User.Id, i, city))
                        {
                            _logger.Error($"Failed to add raid boss {i} in city {city} added to {ctx.User.Id} subscription list.");
                            continue;
                        }

                        _logger.Info($"Raid boss {i} in city {city} added to {ctx.User.Id} subscription list.");
                    }
                }

                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync($"{ctx.User.Mention} Subscribed to **all** raid boss notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}.");
                return;
            }

            var alreadySubscribed = new List<string>();
            var subscribed = new List<string>();

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.User.Id);

            foreach (var arg in poke.Replace(" ", "").Split(','))
            {
                if (!isSupporter && subscription.Raids.Count >= Strings.MaxRaidSubscriptions)
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondAsync($"{ctx.User.Mention} Non-supporter members have a limited notification amount of {Strings.MaxRaidSubscriptions} different raid bosses, please consider donating to lift this to every raid Pokemon. Otherwise you will need to remove some subscriptions in order to subscribe to new raid Pokemon.");
                    return;
                }

                var pokeId = arg.PokemonIdFromName();
                if (pokeId == 0)
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondAsync($"{ctx.User.Mention} Failed to find raid Pokemon {arg}.");
                    continue;
                }

                var pokemon = Database.Instance.Pokemon[pokeId];
                var result = false;
                if (string.IsNullOrEmpty(city))
                {
                    for (var cty = 0; cty < _dep.WhConfig.CityRoles.Count; cty++)
                    {
                        result |= _dep.SubscriptionProcessor.Manager.AddRaid(ctx.User.Id, pokeId, _dep.WhConfig.CityRoles[cty]);
                    }
                }
                else
                {
                    result |= _dep.SubscriptionProcessor.Manager.AddRaid(ctx.User.Id, pokeId, city);
                }

                if (result)
                {
                    subscribed.Add(pokemon.Name);
                }
                else
                {
                    alreadySubscribed.Add(pokemon.Name);
                }
            }

            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync
            (
                (subscribed.Count > 0
                    ? $"{ctx.User.Mention} has subscribed to **{string.Join("**, **", subscribed)}** raid notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}."
                    : string.Empty) +
                (alreadySubscribed.Count > 0
                    ? $" {ctx.User.Mention} is already subscribed to {string.Join(",", alreadySubscribed)} raid notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}."
                    : string.Empty)
            );
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
                //await ctx.RespondAsync($"{ctx.User.Mention} Subscriptions are not enabled in the config.");
                await ctx.RespondEmbed(string.Format(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED"), ctx.User.Username));// $"{ctx.User.Username} Subscriptions are not enabled in the config.");
                return;
            }

            if (string.Compare(city, Strings.All, true) != 0 && !string.IsNullOrEmpty(city))
            {
                if (_dep.WhConfig.CityRoles.Find(x => string.Compare(x.ToLower(), city.ToLower(), true) == 0) == null)
                {
                    await ctx.RespondAsync($"{ctx.User.Mention} Could not find city role {city}. To see a list of valid city roles type the command `.cities` or `.feeds`.");
                    return;
                }
            }

            if (!_dep.SubscriptionProcessor.Manager.UserExists(ctx.User.Id))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync($"{ctx.User.Mention} is not subscribed to any raid notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}.");
                return;
            }

            var notSubscribed = new List<string>();
            var unsubscribed = new List<string>();

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.User.Id);

            if (string.Compare(poke, Strings.All, true) == 0)
            {
                var result = await ctx.Confirm($"{ctx.User.Mention} are you sure you want to remove **all** {subscription.Pokemon.Count.ToString("N0")} of your raid boss subscriptions? Please reply back with `y` or `yes` to confirm.");
                if (!result)
                    return;

                await ctx.TriggerTypingAsync();
                if (!_dep.SubscriptionProcessor.Manager.RemoveAllRaids(ctx.User.Id))
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondAsync($"{ctx.User.Mention} Could not remove all raid boss subscriptions.");
                    return;
                }

                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync($"{ctx.User.Mention} has unsubscribed from **all** raid boss notifications!");
                return;
            }

            var validation = ValidatePokemon(poke.Replace(" ", "").Split(','));
            if (validation.Valid != null && validation.Valid.Count > 0)
            {
                var result = _dep.SubscriptionProcessor.Manager.RemoveRaid(
                    ctx.User.Id, 
                    validation.Valid, 
                    string.IsNullOrEmpty(city) 
                        ? _dep.WhConfig.CityRoles 
                        : new List<string> { city });
            }

            var pokemonNames = validation.Valid.Select(x => Database.Instance.Pokemon[x]);
            var msg = $"{ctx.User.Mention} has unsubscribed from **{string.Join("**, **", pokemonNames)}** raid notifications{(string.IsNullOrEmpty(city) ? " from **all** cities" : $" from city **{city}**")}.";
            if (validation.Invalid != null && validation.Invalid.Count > 0)
            {
                msg += $"\r\n{string.Join(", ", validation.Invalid)} are not valid raid boss Pokemon.";
            }


            await ctx.RespondAsync($"{ctx.User.Mention}");

            //foreach (var arg in poke.Replace(" ", "").Split(','))
            //{
            //    var pokeId = arg.PokemonIdFromName();
            //    if (pokeId == 0)
            //    {
            //        await ctx.TriggerTypingAsync();
            //        await ctx.RespondAsync($"{ctx.User.Mention} Failed to find raid boss Pokemon {arg}.");
            //        continue;
            //    }

            //    var pokemon = Database.Instance.Pokemon[pokeId];
            //    var result = false;
            //    _dep.SubscriptionProcessor.Manager.RemoveRaid(ctx.User.Id, poke.Replace(" ", "").Split(','), string.IsNullOrEmpty(city) ? _dep.WhConfig.CityRoles : city);
            //    if (string.IsNullOrEmpty(city))
            //    {
            //        for (var cty = 0; cty < _dep.WhConfig.CityRoles.Count; cty++)
            //        {
            //            result |= _dep.SubscriptionProcessor.Manager.RemoveRaid(ctx.User.Id, pokeId, _dep.WhConfig.CityRoles[cty]);
            //        }
            //    }
            //    else
            //    {
            //        result |= _dep.SubscriptionProcessor.Manager.RemoveRaid(ctx.User.Id, pokeId, city);
            //    }

            //    if (result)
            //    {
            //        unsubscribed.Add(pokemon.Name);
            //    }
            //    else
            //    {
            //        notSubscribed.Add(pokemon.Name);
            //    }
            //}

            //await ctx.TriggerTypingAsync();
            //await ctx.RespondAsync
            //(
            //    (unsubscribed.Count > 0
            //        ? $"{ctx.User.Mention} has unsubscribed from **{string.Join("**, **", unsubscribed)}** raid notifications{(string.IsNullOrEmpty(city) ? " from **all** cities" : $" from city **{city}**")}."
            //        : string.Empty) +
            //    (notSubscribed.Count > 0
            //        ? $" {ctx.User.Mention} is not subscribed to {string.Join(",", notSubscribed)} raid notifications{(string.IsNullOrEmpty(city) ? " from **all** cities" : $" from city **{city}**")}."
            //        : string.Empty)
            //);
        }

        private PokemonValidation ValidatePokemon(IEnumerable<string> pokemon)
        {
            var valid = new List<int>();
            var invalid = new List<string>();
            //for (var i = 0; i < pokemon.Count; i++)
            foreach (var poke in pokemon)
            {
                var pokeId = poke.PokemonIdFromName();
                if (pokeId == 0)
                {
                    //await ctx.RespondAsync($"{ctx.User.Mention} Failed to find raid boss Pokemon {arg}.");
                    invalid.Add(poke);
                    continue;
                }

                if (!Database.Instance.Pokemon.ContainsKey(pokeId))
                {
                    continue;
                }

                valid.Add(pokeId);
            }

            return new PokemonValidation { Valid = valid, Invalid = invalid };
        }

        public class PokemonValidation
        {
            public List<int> Valid { get; set; }

            public List<string> Invalid { get; set; }

            public PokemonValidation()
            {
                Valid = new List<int>();
                Invalid = new List<string>();
            }
        }

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
                //await ctx.RespondAsync($"{ctx.User.Mention} Subscriptions are not enabled in the config.");
                await ctx.RespondEmbed(string.Format(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED"), ctx.User.Username));// $"{ctx.User.Username} Subscriptions are not enabled in the config.");
                return;
            }

            if (string.Compare(city, Strings.All, true) != 0 && !string.IsNullOrEmpty(city))
            {
                if (_dep.WhConfig.CityRoles.Find(x => string.Compare(x.ToLower(), city.ToLower(), true) == 0) == null)
                {
                    await ctx.RespondAsync($"{ctx.User.Mention} Failed to find city role {city}. To see a list of valid city roles type the command `.cities` or `.feeds`.");
                    return;
                }
            }
            else
            {
                //Assign to all cities.
                city = string.Empty;
            }

            var isSupporter = ctx.Client.IsSupporterOrHigher(ctx.User.Id, _dep.WhConfig);
            if (!isSupporter)
            {
                await ctx.DonateUnlockFeaturesMessage();
                return;
            }

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.User.Id);
            if (!isSupporter && subscription.Raids.Count >= Strings.MaxRaidSubscriptions)
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync($"{ctx.User.Mention} Non-supporter members have a limited notification amount of {Strings.MaxQuestSubscriptions} different field research quests, please consider donating to lift this to every field research quest. Otherwise you will need to remove some subscriptions in order to subscribe to new field research quests.");
                return;
            }

            var result = false;
            await ctx.TriggerTypingAsync();
            if (string.IsNullOrEmpty(city))
            {
                for (var cty = 0; cty < _dep.WhConfig.CityRoles.Count; cty++)
                {
                    result |= _dep.SubscriptionProcessor.Manager.AddQuest(ctx.User.Id, rewardKeyword, _dep.WhConfig.CityRoles[cty]);
                }
            }
            else
            {
                result |= _dep.SubscriptionProcessor.Manager.AddQuest(ctx.User.Id, rewardKeyword, city);
            }

            _dep.SubscriptionProcessor.Manager.Save(subscription);

            await ctx.TriggerTypingAsync();
            if (result)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} has subscribed to **{rewardKeyword}** quest notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}.");
            }
            else
            {
                await ctx.RespondAsync($"{ctx.User.Mention} is already subscribed to **{rewardKeyword}** quest notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}.");
            }
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
                //await ctx.RespondAsync($"{ctx.User.Mention} Subscriptions are not enabled in the config.");
                await ctx.RespondEmbed(string.Format(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED"), ctx.User.Username));// $"{ctx.User.Username} Subscriptions are not enabled in the config.");
                return;
            }

            if (string.Compare(city, Strings.All, true) != 0 && !string.IsNullOrEmpty(city))
            {
                if (_dep.WhConfig.CityRoles.Find(x => string.Compare(x.ToLower(), city.ToLower(), true) == 0) == null)
                {
                    await ctx.RespondAsync($"{ctx.User.Mention} Failed to find city role {city}. To see a list of valid city roles type the command `.cities` or `.feeds`.");
                    return;
                }
            }
            else
            {
                //Assign to all cities.
                city = string.Empty;
            }

            if (!_dep.SubscriptionProcessor.Manager.UserExists(ctx.User.Id))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync($"{ctx.User.Mention} is not subscribed to any raid notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}.");
                return;
            }

            var notSubscribed = new List<string>();
            var unsubscribed = new List<string>();

            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(ctx.User.Id);
            await ctx.TriggerTypingAsync();

            if (string.Compare(rewardKeyword, Strings.All, true) == 0)
            {
                var removeAllResult = await ctx.Confirm($"{ctx.User.Mention} are you sure you want to remove **all** {subscription.Quests.Count.ToString("N0")} of your field research quest subscriptions? Please reply back with `y` or `yes` to confirm.");
                if (!removeAllResult) return;

                if (!_dep.SubscriptionProcessor.Manager.RemoveAllQuests(ctx.User.Id))
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondAsync($"{ctx.User.Mention} Failed to remove all quest subscriptions.");
                    return;
                }

                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync($"{ctx.User.Mention} has unsubscribed from **all** quest notifications!");
                return;
            }

            var result = false;
            if (string.IsNullOrEmpty(city))
            {
                for (var cty = 0; cty < _dep.WhConfig.CityRoles.Count; cty++)
                {
                    result |= _dep.SubscriptionProcessor.Manager.RemoveQuest(ctx.User.Id, rewardKeyword, _dep.WhConfig.CityRoles[cty]);
                }
            }
            else
            {
                result |= _dep.SubscriptionProcessor.Manager.RemoveQuest(ctx.User.Id, rewardKeyword, city);
            }

            await ctx.TriggerTypingAsync();
            if (result)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} has unsubscribed from **{rewardKeyword}** quest notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}.");
            }
            else
            {
                await ctx.RespondAsync($"{ctx.User.Mention} is not subscribed to **{rewardKeyword}** quest notifications{(string.IsNullOrEmpty(city) ? " from **all** areas" : $" from city **{city}**")}.");
            }
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
                //await ctx.RespondAsync($"{ctx.User.Mention} Subscriptions are not enabled in the config.");
                await ctx.RespondEmbed(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED").FormatText(ctx.User.Username));
                return;
            }

            if (!_dep.SubscriptionProcessor.Manager.UserExists(ctx.User.Id))
            {
                await ctx.RespondEmbed(string.Format(_dep.Language.Translate("MSG_USER_NOT_SUBSCRIBED")));
                return;
            }

            var parts = coordinates.Replace(" ", null).Split(',');
            if (!double.TryParse(parts[0], out var lat) || !double.TryParse(parts[1], out var lng))
            {
                await ctx.RespondEmbed($"{ctx.User.Mention} Could not parse {coordinates} as valid coordinates.");
                return;
            }

            if (!_dep.SubscriptionProcessor.Manager.SetDistance(ctx.User.Id, distance, lat, lng))
            {
                await ctx.RespondEmbed($"{ctx.User.Mention} Could not update database, please try again later.");
                return;
            }

            await ctx.RespondEmbed($"{ctx.User.Mention} Raid notifications within a {distance} meter radius of location {lat},{lng}.");
        }

        [
            Command("gymme"),
            Description("Add raid notifications for specific gyms.")
        ]
        public async Task GymMeAsync(CommandContext ctx,
            [Description("Gym name to subscribed to."), RemainingText] string gymName)
        {
            if (!_dep.WhConfig.EnableSubscriptions)
            {
                //await ctx.RespondAsync($"{ctx.User.Mention} Subscriptions are not enabled in the config.");
                await ctx.RespondEmbed(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED").FormatText(ctx.User.Username));
                return;
            }

            if (!_dep.SubscriptionProcessor.Manager.AddGym(ctx.User.Id, gymName))
            {
                await ctx.RespondEmbed($"{ctx.User.Mention} Could not add gym subscription '{gymName}' to database.");
                return;
            }

            await ctx.RespondEmbed($"{ctx.User.Mention} Added gym subscription '{gymName}' to database.");
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
                //await ctx.RespondAsync($"{ctx.User.Mention} Subscriptions are not enabled in the config.");
                await ctx.RespondEmbed(_dep.Language.Translate("MSG_SUBSCRIPTIONS_NOT_ENABLED").FormatText(ctx.User.Username));
                return;
            }

            if (string.Compare(Strings.All, gymName, true) == 0)
            {
                if (!_dep.SubscriptionProcessor.Manager.RemoveAllGyms(ctx.User.Id))
                {
                    await ctx.RespondEmbed($"{ctx.User.Mention} Could not remove all gym subscriptions from database.");
                    return;
                }

                await ctx.RespondEmbed($"{ctx.User.Mention} Removed all gym subscriptions from database.");
                return;
            }

            if (!_dep.SubscriptionProcessor.Manager.RemoveGym(ctx.User.Id, gymName))
            {
                await ctx.RespondEmbed($"{ctx.User.Mention} Could not remove gym subscription '{gymName}' from database.");
                return;
            }

            await ctx.RespondEmbed($"{ctx.User.Mention} Removed gym subscription '{gymName}' from database.");
        }

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

            eb.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"versx | {DateTime.Now}",
                IconUrl = ctx.Guild?.IconUrl
            };
            await ctx.RespondAsync(ctx.User.Mention, false, eb);
        }

        #region Private Methods

        private async Task SendUserSubscriptionSettings(DiscordClient client, DiscordUser receiver, DiscordUser user)
        {
            var userSettings = BuildUserSubscriptionSettings(client, user);
            userSettings = userSettings.Length > 2000 ? userSettings.Substring(0, Math.Min(userSettings.Length, 1500)) : userSettings;
            var eb = new DiscordEmbedBuilder
            {
                Title = $"**{receiver.Username} Notification Settings:**\r\n",
                Description = userSettings,
                Color = DiscordColor.CornflowerBlue,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"versx | {DateTime.Now}"
                }
            };
            await client.SendDirectMessage(receiver, eb.Build());
        }

        private string BuildUserSubscriptionSettings(DiscordClient client, DiscordUser user)
        {
            var author = user.Id;
            var isSubbed = _dep.SubscriptionProcessor.Manager.UserExists(author);
            var subscription = _dep.SubscriptionProcessor.Manager.GetUserSubscriptions(user.Id);
            var hasPokemon = isSubbed && subscription?.Pokemon.Count > 0;
            var hasRaids = isSubbed && subscription?.Raids.Count > 0;
            var hasQuests = isSubbed && subscription?.Quests.Count > 0;
            var msg = string.Empty;
            var isSupporter = client.IsSupporterOrHigher(author, _dep.WhConfig);

            if (hasPokemon)
            {
                var member = client.GetMemberById(_dep.WhConfig.GuildId, author);
                if (member == null)
                {
                    var error = $"Failed to get discord member from id {author}.";
                    _logger.Error(error);
                    return error;
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

                msg += $"Enabled: **{(subscription.Enabled ? "Yes" : "No")}**\r\n";
                msg += $"Feed Zones: **{string.Join("**, **", feeds)}**\r\n";
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
            }

            if (hasRaids)
            {
                msg += $"Raid Subscriptions: ({subscription.Raids.Count.ToString("N0")}/{(isSupporter ? "∞" : Strings.MaxRaidSubscriptions.ToString())} used)\r\n";
                msg += "```";
                msg += string.Join(Environment.NewLine, GetRaidSubscriptionNames(author));
                msg += "```" + Environment.NewLine + Environment.NewLine;
            }

            if (hasQuests)
            {
                msg += $"Quest Subscriptions: ({subscription.Quests.Count.ToString("N0")}/{(isSupporter ? "∞" : Strings.MaxQuestSubscriptions.ToString())} used)\r\n";
                msg += "```";
                msg += string.Join(Environment.NewLine, GetQuestSubscriptionNames(author));
                msg += "```";
            }

            if (string.IsNullOrEmpty(msg))
            {
                msg = $"**{user.Mention}** is not subscribed to any Pokemon or Raid notifications.";
            }

            return msg;
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

            var results = subscribedRaids.GroupBy(x => x.PokemonId, (key, g) => new { PokemonId = key, Cities = g.ToList() });
            foreach (var raid in results)
            {
                if (!Database.Instance.Pokemon.ContainsKey(raid.PokemonId))
                    continue;

                var pokemon = Database.Instance.Pokemon[raid.PokemonId];
                if (pokemon == null)
                    continue;

                var isAllCities = _dep.WhConfig.CityRoles.UnorderedEquals(raid.Cities.Select(x => x.City).ToList());
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
            subscribedQuests.Sort((x, y) => string.Compare(x.RewardKeyword.ToLower(), y.RewardKeyword.ToLower(), StringComparison.Ordinal));

            var results = subscribedQuests.GroupBy(p => p.RewardKeyword, (key, g) => new { Reward = key, Cities = g.ToList() });
            foreach (var quest in results)
            {
                var isAllCities = _dep.WhConfig.CityRoles.UnorderedEquals(quest.Cities.Select(x => x.City).ToList());
                list.Add($"{quest.Reward} (From: {(isAllCities ? "All Areas" : string.Join(", ", quest.Cities.Select(x => x.City)))})");
            }

            return list;
        }

        private bool IsCommonPokemon(int pokeId)
        {
            var commonPokemon = new List<int>
            {
                1, //Bulbasaur
                4, //Charmander
                //7, //Squirtle
                //10, //Caterpie
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
                41, //Zubat
                46, //Paras
                48, //Venonat
                50, //Diglett
                52, //Meowth
                104, //Cubone
                133, //Eevee
                152, //Chikorita
                155, //Cyndaquil
                161, //Sentret
                163, //Hoothoot
                165, //Ledyba
                167, //Spinarak
                177, //Natu
                187, //Hoppip
                191, //Sunkern
                193, //Yanma
                194, //Wooper
                198, //Murkrow
                209, //Snubbull
                228, //Houndour
                252, //Treecko
                255, //Torchic
                261, //Poochyena
                263, //Zigzagoon
                265, //Wurmple
                273, //Seedot
                276, //Taillow
                293, //Whismur
                300, //Skitty
                307, //Meditite
                309, //Electrike
                315, //Roselia
                316, //Gulpin
                322, //Numel
                325, //Spoink
                331, //Cacnea
                333, //Swablu
                363, //Spheal

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