namespace WhMgr.Localization
{
    using System.Collections.Generic;

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
    }
}