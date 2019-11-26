namespace WhMgr
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using WhMgr.Data;

    public class PvpRankCalculator
    {
        #region Public Methods

        public async Task<BestPvPStat> CalculateBestPvPStat(int pokemonId, int formId, int atk, int def, int sta, int cap)
        {
            var bestStat = 0d;
            var level = 0d;
            for (var i = 1d; i <= 40; i += .5)
            {
                var useForm = MasterFile.Instance.Pokedex[pokemonId].Attack == null;
                var pkmnAtk = (useForm ?
                    MasterFile.Instance.Pokedex[pokemonId].Forms[formId].Attack :
                    MasterFile.Instance.Pokedex[pokemonId].Attack) ?? 0;
                var pkmnDef = (useForm ?
                    MasterFile.Instance.Pokedex[pokemonId].Forms[formId].Defense :
                    MasterFile.Instance.Pokedex[pokemonId].Defense) ?? 0;
                var pkmnSta = (useForm ?
                    MasterFile.Instance.Pokedex[pokemonId].Forms[formId].Stamina :
                    MasterFile.Instance.Pokedex[pokemonId].Stamina) ?? 0;
                var cp = GetCP(pkmnAtk + atk, pkmnDef + def, pkmnSta + sta, MasterFile.Instance.CpMultipliers[i]);
                //var cp = CalculateCP(pokemonId, formId, atk, def, sta, i);
                if (cp <= cap)
                {
                    var stat = CalculatePvPStat(pokemonId, formId, atk, def, sta, i);
                    if (stat > bestStat)
                    {
                        bestStat = stat;
                        level = i;
                    }
                }
                else if (cp > cap)
                {
                    i = 41;
                }
            }
            return await Task.FromResult(new BestPvPStat
            {
                Value = bestStat,
                Level = level,
                Attack = atk,
                Defense = def,
                Stamina = sta
            });
        }

        //public async Task<List<BestPvPStat>> CalculateTopRanks(int pokemonId, int formId, int cap, int topRanks)
        //{
        //    var bestStat = new BestPvPStat();
        //    var arrayToSort = new List<BestPvPStat>();
        //    for (var a = 0; a <= 15; a++)
        //    {
        //        for (var d = 0; d <= 15; d++)
        //        {
        //            for (var s = 0; s <= 15; s++)
        //            {
        //                var currentStat = await CalculateBestPvPStat(pokemonId, formId, a, d, s, cap);
        //                if (currentStat.Value > bestStat.Value)
        //                {
        //                    bestStat = new BestPvPStat { Attack = a, Defense = d, Stamina = s, Value = currentStat.Value, Level = currentStat.Level };
        //                }
        //                arrayToSort.Add(currentStat);
        //            }
        //        }
        //    }

        //    arrayToSort.Sort((x, y) => (int)(y.Value - x.Value));
        //    var best = arrayToSort[0].Value;
        //    for (var i = 0; i < arrayToSort.Count; i++)
        //    {
        //        var rank = i + 1;
        //        if (rank > topRanks || rank == 0)
        //            continue;
        //        var percent = PrecisionRound((arrayToSort[i].Value / best) * 100, 2);
        //        arrayToSort[i].Percent = percent;
        //        arrayToSort[i].Rank = rank;
        //        //Console.WriteLine($"{arrayToSort[i].Attack}/{arrayToSort[i].Defense}/{arrayToSort[i].Stamina} L{arrayToSort[i].Level} Value={arrayToSort[i].Value} Rank #{arrayToSort[i].Rank} Percent: {percent}%");
        //    }

        //    return await Task.FromResult(arrayToSort.FindAll(x => x.Rank <= topRanks));
        //}

        public async Task<List<PvPCP>> CalculatePossibleCPs(int pokemonId, int formId, int atk, int def, int sta, double level, string gender, int minCP, int maxCP) //TODO: Change gender to PokemonGender and below from gender to gender.ToString()
        {
            var possibleCPs = new List<PvPCP>();
            if (!string.IsNullOrEmpty(MasterFile.Instance.Pokedex[pokemonId].GenderRequirement) && MasterFile.Instance.Pokedex[pokemonId].GenderRequirement != gender)
            {
                return possibleCPs;
            }

            for (var i = level; i <= 40; i += .5)
            {
                var useForm = MasterFile.Instance.Pokedex[pokemonId].Attack == null;
                var pkmnAtk = (useForm ?
                    MasterFile.Instance.Pokedex[pokemonId].Forms[formId].Attack :
                    MasterFile.Instance.Pokedex[pokemonId].Attack) ?? 0;
                var pkmnDef = (useForm ?
                    MasterFile.Instance.Pokedex[pokemonId].Forms[formId].Defense :
                    MasterFile.Instance.Pokedex[pokemonId].Defense) ?? 0;
                var pkmnSta = (useForm ?
                    MasterFile.Instance.Pokedex[pokemonId].Forms[formId].Stamina :
                    MasterFile.Instance.Pokedex[pokemonId].Stamina) ?? 0;
                //var currentCP = CalculateCP(pokemonId, formId, atk, def, sta, i);
                var currentCP = GetCP(pkmnAtk + atk, pkmnDef + def, pkmnSta + sta, MasterFile.Instance.CpMultipliers[i]);
                if (currentCP >= minCP && currentCP <= maxCP)
                {
                    possibleCPs.Add(new PvPCP
                    {
                        PokemonId = pokemonId,
                        FormId = formId,
                        Attack = atk,
                        Defense = def,
                        Stamina = sta,
                        Level = i,
                        CP = currentCP
                    });
                    if (currentCP > maxCP) { i = 41; }
                }
            }

            if (MasterFile.Instance.Pokedex[pokemonId].Evolutions.Count == 0)
            {
                return possibleCPs;
            }

            for (var i = 0; i < MasterFile.Instance.Pokedex[pokemonId].Evolutions.Count; i++)
            {
                int evolvedForm;
                if (formId > 0)
                {
                    if (!MasterFile.Instance.Pokedex[pokemonId].Forms.ContainsKey(formId))
                    {
                        evolvedForm = MasterFile.Instance.Pokedex[int.Parse(MasterFile.Instance.Pokedex[pokemonId].Evolutions[i])].DefaultForm ?? 0;
                    }
                    else
                    {
                        evolvedForm = MasterFile.Instance.Pokedex[pokemonId].Forms[formId].EvolvedForm ?? 0;
                    }
                }
                else if (MasterFile.Instance.Pokedex[pokemonId].EvolvedForm.HasValue)
                {
                    evolvedForm = MasterFile.Instance.Pokedex[pokemonId].EvolvedForm ?? 0;
                }
                else
                {
                    evolvedForm = formId;
                }

                possibleCPs.AddRange(await CalculatePossibleCPs(int.Parse(MasterFile.Instance.Pokedex[pokemonId].Evolutions[i]), evolvedForm, atk, def, sta, level, gender, minCP, maxCP));
            }
            return await Task.FromResult(possibleCPs);
        }

        public async Task<KeyValuePair<int, double>> GetRank(int pokemonId, int formId, int maxCP, BestPvPStat bestPvPStat)//, List<BestPvPStat> topRanks)
        {
            //var topRanks = await CalculateTopRanks(pokemonId, formId, maxCP, Net.Models.PokemonData.TopPvPRanks);
            //var topRank = topRanks?.Where(x => x.Attack == bestPvPStat.Attack && x.Defense == bestPvPStat.Defense && x.Stamina == bestPvPStat.Stamina);
            //var myRank = topRank.FirstOrDefault();
            try
            {
                var myRank = maxCP == 2500 ?
                    (PvPRank)Database.Instance.UltraPvPLibrary[pokemonId][formId][bestPvPStat.Attack][bestPvPStat.Defense][bestPvPStat.Stamina] :
                    (PvPRank)Database.Instance.GreatPvPLibrary[pokemonId][formId][bestPvPStat.Attack][bestPvPStat.Defense][bestPvPStat.Stamina];
                var rank = myRank?.Rank ?? 4096;
                var percent = myRank?.Percent ?? 0;

                return await Task.FromResult(new KeyValuePair<int, double>(rank, percent));
            }
            catch (Exception)
            {
                return await Task.FromResult(new KeyValuePair<int, double>(4096, 0));
            }
        }

        #endregion

        private double CalculatePvPStat(int pokemonId, int formId, int atk, int def, int sta, double level)
        {
            var cpMultiplier = MasterFile.Instance.CpMultipliers[level];
            if (MasterFile.Instance.Pokedex[pokemonId].Attack == null)
            {
                atk = Convert.ToInt32((atk + MasterFile.Instance.Pokedex[pokemonId].Forms[formId].Attack) * cpMultiplier);
                def = Convert.ToInt32((def + MasterFile.Instance.Pokedex[pokemonId].Forms[formId].Defense) * cpMultiplier);
                sta = Convert.ToInt32((sta + MasterFile.Instance.Pokedex[pokemonId].Forms[formId].Stamina) * cpMultiplier);
            }
            else
            {
                atk = Convert.ToInt32((atk + MasterFile.Instance.Pokedex[pokemonId].Attack) * cpMultiplier);
                def = Convert.ToInt32((def + MasterFile.Instance.Pokedex[pokemonId].Defense) * cpMultiplier);
                sta = Convert.ToInt32((sta + MasterFile.Instance.Pokedex[pokemonId].Stamina) * cpMultiplier);
            }
            var product = atk * def * Math.Floor((double)sta);
            product = Math.Round(product);
            return product;
        }

        private double PrecisionRound(double number, int precision)
        {
            var factor = Math.Pow(10, precision);
            return Math.Round(number * factor) / factor;
        }


        public static int GetCP(int attack, int defense, int stamina, double cpm)
        {
            var cp = Math.Floor(attack * Math.Pow(defense, 0.5) * Math.Pow(stamina, 0.5) * Math.Pow(cpm, 2) / 10);
            return Convert.ToInt32(cp < 10 ? 10 : cp);
        }
    }

    public class PvPCP
    {
        public int PokemonId { get; set; }

        public int FormId { get; set; }

        public int Attack { get; set; }

        public int Defense { get; set; }

        public int Stamina { get; set; }

        public double Level { get; set; }

        public int CP { get; set; }
    }

    public class BestPvPStat
    {
        public double Value { get; set; }

        public double Level { get; set; }

        public double Percent { get; set; }

        public int Rank { get; set; }

        public int Attack { get; set; }

        public int Defense { get; set; }

        public int Stamina { get; set; }
    }
}