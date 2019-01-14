namespace WhMgr.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using DSharpPlus;

    using WhMgr.Data;
    using WhMgr.Diagnostics;
    using WhMgr.Net.Models;

    public static class PokemonExtensions
    {
        private const string Alolan = "Alolan";

        private static readonly IEventLogger _logger = EventLogger.GetLogger();

        public static readonly double[] CpMultipliers =
        {
            0.094, 0.16639787, 0.21573247, 0.25572005, 0.29024988,
            0.3210876, 0.34921268, 0.37523559, 0.39956728, 0.42250001,
            0.44310755, 0.46279839, 0.48168495, 0.49985844, 0.51739395,
            0.53435433, 0.55079269, 0.56675452, 0.58227891, 0.59740001,
            0.61215729, 0.62656713, 0.64065295, 0.65443563, 0.667934,
            0.68116492, 0.69414365, 0.70688421, 0.71939909, 0.7317,
            0.73776948, 0.74378943, 0.74976104, 0.75568551, 0.76156384,
            0.76739717, 0.7731865, 0.77893275, 0.78463697, 0.79030001
        };
        /**Game Master
- 0.094,
- 0.16639787,
- 0.21573247,
- 0.255720049,
- 0.290249884,
- 0.321087599,
- 0.349212676,
- 0.375235587,
- 0.399567276,
- 0.4225,
- 0.443107545,
- 0.462798387,
- 0.481684953,
- 0.499858439,
- 0.517393947,
- 0.534354329,
- 0.550792694,
- 0.56675452,
- 0.582278907,
- 0.5974,
- 0.612157285,
- 0.626567125,
- 0.640652955,
- 0.654435635,
- 0.667934,
- 0.68116492,
- 0.694143653,
- 0.706884205,
- 0.719399095,
- 0.7317,
- 0.737769485,
- 0.743789434,
- 0.749761045,
- 0.755685508,
- 0.761563838,
- 0.767397165,
- 0.773186505,
- 0.77893275,
- 0.784637,
- 0.7903
         */

        public static int MaxCpAtLevel(this int id, int level)
        {
            if (!Database.Instance.Pokemon.ContainsKey(id))
                return 0;

            var pkmn = Database.Instance.Pokemon[id];

            var multiplier = CpMultipliers[level - 1];
            var maxAtk = (pkmn.BaseStats.Attack + 15) * multiplier;
            var maxDef = (pkmn.BaseStats.Defense + 15) * multiplier;
            var maxSta = (pkmn.BaseStats.Stamina + 15) * multiplier;

            return (int)Math.Max(10, Math.Floor(Math.Sqrt(maxAtk * maxAtk * maxDef * maxSta) / 10));
        }

        public static int GetLevel(this int id, int cp)
        {
            if (!Database.Instance.Pokemon.ContainsKey(id))
                return 0;

            var pkmn = Database.Instance.Pokemon[id];
            for (var i = 0; i < CpMultipliers.Length; i++)
            {
                var spawnCP = GetCP(pkmn.BaseStats.Attack + 15, pkmn.BaseStats.Defense + 15, pkmn.BaseStats.Stamina + 15, CpMultipliers[i]);
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
            if (!Database.Instance.Pokemon.ContainsKey(id))
                return PokemonSize.Normal;

            var stats = Database.Instance.Pokemon[id];
            var weightRatio = weight / (float)stats.BaseStats.Weight;
            var heightRatio = height / (float)stats.BaseStats.Height;
            var size = heightRatio + weightRatio;

            if (size < 1.5)   return PokemonSize.Tiny;
            if (size <= 1.75) return PokemonSize.Small;
            if (size < 2.25)  return PokemonSize.Normal;
            if (size <= 2.5)  return PokemonSize.Large;
            return PokemonSize.Big;
        }

        public static string GetPokemonForm(this int pokeId, string formId)
        {
            if (!int.TryParse(formId, out int form))
                return null;

            if (form == 0)
                return null;

            switch (pokeId)
            {
                case 7: //Squirtle
                    switch (form)
                    {
                        case 5: //Glasses
                            return "Glasses";
                    }
                    break;
                case 19: //Rattata
                    switch (form)
                    {
                        case 45: //Normal
                            break;
                        case 46: //Alolan
                            return Alolan;
                    }
                    break;
                case 20: //Raticate
                    switch (form)
                    {
                        case 47: //Normal
                            break;
                        case 48: //Alolan
                            return Alolan;
                    }
                    break;
                case 25: //Pikachu
                    switch (form)
                    {
                        case 1: //X-Mas Hat/Christmas
                            return "Christmas";
                        case 2: //Party Hat/Birthday
                            return "Birthday";
                        case 3: //Ash Hat
                            return "Ash Hat";
                        case 4: //Witch Hat/Halloween
                            return "Halloween";
                        case 5: //Straw Hat/Sun Glasses
                            return "Straw Hat";
                        case 6: //FM/
                            return "FM";
                    }
                    break;
                case 26: //Raichu
                    switch (form)
                    {
                        case 1: //X-Mas Hat/Christmas
                            return "Christmas";
                        case 2: //Party Hat/Birthday
                            return "Birthday";
                        case 3: //Ash Hat
                            return "Ash Hat";
                        case 4: //Witch Hat/Halloween
                            return "Halloween";
                        case 5: //Straw Hat/Sun Glasses
                            return "Straw Hat";
                        case 6: //FM/
                            return "FM";
                        case 49: //Normal
                            break;
                        case 50:
                            return Alolan;
                    }
                    break;
                case 27: //Sandshrew
                    switch (form)
                    {
                        case 51: //Normal
                            break;
                        case 52: //Alolan
                            return Alolan;
                    }
                    break;
                case 28: //Sandslash
                    switch (form)
                    {
                        case 53: //Normal
                            break;
                        case 54: //Alolan
                            return Alolan;
                    }
                    break;
                case 37: //Vulpix
                    switch (form)
                    {
                        case 55: //Normal
                            break;
                        case 56: //Alolan
                            return Alolan;
                    }
                    break;
                case 38: //Ninetales
                    switch (form)
                    {
                        case 57: //Normal
                            break;
                        case 58: //Alolan
                            return Alolan;
                    }
                    break;
                case 50: //Diglett
                    switch (form)
                    {
                        case 59: //Normal
                            break;
                        case 60: //Alolan
                            return Alolan;
                    }
                    break;
                case 51: //Dugtrio
                    switch (form)
                    {
                        case 61: //Normal
                            break;
                        case 62: //Alolan
                            return Alolan;
                    }
                    break;
                case 52: //Meowth
                    switch (form)
                    {
                        case 63: //Normal
                            break;
                        case 64: //Alolan
                            return Alolan;
                    }
                    break;
                case 53: //Persian
                    switch (form)
                    {
                        case 65: //Normal
                            break;
                        case 66: //Alolan
                            return Alolan;
                    }
                    break;
                case 74: //Geodude
                    switch (form)
                    {
                        case 67: //Normal
                            break;
                        case 68: //Alolan
                            return Alolan;
                    }
                    break;
                case 75: //Graveler
                    switch (form)
                    {
                        case 69: //Normal
                            break;
                        case 70: //Alolan
                            return Alolan;
                    }
                    break;
                case 76: //Golem
                    switch (form)
                    {
                        case 71: //Normal
                            break;
                        case 72: //Alolan
                            return Alolan;
                    }
                    break;
                case 88: //Grimer
                    switch (form)
                    {
                        case 73: //Normal
                            break;
                        case 74: //Alolan
                            return Alolan;
                    }
                    break;
                case 89: //Muk
                    switch (form)
                    {
                        case 75: //Normal
                            break;
                        case 76: //Alolan
                            return Alolan;
                    }
                    break;
                case 103: //Exeggutor
                    switch (form)
                    {
                        case 77: //Normal
                            break;
                        case 78: //Alolan
                            return Alolan;
                    }
                    break;
                case 105: //Marowak
                    switch (form)
                    {
                        case 79: //Normal
                            break;
                        case 80: //Alolan
                            return Alolan;
                    }
                    break;
                case 172: //Pichu
                    switch (form)
                    {
                        case 1: //X-Mas Hat/Christmas
                            return "Christmas";
                        case 2: //Party Hat/Birthday
                            return "Birthday";
                        case 3: //Ash Hat
                            return "Ash Hat";
                        case 4: //Witch Hat/Halloween
                            return "Halloween";
                        case 5: //Straw Hat/Sun Glasses
                            return "Straw Hat";
                        case 6: //FM/
                            return "FM Hat";
                    }
                    break;
                case 201: //Unown
                    switch (form)
                    {
                        case 27:
                            return "!";
                        case 28:
                            return "?";
                        default:
                            return form.NumberToAlphabet().ToString();
                    }
                case 327: //Spinda
                    switch (form)
                    {
                        case 37:
                            return "01";
                        case 38:
                            return "02";
                        case 39:
                            return "03";
                        case 40:
                            return "04";
                        case 41:
                            return "05";
                        case 42:
                            return "06";
                        case 43:
                            return "07";
                        case 44:
                            return "08";
                    }
                    break;
                case 351: //Castform
                    switch (form)
                    {
                        case 29: //Normal
                            break;
                        case 30: //Sunny
                            return "Sunny";
                        case 31: //Water
                            return "Rainy";
                        case 32: //Snow
                            return "Snowy";
                    }
                    break;
                case 386: //Deoxys
                    switch (form)
                    {
                        case 33: //Normal
                            break;
                        case 34: //Attack
                            return "Attack";
                        case 35: //Defense
                            return "Defense";
                        case 36: //Speed
                            return "Speed";
                    }
                    break;
                case 413: //Wormadam
                    switch (form)
                    {
                        case 87: //Plant
                            return "Plant";
                        case 88: //Sandy
                            return "Sandy";
                        case 89: //Trash
                            return "Trash";
                    }
                    break;
                case 421: //Cherrim
                    switch (form)
                    {
                        case 94: //Overcast
                            return "Overcast";
                        case 95: //Sunny
                            return "Sunny";
                    }
                    break;
                case 422: //Shellos
                    switch (form)
                    {
                        case 96:
                            return "West Sea";
                        case 97:
                            return "East Sea";
                    }
                    break;
                case 423: //Gastrodon
                    switch (form)
                    {
                        case 98:
                            return "West Sea";
                        case 99:
                            return "East Sea";
                    }
                    break;
                case 479: //Rotom
                    switch (form)
                    {
                        case 82: //Normal
                            break;
                        case 83: //Frost
                            return "Frost";
                        case 84: //Fan
                            return "Fan";
                        case 85: //Mow
                            return "Mow";
                        case 86: //Wash
                            return "Wash";
                        case 87: //Heat
                            return "Heat";
                    }
                    break;
                case 487: //Giratina
                    switch (form)
                    {
                        case 90: //Altered
                            return "Altered";
                        case 91: //Origin
                            return "Origin";
                    }
                    break;
                case 492: //Shaymin
                    switch (form)
                    {
                        case 92: //Sky
                            return "Sky";
                        case 93: //Land
                            return "Land";
                    }
                    break;
                case 493: //Arceus
                    switch (form)
                    {
                        case 100: //Normal
                            break;
                        case 101: //Fighting
                            return "Fighting";
                        case 102: //Flying
                            return "Flying";
                        case 103: //Poison
                            return "Poison";
                        case 104: //Ground
                            return "Ground";
                        case 105: //Rock
                            return "Rock";
                        case 106: //Bug
                            return "Bug";
                        case 107: //Ghost
                            return "Ghost";
                        case 108: //Steel
                            return "Steel";
                        case 109: //Fire
                            return "Fire";
                        case 110: //Water
                            return "Water";
                        case 111: //Grass
                            return "Grass";
                        case 112: //Electric
                            return "Electric";
                        case 113: //Psychic
                            return "Psychic";
                        case 114: //Ice
                            return "Ice";
                        case 115: //Dragon
                            return "Dragon";
                        case 116: //Dark
                            return "Dark";
                        case 117: //Fairy
                            return "Fairy";
                    }
                    break;
            }

            return null;
        }

        public static string GetPokemonImage(this int pokemonId, PokemonGender gender, string form)
        {
            if (int.TryParse(form, out var formId))
            {
                return string.Format(Strings.PokemonImage, pokemonId, formId);
            }

            var genderId = gender == PokemonGender.Female ? 1 : 0;
            return string.Format(Strings.PokemonImage, pokemonId, genderId);
        }

        //public static string GetPokemonImage(this int pokemonId, PokemonGender gender, string form, bool shiny)
        //{
        //    var isShiny = shiny ? "_shiny" : null;
        //    var formTag = int.TryParse(form, out var formId) && formId > 0 ? "_" + string.Format("{0:D2}", formId) : null;
        //    var genderId = (int)gender > 1 ? 0 : (int)gender;
        //    var url = string.Format(Strings.PokemonImage, pokemonId, genderId, formTag, isShiny);
        //    if (IsUrlExist(url))
        //    {
        //        return url;
        //    }

        //    url = string.Format(Strings.PokemonImage, pokemonId, 0, int.TryParse(form, out formId) ? "_" + string.Format("{0:D2}", formId) : null, isShiny);
        //    if (IsUrlExist(url))
        //    {
        //        return url;
        //    }

        //    url = string.Format(Strings.PokemonImage, pokemonId, form, isShiny, string.Empty);
        //    if (int.TryParse(form, out formId))
        //    {
        //        return url;
        //    }

        //    return string.Format(Strings.PokemonImage, pokemonId, genderId, isShiny, string.Empty);
        //}

        public static string GetPokemonGenderIcon(this PokemonGender gender)
        {
            switch (gender)
            {
                case PokemonGender.Male:
                    return "♂";//\u2642
                case PokemonGender.Female:
                    return "♀";//\u2640
                default:
                    return "⚲";//?
            }
        }

        public static List<PokemonType> GetStrengths(this PokemonType type)
        {
            var types = new PokemonType[0];
            switch (type)
            {
                case PokemonType.Normal:
                    break;
                case PokemonType.Fighting:
                    types = new PokemonType[] { PokemonType.Normal, PokemonType.Rock, PokemonType.Steel, PokemonType.Ice, PokemonType.Dark };
                    break;
                case PokemonType.Flying:
                    types = new PokemonType[] { PokemonType.Fighting, PokemonType.Bug, PokemonType.Grass };
                    break;
                case PokemonType.Poison:
                    types = new PokemonType[] { PokemonType.Grass, PokemonType.Fairy };
                    break;
                case PokemonType.Ground:
                    types = new PokemonType[] { PokemonType.Poison, PokemonType.Rock, PokemonType.Steel, PokemonType.Fire, PokemonType.Electric };
                    break;
                case PokemonType.Rock:
                    types = new PokemonType[] { PokemonType.Flying, PokemonType.Bug, PokemonType.Fire, PokemonType.Ice };
                    break;
                case PokemonType.Bug:
                    types = new PokemonType[] { PokemonType.Grass, PokemonType.Psychic, PokemonType.Dark };
                    break;
                case PokemonType.Ghost:
                    types = new PokemonType[] { PokemonType.Ghost, PokemonType.Psychic };
                    break;
                case PokemonType.Steel:
                    types = new PokemonType[] { PokemonType.Rock, PokemonType.Ice };
                    break;
                case PokemonType.Fire:
                    types = new PokemonType[] { PokemonType.Bug, PokemonType.Steel, PokemonType.Grass, PokemonType.Ice };
                    break;
                case PokemonType.Water:
                    types = new PokemonType[] { PokemonType.Ground, PokemonType.Rock, PokemonType.Fire };
                    break;
                case PokemonType.Grass:
                    types = new PokemonType[] { PokemonType.Ground, PokemonType.Rock, PokemonType.Water };
                    break;
                case PokemonType.Electric:
                    types = new PokemonType[] { PokemonType.Flying, PokemonType.Water };
                    break;
                case PokemonType.Psychic:
                    types = new PokemonType[] { PokemonType.Fighting, PokemonType.Poison };
                    break;
                case PokemonType.Ice:
                    types = new PokemonType[] { PokemonType.Flying, PokemonType.Ground, PokemonType.Grass, PokemonType.Dragon };
                    break;
                case PokemonType.Dragon:
                    types = new PokemonType[] { PokemonType.Dragon };
                    break;
                case PokemonType.Dark:
                    types = new PokemonType[] { PokemonType.Ghost, PokemonType.Psychic };
                    break;
                case PokemonType.Fairy:
                    types = new PokemonType[] { PokemonType.Fighting, PokemonType.Dragon, PokemonType.Dark };
                    break;
            }
            return types.ToList();
        }

        public static List<PokemonType> GetWeaknesses(this PokemonType type)
        {
            var types = new PokemonType[0];
            switch (type)
            {
                case PokemonType.Normal:
                    types = new PokemonType[] { PokemonType.Fighting };
                    break;
                case PokemonType.Fighting:
                    types = new PokemonType[] { PokemonType.Flying, PokemonType.Psychic, PokemonType.Fairy };
                    break;
                case PokemonType.Flying:
                    types = new PokemonType[] { PokemonType.Rock, PokemonType.Electric, PokemonType.Ice };
                    break;
                case PokemonType.Poison:
                    types = new PokemonType[] { PokemonType.Ground, PokemonType.Psychic };
                    break;
                case PokemonType.Ground:
                    types = new PokemonType[] { PokemonType.Water, PokemonType.Grass, PokemonType.Ice };
                    break;
                case PokemonType.Rock:
                    types = new PokemonType[] { PokemonType.Fighting, PokemonType.Ground, PokemonType.Steel, PokemonType.Water, PokemonType.Grass };
                    break;
                case PokemonType.Bug:
                    types = new PokemonType[] { PokemonType.Flying, PokemonType.Rock, PokemonType.Fire };
                    break;
                case PokemonType.Ghost:
                    types = new PokemonType[] { PokemonType.Ghost, PokemonType.Dark };
                    break;
                case PokemonType.Steel:
                    types = new PokemonType[] { PokemonType.Fighting, PokemonType.Ground, PokemonType.Fire };
                    break;
                case PokemonType.Fire:
                    types = new PokemonType[] { PokemonType.Ground, PokemonType.Rock, PokemonType.Water };
                    break;
                case PokemonType.Water:
                    types = new PokemonType[] { PokemonType.Grass, PokemonType.Electric };
                    break;
                case PokemonType.Grass:
                    types = new PokemonType[] { PokemonType.Flying, PokemonType.Poison, PokemonType.Bug, PokemonType.Fire, PokemonType.Ice };
                    break;
                case PokemonType.Electric:
                    types = new PokemonType[] { PokemonType.Ground };
                    break;
                case PokemonType.Psychic:
                    types = new PokemonType[] { PokemonType.Bug, PokemonType.Ghost, PokemonType.Dark };
                    break;
                case PokemonType.Ice:
                    types = new PokemonType[] { PokemonType.Fighting, PokemonType.Rock, PokemonType.Steel, PokemonType.Fire };
                    break;
                case PokemonType.Dragon:
                    types = new PokemonType[] { PokemonType.Ice, PokemonType.Dragon, PokemonType.Fairy };
                    break;
                case PokemonType.Dark:
                    types = new PokemonType[] { PokemonType.Fighting, PokemonType.Bug, PokemonType.Fairy };
                    break;
                case PokemonType.Fairy:
                    types = new PokemonType[] { PokemonType.Poison, PokemonType.Steel };
                    break;
            }
            return types.ToList();
        }

        public static string GetTypeEmojiIcons(this List<PokemonType> pokemonTypes, DiscordClient client, ulong guildId)
        {
            var list = new List<string>();
            foreach (var type in pokemonTypes)
            {
                if (!client.Guilds.ContainsKey(guildId))
                    continue;

                var emojiId = client.Guilds[guildId].GetEmojiId($"types_{type.ToString().ToLower()}");
                var emojiName = emojiId > 0 ? string.Format(Strings.TypeEmojiSchema, type.ToString().ToLower(), emojiId) : type.ToString();
                if (!list.Contains(emojiName))
                {
                    list.Add(emojiName);
                }
            }

            return string.Join(" ", list);
        }

        public static string GetWeaknessEmojiIcons(this List<PokemonType> pokemonTypes, DiscordClient client, ulong guildId)
        {
            var list = new List<string>();
            foreach (var type in pokemonTypes)
            {
                var weaknessLst = type.ToString().StringToObject<PokemonType>().GetWeaknesses().Distinct();
                foreach (var weakness in weaknessLst)
                {
                    if (!client.Guilds.ContainsKey(guildId))
                        continue;

                    var emojiId = client.Guilds[guildId].GetEmojiId($"types_{weakness.ToString().ToLower()}");
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

            var db = Database.Instance;
            var pkmn = db.Pokemon.FirstOrDefault(x => string.Compare(x.Value.Name, name, true) == 0);

            if (pkmn.Key > 0)
                return pkmn.Key;

            foreach (var p in db.Pokemon)
                if (p.Value.Name.ToLower().Contains(name.ToLower()))
                    return p.Key;

            if (!int.TryParse(name, out var pokeId))
                return 0;

            if (db.Pokemon.ContainsKey(pokeId))
                return pokeId;

            return 0;
        }

        private static bool UrlExist(string url)
        {
            try
            {
                //Creating the HttpWebRequest
                var request = WebRequest.Create(url) as HttpWebRequest;

                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";

                //Getting the Web Response.
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    //Returns TRUE if the Status code == 200
                    response.Close();

                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }

        public static PokemonValidation ValidatePokemon(this IEnumerable<string> pokemon)
        {
            var valid = new List<int>();
            var invalid = new List<string>();
            foreach (var poke in pokemon)
            {
                var pokeId = poke.PokemonIdFromName();
                if (pokeId == 0)
                {
                    invalid.Add(poke);
                    continue;
                }

                if (!Database.Instance.Pokemon.ContainsKey(pokeId))
                    continue;

                valid.Add(pokeId);
            }

            return new PokemonValidation { Valid = valid, Invalid = invalid };
        }
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
}