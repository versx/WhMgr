namespace WhMgr.Test
{
    using System;
    using System.Collections.Generic;

    using NUnit.Framework;

    using WhMgr.Common;
    using WhMgr.Services;
    using WhMgr.Services.Webhook.Models;

    [TestFixture]
    public class TemplateTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase(0)]
        [TestCase(1)]
        public void Test_TemplatePowerUpLevel_ReturnsText(int level)
        {
            var content = new List<string>
            {
                "{{#if team_changed}}Gym changed from {{old_gym_team_emoji}} {{old_gym_team}} to {{gym_team_emoji}} {{gym_team}}",
                "{{/if}}{{#if in_battle}}Gym is under attack!",
                "{{/if}}**Slots Available:** {{slots_available}}",
                "{{#if power_up_level}}**Power Level**",
                "Level: {{power_up_level}} | Points: {{power_up_points}}",
                "Time Left: {{power_up_end_time_left}}",
                "{{/if}}{{#if is_ex}}{{ex_gym_emoji}} Gym!",
                "{{/if}}**[Google]({{gmaps_url}}) | [Apple]({{applemaps_url}}) | [Waze]({{wazemaps_url}}) | [Scanner]({{scanmaps_url}})**"
            };
            var embedData = string.Join("\r\n", content);
            var data = new
            {
                team_changed = true,
                old_gym_team = "Mystic",
                gym_team = "Valor",
                in_battle = true,
                slots_available = 3,
                power_up_level = level,
                power_up_points = 100,
                power_up_end_timestamp = 1234567890,
            };
            var templateData = TemplateRenderer.Parse(embedData, data);
            Console.WriteLine($"Template data: {templateData}");
        }

        [Test]
        public void Test_TemplatePvpRankings_ReturnsText()
        {
            var embedData = @"
{{#each pvp}}{{@key}}\n
    {{#each this}}
        {{rank}} {{getPokemonName pokemonId}} {{getFormName formId}} {{cp}}CP @ L{{level}} {{formatPercentage percentage}}%
    {{/each}}
{{/each}}
";
            var pvpRanks = new Dictionary<PvpLeague, List<PvpRankData>>
            {
                {
                    PvpLeague.Great, new List<PvpRankData>
                    {
                        new PvpRankData
                        {
                            CP = 1420,
                            FormId = 0,
                            Level = 20,
                            PokemonId = 43,
                            Rank = 1,
                            Percentage = 99.05,
                            CompetitionRank = 1,
                            DenseRank = 1,
                            OrdinalRank = 1,
                        },
                    }
                },
            };
            var templateData = TemplateRenderer.Parse(embedData, new { pvp = pvpRanks });
            Console.WriteLine($"Template data: {templateData}");
        }

        [Test]
        [TestCase("Pikachu", "Witch Hat")]
        [TestCase("Charmander", "")]
        [TestCase("Squirtle", null)]
        public void Test_TemplateBasic_ReturnsIsEqual(string pokemon, string form)
        {
            var template = "{{name}} {{#if form}}{{form}}{{/if}} {{iv}}%";
            var templateModel = new
            {
                name = pokemon,
                form,
                iv = 100
            };
            var result = TemplateRenderer.Parse(template, templateModel);
            Assert.IsNotEmpty(result);
            Assert.AreEqual($"{pokemon} {form} 100%", result);
        }

        [Test]
        [TestCase("Visit <#1234567890> for more info")]
        [TestCase("This Pokemon was IV scanned. For access to IV channels, please consider taking a look at <#286309264446849025> for more information!")]
        public void Test_TemplateHtmlChars_ReturnsTrue(string text)
        {
            var templateModel = new
            {
                test = "John",
            };
            var result = TemplateRenderer.Parse(text, templateModel);
            Assert.IsNotEmpty(result);
            Assert.AreEqual(text, result);
        }

        [Test]
        public void Test_TemplateForEach_ReturnsIsEqual()
        {
            var template = "{{#each products}}<b>{{name}}</b> {{type}}<br>{{/each}}";
            var templateModel = new
            {
                products = new List<dynamic>
                {
                    new
                    {
                        name = "Charmander",
                        type = "Fire",
                    },
                    new
                    {
                        name = "Diglett",
                        type = "Ground",
                    },
                },
            };
            var result = TemplateRenderer.Parse(template, templateModel);
            Assert.IsNotEmpty(result);
            Assert.AreEqual("<b>Charmander</b> Fire<br><b>Diglett</b> Ground<br>", result);
        }
    }
}