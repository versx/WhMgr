namespace WhMgr.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using WhMgr.Configuration;
    using WhMgr.Data;
    using WhMgr.Data.Models;
    using WhMgr.Net.Models;

    public static class PokemonExtensions
    {
        private const string Alolan = "Alolan";
        private const string Armored = "Armored";
        private const string Shadow = "Shadow";
        private const string Purified = "Purified";
        private const string Normal = "";
        private const string Glasses = "Glasses";
        private const string NoEvolve = "No-Evolve";
        private const string Anniversary = "Anniversary";
        private const string Christmas = "Christmas";
        private const string Birthday = "Birthday";
        private const string Halloween = "Halloween";
        private const string Sunny = "Sunny";
        private const string Overcast = "Overcast";
        private const string Rainy = "Rainy";
        private const string Snowy = "Snowy";
        private const string Attack = "Attack";
        private const string Defense = "Defense";
        private const string Speed = "Speed";
        private const string Plant = "Plant";
        private const string Sandy = "Sandy";
        private const string Trash = "Trash";
        private const string Altered = "Altered";
        private const string Origin = "Origin";
        private const string WestSea = "West Sea";
        private const string EastSea = "East Sea";

        private const string ExclaimationMark = "!";
        private const string QuestionMark = "?";

        //private static readonly IEventLogger _logger = EventLogger.GetLogger();

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

        public static string GetPokemonForm(this int pokeId, string formId)
        {
            // TODO: Localize
            if (!int.TryParse(formId, out int form))
                return null;

            if (form == 0)
                return null;

            switch (pokeId)
            {
                case 1: //Bulbasaur
                    switch (form)
                    {
                        case 163: //Normal
                            return Normal;
                        case 164: //Shadow
                            return Shadow;
                        case 165: //Purified
                            return Purified;
                        case 604: //No-Evolve
                            return NoEvolve;
                    }
                    break;
                case 2: //Ivysaur
                    switch (form)
                    {
                        case 166: //Normal
                            return Normal;
                        case 167: //Shadow
                            return Shadow;
                        case 168: //Purified
                            return Purified;
                    }
                    break;
                case 3: //Venasaur
                    switch (form)
                    {
                        case 169: //Normal
                            return Normal;
                        case 170: //Shadow
                            return Shadow;
                        case 171: //Purified
                            return Purified;
                    }
                    break;
                case 4: //Charmander
                    switch (form)
                    {
                        case 172: //Normal
                            return Normal;
                        case 173: //Shadow
                            return Shadow;
                        case 174: //Purified
                            return Purified;
                        case 605: //No-Evolve
                            return NoEvolve;
                    }
                    break;
                case 5: //Charmeleon
                    switch (form)
                    {
                        case 175: //Normal
                            return Normal;
                        case 176: //Shadow
                            return Shadow;
                        case 177: //Purified
                            return Purified;
                    }
                    break;
                case 6: //Charizard
                    switch (form)
                    {
                        case 178: //Normal
                            return Normal;
                        case 179: //Shadow
                            return Shadow;
                        case 180: //Purified
                            return Purified;
                        case 606: //No-Evolve
                            return NoEvolve;
                    }
                    break;
                case 7: //Squirtle
                    switch (form)
                    {
                        case 5: //Glasses
                            return Glasses;
                        case 181: //Normal
                            return Normal;
                        case 182: //Shadow
                            return Shadow;
                        case 183: //Purified
                            return Purified;
                        case 607: //No-Evolve
                            return NoEvolve;
                    }
                    break;
                case 8: //Wartortle
                    switch (form)
                    {
                        case 184: //Normal
                            return Normal;
                        case 185: //Shadow
                            return Shadow;
                        case 186: //Purified
                            return Purified;
                    }
                    break;
                case 9: //Blastoise
                    switch (form)
                    {
                        case 187: //Normal
                            return Normal;
                        case 188: //Shadow
                            return Shadow;
                        case 189: //Purified
                            return Purified;
                        case 608: //No-Evolve
                            return NoEvolve;
                    }
                    break;
                case 19: //Rattata
                    switch (form)
                    {
                        case 45: //Normal
                            return Normal;
                        case 46: //Alolan
                            return Alolan;
                        case 153: //Shadow
                            return Shadow;
                        case 154: //Purified
                            return Purified;
                    }
                    break;
                case 20: //Raticate
                    switch (form)
                    {
                        case 47: //Normal
                            return Normal;
                        case 48: //Alolan
                            return Alolan;
                        case 155: //Shadow
                            return Shadow;
                        case 156: //Purified
                            return Purified;
                        case 609: //No-Evolve
                            return NoEvolve;
                    }
                    break;
                case 25: //Pikachu
                    switch (form)
                    {
                        case 1: //X-Mas Hat/Christmas
                            return Christmas;
                        case 2: //Party Hat/Birthday
                            return Birthday;
                        case 3: //Ash Hat/Anniversary
                            return Anniversary;
                        case 4: //Witch Hat/Halloween
                            return Halloween;
                        case 5: //Straw Hat/Sun Glasses
                            return "Straw Hat";
                        case 6: //FM/
                            return "FM";
                        case 598: //Normal
                            return Normal;
                        case 599: //No-Evolve
                            return NoEvolve;
                        case 894: //Fall
                            return "Fall";
                    }
                    break;
                case 26: //Raichu
                    switch (form)
                    {
                        case 1: //X-Mas Hat/Christmas
                            return Christmas;
                        case 2: //Party Hat/Birthday
                            return Birthday;
                        case 3: //Ash Hat/Anniversary
                            return Anniversary;
                        case 4: //Witch Hat/Halloween
                            return Halloween;
                        case 5: //Straw Hat/Sun Glasses
                            return "Straw Hat";
                        case 6: //FM/
                            return "FM";
                        case 49: //Normal
                            return Normal;
                        case 50:
                            return Alolan;
                    }
                    break;
                case 27: //Sandshrew
                    switch (form)
                    {
                        case 51: //Normal
                            return Normal;
                        case 52: //Alolan
                            return Alolan;
                    }
                    break;
                case 28: //Sandslash
                    switch (form)
                    {
                        case 53: //Normal
                            return Normal;
                        case 54: //Alolan
                            return Alolan;
                    }
                    break;
                case 37: //Vulpix
                    switch (form)
                    {
                        case 55: //Normal
                            return Normal;
                        case 56: //Alolan
                            return Alolan;
                    }
                    break;
                case 38: //Ninetales
                    switch (form)
                    {
                        case 57: //Normal
                            return Normal;
                        case 58: //Alolan
                            return Alolan;
                    }
                    break;
                case 41:
                    switch (form)
                    {
                        case 157: //Normal
                            return Normal;
                        case 158: //Shadow
                            return Shadow;
                        case 159: //Purified
                            return Purified;
                    }
                    break;
                case 42:
                    switch (form)
                    {
                        case 160: //Normal
                            return Normal;
                        case 161: //Shadow
                            return Shadow;
                        case 162: //Purified
                            return Purified;
                    }
                    break;
                case 50: //Diglett
                    switch (form)
                    {
                        case 59: //Normal
                            return Normal;
                        case 60: //Alolan
                            return Alolan;
                    }
                    break;
                case 51: //Dugtrio
                    switch (form)
                    {
                        case 61: //Normal
                            return Normal;
                        case 62: //Alolan
                            return Alolan;
                    }
                    break;
                case 52: //Meowth
                    switch (form)
                    {
                        case 63: //Normal
                            return Normal;
                        case 64: //Alolan
                            return Alolan;
                    }
                    break;
                case 53: //Persian
                    switch (form)
                    {
                        case 65: //Normal
                            return Normal;
                        case 66: //Alolan
                            return Alolan;
                    }
                    break;
                case 58: //Growlithe
                    switch (form)
                    {
                        case 280: //Normal
                            return Normal;
                        case 281: //Shadow
                            return Shadow;
                        case 282: //Purified
                            return Purified;
                    }
                    break;
                case 74: //Geodude
                    switch (form)
                    {
                        case 67: //Normal
                            return Normal;
                        case 68: //Alolan
                            return Alolan;
                    }
                    break;
                case 75: //Graveler
                    switch (form)
                    {
                        case 69: //Normal
                            return Normal;
                        case 70: //Alolan
                            return Alolan;
                    }
                    break;
                case 76: //Golem
                    switch (form)
                    {
                        case 71: //Normal
                            return Normal;
                        case 72: //Alolan
                            return Alolan;
                    }
                    break;
                case 88: //Grimer
                    switch (form)
                    {
                        case 73: //Normal
                            return Normal;
                        case 74: //Alolan
                            return Alolan;
                    }
                    break;
                case 89: //Muk
                    switch (form)
                    {
                        case 75: //Normal
                            return Normal;
                        case 76: //Alolan
                            return Alolan;
                    }
                    break;
                case 103: //Exeggutor
                    switch (form)
                    {
                        case 77: //Normal
                            return Normal;
                        case 78: //Alolan
                            return Alolan;
                    }
                    break;
                case 105: //Marowak
                    switch (form)
                    {
                        case 79: //Normal
                            return Normal;
                        case 80: //Alolan
                            return Alolan;
                    }
                    break;
                case 115: //Kangaskhan
                    switch (form)
                    {
                        case 839: //Normal
                            return Normal;
                        case 840: //Shadow
                            return Shadow;
                        case 841: //Purified
                            return Purified;
                    }
                    break;
                case 127: //Pinsir
                    switch (form)
                    {
                        case 898: //Normal
                            return Normal;
                        case 899: //Shadow
                            return Shadow;
                        case 900: //Purified
                            return Purified;
                    }
                    break;
                case 143: //Snorlax
                    switch (form)
                    {
                        case 199: //Normal
                            return Normal;
                        case 200: //Shadow
                            return Shadow;
                        case 201: //Purified
                            return Purified;
                    }
                    break;
                case 147: //Dratini
                    switch (form)
                    {
                        case 190: //Normal
                            return Normal;
                        case 191: //Shadow
                            return Shadow;
                        case 192: //Purified
                            return Purified;
                    }
                    break;
                case 148: //Dragonair
                    switch (form)
                    {
                        case 193: //Normal
                            return Normal;
                        case 194: //Shadow
                            return Shadow;
                        case 195: //Purified
                            return Purified;
                    }
                    break;
                case 149: //Dragonite
                    switch (form)
                    {
                        case 196: //Normal
                            return Normal;
                        case 197: //Shadow
                            return Shadow;
                        case 198: //Purified
                            return Purified;
                    }
                    break;
                case 150: //Mewtwo
                    switch (form)
                    {
                        case 133: //Armored
                            return Armored;
                        case 135: //Normal
                            return Normal;
                    }
                    break;
                case 169: //Crobat
                    switch (form)
                    {
                        case 202: //Normal
                            return Normal;
                        case 203: //Shadow
                            return Shadow;
                        case 204: //Purified
                            return Purified;
                    }
                    break;
                case 172: //Pichu
                    switch (form)
                    {
                        case 1: //X-Mas Hat/Christmas
                            return Christmas;
                        case 2: //Party Hat/Birthday
                            return Birthday;
                        case 3: //Ash Hat/Anniversary
                            return Anniversary;
                        case 4: //Witch Hat/Halloween
                            return Halloween;
                        case 5: //Straw Hat/Sun Glasses
                            return "Straw Hat";
                        case 6: //FM/
                            return "FM Hat";
                    }
                    break;
                case 201: //Unown
                    switch (form)
                    {
                        case 27: //!
                            return ExclaimationMark;
                        case 28: //?
                            return QuestionMark;
                        default:
                            return form.NumberToAlphabet().ToString();
                    }
                case 202: //Wobbuffet
                    switch (form)
                    {
                        case 602: //Normal
                            return Normal;
                        case 603: //No-Evolve
                            return NoEvolve;
                    }
                    break;
                case 258: //Mudkip
                    switch (form)
                    {
                        case 205: //Normal
                            return Normal;
                        case 206: //Shadow
                            return Shadow;
                        case 207: //Purified
                            return Purified;
                    }
                    break;
                case 259: //Marshtomp
                    switch (form)
                    {
                        case 208: //Normal
                            return Normal;
                        case 209: //Shadow
                            return Shadow;
                        case 210: //Purified
                            return Purified;
                    }
                    break;
                case 260: //Swampert
                    switch (form)
                    {
                        case 211: //Normal
                            return Normal;
                        case 212: //Shadow
                            return Shadow;
                        case 213: //Purified
                            return Purified;
                    }
                    break;
                case 265: //Wurmple
                    switch (form)
                    {
                        case 600: //Normal
                            return Normal;
                        case 601: //No-Evolve
                            return NoEvolve;
                    }
                    break;
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
                            return Normal;
                        case 30: //Sunny
                            return Sunny;
                        case 31: //Water
                            return Rainy;
                        case 32: //Snow
                            return Snowy;
                    }
                    break;
                case 386: //Deoxys
                    switch (form)
                    {
                        case 33: //Normal
                            return Normal;
                        case 34: //Attack
                            return Attack;
                        case 35: //Defense
                            return Defense;
                        case 36: //Speed
                            return Speed;
                    }
                    break;
                case 412: //Burmy
                case 413: //Wormadam
                    switch (form)
                    {
                        case 87: //Plant
                        case 118:
                            return Plant;
                        case 88: //Sandy
                        case 119:
                            return Sandy;
                        case 89: //Trash
                        case 120:
                            return Trash;
                    }
                    break;
                case 421: //Cherrim
                    switch (form)
                    {
                        case 94: //Overcast
                            return Overcast;
                        case 95: //Sunny
                            return Sunny;
                    }
                    break;
                case 422: //Shellos
                    switch (form)
                    {
                        case 96:
                            return WestSea;
                        case 97:
                            return EastSea;
                    }
                    break;
                case 423: //Gastrodon
                    switch (form)
                    {
                        case 98:
                            return WestSea;
                        case 99:
                            return EastSea;
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
                            return Altered;
                        case 91: //Origin
                            return Origin;
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
                            return Normal;
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
                case 550: //Basculin
                    switch (form)
                    {
                        case 136: //Red Striped
                            return "Red Striped";
                        case 137: //Blue Striped
                            return "Blue Striped";
                    }
                    break;
                case 555: //Darmanitan
                    switch (form)
                    {
                        case 138: //Standard
                            return "Standard";
                        case 139: //Zen
                            return "Zen";
                    }
                    break;
                case 585: //Deerling
                    switch (form)
                    {
                        case 585: //Spring
                            return "Spring";
                        case 586: //Summer
                            return "Summer";
                        case 587: //Autumn
                            return "Autumn";
                        case 588: //Winter
                            return "Winter";
                    }
                    break;
                case 586: //Sawsbuck
                    switch (form)
                    {
                        case 589: //Spring
                            return "Spring";
                        case 590: //Summer
                            return "Summer";
                        case 591: //Autumn
                            return "Autumn";
                        case 592: //Winter
                            return "Winter";
                    }
                    break;
                case 641: //Tornadus
                    switch (form)
                    {
                        case 140: //Incarnate
                            return "Incarnate";
                        case 141: //Therian
                            return "Therian";
                    }
                    break;
                case 642: //Thundurus
                    switch (form)
                    {
                        case 142: //Incarnate
                            return "Incarnate";
                        case 143: //Therian
                            return "Therian";
                    }
                    break;
                case 645: //Landorus
                    switch (form)
                    {
                        case 144: //Incarnate
                            return "Incarnate";
                        case 145: //Therian
                            return "Therian";
                    }
                    break;
                case 646: //Kyurem
                    switch (form)
                    {
                        case 146: //Normal
                            return Normal;
                        case 147: //Black
                            return "Black";
                        case 148: //White
                            return "White";
                    }
                    break;
                case 647: //Keldeo
                    switch (form)
                    {
                        case 149: //Ordinary
                            return "Ordinary";
                        case 150: //Resolute
                            return "Resolute";
                    }
                    break;
                case 648: //Meloetta
                    switch (form)
                    {
                        case 151: //Aria
                            return "Aria";
                        case 152: //Pirouette
                            return "Pirouette";
                    }
                    break;
                case 649: //Genesect
                    switch (form)
                    {
                        case 593: //Normal
                            return Normal;
                        case 594: //Shock
                            return "Shock";
                        case 595: //Burn
                            return "Burn";
                        case 596: //Chill
                            return "Chill";
                        case 597: //Douse
                            return "Douse";
                    }
                    break;
            }

            return null;
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
                    return $"{url}/reward_1301_{(quest.Rewards?[0].Info.Amount ?? 1)}.png";
                case QuestRewardType.Item:
                    return $"{url}/reward_{(int)quest.Rewards?[0].Info.Item}.png";
                case QuestRewardType.PokemonEncounter:
                    return (quest.IsDitto ? 132 : quest.Rewards[0].Info.PokemonId).GetPokemonIcon(quest.Rewards?[0].Info.FormId ?? 0, quest.Rewards?[0].Info.CostumeId ?? 0, whConfig, style);
                case QuestRewardType.Stardust:
                    return $"{url}/reward_stardust_{quest.Rewards[0].Info.Amount}.png";
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
                var emojiId = MasterFile.Instance.Emojis[$"types_{type.ToString().ToLower()}"];
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