namespace WhMgr.Test
{
    using NUnit.Framework;

    using WhMgr.Services.Geofence;

    [TestFixture]
    public class ReverseGeocodingTests
    {
        //https://nominatim.openstreetmap.org/reverse?lat=<value>&lon=<value>&<params>

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestGoogleReverseGeocoding()
        {
            // TODO: Test Google
            //Assert.IsNotEmpty(googleAddress);
        }

        [TestCase(
            34.01,
            -117.01,
            "{{Address.Road}} {{Address.State}} {{Address.Postcode}} {{Address.Country}}",
            "Canyon Crest Road California 92399 United States"
        )]
        public void TestNominatimReverseGeocoding(double lat, double lon, string schema, string address)
        {
            var baseUrl = "https://nominatim.openstreetmap.org";
            var nominatimAddress = Coordinate.GetNominatimAddress("Test", lat, lon, baseUrl, schema);
            //Console.WriteLine($"Nominatim address: {nominatimAddress}");
            Assert.IsNotNull(nominatimAddress);
            Assert.IsNotEmpty(nominatimAddress?.Address);
            Assert.AreEqual(address, nominatimAddress?.Address);
        }
    }
}