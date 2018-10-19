namespace T.Extensions
{
    using T.Net;

    public static class PokemonExtensions
    {
        public static string GetPokemonForm(this int pokeId, string formId)
        {
            if (!int.TryParse(formId, out int form)) return null;

            switch (pokeId)
            {
                case 201: //Unown
                    switch (form)
                    {
                        case 27:
                            return "!";
                        case 28:
                            return "?";
                        default:
                            return form.NumberToAlphabet(true).ToString();
                    }
                case 351: //Castform
                    switch (form)
                    {
                        case 29: //Normal
                            break;
                        case 30: //Sunny
                            return "Sunny";
                        case 31: //Water
                            return "Rain";
                        case 32: //Snow
                            return "Snow";
                    }
                    break;
                case 327: //Spinda
                case 386: //Deoxys
                    return "N/A";
            }

            return null;
        }

        public static string GetPokemonGenderIcon(this PokemonGender gender)
        {
            switch (gender)
            {
                case PokemonGender.Male:
                    return "♂";//\u2642
                case PokemonGender.Female:
                    return "♀";//\u2640
                default:
                    return "⚲";//?
            }
        }
    }
}