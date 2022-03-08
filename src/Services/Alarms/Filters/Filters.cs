namespace WhMgr.Services.Alarms.Filters
{
    using System;

    using Gender = POGOProtos.Rpc.PokemonDisplayProto.Types.Gender;

    using WhMgr.Common;

    public static class Filters
    {
        public static bool MatchesIV(string iv, uint minimumIV, uint maximumIV)
        {
            var matchesIV = false;
            var missing = iv == "?" || string.IsNullOrEmpty(iv);
            if (!missing)
            {
                if (!double.TryParse(iv.Replace("%", ""), out double resultIV))
                {
                    Console.WriteLine($"Failed to parse pokemon IV value '{iv}', skipping filter check.");
                    return false;
                }

                matchesIV |= Math.Round(resultIV) >= minimumIV && Math.Round(resultIV) <= maximumIV;
            }

            matchesIV |= (missing && minimumIV == 0);

            return matchesIV;
        }

        public static bool MatchesCP(uint? cp, uint minimumCP, uint maximumCP)
        {
            var matchesCP = false;
            var missing = cp == null;
            if (!missing)
            {
                matchesCP |= cp >= minimumCP && cp <= maximumCP;
            }
            matchesCP |= (missing && minimumCP == 0);
            return matchesCP;
        }

        public static bool MatchesLvl(ushort? lvl, uint minimumLvl, uint maximumLvl)
        {
            var matchesLvl = false;
            var missing = lvl == null;
            if (!missing)
            {
                matchesLvl |= lvl >= minimumLvl && lvl <= maximumLvl;
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

        public static bool MatchesIV(double? iv, int minimumIV)
        {
            var matchesIV = false;
            var missing = iv == null;
            if (!missing)
            {
                matchesIV |= Math.Round(iv ?? 0) >= minimumIV;
            }
            matchesIV |= (missing && minimumIV == 0);
            return matchesIV;
        }

        public static bool MatchesCP(uint? cp, int minimumCP)
        {
            var matchesCP = false;
            var missing = cp == null;
            if (!missing)
            {
                matchesCP |= cp >= minimumCP;
            }
            matchesCP |= (missing && minimumCP == 0);
            return matchesCP;
        }

        public static bool MatchesGender(Gender gender, Gender desiredGender)
        {
            return gender == desiredGender ||
                   gender == Gender.Unset ||
                   gender == Gender.Less;
        }

        public static bool MatchesGender(Gender gender, string desiredGender)
        {
            desiredGender = desiredGender.ToLower();

            if (desiredGender == "*" || gender == Gender.Less || gender == Gender.Unset)
                return true;

            if (desiredGender == "m" && gender == Gender.Male)
                return true;

            if (desiredGender == "f" && gender == Gender.Female)
                return true;

            return false;
        }

        /// <summary>
        /// Check whether a Pokemon's size matches the filter size or if the filter size
        /// value is <c>PokemonSize.All</c> return true, otherwise return false.
        /// </summary>
        /// <param name="pkmnSize">The calculated Pokemon size</param>
        /// <param name="filterSize">The expected filter size</param>
        /// <returns></returns>
        public static bool MatchesSize(PokemonSize pkmnSize, PokemonSize? filterSize)
        {
            // Check if filter size set and if Pokemon size matches filter size
            return (filterSize.HasValue && pkmnSize == filterSize.Value) ||
                // Otherwise check if filter size is All
                filterSize == PokemonSize.All ||
                // Lastly check if no filter size set, if not return true
                !filterSize.HasValue ||
                filterSize == null;
        }

        /// <summary>
        /// Check whether a Pokemon's size matches the filter size or if the filter size
        /// value is <c>PokemonSize.All</c> return true, otherwise return false.
        /// </summary>
        /// <param name="pkmnSize">The calculated Pokemon size</param>
        /// <param name="filterSize">The expected filter size</param>
        /// <returns></returns>
        public static bool MatchesSize(PokemonSize pkmnSize, PokemonSize filterSize)
        {
            // Check if filter size set and if Pokemon size matches filter size, otherwise
            // if set to All return true
            return pkmnSize == filterSize || filterSize == PokemonSize.All;
        }
    }
}