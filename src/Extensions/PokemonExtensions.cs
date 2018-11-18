namespace WhMgr.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using WhMgr.Data;
    using WhMgr.Net.Models;

    public static class PokemonExtensions
    {
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

            switch (pokeId)
            {
                case 201: //Unown
                    switch (form)
                    {
                        case 27:
                            return "!";
                        case 28:
                            return "?";
                        default:
                            return form.NumberToAlphabet(true).ToString();
                    }
                case 351: //Castform
                    switch (form)
                    {
                        case 29: //Normal
                            break;
                        case 30: //Sunny
                            return "Sunny";
                        case 31: //Water
                            return "Rain";
                        case 32: //Snow
                            return "Snow";
                    }
                    break;
                case 327: //Spinda
                case 386: //Deoxys
                    return "N/A";
            }

            return null;
        }

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
    }
}