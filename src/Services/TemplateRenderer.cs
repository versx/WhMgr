namespace WhMgr.Services
{
    using HandlebarsDotNet;

    public static class TemplateRenderer
    {
        public static string Parse(string text, dynamic model)
        {
            Handlebars.Configuration.TextEncoder = null;
            var template = Handlebars.Compile(text ?? string.Empty);
            return template(model);
        }
    }
}