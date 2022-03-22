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

        [Test]
        public void Test_TemplatePvpRankings_ReturnsText()
        {
            var embedData = @"
{{#each pvp}}{{@key}}\n
    {{#each this}}
        {{rank}} {{pokemonName}} {{cp}}CP @ L{{level}} {{percentage}}%
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
                            PokemonName = "Oddish",
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