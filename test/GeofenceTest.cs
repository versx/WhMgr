using System.IO;
using NUnit.Framework;
using WhMgr.Geofence;

namespace WhMgr.Test
{
    [TestFixture]
    public class GeofenceTests
    {
        private const string JsonGeofencesFolder = "JsonGeofences";
        private const string IniGeofencesFolder = "IniGeofences";

        [Test]
        public void TestLoadingJson()
        {
            var effectiveFolder = Path.Combine(TestContext.CurrentContext.TestDirectory, JsonGeofencesFolder);
            var geofences = GeofenceService.LoadGeofences(effectiveFolder);

            Assert.IsNotEmpty(geofences);
        }

        [Test]
        public void TestLoadingIni()
        {
            var effectiveFolder = Path.Combine(TestContext.CurrentContext.TestDirectory, IniGeofencesFolder);
            var geofences = GeofenceService.LoadGeofences(effectiveFolder);

            Assert.IsNotEmpty(geofences);
        }

        [TestCase(51.500730, -0.1246304, "London")]
        [TestCase(51.501438, -0.1419019, "London")]
        [TestCase(48.858823, 2.2946221, "Paris")]
        [TestCase(48.861023, 2.3368031, "Paris")]
        [TestCase(40.691242, -74.046787, null)]
        public void TestInsideJson(double latitude, double longitude, string expectedGeofence)
        {
            var effectiveFolder = Path.Combine(TestContext.CurrentContext.TestDirectory, JsonGeofencesFolder);
            var geofences = GeofenceService.LoadGeofences(effectiveFolder);
            var insideOf = GeofenceService.GetGeofence(geofences, new Location(latitude, longitude));

            if (!string.IsNullOrEmpty(expectedGeofence))
            {
                Assert.IsNotNull(insideOf);
                Assert.AreEqual(expectedGeofence, insideOf.Name);
            }
            else
            {
                Assert.IsNull(insideOf);
            }
        }

        [TestCase(51.500730, -0.1246304, "London")]
        [TestCase(51.501438, -0.1419019, "London")]
        [TestCase(48.858823, 2.2946221, "Paris")]
        [TestCase(48.861023, 2.3368031, "Paris")]
        [TestCase(40.691242, -74.046787, null)]
        public void TestInsideIni(double latitude, double longitude, string expectedGeofence)
        {
            var effectiveFolder = Path.Combine(TestContext.CurrentContext.TestDirectory, IniGeofencesFolder);
            var geofences = GeofenceService.LoadGeofences(effectiveFolder);
            var insideOf = GeofenceService.GetGeofence(geofences, new Location(latitude, longitude));

            if (!string.IsNullOrEmpty(expectedGeofence))
            {
                Assert.IsNotNull(insideOf);
                Assert.AreEqual(expectedGeofence, insideOf.Name);
            }
            else
            {
                Assert.IsNull(insideOf);
            }
        }
    }
}