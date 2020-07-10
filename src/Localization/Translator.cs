namespace WhMgr.Localization
{
    using System.Collections.Generic;

    using WhMgr.Net.Models;

    public class Translator : Language<string, string, Dictionary<string, string>>
    {
        #region Singleton

        private static Translator _instance;
        
        public static Translator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Translator();
                }
                return _instance;
            }
        }

        #endregion

        public override string Translate(string value)
        {
            return base.Translate(value);
        }

        public string Translate(string value, params object[] args)
        {
            return args.Length > 0
                ? string.Format(base.Translate(value), args)
                : base.Translate(value);
        }

        public string GetPokemonName(int pokeId)
        {
            return Translate($"poke_{pokeId}");
        }

        public string GetFormName(int formId)
        {
            if (formId == 0)
                return null;

            var form = Translate("form_" + formId);
            // TODO: Localize
            if (string.Compare(form, "Normal", true) == 0)
                return string.Empty;
            return form;
        }

        public string GetMoveName(int moveId)
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
    }
}