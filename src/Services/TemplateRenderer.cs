namespace WhMgr.Services
{
    using System;

    using HandlebarsDotNet;
    using HandlebarsDotNet.Helpers;

    public static class TemplateRenderer
    {
        private static readonly IHandlebars _context;

        static TemplateRenderer()
        {
            _context = Handlebars.Create();
            _context.Configuration.TextEncoder = null;
            _context.RegisterHelper("getPokemonName", new HandlebarsHelper((writer, ctx, args) =>
            {
                // TODO: GetPokemonName helper
            }));
            HandlebarsHelpers.Register(_context);
        }

        public static string Parse(string text, dynamic model)
        {
            var template = _context.Compile(text ?? string.Empty);
            return template(model);
        }
    }
}