namespace WhMgr.Services.Alarms.Embeds
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public static class DynamicReplacementEngine
    {
        /// <summary>
        /// Replace text placeholders with Pokemon values
        /// </summary>
        /// <param name="alarmText">Placeholder alarm text</param>
        /// <param name="pkmnInfo">Replacement values dictionary</param>
        /// <returns></returns>
        public static string ReplaceText(string alarmText, IReadOnlyDictionary<string, string> pkmnInfo)
        {
            if (string.IsNullOrEmpty(alarmText))
                return string.Empty;

            var placeHolder = alarmText;

            // Loop through all available keys, replace any place holders with values.
            foreach (var (key, value) in pkmnInfo)
            {
                placeHolder = placeHolder.Replace($"<{key}>", value);
            }

            // Replace IF statement blocks i.e. <#is_ditto>**Catch Pokemon:** <original_pkmn_name></is_ditto>. If value is true return value inside IF block, otherwise return an empty string.
            foreach (var (key, value) in pkmnInfo)
            {
                if (bool.TryParse(value, out var result))
                {
                    placeHolder = ReplaceBlock(placeHolder, key, result);
                }
            }
            return placeHolder;
        }

        /// <summary>
        /// Replace conditional block with value within itself
        /// </summary>
        /// <param name="text">Placeholder text to check</param>
        /// <param name="property">Property key</param>
        /// <param name="value">Default replacement value</param>
        /// <returns></returns>
        private static string ReplaceBlock(string text, string property, bool value = false)
        {
            var expr = @"\<#" + property + @">([^\}]+)\</" + property + @">";
            var regex = new Regex(expr);
            var match = regex.Match(text);
            return string.IsNullOrEmpty(match?.Value) ?
                text :
                text.Replace(match.Value, value ?
                    match?.Groups[1]?.Value :
                    string.Empty
            );
        }
    }
}