namespace WhMgr.Test
{
    using System;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WhMgr.Configuration;
    using WhMgr.Services.Geofence;
    using WhMgr.Services.Geofence.Geocoding;

    [TestFixture]
    public class ReverseGeocodingTests
    {
        //https://nominatim.openstreetmap.org/reverse?lat=<value>&lon=<value>&<params>

        [SetUp]
        public void Setup()
        {
        }

        [TestCase(
            34.01,
            -117.01,
            "{{Results.[0].FormattedAddress}}",
            "13403 Canyon Crest Rd, Yucaipa, CA 92399, USA"
        )]
        public async Task TestGoogleReverseGeocoding(double lat, double lon, string schema, string address)
        {
            var reverseGeocoding = new ReverseGeocodingLookup(
                GetConfig(ReverseGeocodingProvider.GMaps, schema)
            );
            var googleAddress = await reverseGeocoding.GetAddressAsync(new Coordinate(lat, lon));
            Console.WriteLine($"Address: {googleAddress}");
            Assert.IsNotNull(googleAddress);
            Assert.IsNotEmpty(googleAddress);
            Assert.AreEqual(address, googleAddress);
        }

        [TestCase(
            34.01,
            -117.01,
            "{{Address.Road}} {{Address.State}} {{Address.Postcode}} {{Address.Country}}",
            //"{{DisplayName}}",
            "Canyon Terrace Drive California 92399 United States"
        )]
        public async Task TestNominatimReverseGeocoding(double lat, double lon, string schema, string address)
        {
            var reverseGeocoding = new ReverseGeocodingLookup(
                GetConfig(ReverseGeocodingProvider.Osm, schema)
            );
            var nominatimAddress = await reverseGeocoding.GetAddressAsync(new Coordinate(lat, lon));
            Console.WriteLine($"Address: {nominatimAddress}");
            Assert.IsNotNull(nominatimAddress);
            Assert.IsNotEmpty(nominatimAddress);
            Assert.AreEqual(address, nominatimAddress);
        }

        private static ReverseGeocodingConfig GetConfig(ReverseGeocodingProvider provider, string schema = null)
        {
            return new ReverseGeocodingConfig
            {
                Provider = provider,
                CacheToDisk = true,
                GoogleMaps = new GoogleMapsConfig
                {
                    Key = "<GOOGLE_MAPS_KEY>",
                    Schema = schema,
                },
                Nominatim = new NominatimConfig
                {
                    Endpoint = "https://nominatim.openstreetmap.org",
                    Schema = schema,
                }
            };
        }
    }
}