namespace WhMgr.Test
{
    using System.Collections.Generic;
    using System.IO;
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
            var localeFolder = Strings.BasePath + Strings.StaticFolder + Path.DirectorySeparatorChar + "locales";
            Translator.Instance.LocaleDirectory = localeFolder;
            //Translator.Instance.CreateLocaleFiles().ConfigureAwait(false).GetAwaiter().GetResult();
            Translator.Instance.SetLocale("en");
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
                "Holidy",
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