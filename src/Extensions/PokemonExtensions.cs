namespace WhMgr.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using WhMgr.Configuration;
    using WhMgr.Data;
    using WhMgr.Data.Models;
    using WhMgr.Localization;
    using WhMgr.Net.Models;

    public static class PokemonExtensions
    {
        //private const string Alolan = "Alolan";
        //private const string Armored = "Armored";
        //private const string Shadow = "Shadow";
        //private const string Purified = "Purified";
        private const string Normal = "";
        //private const string Glasses = "Glasses";
        private const string NoEvolve = "No-Evolve";
        //private const string Anniversary = "Anniversary";
        //private const string Christmas = "Christmas";
        //private const string Birthday = "Birthday";
        //private const string Halloween = "Halloween";
        //private const string Sunny = "Sunny";
        //private const string Overcast = "Overcast";
        //private const string Rainy = "Rainy";
        //private const string Snowy = "Snowy";
        //private const string Attack = "Attack";
        //private const string Defense = "Defense";
        //private const string Speed = "Speed";
        //private const string Plant = "Plant";
        //private const string Sandy = "Sandy";
        //private const string Trash = "Trash";
        //private const string Altered = "Altered";
        //private const string Origin = "Origin";
        //private const string WestSea = "West Sea";
        //private const string EastSea = "East Sea";

        //private const string ExclaimationMark = "!";
        //private const string QuestionMark = "?";

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

        public static string GetPokemonForm(this int formId)
        {
            if (formId == 0)
                return null;

            return Translator.Instance.Translate("form_" + formId);
        }

        public static string GetCostume(this int pokeId, string costumeId)
        {
            if (!int.TryParse(costumeId, out int costume))
                return null;

            if (costume == 0)
                return null;

            switch (pokeId)
            {
                case 7:
                    switch (costume)
                    {
                        case 0:
                            break;
                    }
                    break;
                case 25: //Pikachu
                    switch (costume)
                    {
                        /*
  case unset // = 0
  case holiday2016 // = 1
  case anniversary // = 2
  case oneYearAnniversary // = 3
  case halloween2017 // = 4
  case summer2018 // = 5
  case fall2018 // = 6
  case november2018 // = 7
  case winter2018 // = 8
  case feb2019 // = 9
  case may2019Noevolve // = 10
                         */
                        case 0: //Unset
                            return null;
                        case 1: //X-Mas Hat/Christmas /holiday2016
                            return "Santa Hat";
                        case 2: //Party Hat/Birthday /anniversary
                            return "Party Hat";
                        case 3: //Ash Hat /Anniversary
                            return "Ash Hat";
                        case 4: //Witch Hat/Halloween /halloween2017
                            return "Witch Hat";
                        case 5: //Straw Hat/Sun Glasses /summer2018
                            return "Summer Hat";
                        case 6: //FM /fall2018
                            return "Fragment Hat";
                        case 7: // /november2018
                            return "Flower Crown";
                        case 8: // /winter2018
                            return "Beanie";
                        case 9: // /feb2019
                            return "Detective";
                        case 10: // /may2019Noevolve
                            return "Straw Hat";
                        case 598: //Normal
                            return Normal;
                        case 599: //No-Evolve
                            return NoEvolve;
                    }
                    break;
                case 26: //Raichu
                    switch (costume)
                    {
                        case 0: //Unset
                            return null;
                        case 1: //X-Mas Hat/Christmas /holiday2016
                            return "Santa Hat";
                        case 2: //Party Hat/Birthday /anniversary
                            return "Party Hat";
                        case 3: //Ash Hat /oneYearAnniversary
                            return "Ash Hat";
                        case 4: //Witch Hat/Halloween /halloween2017
                            return "Witch Hat";
                        case 5: //Straw Hat/Sun Glasses /summer2018
                            return "Summer Hat";
                        case 6: //FM /fall2018
                            return "Fragment Hat";
                        case 7: // /november2018
                            return "Flower Crown";
                        case 8: // /winter2018
                            return "Beanie";
                        case 9: // /feb2019
                            return "Detective";
                        //case 10: // /may2019Noevolve
                        //    return "One Piece";
                        case 598: //Normal
                            return Normal;
                        case 599: //No-Evolve
                            return NoEvolve;
                    }
                    break;
                case 172:
                    switch (costume)
                    {
                        case 0: //Unset
                            return null;
                        case 1: //X-Mas Hat/Christmas /holiday2016
                            return "Santa Hat";
                        case 2: //Party Hat/Birthday /anniversary
                            return "Party Hat";
                        case 3: //Ash Hat /oneYearAnniversary
                            return "Ash Hat";
                        case 4: //Witch Hat/Halloween /halloween2017
                            return "Witch Hat";
                        case 5: //Straw Hat/Sun Glasses /summer2018
                            return "Summer Hat";
                        //case 6: //FM /fall2018
                        //    return "Fragment Hat";
                        //case 7: // /november2018
                        //    return "Flower Crown";
                        //case 8: // /winter2018
                        //    return "Beanie";
                        //case 9: // /feb2019
                        //    return "Detective";
                        case 10: // /may2019Noevolve
                            return "Straw Hat";
                        case 598: //Normal
                            return Normal;
                        case 599: //No-Evolve
                            return NoEvolve;
                    }
                    break;
            }

            return string.Empty;
        }

        public static string GetPokemonIcon(this int pokemonId, int formId, int costumeId, WhConfig whConfig, string style)
        {
            var iconStyleUrl = whConfig.IconStyles[style];
            var url = iconStyleUrl.EndsWith('/') ? iconStyleUrl : iconStyleUrl;
            var id = string.Format("{0:D3}", pokemonId);
            var form = formId > 0 ? formId.ToString() : "00";
            return costumeId == 0
                ? $"{url}/pokemon_icon_{id}_{form}.png"
                : $"{url}/pokemon_icon_{id}_{form}_{costumeId}.png";
        }

        public static string GetRaidEggIcon(this RaidData raid, WhConfig whConfig, string style)
        {
            var iconStyleUrl = whConfig.IconStyles[style];
            var url = iconStyleUrl.EndsWith('/') ? iconStyleUrl + "eggs" : $"{iconStyleUrl}/eggs";
            return $"{url}/{raid.Level}.png";
        }

        public static string GetQuestIcon(this QuestData quest, WhConfig whConfig, string style)
        {
            var iconStyleUrl = whConfig.IconStyles[style];
            var url = iconStyleUrl.EndsWith('/') ? iconStyleUrl + "rewards" : $"{iconStyleUrl}/rewards";
            switch (quest.Rewards?[0].Type)
            {
                case QuestRewardType.Candy:
                    return $"{url}/reward_{Convert.ToInt32(ItemId.Rare_Candy)}_{(quest.Rewards?[0].Info.Amount ?? 1)}.png";
                case QuestRewardType.Item:
                    return $"{url}/reward_{(int)quest.Rewards?[0].Info.Item}_{(quest.Rewards?[0].Info.Amount ?? 1)}.png";
                case QuestRewardType.PokemonEncounter:
                    return (quest.IsDitto ? 132 : quest.Rewards[0].Info.PokemonId).GetPokemonIcon(quest.Rewards?[0].Info.FormId ?? 0, quest.Rewards?[0].Info.CostumeId ?? 0, whConfig, style);
                case QuestRewardType.Stardust:
                    if ((quest.Rewards[0]?.Info?.Amount ?? 0) > 0)
                    {
                        return $"{url}/reward_stardust_{quest.Rewards[0].Info.Amount}.png";
                    }
                    return $"{url}/reward_stardust.png";
                case QuestRewardType.AvatarClothing:
                case QuestRewardType.Experience:
                case QuestRewardType.Quest:
                case QuestRewardType.Unset:
                default:
                    return null;
            }
        }

        public static string GetLureIcon(this PokestopData pokestop, WhConfig whConfig, string style)
        {
            var iconStyleUrl = whConfig.IconStyles[style];
            var url = iconStyleUrl.EndsWith('/') ? iconStyleUrl + "rewards" : $"{iconStyleUrl}/rewards";
            return $"{url}/reward_{Convert.ToInt32(pokestop.LureType)}_1.png";
        }

        public static string GetInvasionIcon(this PokestopData pokestop, WhConfig whConfig, string style)
        {
            var iconStyleUrl = whConfig.IconStyles[style];
            var url = iconStyleUrl.EndsWith('/') ? iconStyleUrl + "grunt/" : $"{iconStyleUrl}/grunt";
            return $"{url}/{Convert.ToInt32(pokestop.GruntType)}.png";
        }

        public static string GetWeatherIcon(this WeatherData weather, WhConfig whConfig, string style)
        {
            var iconStyleUrl = whConfig.IconStyles[style];
            var url = iconStyleUrl.EndsWith('/') ? iconStyleUrl + "weather/" : $"{iconStyleUrl}/weather";
            return $"{url}/weather_{Convert.ToInt32(weather.GameplayCondition)}.png";
        }

        public static string GetPokemonGenderIcon(this PokemonGender gender)
        {
            switch (gender)
            {
                case PokemonGender.Male:
                    return "-m"; //♂ \u2642
                case PokemonGender.Female:
                    return "-f"; //♀ \u2640
                default:
                    return ""; //⚲
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
                if (!MasterFile.Instance.Emojis.ContainsKey(emojiKey))
                    continue;

                var emojiId = MasterFile.Instance.Emojis[emojiKey];
                var emojiName = emojiId > 0 ? string.Format(Strings.TypeEmojiSchema, type.ToString().ToLower(), emojiId) : type.ToString();
                if (!list.Contains(emojiName))
                {
                    list.Add(emojiName);
                }
            }

            return string.Join(" ", list);
        }

        public static string GetWeatherEmojiIcon(this WeatherType weather)
        {
            var key = $"weather_" + Convert.ToInt32(weather);
            var emojiId = MasterFile.Instance.Emojis[key];
            var emojiName = emojiId > 0 ? string.Format(Strings.EmojiSchema, key, emojiId) : weather.ToString();
            return emojiName;
        }

        public static string GetCaptureRateEmojiIcon(this CaptureRateType type)
        {
            var key = $"capture_" + Convert.ToInt32(type);
            var emojiId = MasterFile.Instance.Emojis[key];
            var emojiName = emojiId > 0 ? string.Format(Strings.EmojiSchema, key, emojiId) : type.ToString();
            return emojiName;
        }

        public static string GetWeaknessEmojiIcons(this List<PokemonType> pokemonTypes)
        {
            if (pokemonTypes == null || pokemonTypes?.Count == 0)
                return string.Empty;

            var list = new List<string>();
            foreach (var type in pokemonTypes)
            {
                var weaknessLst = type.ToString().StringToObject<PokemonType>().GetWeaknesses().Distinct();
                foreach (var weakness in weaknessLst)
                {
                    var emojiId = MasterFile.Instance.Emojis[$"types_{weakness.ToString().ToLower()}"];
                    var emojiName = emojiId > 0 ? string.Format(Strings.TypeEmojiSchema, weakness.ToString().ToLower(), emojiId) : weakness.ToString();
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