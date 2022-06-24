﻿namespace WhMgr.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Gender = POGOProtos.Rpc.PokemonDisplayProto.Types.Gender;
    using WeatherCondition = POGOProtos.Rpc.GameplayWeatherProto.Types.WeatherCondition;

    using WhMgr.Common;
    using WhMgr.Data;

    public static class PokemonExtensions
    {
        public static int GetCpAtLevel(this uint id, int level, int statValue = 10)
        {
            if (!GameMaster.Instance.Pokedex.ContainsKey(id) || id == 0)
                return 0;

            var pkmn = GameMaster.Instance.Pokedex[id];
            var multiplier = GameMaster.Instance.CpMultipliers[level];
            var maxAtk = ((pkmn.Attack + statValue) * multiplier) ?? 0;
            var maxDef = ((pkmn.Defense + statValue) * multiplier) ?? 0;
            var maxSta = ((pkmn.Stamina + statValue) * multiplier) ?? 0;
            var cp = Convert.ToInt32(Math.Max(10, Math.Floor(Math.Sqrt(maxAtk * maxAtk * maxDef * maxSta) / 10)));
            return cp;
        }

        public static bool IsCommonPokemon(this uint pokeId)
        {
            return GameMaster.Instance.PokemonRarity[PokemonRarity.Common].Contains(pokeId);
        }

        public static bool IsRarePokemon(this uint pokeId)
        {
            return GameMaster.Instance.PokemonRarity[PokemonRarity.Rare].Contains(pokeId);
        }

        public static PokemonSize GetSize(this uint id, double height, double weight)
        {
            if (!GameMaster.Instance.Pokedex.ContainsKey(id))
                return PokemonSize.Normal;

            var stats = GameMaster.Instance.Pokedex[id];
            var weightRatio = weight / Convert.ToDouble(stats?.Weight ?? 0);
            var heightRatio = height / Convert.ToDouble(stats?.Height ?? 0);
            var size = heightRatio + weightRatio;

            if (size < 1.5) return PokemonSize.Tiny;
            if (size <= 1.75) return PokemonSize.Small;
            if (size < 2.25) return PokemonSize.Normal;
            if (size <= 2.5) return PokemonSize.Large;
            return PokemonSize.Big;
        }

        public static string GetPokemonGenderIcon(this Gender gender)
        {
            return gender switch
            {
                Gender.Male => "♂", //♂ \u2642
                Gender.Female => "♀", //♀ \u2640
                _ => "⚲", //⚲
            };
        }

        public static List<PokemonType> GetStrengths(this PokemonType type)
        {
            if (GameMaster.Instance.PokemonTypes.ContainsKey(type))
            {
                return GameMaster.Instance.PokemonTypes[type].Strengths;
            }
            return new List<PokemonType>();
        }

        public static List<PokemonType> GetWeaknesses(this PokemonType type)
        {
            if (GameMaster.Instance.PokemonTypes.ContainsKey(type))
            {
                return GameMaster.Instance.PokemonTypes[type].Weaknesses;
            }
            return new List<PokemonType>();
        }

        public static string GetTypeEmojiIcons(this PokemonType pokemonType)
        {
            return GetTypeEmojiIcons(new List<PokemonType> { pokemonType });
        }

        public static string GetTypeEmojiIcons(this IEnumerable<PokemonType> pokemonTypes)
        {
            var list = new List<string>();
            foreach (var type in pokemonTypes)
            {
                var emojiName = type.GetEmojiIcon("types", true);
                if (!list.Contains(emojiName))
                {
                    list.Add(emojiName);
                }
            }
            return string.Join(" ", list);
        }

        public static string GetEmojiIcon<T>(this T type, string keyPrefix, bool asString, string defaultValue = null)
        {
            var value = asString ? type.ToString().ToLower() : Convert.ToInt32(type).ToString();
            var key = string.IsNullOrEmpty(keyPrefix) ? value : $"{keyPrefix}_{value}";
            var emojiId = GameMaster.Instance.Emojis.ContainsKey(key)
                ? GameMaster.Instance.Emojis[key]
                : 0;

            if (emojiId == 0)
            {
                Console.WriteLine($"Emoji '{key}' does not exist! Using fallback text.");
            }

            // Check if custom emoji list contains specified emoji string or if custom emoji is not overwritten.
            var emojiName = !GameMaster.Instance.CustomEmojis.ContainsKey(key) || string.IsNullOrEmpty(GameMaster.Instance.CustomEmojis[key])
                // Check if we retrieved Discord emoji successfully
                ? emojiId > 0
                    // Construct Discord emoji string
                    ? string.Format(Strings.Defaults.EmojiSchema, key, emojiId)
                    // Fallback to text instead of emoji
                    : type.ToString()
                // Custom emoji is set which overwrites Discord emojis
                : GameMaster.Instance.CustomEmojis[key] ?? type.ToString();
            var result = !string.IsNullOrEmpty(emojiName)
                ? emojiName
                : defaultValue;
            return result;
        }

        public static string GetWeaknessEmojiIcons(this List<PokemonType> pokemonTypes)
        {
            if (pokemonTypes == null || pokemonTypes?.Count == 0)
                return string.Empty;

            var list = new List<PokemonType>();
            pokemonTypes.ForEach(type =>
            {
                list.AddRange(type.ToString().StringToObject<PokemonType>().GetWeaknesses().Distinct());
            });
            var emojis = list.GetTypeEmojiIcons();
            return emojis;
        }

        public static uint PokemonIdFromName(this string nameOrId)
        {
            if (string.IsNullOrEmpty(nameOrId))
                return 0;

            var pkmn = uint.TryParse(nameOrId, out var id)
                ? GameMaster.Instance.Pokedex.FirstOrDefault(pokemon => pokemon.Key == id)
                : GameMaster.Instance.Pokedex.FirstOrDefault(pokemon => string.Compare(pokemon.Value.Name, nameOrId, true) == 0);

            if (pkmn.Key > 0)
                return pkmn.Key;

            foreach (var p in GameMaster.Instance.Pokedex)
                if (p.Value.Name.ToLower().Contains(nameOrId.ToLower()))
                    return p.Key;

            if (!uint.TryParse(nameOrId, out var pokeId))
                return 0;

            if (GameMaster.Instance.Pokedex.ContainsKey(pokeId))
                return pokeId;

            return 0;
        }

        public static PokemonValidation ValidatePokemon(this IEnumerable<string> pokemon)
        {
            var valid = new Dictionary<uint, string>();
            var invalid = new List<string>();
            foreach (var poke in pokemon)
            {
                string form = null;
                var pokeIdStr = poke;
                var pokeId = pokeIdStr.PokemonIdFromName();
                if (pokeId == 0)
                {
                    if (!poke.Contains("-"))
                    {
                        invalid.Add(poke);
                        continue;
                    }

                    //Has form
                    var formSplit = poke.Split('-');
                    if (formSplit.Length != 2)
                        continue;

                    pokeIdStr = formSplit[0];
                    pokeId = pokeIdStr.PokemonIdFromName();
                    form = formSplit[1];
                }

                if (!GameMaster.Instance.Pokedex.ContainsKey(pokeId))
                {
                    invalid.Add(poke);
                    continue;
                }

                if (!valid.ContainsKey(pokeId))
                {
                    valid.Add(pokeId, form);
                }
            }

            return new PokemonValidation { Valid = valid, Invalid = invalid };
        }

        public static bool IsWeatherBoosted(this PokedexPokemon pkmn, WeatherCondition weather)
        {
            var types = pkmn?.Types;
            var boosts = Strings.Defaults.WeatherBoosts[weather];
            var isBoosted = types?.Exists(type => boosts.Contains(type)) ?? false;
            return isBoosted;
        }
    }

    public class PokemonValidation
    {
        public IReadOnlyDictionary<uint, string> Valid { get; set; }

        public IReadOnlyList<string> Invalid { get; set; }

        public PokemonValidation()
        {
            Valid = new Dictionary<uint, string>();
            Invalid = new List<string>();
        }

        public static PokemonValidation Validate(string pokemonList, int maxPokemonId)// = 999)
        {
            if (string.IsNullOrEmpty(pokemonList))
                return null;

            pokemonList = pokemonList.Replace(" ", "");

            var generations = Strings.Defaults.PokemonGenerationRanges;
            PokemonValidation validation;
            if (pokemonList.Contains("-") && int.TryParse(pokemonList.Split('-')[0], out var startRange) && int.TryParse(pokemonList.Split('-')[1], out var endRange))
            {
                // If `poke` param is a range
                var range = GetListFromRange(startRange, endRange);
                validation = range.ValidatePokemon();
            }
            else if (generations.Select(gen => "gen" + gen.Key).ToList().Contains(pokemonList))
            {
                // If `poke` is pokemon generation
                if (!int.TryParse(pokemonList.Replace("gen", ""), out var gen) || !generations.ContainsKey(gen))
                {
                    var keys = generations.Keys.ToList();
                    var minValue = keys[0];
                    var maxValue = keys[^1];
                    return null;
                }

                var genRange = generations[gen];
                var range = GetListFromRange(genRange.Start, genRange.End);
                validation = range.ValidatePokemon();
            }
            else if (string.Compare(pokemonList, Strings.Defaults.All, true) == 0)
            {
                var list = GetListFromRange(1, maxPokemonId);
                validation = list.ValidatePokemon();
            }
            else
            {
                // If `poke` param is a list
                validation = pokemonList.Replace(" ", "").Split(',').ValidatePokemon();
            }

            return validation;
        }

        public static List<string> GetListFromRange(int startRange, int endRange)
        {
            var list = new List<string>();
            for (; startRange <= endRange; startRange++)
            {
                list.Add(startRange.ToString());
            }
            return list;
        }
    }
}