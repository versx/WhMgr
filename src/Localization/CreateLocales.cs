namespace WhMgr.Localization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using WhMgr.Extensions;

    public class CreateLocales
    {
        private readonly string _appLocalesFolder = "";
        private readonly string _binLocalesFolder = "";
        private readonly string _pogoLocalesFolder = "";

        public void CreateLocaleFiles()
        {
            var files = Directory.GetFiles(_appLocalesFolder, ".json");
            var pogoLocalesFiles = new List<string>();
            if (File.Exists(_pogoLocalesFolder))
            {
                pogoLocalesFiles = Directory.GetFiles(".").ToList();
            }

            foreach (var file in files)
            {
                // TODO: Filter by `_` prefix
                var locale = file.Replace("_", null);
                var localeFile = locale + ".json";
                var translations = new Dictionary<string, string>();

                Console.WriteLine($"Creating locale {locale}");

                if (pogoLocalesFiles.Contains(localeFile))
                {
                    Console.WriteLine($"Found pogo-translations for locale {locale}");
                    var pogoTranslations = File.ReadAllText(Path.Combine(_pogoLocalesFolder, localeFile));
                    translations = pogoTranslations.FromJson<Dictionary<string, string>>();
                    // TODO: Loop translations relating key "%{ with {{ and %} with }}
                }

                if (locale != "en")
                {
                    // Include en as fallback first
                    var appTransFallback = File.ReadAllText(
                        Path.Combine(_appLocalesFolder, "_en.json")
                    );
                    // TODO: translations = Object.assign(translations, JSON.parse(appTransFallback.toString()));
                }

                var appTranslations = File.ReadAllText(Path.Combine(_appLocalesFolder, file));
                // TODO: translations = Object.assign(translations, JSON.parse(appTranslations.toString()));

                File.WriteAllText(
                    Path.Combine(_binLocalesFolder, localeFile),
                    translations.ToJson()
                );
                Console.WriteLine($"{localeFile} file saved.");
            }
        }
    }
}