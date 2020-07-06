namespace WhMgr.Utilities
{
    using System;
    using System.IO;

    using DotLiquid;

    public static class Renderer
    {
        public static string Parse(string path, dynamic model)
        {
            var templateData = ReadData(path);
            var template = Template.Parse(templateData);
            var result = template.Render(Hash.FromAnonymousObject(model));
            return result;
        }

        private static string ReadData(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Template does not exist at path: {path}", path);
            }

            using (var sr = new StreamReader(path))
            {
                return sr.ReadToEnd();
            }
        }
    }
}