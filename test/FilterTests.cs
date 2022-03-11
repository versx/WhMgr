namespace WhMgr.Test
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using NUnit.Framework;
    using Gender = POGOProtos.Rpc.PokemonDisplayProto.Types.Gender;

    using WhMgr.Common;
    using WhMgr.Localization;
    using WhMgr.Services.Alarms.Filters;

    [TestFixture]
    public class FilterTests
    {
        [SetUp]
        public void Setup()
        {
            // TODO: Fix locale path, copy from src/bin to test/bin/debug|release
            /*
            var localeFolder = Strings.BasePath + Strings.StaticFolder + Path.DirectorySeparatorChar + "locales";
            Translator.Instance.LocaleDirectory = localeFolder;
            //Translator.Instance.CreateLocaleFiles().ConfigureAwait(false).GetAwaiter().GetResult();
            Translator.Instance.SetLocale("en");
            */
        }

        private static bool IvWildcardMatches(string ivEntry, ushort? pokemonIvEntry)
        {
            // Skip IV ranges
            if (ivEntry.Contains("-"))
            {
                return false;
            }

            // Return true if wildcard specified.
            if (ivEntry == "*")
            {
                return true;
            }

            // Validate IV list entry is a valid integer.
            if (!ushort.TryParse(ivEntry, out var ivValue))
            {
                return false;
            }

            // Check if individual value is the same or if wildcard is specified.
            return ivValue == pokemonIvEntry;
        }

        private static bool IvListMatches(List<string> ivList, ushort? atk, ushort? def, ushort? sta)
        {
            // Check if IV list is null or no entries and Pokemon has IV values, otherwise return false.
            if (ivList?.Count == 0 ||
                atk == null ||
                def == null ||
                sta == null)
            {
                return false;
            }

            // Construct expected formatted IV entry string
            var ivEntry = $"{atk}/{def}/{sta}";

            // Check if IV matches any IV list range or wildcard entries
            var matches = ivList?.Exists(iv =>
            {
                // Check if IV list entries matches Pokemon IV string verbatim
                if (string.Equals(iv, ivEntry))
                {
                    return true;
                }

                var split = iv.Split('/');

                // Ensure user specified all IV parts required
                if (split.Length != 3)
                    return false;

                var ivAttack = split[0];
                var ivDefense = split[1];
                var ivStamina = split[2];

                var matchesWildcard =
                    IvWildcardMatches(ivAttack, atk) &&
                    IvWildcardMatches(ivDefense, def) &&
                    IvWildcardMatches(ivStamina, sta);

                var matchesRange = IvRangeMatches(ivAttack, ivDefense, ivStamina, atk, def, sta);
                return matchesWildcard || matchesRange;
            }) ?? false;

            return matches;
        }

        private static bool IvRangeMatches(string ivAttack, string ivDefense, string ivStamina, ushort? attack, ushort? defense, ushort? stamina)
        {
            if (attack == null ||
                defense == null ||
                stamina == null)
            {
                return false;
            }

            // Check if none of the IV entries contain range indicator
            if (!ivAttack.Contains("-") &&
                !ivDefense.Contains("-") &&
                !ivStamina.Contains("-"))
            {
                return false;
            }

            // Parse min/max IV values for all entries
            var (minAttack, maxAttack) = ParseMinMaxValues(ivAttack);
            var (minDefense, maxDefense) = ParseMinMaxValues(ivDefense);
            var (minStamina, maxStamina) = ParseMinMaxValues(ivStamina);
            /*
            ushort minAttack;
            ushort maxAttack;
            if (ivAttack.Contains("-"))
            {
                // Parse min/max range values
                var (min, max) = ParseRangeEntry(ivAttack);
                minAttack = min;
                maxAttack = max;
            }
            else
            {
                // Check if attack IV contains wildcard, otherwise value should be a whole value
                if (ivAttack.Contains("*"))
                {
                    // Wildcard specified, set min/max to 0-15
                    minAttack = 0;
                    maxAttack = 15;
                }
                else
                {
                    // No range indicator found for attack IV, parse and assign whole IV value to min/max values
                    var atk = ushort.Parse(ivAttack);
                    minAttack = atk;
                    maxAttack = atk;
                }
            }

            // Check if defense contains range indicator, if so parse min/max values
            ushort minDefense;
            ushort maxDefense;
            if (ivDefense.Contains("-"))
            {
                // Parse min/max range values
                var (min, max) = ParseRangeEntry(ivDefense);
                minDefense = min;
                maxDefense = max;
            }
            else
            {
                // Check if attack IV contains wildcard, otherwise value should be a whole value
                if (ivDefense.Contains("*"))
                {
                    minDefense = 0;
                    maxDefense = 15;
                }
                else
                {
                    // No range indicator found for defense IV, parse and assign whole IV value to min/max values
                    var def = ushort.Parse(ivDefense);
                    minDefense = def;
                    maxDefense = def;
                }
            }

            // Check if stamina contains range indicator, if so parse min/max values
            ushort minStamina;
            ushort maxStamina;
            if (ivStamina.Contains("-"))
            {
                // Parse min/max range values
                var (min, max) = ParseRangeEntry(ivStamina);
                minStamina = min;
                maxStamina = max;
            }
            else
            {
                // Check if attack IV contains wildcard, otherwise value should be a whole value
                if (ivStamina.Contains("*"))
                {
                    minStamina = 0;
                    maxStamina = 15;
                }
                else
                {
                    // No range indicator found for stamina IV, parse and assign whole IV value to min/max values
                    var sta = ushort.Parse(ivStamina);
                    minStamina = sta;
                    maxStamina = sta;
                }
            }
            */

            // Check if Pokemon IV is within min/max range
            var matches = (attack ?? 0) >= minAttack && (attack ?? 0) <= maxAttack &&
                          (defense ?? 0) >= minDefense && (defense ?? 0) <= maxDefense &&
                          (stamina ?? 0) >= minStamina && (stamina ?? 0) <= maxStamina;

            return matches;
        }

        private static (ushort, ushort) ParseRangeEntry(string ivEntry)
        {
            // Parse IV range min/max values
            var split = ivEntry.Split('-');

            // If count mismatch, skip
            if (split.Length != 2)
            {
                return default;
            }

            // Parse first range value for minimum
            if (!ushort.TryParse(split[0], out var minRange))
            {
                return default;
            }

            // Parse second range value for maximum
            if (!ushort.TryParse(split[1], out var maxRange))
            {
                return default;
            }
            return (minRange, maxRange);
        }

        private static (ushort, ushort) ParseMinMaxValues(string ivEntry)
        {
            ushort minRange;
            ushort maxRange;
            if (ivEntry.Contains("-"))
            {
                // Parse min/max range values
                var (min, max) = ParseRangeEntry(ivEntry);
                minRange = min;
                maxRange = max;
            }
            // Check if attack IV contains wildcard, otherwise value should be a whole value
            else if (ivEntry.Contains("*"))
            {
                // Wildcard specified, set min/max to 0-15
                minRange = 0;
                maxRange = 15;
            }
            else
            {
                // No range indicator found for IV entry, parse and assign whole IV value to min/max values
                var atk = ushort.Parse(ivEntry);
                minRange = atk;
                maxRange = atk;
            }
            return (minRange, maxRange);
        }

        [Test]
        [TestCase(1, 15, 15)]
        [TestCase(1, 15, 14)]
        public void Test_PokemonIVListRange_ReturnsIsTrue(int atk, int def, int sta)
        {
            var ivList = new List<string>
            {
                "1-2/15/14",
                "0-15/0-15/0-15",
                "*/15/14-15"
            };
            var matches = IvListMatches(ivList, (ushort)atk, (ushort)def, (ushort)sta);
            /*
            var matches = IvRangeMatches(
                ivAttack, ivDefense, ivStamina,
                (ushort)atk, (ushort)def, (ushort)sta
            );
            */
            Assert.IsTrue(matches);
        }

        [Test]
        //[TestCase("1-2", "15", "15", 1, 15, 15)]
        //[TestCase("0-15", "0-15", "0-15", 1, 15, 15)]
        [TestCase(1, 15, 15)]
        [TestCase(7, 15, 14)]
        public void Test_PokemonIVListRange_ReturnsIsFalse(int atk, int def, int sta)
        {
            var ivList = new List<string>
            {
                "3-4/12/12",
                "0/0/0",
                "15/15/15",
                "*/14/14",
            };
            var matches = IvListMatches(ivList, (ushort)atk, (ushort)def, (ushort)sta);
            /*
            var matches = IvRangeMatches(
                ivAttack, ivDefense, ivStamina,
                (ushort)atk, (ushort)def, (ushort)sta
            );
            */
            Assert.IsFalse(matches);
        }

        [Test]
        [TestCase(1)] // Unown A
        [TestCase(33)] // Deoxys Normal
        [TestCase(34)] // Deoxys Attack
        [TestCase(76)] // Geodude or something Alola
        [TestCase(121)] // Spinda 08
        public void Test_PokemonForm_ReturnsIsTrue(int formId)
        {
            var forms = new List<string>
            {
                "A",
                "Normal",
                "Attack",
                "Alola",
                "08",
            };
            var form = Translator.Instance.GetFormName((uint)formId, includeNormal: true);
            var matches = forms.Contains(form) || forms.Count == 0;
            Assert.IsTrue(matches);
        }

        [Test]
        [TestCase(1)] // Unown A
        [TestCase(33)] // Deoxys Normal
        [TestCase(34)] // Deoxys Attack
        [TestCase(76)] // Geodude or something Alola
        [TestCase(121)] // Spinda 08
        public void Test_PokemonForm_ReturnsIsFalse(int formId)
        {
            var forms = new List<string>
            {
                "Holiday",
                //"",
                /*
                "A",
                "Normal",
                "Attack",
                "Alola",
                "08",
                */
            };
            var form = Translator.Instance.GetFormName((uint)formId, includeNormal: false);
            var matches = forms.Contains(form) || forms.Count == 0;
            Assert.IsFalse(matches);
        }

        [Test]
        [TestCase(1)] // Unown A
        public void Test_PokemonFormsEmpty_ReturnsIsTrue(int formId)
        {
            var forms = new List<string>();
            var form = Translator.Instance.GetFormName((uint)formId, includeNormal: false);
            var matches = forms.Contains(form) || forms.Count == 0;
            Assert.IsTrue(matches);
        }

        [Test]
        [TestCase(2)] // Unown B
        public void Test_PokemonFormsNull_ReturnsIsTrue(int formId)
        {
            List<string> forms = null;
            var form = Translator.Instance.GetFormName((uint)formId, includeNormal: false);
            var matches = forms?.Contains(form) ?? true || forms?.Count == 0;
            Assert.IsTrue(matches);
        }

        [Test]
        public void Test_PokemonFilter_ReturnsIsTrue()
        {
            /*
            TODO: Test PokemonFilter
            if (!(
                (!hasIVStats && matchesIV && matchesLvl && matchesGender) ||
                (hasIVStats && matchesIVList)
                ))
                continue;
             */
             Assert.IsTrue(true);
        }

        [Test]
        [TestCase(15, 15, 15)]
        [TestCase(0, 0, 0)]
        [TestCase(1, 2, 3)]
        [TestCase(1, 0, 0)]
        // TODO: Test for null
        public void Test_PokemonIVList_ReturnsIsTrue(int attack, int defense, int stamina)
        {
            var ivList = new List<string>
            {
                "15/15/15",
                "0/0/0",
                "1/2/3",
                "1/0/0",
            };
            var matches = ivList.Contains($"{attack}/{defense}/{stamina}");
            Assert.IsTrue(matches);
        }

        [Test]
        [TestCase(12, 14, 15)]
        [TestCase(1, 15, 15)]
        [TestCase(9, 0, 9)]
        [TestCase(1, 1, 1)]
        // TODO: Test for null
        public void Test_PokemonIVList_ReturnsIsFalse(int attack, int defense, int stamina)
        {
            var ivList = new List<string>
            {
                "15/15/15",
                "0/0/0",
                "1/2/3",
                "1/0/0",
            };
            var matches = ivList.Contains($"{attack}/{defense}/{stamina}");
            Assert.IsFalse(matches);
        }

        [Test]
        [TestCase("95.6%", 95, 100)]
        [TestCase("0%", 0, 0)]
        [TestCase("95.6%", 95, 100)]
        [TestCase("100", 100, 100)]
        [TestCase("?", 0, 0)]
        [TestCase("?", 0, 100)]
        public void Test_PokemonIV_ReturnsIsTrue(string iv, int minimumIV, int maximumIV)
        {
            var matches = Filters.MatchesIV(iv, (uint)minimumIV, (uint)maximumIV);
            Assert.IsTrue(matches);
        }

        [Test]
        [TestCase("?", 90, 100)]
        [TestCase("0%", 90, 100)]
        [TestCase("91.1%", 93, 100)]
        public void Test_PokemonIV_ReturnsIsFalse(string iv, int minimumIV, int maximumIV)
        {
            var matches = Filters.MatchesIV(iv, (uint)minimumIV, (uint)maximumIV);
            Assert.IsFalse(matches);
        }

        [Test]
        [TestCase(35, 35, 35)]
        [TestCase(1, 1, 1)]
        [TestCase(null, 0, 0)]
        public void Test_PokemonLevel_ReturnsIsTrue(int? lvl, int minimumLevel, int maximumLevel)
        {
            var matches = Filters.MatchesLvl((ushort?)lvl, (uint)minimumLevel, (uint)maximumLevel);
            Assert.IsTrue(matches);
        }

        [Test]
        [TestCase(29, 35, 35)]
        [TestCase(40, 20, 35)]
        [TestCase(null, 1, 1)]
        public void Test_PokemonLevel_ReturnsIsFalse(int? lvl, int minimumLevel, int maximumLevel)
        {
            var matches = Filters.MatchesLvl((ushort?)lvl, (uint)minimumLevel, (uint)maximumLevel);
            Assert.IsFalse(matches);
        }

        [Test]
        [TestCase(Gender.Female, Gender.Female)]
        [TestCase(Gender.Less, Gender.Female)]
        [TestCase(Gender.Unset, Gender.Male)]
        [TestCase(Gender.Unset, Gender.Less)]
        public void Test_PokemonGender_ReturnsIsTrue(Gender gender, Gender expected)
        {
            var matches = Filters.MatchesGender(gender, expected);
            Assert.IsTrue(matches);
        }

        [Test]
        [TestCase(Gender.Male, Gender.Unset)]
        [TestCase(Gender.Female, Gender.Male)]
        [TestCase(Gender.Female, Gender.Less)]
        public void Test_PokemonGender_ReturnsIsFalse(Gender gender, Gender expected)
        {
            var matches = Filters.MatchesGender(gender, expected);
            Assert.IsFalse(matches);
        }

        [Test]
        [TestCase(PokemonSize.Large, PokemonSize.All)]
        [TestCase(PokemonSize.Tiny, PokemonSize.Tiny)]
        [TestCase(PokemonSize.All, null)]
        [TestCase(PokemonSize.Big, null)]
        public void Test_PokemonSize_ReturnsIsTrue(PokemonSize size, PokemonSize? expected)
        {
            var matches = Filters.MatchesSize(size, expected);
            Assert.IsTrue(matches);
        }

        [Test]
        [TestCase(PokemonSize.All, PokemonSize.Large)]
        [TestCase(PokemonSize.Normal, PokemonSize.Small)]
        [TestCase(null, PokemonSize.Big)]
        public void Test_PokemonSize_ReturnsIsFalse(PokemonSize size, PokemonSize? expected)
        {
            var matches = Filters.MatchesSize(size, expected);
            Assert.IsFalse(matches);
        }

        [Test]
        [TestCase(1200, 1000, 9999)]
        [TestCase(1, 1, 1)]
        [TestCase(null, 0, 0)]
        public void Test_PokemonCP_ReturnsIsTrue(int? cp, int minimumCP, int maximumCP) =>
            Assert.IsTrue(Filters.MatchesCP((uint?)cp, (uint)minimumCP, (uint)maximumCP));

        [Test]
        [TestCase(1492, 1495, 1500)]
        [TestCase(400, 200, 350)]
        [TestCase(null, 1, 1)]
        public void Test_PokemonCP_ReturnsIsFalse(int? cp, int minimumCP, int maximumCP) =>
            Assert.IsFalse(Filters.MatchesCP((uint?)cp, (uint)minimumCP, (uint)maximumCP));
    }
}