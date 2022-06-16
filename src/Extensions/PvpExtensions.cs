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
        public static List<uint> GetPokemonEvolutionIds(this PokedexPokemon pkmn, bool includeBasePokemon = true)
        {
            var pokemonIds = includeBasePokemon
                ? new List<uint> { includeBasePokemon ? pkmn.PokedexId : 0 }
                : new List<uint>();

            if ((pkmn?.Evolutions?.Count ?? 0) == 0)
                return pokemonIds;

            void GetEvolutionIds(List<PokedexPokemonEvolution> evolutions)
            {
                foreach (var evolution in evolutions)
                {
                    pokemonIds.Add(evolution.PokemonId);
                    var pokemon = GameMaster.GetPokemon(evolution.PokemonId, evolution.FormId);
                    if (pokemon == null)
                        continue;

                    if (pokemon.Evolutions?.Count > 0)
                    {
                        GetEvolutionIds(pokemon.Evolutions);
                    }
                }
            }

            GetEvolutionIds(pkmn.Evolutions);
            pokemonIds = pokemonIds.Distinct().ToList();
            return pokemonIds;
        }

        // TODO: Use for PokemonData class with embed template parsing to filter useless ranks or remove from config
        public static Dictionary<PvpLeague, List<PvpRankData>> GetLeagueRanks(this PokemonData pokemon)
        {
            var dict = new Dictionary<PvpLeague, List<PvpRankData>>();
            if (!pokemon.HasPvpRankings)
            {
                return dict;
            }

            // Loop all available PvP leagues for Pokemon
            foreach (var (pokemonPvpLeague, pokemonPvpRanks) in pokemon.PvpRankings)
            {
                // Skip if Pokemon's PvP ranking league is not allowed and/or not set in config
                if (!Startup.Config.PvpLeagues.ContainsKey(pokemonPvpLeague))
                    continue;

                // Loop all PvP rankings for league
                foreach (var pvp in pokemonPvpRanks)
                {
                    var pvpConfig = Startup.Config.PvpLeagues[pokemonPvpLeague];
                    var withinCpRange = pvp.CP >= pvpConfig.MinimumCP && pvp.CP <= pvpConfig.MaximumCP;
                    var withinRankRange = pvp.Rank >= pvpConfig.MinimumRank && pvp.Rank <= pvpConfig.MaximumRank;
                    if (pvp.Rank == 0 || (!withinCpRange && !withinRankRange))
                        continue;

                    if (!GameMaster.Instance.Pokedex.ContainsKey(pvp.PokemonId))
                    {
                        Console.WriteLine($"Pokemon database does not contain pokemon id {pvp.PokemonId}");
                        continue;
                    }
                    if (pvp.Rank.HasValue && pvp.Rank.Value <= pvpConfig.MaximumRank &&
                        pvp.Percentage.HasValue &&
                        pvp.Level.HasValue &&
                        pvp.CP.HasValue && pvp.CP <= pvpConfig.MaximumCP)
                    {
                        //var name = Translator.Instance.GetPokemonName(pvp.PokemonId);
                        //var form = Translator.Instance.GetFormName(pvp.FormId);
                        //var pkmnName = string.IsNullOrEmpty(form) ? name : $"{name} ({form})";
                        pvp.Percentage = Math.Round(pvp.Percentage.Value, 2);
                        //pvp.PokemonName = pkmnName;
                        if (dict.ContainsKey(pokemonPvpLeague))
                        {
                            dict[pokemonPvpLeague].Add(pvp);
                        }
                        else
                        {
                            dict.Add(pokemonPvpLeague, new List<PvpRankData> { pvp });
                        }
                    }
                }
            }
            // TODO: dict.Sort((a, b) => a.Rank.Value.CompareTo(b.Rank.Value));
            return dict;
        }

        public static DiscordColor GetPvPColor(this DiscordEmbedColorsConfig config, Dictionary<PvpLeague, List<PvpRankData>> rankings)
        {
            const ushort maxRank = 25;
            var matchedRank = rankings?.FirstOrDefault(x =>
            {
                return x.Value?.Exists(y =>
                    y.Rank > 0 &&
                    y.Rank <= maxRank &&
                    y.CP >= Strings.Defaults.Pvp[x.Key].MinimumLeagueCP &&
                    y.CP <= Strings.Defaults.Pvp[x.Key].MaximumLeagueCP
                ) ?? false;
            }).Value?.FirstOrDefault();
            var color = config.Pokemon.PvP.FirstOrDefault(x =>
                (matchedRank?.Rank ?? 0) >= x.Minimum &&
                (matchedRank?.Rank ?? 0) <= x.Maximum
            );
            if (color == null)
            {
                return DiscordColor.White;
            }
            return new DiscordColor(color.Color);
        }
    }
}