namespace WhMgr.Alarms.Filters
{
    using System;

    using WhMgr.Diagnostics;
    using WhMgr.Net.Models;

    public static class Filters
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger("FILTERS");

        static Filters()
        {
            _logger.Trace($"Filters::Filters");
        }

        public static bool MatchesIV(string iv, uint minimumIV, uint maximumIV)
        {
            var matchesIV = false;
            var missing = iv == "?" || string.IsNullOrEmpty(iv);
            if (!missing)
            {
                if (!double.TryParse(iv.Replace("%", ""), out double resultIV))
                {
                    _logger.Error($"Failed to parse pokemon IV value '{iv}', skipping filter check.");
                    return false;
                }

                matchesIV |= Math.Round(resultIV) >= minimumIV && Math.Round(resultIV) <= maximumIV;
            }

            matchesIV |= (missing && minimumIV == 0);

            return matchesIV;
        }

        public static bool MatchesCP(string cp, uint minimumCP, uint maximumCP)
        {
            var matchesCP = false;
            var missing = cp == "?" || string.IsNullOrEmpty(cp);
            if (!missing)
            {
                if (!int.TryParse(cp, out int resultCP))
                {
                    _logger.Error($"Failed to parse pokemon CP value '{cp}', skipping filter check.");
                    return false;
                }

                matchesCP |= resultCP >= minimumCP && resultCP <= maximumCP;
            }

            matchesCP |= (missing && minimumCP == 0);

            return matchesCP;
        }

        public static bool MatchesLvl(string lvl, uint minimumLvl, uint maximumLvl)
        {
            var matchesLvl = false;
            var missing = lvl == "?" || string.IsNullOrEmpty(lvl);
            if (!missing)
            {
                if (!int.TryParse(lvl, out int resultLvl))
                {
                    _logger.Error($"Failed to parse pokemon level value '{lvl}', skipping filter check.");
                    return false;
                }

                matchesLvl |= resultLvl >= minimumLvl && resultLvl <= maximumLvl;
            }

            matchesLvl |= (missing && minimumLvl == 0);

            return matchesLvl;
        }

        public static bool MatchesPvPRank(int rank, uint minimumRank, uint maximumRank)
        {
            var matchesRank = false;
            var missing = rank == 0;
            if (!missing)
            {
                matchesRank |= rank >= minimumRank && rank <= maximumRank;
            }

            matchesRank |= (missing && minimumRank == 0);

            return matchesRank;
        }

        public static bool MatchesIV(string iv, int minimumIV)
        {
            var matchesIV = false;
            var missing = iv == "?" || string.IsNullOrEmpty(iv);
            if (!missing)
            {
                if (!double.TryParse(iv.Replace("%", ""), out double resultIV))
                {
                    _logger.Error($"Failed to parse pokemon IV value '{iv}', skipping filter check.");
                    return false;
                }

                matchesIV |= Math.Round(resultIV) >= minimumIV;
            }

            matchesIV |= (missing && minimumIV == 0);

            return matchesIV;
        }

        public static bool MatchesCP(string cp, int minimumCP)
        {
            var matchesCP = false;
            var missing = cp == "?" || string.IsNullOrEmpty(cp);
            if (!missing)
            {
                if (!int.TryParse(cp, out int resultCP))
                {
                    _logger.Error($"Failed to parse pokemon CP value '{cp}', skipping filter check.");
                    return false;
                }

                matchesCP |= resultCP >= minimumCP;
            }

            matchesCP |= (missing && minimumCP == 0);

            return matchesCP;
        }

        public static bool MatchesGender(PokemonGender gender, PokemonGender desiredGender)
        {
            return gender == desiredGender ||
                   gender == PokemonGender.Unset ||
                   gender == PokemonGender.Genderless;
        }

        public static bool MatchesGender(PokemonGender gender, string desiredGender)
        {
            desiredGender = desiredGender.ToLower();

            if (desiredGender == "*" || gender == PokemonGender.Genderless || gender == PokemonGender.Unset)
                return true;

            if (desiredGender == "m" && gender == PokemonGender.Male)
                return true;

            if (desiredGender == "f" && gender == PokemonGender.Female)
                return true;

            return false;
        }

        public static bool MatchesSize(PokemonSize pkmnSize, PokemonSize? filterSize)
        {
            return (filterSize.HasValue && pkmnSize == filterSize.Value) || !filterSize.HasValue || filterSize == null;
        }
    }
}