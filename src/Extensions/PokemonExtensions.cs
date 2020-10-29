namespace WhMgr.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using WhMgr.Data;
    using WhMgr.Data.Models;
    using WhMgr.Data.Subscriptions.Models;
    using WhMgr.Net.Models;

    public static class PokemonExtensions
    {
        public static int MaxCpAtLevel(this int id, int level)
        {
            if (!MasterFile.Instance.Pokedex.ContainsKey(id) || id == 0)
                return 0;

            var pkmn = MasterFile.Instance.Pokedex[id];
            var multiplier = MasterFile.Instance.CpMultipliers[level];
            var maxAtk = ((pkmn.Attack + 15) * multiplier) ?? 0;
            var maxDef = ((pkmn.Defense + 15) * multiplier) ?? 0;
            var maxSta = ((pkmn.Stamina + 15) * multiplier) ?? 0;

            return (int)Math.Max(10, Math.Floor(Math.Sqrt(maxAtk * maxAtk * maxDef * maxSta) / 10));
        }

        public static int MinCpAtLevel(this int id, int level)
        {
            if (!MasterFile.Instance.Pokedex.ContainsKey(id) || id == 0)
                return 0;

            var pkmn = MasterFile.Instance.Pokedex[id];
            var multiplier = MasterFile.Instance.CpMultipliers[level];
            var minAtk = ((pkmn.Attack + 10) * multiplier) ?? 0;
            var minDef = ((pkmn.Defense + 10) * multiplier) ?? 0;
            var minSta = ((pkmn.Stamina + 10) * multiplier) ?? 0;

            return (int)Math.Max(10, Math.Floor(Math.Sqrt(minAtk * minAtk * minDef * minSta) / 10));
        }

        public static bool IsCommonPokemon(this int pokeId)
        {
            return MasterFile.Instance.PokemonRarity[PokemonRarity.Common].Contains(pokeId);
        }

        public static bool IsRarePokemon(this int pokeId)
        {
            return MasterFile.Instance.PokemonRarity[PokemonRarity.Rare].Contains(pokeId);
        }

        /*
        public static int GetLevel(this int id, int cp, int atk, int def, int sta)
        {
            if (!MasterFile.Instance.Pokedex.ContainsKey(id))
                return 0;

            var pkmn = MasterFile.Instance.Pokedex[id];
            for (var i = 0; i < MasterFile.Instance.CpMultipliers.Count; i++)
            {
                var spawnCP = GetCP(pkmn.Attack ?? 0 + atk, pkmn.Defense ?? 0 + def, pkmn.Stamina ?? 0 + sta, MasterFile.Instance.CpMultipliers[i + 1]);
                if (cp == spawnCP)
                {
                    var level = i + 1;
                    return level;
                }
            }

            return 0;
        }

        public static int GetCP(int attack, int defense, int stamina, double cpm)
        {
            var cp = Math.Floor(attack * Math.Pow(defense, 0.5) * Math.Pow(stamina, 0.5) * Math.Pow(cpm, 2) / 10);
            return Convert.ToInt32(cp < 10 ? 10 : cp);
        }
        */

        public static PokemonSize GetSize(this int id, float height, float weight)
        {
            if (!MasterFile.Instance.Pokedex.ContainsKey(id))
                return PokemonSize.Normal;

            var stats = MasterFile.Instance.Pokedex[id];
            var weightRatio = weight / Convert.ToDouble(stats?.Weight ?? 0);
            var heightRatio = height / Convert.ToDouble(stats?.Height ?? 0);
            var size = heightRatio + weightRatio;

            if (size < 1.5)   return PokemonSize.Tiny;
            if (size <= 1.75) return PokemonSize.Small;
            if (size < 2.25)  return PokemonSize.Normal;
            if (size <= 2.5)  return PokemonSize.Large;
            return PokemonSize.Big;
        }

        public static string GetPokemonGenderIcon(this PokemonGender gender)
        {
            switch (gender)
            {
                case PokemonGender.Male:
                    return "♂"; //♂ \u2642
                case PokemonGender.Female:
                    return "♀"; //♀ \u2640
                default:
                    return "⚲"; //⚲
            }
        }

        public static List<PokemonType> GetStrengths(this PokemonType type)
        {
            if (MasterFile.Instance.PokemonTypes.ContainsKey(type))
            {
                return MasterFile.Instance.PokemonTypes[type].Strengths;
            }
            return new List<PokemonType>();
        }

        public static List<PokemonType> GetWeaknesses(this PokemonType type)
        {
            if (MasterFile.Instance.PokemonTypes.ContainsKey(type))
            {
                return MasterFile.Instance.PokemonTypes[type].Weaknesses;
            }
            return new List<PokemonType>();
         }

        public static string GetTypeEmojiIcons(this PokemonType pokemonType)
        {
            return GetTypeEmojiIcons(new List<PokemonType> { pokemonType });
        }

        public static string GetTypeEmojiIcons(this List<PokemonType> pokemonTypes)
        {
            var list = new List<string>();
            foreach (var type in pokemonTypes)
            {
                var emojiKey = $"types_{type.ToString().ToLower()}";
                //if (!MasterFile.Instance.Emojis.ContainsKey(emojiKey))
                //    continue;

                var emojiId = MasterFile.Instance.Emojis[emojiKey];
                var emojiName = string.IsNullOrEmpty(MasterFile.Instance.CustomEmojis[emojiKey])
                    ? emojiId > 0
                        ? string.Format(Strings.TypeEmojiSchema, type.ToString().ToLower(), emojiId)
                        : type.ToString()
                    : MasterFile.Instance.CustomEmojis[emojiKey];
                if (!list.Contains(emojiName))
                {
                    list.Add(emojiName);
                }
            }

            return string.Join(" ", list);
        }

        public static string GetWeatherEmojiIcon(this WeatherType weather)
        {
            var key = $"weather_{Convert.ToInt32(weather)}";
            var emojiId = MasterFile.Instance.Emojis[key];
            var emojiName = emojiId > 0 ? string.Format(Strings.EmojiSchema, key, emojiId) : weather.ToString();
            return emojiName;
        }

        public static string GetCaptureRateEmojiIcon(this CaptureRateType type)
        {
            var key = $"capture_{Convert.ToInt32(type)}";
            var emojiId = MasterFile.Instance.Emojis[key];
            var emojiName = string.IsNullOrEmpty(MasterFile.Instance.CustomEmojis[key])
                ? emojiId > 0
                    ? string.Format(Strings.EmojiSchema, key, emojiId)
                    : type.ToString()
                : MasterFile.Instance.CustomEmojis[key];
            return emojiName;
        }

        public static string GetLeagueEmojiIcon(this PvPLeague league)
        {
            var key = $"league_{league.ToString().ToLower()}";
            var emojiId = MasterFile.Instance.Emojis[key];
            var emojiName = string.IsNullOrEmpty(MasterFile.Instance.CustomEmojis[key])
                ? emojiId > 0
                    ? string.Format(Strings.EmojiSchema, key, emojiId)
                    : league.ToString()
                : MasterFile.Instance.CustomEmojis[key];
            return emojiName;
        }

        public static string GetGenderEmojiIcon(this PokemonGender gender)
        {
            var key = $"gender_{gender.ToString().ToLower()}";
            var emojiId = MasterFile.Instance.Emojis[key];
            var emojiName = string.IsNullOrEmpty(MasterFile.Instance.CustomEmojis[key])
                ? emojiId > 0
                    ? string.Format(Strings.EmojiSchema, key, emojiId)
                    : gender.ToString()
                : MasterFile.Instance.CustomEmojis[key];
            return emojiName;
        }

        public static string GetWeaknessEmojiIcons(this List<PokemonType> pokemonTypes)
        {
            if (pokemonTypes == null || pokemonTypes?.Count == 0)
                return string.Empty;

            var list = new List<string>();
            foreach (var type in pokemonTypes)
            {
                var weaknesses = type.ToString().StringToObject<PokemonType>().GetWeaknesses().Distinct();
                foreach (var weakness in weaknesses)
                {
                    var typeKey = $"types_{weakness.ToString().ToLower()}";
                    var emojiId = MasterFile.Instance.Emojis[typeKey];
                    var emojiName = string.IsNullOrEmpty(MasterFile.Instance.CustomEmojis[typeKey])
                        ? emojiId > 0
                            ? string.Format(Strings.TypeEmojiSchema, weakness.ToString().ToLower(), emojiId)
                            : weakness.ToString()
                        : MasterFile.Instance.CustomEmojis[typeKey];
                    if (!list.Contains(emojiName))
                    {
                        list.Add(emojiName);
                    }
                }
            }

            return string.Join(" ", list);
        }

        public static int PokemonIdFromName(this string name)
        {
            if (string.IsNullOrEmpty(name))
                return 0;

            var pkmn = int.TryParse(name, out var id)
                ? MasterFile.Instance.Pokedex.FirstOrDefault(x => x.Key == id)
                : MasterFile.Instance.Pokedex.FirstOrDefault(x => string.Compare(x.Value.Name, name, true) == 0);

            if (pkmn.Key > 0)
                return pkmn.Key;

            foreach (var p in MasterFile.Instance.Pokedex)
                if (p.Value.Name.ToLower().Contains(name.ToLower()))
                    return p.Key;

            if (!int.TryParse(name, out var pokeId))
                return 0;

            if (MasterFile.Instance.Pokedex.ContainsKey(pokeId))
                return pokeId;

            return 0;
        }

        public static PokemonValidation ValidatePokemon(this IEnumerable<string> pokemon)
        {
            var valid = new Dictionary<int, string>();
            var invalid = new List<string>();
            foreach (var poke in pokemon)
            {
                string form = null;
                var pokeIdStr = poke;
                var pokeId = pokeIdStr.PokemonIdFromName();
                if (pokeId == 0)
                {
                    if (poke.Contains("-"))
                    {
                        //Has form
                        var formSplit = poke.Split('-');
                        if (formSplit.Length != 2)
                            continue;

                        pokeIdStr = formSplit[0];
                        pokeId = pokeIdStr.PokemonIdFromName();
                        form = formSplit[1];
                    }
                    else
                    {
                        invalid.Add(poke);
                        continue;
                    }
                }

                if (!MasterFile.Instance.Pokedex.ContainsKey(pokeId))
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

        public static bool IsWeatherBoosted(this PokedexPokemon pkmn, WeatherType weather)
        {
            var types = pkmn?.Types;
            var isBoosted = types?.Exists(x => Strings.WeatherBoosts[weather].Contains(x)) ?? false;
            return isBoosted;
        }
    }

    public class PokemonValidation
    {
        public Dictionary<int, string> Valid { get; set; }

        public List<string> Invalid { get; set; }

        public PokemonValidation()
        {
            Valid = new Dictionary<int, string>();
            Invalid = new List<string>();
        }
    }
}