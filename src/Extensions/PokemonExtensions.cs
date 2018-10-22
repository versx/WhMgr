namespace T.Extensions
{
    using System;
    using System.Collections.Generic;

    using T.Data;
    using T.Net;

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
            var cpMulti = CalcIV.CpMultipliers[level - 1];

            int minCp = CalcIV.GetCP(baseAtk + 10, baseDef + 10, baseSta + 10, cpMulti);
            int maxCp = CalcIV.GetCP(baseAtk + 15, baseDef + 15, baseSta + 15, cpMulti);

            return new CpRange(maxCp, minCp);
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

        public static List<string> GetStrengths(string type)
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

        public static List<string> GetWeaknesses(string type)
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
    }
}