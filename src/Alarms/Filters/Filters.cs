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

        public bool MatchesIV(string iv, int minimumIV, int maximumIV)
        {
            var matchesIV = false;
            if (iv != "?")
            {
                if (!double.TryParse(iv.Replace("%", ""), out double resultIV))
                {
                    _logger.Error($"Failed to parse pokemon IV value '{iv}', skipping filter check.");
                    return false;
                }

                matchesIV |= Math.Round(resultIV) >= minimumIV && Math.Round(resultIV) <= maximumIV;
            }

            matchesIV |= (iv == "?" && minimumIV == 0);

            return matchesIV;
        }

        public bool MatchesIV(string iv, int minimumIV)
        {
            var matchesIV = false;
            if (iv != "?")
            {
                if (!double.TryParse(iv.Replace("%", ""), out double resultIV))
                {
                    _logger.Error($"Failed to parse pokemon IV value '{iv}', skipping filter check.");
                    return false;
                }

                matchesIV |= Math.Round(resultIV) >= minimumIV;
            }

            matchesIV |= (iv == "?" && minimumIV == 0);

            return matchesIV;
        }

        public bool MatchesCP(string cp, int minimumCP)
        {
            var matchesCP = false;
            if (cp != "?")
            {
                if (!int.TryParse(cp, out int resultCP))
                {
                    _logger.Error($"Failed to parse pokemon CP value '{cp}', skipping filter check.");
                    return false;
                }

                matchesCP |= resultCP >= minimumCP;
            }

            matchesCP |= (cp == "?" && minimumCP == 0);

            return matchesCP;
        }

        public bool MatchesLvl(string lvl, int minimumLvl)
        {
            var matchesLvl = false;
            if (lvl != "?" && !string.IsNullOrEmpty(lvl))
            {
                if (!int.TryParse(lvl, out int resultLvl))
                {
                    _logger.Error($"Failed to parse pokemon level value '{lvl}', skipping filter check.");
                    return false;
                }

                matchesLvl |= resultLvl >= minimumLvl;
            }

            matchesLvl |= ((string.IsNullOrEmpty(lvl) || lvl == "?") && minimumLvl == 0);

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

            if (desiredGender == "*" || gender == PokemonGender.Genderless || gender == PokemonGender.Unset) return true;

            if (desiredGender == "m" && gender == PokemonGender.Male) return true;

            if (desiredGender == "f" && gender == PokemonGender.Female) return true;

            return false;
        }

        public bool MatchesAttack(string atk, int minimumAtk)
        {
            var matchesAtk = false;
            if (atk != "?")
            {
                if (!int.TryParse(atk, out int resultAtk))
                {
                    _logger.Error($"Failed to parse pokemon attack IV value '{atk}', skipping filter check.");
                    return false;
                }

                matchesAtk |= resultAtk >= minimumAtk;
            }

            matchesAtk |= (atk == "?" && minimumAtk == 0);

            return matchesAtk;
        }

        public bool MatchesDefense(string def, int minimumDef)
        {
            var matchesDef = false;
            if (def != "?")
            {
                if (!int.TryParse(def, out int resultAtk))
                {
                    _logger.Error($"Failed to parse pokemon defense IV value '{def}', skipping filter check.");
                    return false;
                }

                matchesDef |= resultAtk >= minimumDef;
            }

            matchesDef |= (def == "?" && minimumDef == 0);

            return matchesDef;
        }

        public bool MatchesStamina(string sta, int minimumSta)
        {
            var matchesSta = false;
            if (sta != "?")
            {
                if (!int.TryParse(sta, out int resultAtk))
                {
                    _logger.Error($"Failed to parse pokemon stamina IV value '{sta}', skipping filter check.");
                    return false;
                }

                matchesSta |= resultAtk >= minimumSta;
            }

            matchesSta |= (sta == "?" && minimumSta == 0);

            return matchesSta;
        }
    }
}