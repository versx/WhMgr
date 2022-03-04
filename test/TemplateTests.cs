namespace WhMgr.Test
{
    using System;
    using System.Collections.Generic;

    using NUnit.Framework;

    using WhMgr.Services;

    [TestFixture]
    public class TemplateTests
    {
        [SetUp]
        public void Setup()
        {
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