namespace WhMgr.Alarms.Filters
{
    using System;

    using WhMgr.Diagnostics;
    using WhMgr.Net.Models;

    public class Filters
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger();

        public Filters()
        {
            _logger.Trace($"Filters::Filters");
        }

        public bool MatchesIV(string iv, uint minimumIV, uint maximumIV)
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

        public bool MatchesCP(string cp, uint minimumCP, uint maximumCP)
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

        public bool MatchesLvl(string lvl, uint minimumLvl, uint maximumLvl)
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

        public bool MatchesIV(string iv, int minimumIV)
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

        public bool MatchesCP(string cp, int minimumCP)
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

        public bool MatchesLvl(string lvl, int minimumLvl)
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

                matchesLvl |= resultLvl >= minimumLvl;
            }

            matchesLvl |= (missing && minimumLvl == 0);

            return matchesLvl;
        }

        public bool MatchesGender(PokemonGender gender, PokemonGender desiredGender)
        {
            return gender == desiredGender ||
                   gender == PokemonGender.Unset ||
                   gender == PokemonGender.Genderless;
        }

        public bool MatchesGender(PokemonGender gender, string desiredGender)
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

        public bool MatchesSize(PokemonSize pkmnSize, PokemonSize? filterSize)
        {
            return (filterSize.HasValue && pkmnSize == filterSize.Value) || !filterSize.HasValue;
        }

        public bool MatchesAttack(string atk, int minimumAtk)
        {
            var matchesAtk = false;
            var missing = atk == "?" || string.IsNullOrEmpty(atk);
            if (!missing)
            {
                if (!int.TryParse(atk, out int resultAtk))
                {
                    _logger.Error($"Failed to parse pokemon attack IV value '{atk}', skipping filter check.");
                    return false;
                }

                matchesAtk |= resultAtk >= minimumAtk;
            }

            matchesAtk |= (missing && minimumAtk == 0);

            return matchesAtk;
        }

        public bool MatchesDefense(string def, int minimumDef)
        {
            var matchesDef = false;
            var missing = def == "?" || string.IsNullOrEmpty(def);
            if (!missing)
            {
                if (!int.TryParse(def, out int resultAtk))
                {
                    _logger.Error($"Failed to parse pokemon defense IV value '{def}', skipping filter check.");
                    return false;
                }

                matchesDef |= resultAtk >= minimumDef;
            }

            matchesDef |= (missing && minimumDef == 0);

            return matchesDef;
        }

        public bool MatchesStamina(string sta, int minimumSta)
        {
            var matchesSta = false;
            var missing = sta == "?" || string.IsNullOrEmpty(sta);
            if (!missing)
            {
                if (!int.TryParse(sta, out int resultAtk))
                {
                    _logger.Error($"Failed to parse pokemon stamina IV value '{sta}', skipping filter check.");
                    return false;
                }

                matchesSta |= resultAtk >= minimumSta;
            }

            matchesSta |= (missing && minimumSta == 0);

            return matchesSta;
        }
    }
}