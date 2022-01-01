namespace WhMgr.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using HandlebarsDotNet;
    using HandlebarsDotNet.Helpers;

    using WhMgr.Common;
    using WhMgr.Configuration;
    using WhMgr.Localization;

    public static class TemplateRenderer
    {
        private static readonly IHandlebars _context;

        static TemplateRenderer()
        {
            _context = Handlebars.Create();
            _context.Configuration.TextEncoder = null;

            // Register helpers
            var helpers = GetHelpers();
            foreach (var (name, function) in helpers)
            {
                _context.RegisterHelper(name, function);
            }
            HandlebarsHelpers.Register(_context);
        }

        public static string Parse(string text, dynamic model)
        {
            var template = _context.Compile(text ?? string.Empty);
            return template(model);
        }

        public static IReadOnlyDictionary<string, HandlebarsHelper> GetHelpers()
        {
            var dict = new Dictionary<string, HandlebarsHelper>
            {
                // GetPokemonName helper
                ["getPokemonName"] = new HandlebarsHelper((writer, ctx, args) =>
                {
                    if (!uint.TryParse(args[0].ToString(), out var pokeId))
                        return;
                    var pkmnName = Translator.Instance.GetPokemonName(pokeId);
                    writer.Write(pkmnName);
                }),
                // GetFormName helper
                ["getFormName"] = new HandlebarsHelper((writer, ctx, args) =>
                {
                    if (!uint.TryParse(args[0].ToString(), out var formId))
                        return;
                    var formName = Translator.Instance.GetFormName(formId);
                    writer.Write(formName);
                }),
                // GetCostumeName helper
                ["getCostumeName"] = new HandlebarsHelper((writer, ctx, args) =>
                {
                    if (!uint.TryParse(args[0].ToString(), out var costumeId))
                        return;
                    var costumeName = Translator.Instance.GetCostumeName(costumeId);
                    writer.Write(costumeName);
                }),
                // GetLength helper
                ["len"] = new HandlebarsHelper((writer, ctx, args) =>
                {
                    Console.WriteLine($"Type: {args[0].GetType().FullName}, Arg[0]: {args[0]}");
                    if (args[0] is IDictionary<ulong, DiscordServerConfig> discords)
                    {
                        writer.Write(discords.Count);
                    }
                    else
                    {
                        writer.Write(0);
                    }
                }),
                // Format boolean value helper
                ["formatBool"] = new HandlebarsHelper((writer, ctx, args) =>
                {
                    if (!bool.TryParse(args[0].ToString(), out var result))
                    {
                        writer.Write("No");
                        return;
                    }
                    writer.Write(result ? "Yes" : "No");
                }),
                ["isSelected"] = new HandlebarsHelper((writer, ctx, args) =>
                {
                    if (args[0] is System.Collections.IEnumerable array)
                    {
                        var item = args[1].ToString();
                        bool result;
                        if (array is List<WeatherCondition> weatherList)
                        {
                            result = weatherList.Select(x => x.ToString())
                                                .Contains(item);
                        }
                        else
                        {
                            result = array.Cast<string>().Contains(item);
                        }
                        writer.Write(result ? "selected" : "");
                    }
                    else if (args[0] is object obj)
                    {
                        var item = args[1];
                        var result = string.Equals(obj.ToString(), item.ToString());
                        writer.Write(result ? "selected" : "");
                    }
                    else
                    {
                        writer.Write("");
                    }
                }),
            };
            // TODO: Load helpers via file
            return dict;
        }
    }
}