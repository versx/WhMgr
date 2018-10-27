namespace WhMgr.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using WhMgr.Data;
    using WhMgr.Net.Models;

    public class CpRange
    {
        public int Best { get; set; }

        public int Worst { get; set; }

        public CpRange(int best, int worst)
        {
            Best = best;
            Worst = worst;
        }
    }

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

        public static CpRange GetPokemonCpRange(this int pokeId, int level)
        {
            var db = Database.Instance;
            if (!db.Pokemon.ContainsKey(pokeId))
                return null;

            var baseStats = db.Pokemon[pokeId].BaseStats;
            if (baseStats == null)
                return null;

            var baseAtk = baseStats.Attack;
            var baseDef = baseStats.Defense;
            var baseSta = baseStats.Stamina;
            var cpMulti = CpMultipliers[level - 1];

            var min = 10;
            var max = 10;
            var minCp = GetCP(baseAtk + min, baseDef + min, baseSta + min, cpMulti);
            var maxCp = GetCP(baseAtk + max, baseDef + max, baseSta + max, cpMulti);

            return new CpRange(maxCp, minCp);
        }

        public static int GetCP(int attack, int defense, int stamina, double cpm)
        {
            var cp = Math.Floor(attack * Math.Pow(defense, 0.5) * Math.Pow(stamina, 0.5) * Math.Pow(cpm, 2) / 10);
            return Convert.ToInt32(cp < 10 ? 10 : cp);
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

        public static List<string> GetStrengths(this string type)
        {
            var types = new string[0];
            switch (type.ToLower())
            {
                case "normal":
                    break;
                case "fighting":
                    types = new string[] { "Normal", "Rock", "Steel", "Ice", "Dark" };
                    break;
                case "flying":
                    types = new string[] { "Fighting", "Bug", "Grass" };
                    break;
                case "poison":
                    types = new string[] { "Grass", "Fairy" };
                    break;
                case "ground":
                    types = new string[] { "Poison", "Rock", "Steel", "Fire", "Electric" };
                    break;
                case "rock":
                    types = new string[] { "Flying", "Bug", "Fire", "Ice" };
                    break;
                case "bug":
                    types = new string[] { "Grass", "Psychic", "Dark" };
                    break;
                case "ghost":
                    types = new string[] { "Ghost", "Psychic" };
                    break;
                case "steel":
                    types = new string[] { "Rock", "Ice" };
                    break;
                case "fire":
                    types = new string[] { "Bug", "Steel", "Grass", "Ice" };
                    break;
                case "water":
                    types = new string[] { "Ground", "Rock", "Fire" };
                    break;
                case "grass":
                    types = new string[] { "Ground", "Rock", "Water" };
                    break;
                case "electric":
                    types = new string[] { "Flying", "Water" };
                    break;
                case "psychic":
                    types = new string[] { "Fighting", "Poison" };
                    break;
                case "ice":
                    types = new string[] { "Flying", "Ground", "Grass", "Dragon" };
                    break;
                case "dragon":
                    types = new string[] { "Dragon" };
                    break;
                case "dark":
                    types = new string[] { "Ghost", "Psychic" };
                    break;
                case "fairy":
                    types = new string[] { "Fighting", "Dragon", "Dark" };
                    break;
            }
            return new List<string>(types);
        }

        public static List<string> GetWeaknesses(this string type)
        {
            var types = new string[0];
            switch (type.ToLower())
            {
                case "normal":
                    types = new string[] { "Fighting" };
                    break;
                case "fighting":
                    types = new string[] { "Flying", "Psychic", "Fairy" };
                    break;
                case "flying":
                    types = new string[] { "Rock", "Electric", "Ice" };
                    break;
                case "poison":
                    types = new string[] { "Ground", "Psychic" };
                    break;
                case "ground":
                    types = new string[] { "Water", "Grass", "Ice" };
                    break;
                case "rock":
                    types = new string[] { "Fighting", "Ground", "Steel", "Water", "Grass" };
                    break;
                case "bug":
                    types = new string[] { "Flying", "Rock", "Fire" };
                    break;
                case "ghost":
                    types = new string[] { "Ghost", "Dark" };
                    break;
                case "steel":
                    types = new string[] { "Fighting", "Ground", "Fire" };
                    break;
                case "fire":
                    types = new string[] { "Ground", "Rock", "Water" };
                    break;
                case "water":
                    types = new string[] { "Grass", "Electric" };
                    break;
                case "grass":
                    types = new string[] { "Flying", "Poison", "Bug", "Fire", "Ice" };
                    break;
                case "electric":
                    types = new string[] { "Ground" };
                    break;
                case "psychic":
                    types = new string[] { "Bug", "Ghost", "Dark" };
                    break;
                case "ice":
                    types = new string[] { "Fighting", "Rock", "Steel", "Fire" };
                    break;
                case "dragon":
                    types = new string[] { "Ice", "Dragon", "Fairy" };
                    break;
                case "dark":
                    types = new string[] { "Fighting", "Bug", "Fairy" };
                    break;
                case "fairy":
                    types = new string[] { "Poison", "Steel" };
                    break;
            }
            return new List<string>(types);
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