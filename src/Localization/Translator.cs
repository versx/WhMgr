namespace WhMgr.Localization
{
    using System.Collections.Generic;

    public class Translator : Language<string, string, Dictionary<string, string>>
    {
        public override string Translate(string value)
        {
            return base.Translate(value);
        }
    }
}