namespace T
{
    using System;
    using System.Collections.Generic;

    using T.Data.Models;

    public static class CalcIV
    {
        public static ushort[] StardustCosts = { 200, 200, 400, 400, 600, 600, 800, 800, 1000, 1000, 1300, 1300, 1600, 1600, 1900, 1900, 2200, 2200, 2500, 2500, 3000, 3000, 3500, 3500, 4000, 4000, 4500, 4500, 5000, 5000, 6000, 6000, 7000, 7000, 8000, 8000, 9000, 9000, 10000, 10000, 0 };

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

        public static List<PokemonIV> CalculateRaidIVs(uint pokeId, BaseStats baseStats, int cp)
        {
            var list = new List<PokemonIV>();
            var cpm = 0.59740001;
            var lvl = 20;
            var perfectCP = GetCP(baseStats.Attack + 15, baseStats.Defense + 15, baseStats.Stamina + 15, cpm);

            if (!(cp <= perfectCP))
            {
                cpm = 0.667934;
                lvl = 25;
            }

            for (int sta = 10; sta < 16; sta++)
            {
                for (int atk = 10; atk < 16; atk++)
                {
                    for (int def = 10; def < 16; def++)
                    {
                        var currCP = GetCP(baseStats.Attack + atk, baseStats.Defense + def, baseStats.Stamina + sta, cpm);
                        if (currCP == cp)
                        {
                            var hp = GetHP(baseStats.Stamina + sta, cpm);
                            list.Add(new PokemonIV
                            {
                                PokemonId = pokeId,
                                Attack = atk,
                                Defense = def,
                                Stamina = sta,
                                CP = cp,
                                HP = hp,
                                Level = lvl
                            });
                        }
                    }
                }
            }

            list.Sort((x, y) => y.IV.CompareTo(x.IV));
            return list;
        }

        public static List<PokemonIV> CalculateIVs(uint pokeId, BaseStats baseStats, int cp, int stardust, int health, bool hatched, Dictionary<string, double> ECpM, bool poweredUp = false)
        {
            var list = new List<PokemonIV>();
            int minAtk = 0, minDef = 0, minSta = 0;
            int maxAtk = 16, maxDef = 16, maxSta = 16;
            if (hatched)
            {
                minAtk = 10;
                minDef = 10;
                minSta = 10;
            }

            foreach (var cpm in ECpM)
            {
                if (cpm.Key.Contains(".5") && !poweredUp) continue;
                if (pokeId == 151 && Convert.ToInt32(cpm.Key) < 15) continue;
                if (hatched && cpm.Key != "20") continue;

                var lvl = Convert.ToInt32(cpm.Key);
                for (int sta = minSta; sta < maxSta; sta++)
                {
                    var hp = GetHP(baseStats.Stamina + sta, cpm.Value);
                    var dustCost = StardustCosts[Convert.ToUInt16(lvl) - 1];
                    if ((dustCost == stardust || stardust == 0) && (health == hp || health == 0))
                    {
                        for (int atk = minAtk; atk < maxAtk; atk++)
                        {
                            for (int def = minDef; def < maxDef; def++)
                            {
                                var currCP = GetCP(baseStats.Attack + atk, baseStats.Defense + def, baseStats.Stamina + sta, cpm.Value);
                                if (currCP == cp)
                                {
                                    list.Add(new PokemonIV
                                    {
                                        PokemonId = pokeId,
                                        Attack = atk,
                                        Defense = def,
                                        Stamina = sta,
                                        CP = cp,
                                        HP = hp,
                                        Level = Convert.ToDouble(lvl)
                                    });
                                }
                            }
                        }
                    }
                }
            }

            list.Sort((x, y) => y.IV.CompareTo(x.IV));
            return list;
        }

        public static DittoTransformInfo CalculateTransformCP(BaseStats targetPokemon, int level, int atkIV, int defIV, int hpIV, Dictionary<string, double> ECpM)
        {
            /**
            copying the opponent's:
            Quick and charge moves
            Base stats
            Pokemon type(s)
            The following are unchanged for Ditto after transform:

            HP and base stamina
            Level
            Individual values (IVs)
            */

            var dittoBaseStats = new
            {
                attack = 91,
                defense = 91,
                stamina = 96
            };

            var cpm = ECpM[level.ToString()];
            var cp = GetCP(targetPokemon.Attack + atkIV, targetPokemon.Defense + defIV, targetPokemon.Stamina + hpIV, cpm);
            var hp = GetHP(targetPokemon.Stamina + hpIV, cpm);
            var iv = GetIV(targetPokemon.Attack + atkIV, targetPokemon.Defense + defIV, targetPokemon.Stamina + hpIV);
            var dittoCP = GetCP(dittoBaseStats.attack + atkIV, dittoBaseStats.defense + defIV, dittoBaseStats.stamina + hpIV, cpm);
            var dittoHP = GetHP(dittoBaseStats.stamina + hpIV, cpm);
            var dittoIV = GetIV(dittoBaseStats.attack + atkIV, dittoBaseStats.defense + defIV, dittoBaseStats.stamina + hpIV);

            return new DittoTransformInfo(dittoCP, dittoHP, dittoIV, cp, hp, iv);
        }

        public static int GetCP(int attack, int defense, int stamina, double cpm)
        {
            var cp = Math.Floor(attack * Math.Pow(defense, 0.5) * Math.Pow(stamina, 0.5) * Math.Pow(cpm, 2) / 10);
            return Convert.ToInt32(cp < 10 ? 10 : cp);
        }

        public static int GetHP(int stamina, double cpm)
        {
            var hp = Math.Floor(cpm * stamina);
            return Convert.ToInt32(hp < 10 ? 10 : hp);
        }

        public static double GetIV(double attack, double defense, double stamina)
        {
            return Math.Round(Convert.ToDouble((stamina + attack + defense) * 100.0 / 45), 1);
        }
    }

    public class DittoTransformInfo
    {
        public double DittoCP { get; set; }

        public double DittoHP { get; set; }

        public double DittoIV { get; set; }

        public double TransformCP { get; set; }

        public double TransformHP { get; set; }

        public double TransformIV { get; set; }

        public DittoTransformInfo(double cp, double hp, double iv, double transformCP, double transformHP, double transformIV)
        {
            DittoCP = cp;
            DittoHP = hp;
            DittoIV = iv;
            TransformCP = transformCP;
            TransformHP = transformHP;
            TransformIV = transformIV;
        }
    }

    public class PokemonIV
    {
        public uint PokemonId { get; set; }

        public int CP { get; set; }

        public int HP { get; set; }

        public double Level { get; set; }

        public double IV
        {
            get
            {
                return Math.Round(Convert.ToDouble((Stamina + Attack + Defense) * 100.0 / 45), 1);
            }
        }

        public int Attack { get; set; }

        public int Defense { get; set; }

        public int Stamina { get; set; }
    }
}