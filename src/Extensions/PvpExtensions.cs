namespace WhMgr.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DSharpPlus.Entities;

    using WhMgr.Common;
    using WhMgr.Configuration;
    using WhMgr.Data;
    using WhMgr.Localization;
    using WhMgr.Services.Webhook.Models;

    public static class PvpExtensions
    {
        /// <summary>
        /// Build Pokemon evolution IDs list
        /// </summary>
        /// <param name="pkmn"></param>
        /// <returns></returns>
        public static List<uint> GetPokemonEvolutionIds(this PokedexPokemon pkmn)
        {
            var list = new List<uint>
            {
                pkmn.PokedexId
            };
            void GetEvolutionIds(List<PokedexPokemonEvolution> evolutions)
            {
                foreach (var evolution in evolutions)
                {
                    list.Add(evolution.PokemonId);
                    var pokemon = GameMaster.GetPokemon(evolution.PokemonId, evolution.FormId);
                    if (pokemon.Evolutions?.Count > 0)
                    {
                        GetEvolutionIds(pokemon.Evolutions);
                    }
                }
            }
            if (pkmn?.Evolutions == null)
                return list;

            GetEvolutionIds(pkmn.Evolutions);
            list = list.Distinct().ToList();
            return list;
        }

        public static List<PvpRankData> GetLeagueRanks(this PokemonData pokemon, PvpLeague league)
        {
            var list = new List<PvpRankData>();
            if (pokemon.GreatLeague == null && pokemon.UltraLeague == null)
            {
                return list;
            }
            var pvpRanks = league == PvpLeague.Ultra ? pokemon.UltraLeague : pokemon.GreatLeague;
            var minCp = league == PvpLeague.Ultra ? Strings.Defaults.MinimumUltraLeagueCP : Strings.Defaults.MinimumGreatLeagueCP;
            var maxCp = league == PvpLeague.Ultra ? Strings.Defaults.MaximumUltraLeagueCP : Strings.Defaults.MaximumGreatLeagueCP;
            for (var i = 0; i < pvpRanks.Count; i++)
            {
                var pvp = pvpRanks[i];
                var withinCpRange = pvp.CP >= minCp && pvp.CP <= maxCp;
                var withinRankRange = pvp.Rank <= Strings.Defaults.MaximumRank;
                if (pvp.Rank == 0 || (!withinCpRange && !withinRankRange))
                    continue;

                if (!GameMaster.Instance.Pokedex.ContainsKey(pvp.PokemonId))
                {
                    Console.WriteLine($"Pokemon database does not contain pokemon id {pvp.PokemonId}");
                    continue;
                }
                if (pvp.Rank.HasValue && pvp.Rank.Value <= Strings.Defaults.MaximumRank &&
                    pvp.Percentage.HasValue &&
                    pvp.Level.HasValue &&
                    pvp.CP.HasValue && pvp.CP <= maxCp)
                {
                    var name = Translator.Instance.GetPokemonName(pvp.PokemonId);
                    var form = Translator.Instance.GetFormName(pvp.FormId);
                    var pkmnName = string.IsNullOrEmpty(form) ? name : $"{name} ({form})";
                    pvp.Percentage = Math.Round(pvp.Percentage.Value, 2);
                    pvp.PokemonName = pkmnName;
                    list.Add(pvp);
                }
            }
            list.Sort((a, b) => a.Rank.Value.CompareTo(b.Rank.Value));
            return list;
        }

        public static DiscordColor GetPvPColor(this DiscordEmbedColorsConfig config, List<PvpRankData> greatLeague, List<PvpRankData> ultraLeague)
        {
            var greatRank = greatLeague?.FirstOrDefault(x => x.Rank > 0 && x.Rank <= 25 && x.CP >= Strings.Defaults.MinimumGreatLeagueCP && x.CP <= Strings.Defaults.MaximumGreatLeagueCP);
            var ultraRank = ultraLeague?.FirstOrDefault(x => x.Rank > 0 && x.Rank <= 25 && x.CP >= Strings.Defaults.MinimumUltraLeagueCP && x.CP <= Strings.Defaults.MaximumUltraLeagueCP);
            var color = config.Pokemon.PvP.FirstOrDefault(x =>
                ((greatRank?.Rank ?? 0) >= x.Minimum && (greatRank?.Rank ?? 0) <= x.Maximum)
                || ((ultraRank?.Rank ?? 0) >= x.Minimum && (ultraRank?.Rank ?? 0) <= x.Maximum)
            );
            if (color == null)
            {
                return DiscordColor.White;
            }
            return new DiscordColor(color.Color);
        }
    }
}