namespace WhMgr.Localization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using ActivityType = POGOProtos.Rpc.HoloActivityType;
    using AlignmentType = POGOProtos.Rpc.PokemonDisplayProto.Types.Alignment;
    using CharacterCategory = POGOProtos.Rpc.EnumWrapper.Types.CharacterCategory;
    using InvasionCharacter = POGOProtos.Rpc.EnumWrapper.Types.InvasionCharacter;
    using ItemId = POGOProtos.Rpc.Item;
    using TemporaryEvolutionId = POGOProtos.Rpc.HoloTemporaryEvolutionId;

    using WhMgr.Common;
    using WhMgr.Extensions;

    public class Translator : Language<string, string, Dictionary<string, string>>
    {
        private readonly string _appLocalesFolder = Directory.GetCurrentDirectory() + "/../static/locales";
        private readonly string _binLocalesFolder = Directory.GetCurrentDirectory() + $"/{Strings.BasePath}/static/locales";
        private readonly string _pogoLocalesFolder = Directory.GetCurrentDirectory() + "/../node_modules/pogo-translations/static/locales";

        #region Singleton

        private static Translator _instance;

        public static Translator Instance =>
            _instance ??= new Translator();

        #endregion

        public void CreateLocaleFiles()
        {
            var files = Directory.GetFiles(_appLocalesFolder, "*.json")
                                 .Select(x => Path.GetFileName(x))
                                 .ToList();
            var pogoLocalesFiles = new List<string>();
            if (Directory.Exists(_pogoLocalesFolder))
            {
                pogoLocalesFiles = Directory.GetFiles(_pogoLocalesFolder, "*.json")
                                            .Select(x => Path.GetFileName(x))
                                            .ToList();
            }

            foreach (var file in files)
            {
                // TODO: Filter by `_` prefix
                var locale = Path.GetFileName(file).Replace("_", null);
                var localeFile = locale;
                var translations = new Dictionary<string, string>();

                Console.WriteLine($"Creating locale {locale}");

                if (pogoLocalesFiles.Contains(localeFile))
                {
                    Console.WriteLine($"Found pogo-translations for locale {locale}");
                    var pogoTranslations = File.ReadAllText(Path.Combine(_pogoLocalesFolder, localeFile));
                    translations = pogoTranslations.FromJson<Dictionary<string, string>>();
                    foreach (var (key, value) in translations)
                    {
                        translations[key] = value.Replace("%", "{");
                        translations[key] = value.Replace("}", "}}");
                    }
                }

                if (locale != "en")
                {
                    // Include en as fallback first
                    var appTransFallback = File.ReadAllText(
                        Path.Combine(_appLocalesFolder, "_en.json")
                    );
                    var fallbackTranslations = appTransFallback.FromJson<Dictionary<string, string>>();
                    translations = MergeDictionaries(translations, fallbackTranslations);
                }

                var appTranslations = File.ReadAllText(Path.Combine(_appLocalesFolder, file));
                translations = MergeDictionaries(translations, appTranslations.FromJson<Dictionary<string, string>>());

                File.WriteAllText(
                    Path.Combine(_binLocalesFolder, localeFile),
                    translations.ToJson()
                );
                Console.WriteLine($"{localeFile} file saved.");
            }
        }


        public override string Translate(string value)
        {
            try
            {
                return base.Translate(value) ?? value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to find locale translation for key '{value}'");
                Console.WriteLine(ex);
            }
            return value;
        }

        public string Translate(string value, params object[] args)
        {
            try
            {
                var text = args?.Length > 0
                    ? string.Format(base.Translate(value), args)
                    : base.Translate(value);
                return text ?? value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to find locale translation for key '{value}' and arguments: '{string.Join(",", args)}'");
                Console.WriteLine(ex);
            }
            return value;
        }

        public string GetPokemonName(uint pokeId)
        {
            return Translate($"poke_{pokeId}");
        }

        public string GetFormName(uint formId, bool includeNormal = false)
        {
            if (formId == 0)
                return null;

            var form = Translate("form_" + formId);
            // TODO: Localize
            if (!includeNormal && string.Compare(form, "Normal", true) == 0)
                return string.Empty;
            return form;
        }

        public string GetCostumeName(uint costumeId)
        {
            if (costumeId == 0)
                return null;

            var costume = Translate("costume_" + costumeId);
            return costume;
        }

        public string GetEvolutionName(uint evoId)
        {
            if (evoId == 0)
                return null;

            var evo = Translate("evo_" + evoId);
            return evo;
        }

        public string GetMoveName(uint moveId)
        {
            if (moveId == 0)
                return "Unknown"; // TODO: Localize

            return Translate($"move_{moveId}");
        }

        public string GetThrowName(ActivityType throwTypeId)
        {
            return Translate($"throw_type_{(int)throwTypeId}");
        }

        public string GetItem(ItemId item)
        {
            return Translate($"item_{(int)item}");
        }

        public string GetWeather(WeatherCondition weather)
        {
            return Translate($"weather_{(int)weather}");
        }

        public string GetAlignmentName(AlignmentType alignment)
        {
            return Translate($"alignment_{(int)alignment}");
        }

        public string GetCharacterCategoryName(CharacterCategory category)
        {
            return Translate($"character_category_{(int)category}");
        }

        public string GetEvolutionName(TemporaryEvolutionId evolution)
        {
            return Translate($"evo_{(int)evolution}");
        }

        public string GetGruntType(InvasionCharacter gruntType)
        {
            return Translate($"grunt_{(int)gruntType}");
        }

        private static Dictionary<string, string> MergeDictionaries(Dictionary<string, string> locales1, Dictionary<string, string> locales2)
        {
            var result = locales1;
            foreach (var (key, value) in locales2)
            {
                if (result.ContainsKey(key))
                {
                    // Key already exists, skip...
                    continue;
                }
                result.Add(key, value);
            }
            return result;
        }
    }
}