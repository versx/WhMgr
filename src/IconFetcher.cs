namespace WhMgr
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;

    using WhMgr.Net.Models;
    using WhMgr.Utilities;

    class IconFetcher
    {
        private static readonly IconSet _availableForms = new IconSet();
        private static Dictionary<string, string> _iconStyles;

        #region Singleton

        private static IconFetcher _instance;

        public static IconFetcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new IconFetcher();
                }
                return _instance;
            }
        }

        #endregion

        public string GetPokemonIcon(string style, int pokemonId, int form = 0, int evolution = 0, PokemonGender gender = PokemonGender.Unset, int costume = 0, bool shiny = false)
        {
            if (!_availableForms.ContainsKey(style))
            {
                return _iconStyles[style] + "0.png"; // Substitute Pokemon
            }
            var evolutionSuffixes = (evolution > 0 ? new [] { "-e" + evolution, string.Empty }   : new string[] { string.Empty }).ToList();
            var formSuffixes      = (form      > 0 ? new [] { "-f" + form, string.Empty }        : new string[] { string.Empty }).ToList();
            var costumeSuffixes   = (costume   > 0 ? new [] { "-c" + costume, string.Empty }     : new string[] { string.Empty }).ToList();
            var genderSuffixes    = (gender    > 0 ? new [] { "-g" + (int)gender, string.Empty } : new string[] { string.Empty }).ToList();
            var shinySuffixes     = (shiny         ? new [] { "-shiny", string.Empty }           : new string[] { string.Empty }).ToList();
            foreach (var evolutionSuffix in evolutionSuffixes)
            {
                foreach (var formSuffix in formSuffixes)
                {
                    foreach (var costumeSuffix in costumeSuffixes)
                    {
                        foreach (var genderSuffix in genderSuffixes)
                        {
                            foreach (var shinySuffix in shinySuffixes)
                            {
                                var result = $"{pokemonId}{evolutionSuffix}{formSuffix}{costumeSuffix}{genderSuffix}{shinySuffix}";
                                if (_availableForms[style].Contains(result))
                                {
                                    return _iconStyles[style] + result + ".png";
                                }
                            }
                        }
                    }
                }
            }
            return _iconStyles[style] + "0.png"; // Substitute Pokemon
        }

        public void BuildFormLists(Dictionary<string, string> iconStyles)
        {
            _iconStyles = iconStyles;
            // Get available forms from remote icons repo to build form list for each icon style
            foreach (var style in _iconStyles)
            {
                // Check if style already checked, if so skip
                if (_availableForms.ContainsKey(style.Key))
                    continue;

                // Get the remote form index file from the icon repository
                var formsListJson = NetUtil.Get(style.Value + "index.json");
                if (string.IsNullOrEmpty(formsListJson))
                {
                    // Failed to get form list, add empty form set and skip to the next style
                    _availableForms.Add(style.Key, new HashSet<string>());
                    continue;
                }
                // Deserialize json list to hash set
                var formsList = JsonConvert.DeserializeObject<HashSet<string>>(formsListJson);
                // Add style and form list
                _availableForms.Add(style.Key, formsList);
            }
        }
    }

    class IconSet : Dictionary<string, HashSet<string>>
    {
    }
}