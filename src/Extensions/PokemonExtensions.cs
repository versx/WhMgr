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

        public static string GetSize(this int id, float height, float weight)
        {
            if (!Database.Instance.Pokemon.ContainsKey(id))
                return string.Empty;

            var stats = Database.Instance.Pokemon[id];
            var weightRatio = weight / (float)stats.BaseStats.Weight;
            var heightRatio = height / (float)stats.BaseStats.Height;
            var size = heightRatio + weightRatio;

            if (size < 1.5) return "Tiny";
            if (size <= 1.75) return "Small";
            if (size < 2.25) return "Normal";
            if (size <= 2.5) return "Large";
            return "Big";
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
                case 20: //Raticate
                case 27: //Sandshrew
                    //switch (form)
                    //{
                    //    case 51:
                    //        return "Alola";
                    //}
                    //break;
                case 28: //Sandslash
                case 37: //Vulpix
                case 38: //Ninetales
                case 50: //Diglett
                case 51: //Dugtrio
                case 52: //Meowth
                case 53: //Persian
                case 88: //Grimer
                case 89: //Muk
                case 103: //Exeggutor
                case 105: //Marowak
                    switch (form)
                    {
                        case 49:
                        case 50:
                        case 51:
                            //53 sandslash normal?
                        case 55:
                        case 61:
                        case 63:
                        case 67:
                        case 78:
                        case 80:
                            return "Alola";
                    }
                    break;
                case 74: //Geodude
                case 75: //Graveler
                case 76: //Golem
                    switch (form)
                    {
                        case 67:
                            return "Alola";
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
                        case 50:
                        case 61:
                        case 78:
                        case 80:
                            return "Alola";
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
                case 351: //Castform
                    switch (form)
                    {
                        case 29: //Normal //GM is 11
                            break;
                        case 30: //Sunny //GM is 12
                            return "Sunny";
                        case 31: //Water //GM is 13
                            return "Rainy";
                        case 32: //Snow //GM is 14
                            return "Snowy";
                    }
                    break;
                case 327: //Spinda
                    switch (form)
                    {
                        case 11:
                            return "00";
                        case 12:
                            return "01";
                        case 13:
                            return "02";
                        case 14:
                            return "03";
                        case 15:
                            return "04";
                        case 16:
                            return "05";
                        case 17:
                            return "06";
                        case 18:
                            return "07";
                    }
                    break;
                case 386: //Deoxys
                    switch (form)
                    {
                        case 11: //Normal
                            break;
                        case 12: //Attack
                            return "Attack";
                        case 13: //Defense
                            return "Defense";
                        case 14: //Speed
                            return "Speed";
                    }
                    break;
                case 413: //Wormadam
                    switch (form)
                    {
                        case 11: //87
                            break;
                        case 12: //88
                            break;
                        case 13: //89
                            break;
                    }
                    break;
                case 421: //Cherrim
                    switch (form)
                    {
                        case 11: //94
                            break;
                        case 12: //95
                            break;
                    }
                    break;
                case 422: //Shellos
                    switch (form)
                    {
                        case 11: //96
                            break;
                        case 12: //97
                            break;
                    }
                    break;
                case 423: //Gastrodon
                    switch (form)
                    {
                        case 11: //98
                            break;
                        case 12: //99
                            break;
                    }
                    break;
                case 479: //Rotom
                    switch (form)
                    {
                        case 11: //81
                            break;
                        case 12: //82
                            break;
                        case 13: //83
                            break;
                        case 14: //84
                            break;
                        case 15: //85
                            break;
                        case 16: //86
                            break;
                    }
                    break;
                case 487: //Giratina
                    switch (form)
                    {
                        case 11:
                            return "Altered";
                        case 12:
                            return "Origin";
                    }
                    break;
                case 492: //Shaymin
                    switch (form)
                    {
                        case 11: //93
                            break;
                        case 12: //92
                            break;
                    }
                    break;
                case 493: //Arceus
                    switch (form)
                    {
                        case 11:
                            return "Normal";
                        case 12:
                            return "Fighting";
                        case 13:
                            return "Flying";
                        case 14:
                            return "Poison";
                        case 15:
                            return "Ground";
                        case 16:
                            return "Rock";
                        case 17:
                            return "Bug";
                        case 18:
                            return "Ghost";
                        case 19:
                            return "Steel";
                        case 20:
                            return "Fire";
                        case 21:
                            return "Water";
                        case 22:
                            return "Grass";
                        case 23:
                            return "Electric";
                        case 24:
                            return "Psychic";
                        case 25:
                            return "Ice";
                        case 26:
                            return "Dragon";
                        case 27:
                            return "Dark";
                        case 28:
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
                var emojiName = emojiId > 0 ? string.Format(Strings.TypeEmojiSchema, type.ToString().ToLower(), emojiId) : type.ToString().ToLower();
                if (!list.Contains(emojiName))
                {
                    list.Add(emojiName);
                }
            }

            return string.Join("/", list);
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

        private static bool IsUrlExist(string url)
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
    }
}