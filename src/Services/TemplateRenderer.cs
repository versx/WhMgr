namespace WhMgr.Services
{
    using HandlebarsDotNet;

    public static class TemplateRenderer
    {
        public static string Parse(string text, dynamic model)
        {
            var template = Handlebars.Compile(text);
            return template(model);
        }
    }
}