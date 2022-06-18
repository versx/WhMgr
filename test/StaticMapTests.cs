namespace WhMgr.Test
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Net;
    using NUnit.Framework;

    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.Services.StaticMap;

    [TestFixture]
    internal class StaticMapTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test_StaticMapPokemon_ReturnsIsTrue()
        {
            var gyms = new List<dynamic>
            {
                new
                {
                    lat = 34.010493,
                    lon = -117.010713,
                    team = "Valor",
                    marker = "https://raw.githubusercontent.com/nileplumb/PkmnHomeIcons/master/UICONS/gym/1.png",
                }
            };
            var pokestops = new List<dynamic>
            {
                new
                {
                    lat = 34.009608,
                    lon = -117.008835,
                    marker = "https://raw.githubusercontent.com/nileplumb/PkmnHomeIcons/master/UICONS/pokestop/0.png",
                }
            };
            var baseUrl = "http://10.0.0.2:9000";
            //var baseUrl = "http://10.0.0.2:43200";
            var lat = 34.01;
            var lon = -117.01;
            var url2 = "https://raw.githubusercontent.com/nileplumb/PkmnHomeIcons/master/UICONS/pokemon/201_f9.png";
            var staticMap = new StaticMapGenerator(new StaticMapOptions
            {
                BaseUrl = baseUrl,
                MapType = StaticMapType.Pokemon,
                TemplateType = StaticMapTemplateType.StaticMap,
                Latitude = lat,
                Longitude = lon,
                Gyms = gyms,
                Pokestops = pokestops,
                SecondaryImageUrl = url2,
                Team = Common.PokemonTeam.Valor,
                Pregenerate = true,
                Regeneratable = true,
            });
            var id = staticMap.GenerateLink();
            var url = baseUrl + "/staticmap/pregenerated/" + id;
            Console.WriteLine($"FinalUrl: {url}");
        }
    }
}