namespace WhMgr.Test
{
    using System;
    using System.Collections.Generic;

    using NUnit.Framework;

    using WhMgr.Common;
    using WhMgr.Extensions;
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
        public void Test_TemplateGymPowerUpLevel_ReturnsText(int level)
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

        [TestCase(2)]
        [TestCase(0)]
        public void Test_TemplatePokestopPowerUpLevel_ReturnsText(int level)
        {
            var content = new List<string>
            {
                "{{#if has_lure}}**Lure Expires** {{lure_expire_time}} ({{lure_expire_time_left}} left)",
                "**Lure Type:** {{lure_type}}",
                "{{/if}}{{#if has_invasion}}**Expires:** {{invasion_expire_time}} ({{invasion_expire_time_left}} left)",
                "**Type:** {{grunt_type_emoji}} | **Gender:** {{grunt_gender}}",
                "**Encounter Reward Chance:**",
                "{{#each invasion_encounters}}",
                "{{chance}} - {{pokemon}}",
                "{{/each}}",
                "{{/if}}{{#if power_up_level}}**Power Level**",
                "Level: {{power_up_level}} | Points: {{power_up_points}}",
                "Time Left: {{power_up_end_time_left}}",
                "{{/if}}**[Google]({{gmaps_url}}) | [Apple]({{applemaps_url}}) | [Waze]({{wazemaps_url}}) | [Scanner]({{scanmaps_url}})**"
            };
            var embedData = string.Join("\r\n", content);
            var now = DateTime.Now;
            var expireTime = now.AddMinutes(10);
            var lureExpireTimeLeft = now.GetTimeRemaining(expireTime).ToReadableStringNoSeconds();
            var data = new
            {
                pokestop_id = "0011386f50d640c084b499d343af610b.16",
                latitude = 34.01,
                longitude = -117.01,
                name = "Test Stop",
                url = "http =//lh3.googleusercontent.com/ybUiI4LuqOw02mMiOSeXnLqVW0d1bJECu9IM5v86e5B6DlMbohrCzpBtRE8bNh5k0OENogqJUgkcBtmtKyPAIgHX_Zo",
                lure_expiration = 1654595380,
                last_modified = 1654568322,
                enabled = true,
                has_lure = true,
                lure_id = 501,
                lure_type = "Normal",
                lure_expire_time = expireTime.ToLongTimeString(),
                lure_expire_time_24h = expireTime.ToString("HH:mm:ss"),
                lure_expire_time_left = lureExpireTimeLeft,
                has_invasion = false,
                grunt_type = "Flying",
                pokestop_display = 0,
                incident_expire_timestamp = 1654595380,
                ar_scan_eligible = false,
                power_up_level = level,
                power_up_points = 250,
                power_up_end_timestamp = 1654595380,
                updated = 1654568322,
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