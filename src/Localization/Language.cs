namespace WhMgr.Localization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Xml;

    public class Translator : Language<string, string, Dictionary<string, string>>
    {
        public override string Translate(string value)
        {
            return base.Translate(value);
        }
    }

    public class Language<TFrom, TTo, TDictionary> : IEnumerable<KeyValuePair<TFrom, TTo>>
        where TDictionary : IDictionary<TFrom, TTo>, new()
    {
        private const string DefaultLanguage = "en";

        // The translation table
        private IDictionary<TFrom, TTo> _map;

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public CultureInfo CurrentCulture { get; set; }

        /// <summary>
        /// Gets the two letter ISO country code.
        /// </summary>
        /// <value>The two letter ISO country code.</value>
        public string CountryCode => CurrentCulture?.TwoLetterISOLanguageName ?? DefaultLanguage;

        /// <summary>
        /// Gets or sets the locale directory.
        /// </summary>
        /// <value>The locale directory.</value>
        public string LocaleDirectory { get; set; }

        /// <summary>
        /// Property to get/set the default to value if a lookup fails
        /// </summary>
        /// <value>The default value.</value>
        public TTo DefaultValue { get; set; }

        /// <summary>
        /// Property that returns the number of elements in the lookup table
        /// </summary>
        /// <value>The translation count.</value>
        public int TranslationCount => _map.Count;

        /// <summary>
        /// Indexes the translator.  On get it will lookup the type calling
        /// Translate.  On set it will Add a new lookup with the given value.
        /// </summary>
        /// <param name="index">Index.</param>
        public TTo this[TFrom index]
        {
            get => Translate(index);
            set => Add(index, value);
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Create a translator using system defaults for the type (NULL if reference
        /// type and ZERO for value types)
        /// </summary>
        public Language()
            : this(default)
        {
            CurrentCulture = new CultureInfo(DefaultLanguage);
            LocaleDirectory = Strings.LocaleFolder;

            _map = LoadCountry(CurrentCulture.TwoLetterISOLanguageName);
        }

        /// <summary>
        /// Create a translator with a default to and from value specified
        /// </summary>
        /// <param name="defaultValue">Default value.</param>
        public Language(TTo defaultValue)
        {
            DefaultValue = defaultValue;

            _map = new TDictionary();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the current locale.
        /// </summary>
        /// <param name="localeCode"></param>
        public void SetLocale(string localeCode)
        {
            CurrentCulture = new CultureInfo(localeCode);
            _map = LoadCountry(localeCode);
        }

        /// <summary>
        /// Performs a translation using the table, returns the default from value
        /// if cannot find a matching result.
        /// </summary>
        /// <returns>The translate.</returns>
        /// <param name="value">Value.</param>
        public virtual TTo Translate(TFrom value)
        {
            // loop through table looking for result
#pragma warning disable RECS0017 // Possible compare of value type with 'null'
            if (value == null || !_map.TryGetValue(value, out TTo result))
#pragma warning restore RECS0017 // Possible compare of value type with 'null'
            {
                result = DefaultValue;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(TFrom name, TTo value)
        {
            if (!Exists(name))
            {
                _map.Add(name, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Exists(TFrom name)
        {
            return _map.ContainsKey(name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Remove(TFrom name)
        {
            if (Exists(name))
            {
                _map.Remove(name);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clears all existing translations and defaults
        /// </summary>
        public void Clear()
        {
            _map.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="languageName"></param>
        /// <returns></returns>
        public string GetTwoLetterIsoLanguageName(string languageName)
        {
            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
            {
                if (string.Compare(ci.EnglishName, languageName, true) == 0 ||
                    string.Compare(ci.DisplayName, languageName, true) == 0)
                {
                    return ci.TwoLetterISOLanguageName;
                }
            }

            return languageName;
        }

        /// <summary>
        /// Switchs the language.
        /// </summary>
        /// <param name="localeCode">Locale code.</param>
        public void SwitchLanguage(string localeCode)
        {
            CurrentCulture = new CultureInfo(localeCode);
            Console.WriteLine($"[INFO] Switching current UI language to {CurrentCulture.DisplayName}...");

            string path = Path.Combine(LocaleDirectory, CountryCode + ".xml");
            if (!File.Exists(path))
                return;

            //if (_settings != null)
            //{
            //    _settings.ConfigPath = path;
            //    _settings.LoadSettings();
            //}
        }

        /// <summary>
        /// Get an enumerator to walk through the list
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<KeyValuePair<TFrom, TTo>> GetEnumerator()
        {
            return _map.GetEnumerator();
        }

        /// <summary>
        /// Get an enumerator to walk through the list
        /// </summary>
        /// <returns>The collections. IE numerable. get enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Private Methods

        private TDictionary LoadCountry(string localeCode)
        {
            CurrentCulture = new CultureInfo(localeCode);

            var path = Path.Combine(LocaleDirectory, localeCode + ".xml");
            var data = File.ReadAllText(path);
            var xml = new XmlDocument();
            xml.LoadXml(data);
            var resources = xml.SelectNodes("resources/string");
            var dict = new TDictionary();

            foreach (XmlNode node in resources)
            {
                var key = StringToObject<TFrom>(node.Attributes["name"].Value);
                var val = StringToObject<TTo>(node.InnerText);
                dict.Add(key, val);
            }

            return dict;
        }

        /// <summary>
        /// Converts a string to its object representation.
        /// </summary>
        /// <typeparam name="T">The type of object to convert the string to.</typeparam>
        /// <param name="value">The actual string value.</param>
        /// <returns>Returns an object relating to the converted string.</returns>
        private static T StringToObject<T>(string value)
        {
            var tc = TypeDescriptor.GetConverter(typeof(T));
            return (T)tc.ConvertFromString(value);
        }

        ///// <summary>
        ///// Converts an object to its string representation.
        ///// </summary>
        ///// <typeparam name="T">The type of object to convert.</typeparam>
        ///// <param name="value">The actual object value.</param>
        ///// <returns>Returns the string representation of the converted object.</returns>
        //private static string ObjectToString<T>(T value)
        //{
        //    var tc = TypeDescriptor.GetConverter(typeof(T));
        //    return tc.ConvertToString(value);
        //}

        #endregion
    }
}