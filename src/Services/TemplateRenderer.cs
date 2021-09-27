namespace WhMgr.Services
{
    using HandlebarsDotNet;
    using HandlebarsDotNet.Helpers;

    using WhMgr.Data;
    using WhMgr.Localization;

    public static class TemplateRenderer
    {
        private static readonly IHandlebars _context;

        static TemplateRenderer()
        {
            _context = Handlebars.Create();
            _context.Configuration.TextEncoder = null;
            // GetPokemonName helper
            _context.RegisterHelper("getPokemonName", new HandlebarsHelper((writer, ctx, args) =>
            {
                if (!uint.TryParse(args[0]?.ToString(), out var pokeId))
                    return;
                var pkmnName = MasterFile.GetPokemon(pokeId).Name;
                writer.Write(pkmnName);
            }));
            // GetFormName helper
            _context.RegisterHelper("getFormName", new HandlebarsHelper((writer, ctx, args) =>
            {
                if (!uint.TryParse(args[0].ToString(), out var formId))
                    return;
                var formName = Translator.Instance.GetFormName(formId);
                writer.Write(formName);
            }));
            // GetCostumeName helper
            _context.RegisterHelper("getCostumeName", new HandlebarsHelper((writer, ctx, args) =>
            {
                if (!uint.TryParse(args[0].ToString(), out var costumeId))
                    return;
                var costumeName = Translator.Instance.GetCostumeName(costumeId);
                writer.Write(costumeName);
            }));
            // TODO: Add other helpers
            HandlebarsHelpers.Register(_context);
        }

        public static string Parse(string text, dynamic model)
        {
            var template = _context.Compile(text ?? string.Empty);
            return template(model);
        }
    }
}