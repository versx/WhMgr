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
                form = form,
                iv = 100
            };
            var result = TemplateRenderer.Parse(template, templateModel);
            Assert.IsNotEmpty(result);
            Assert.AreEqual($"{pokemon} {form} 100%", result);
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