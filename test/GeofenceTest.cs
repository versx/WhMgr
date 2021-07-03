namespace WhMgr.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using NUnit.Framework;

    using WhMgr.Services.Geofence;

    [TestFixture]
    public class GeofenceTests
    {
        private const string JsonGeofencesFolder = "JsonGeofences";
        private const string IniGeofencesFolder = "IniGeofences";

        private static IEnumerable<Geofence> LoadGeofences(string geofencesFolder)
        {
            var geofences = new List<Geofence>();

            foreach (var file in Directory.EnumerateFiles(geofencesFolder))
            {
                try
                {
                    var fileGeofences = Geofence.FromFile(file);
                    geofences.AddRange(fileGeofences);
                }
                catch (Exception ex)
                {
                    TestContext.Error.WriteLine($"Could not load Geofence file {file}:");
                    TestContext.Error.WriteLine(ex);
                }
            }

            return geofences;
        }

        [Test]
        public void TestLoadingJson()
        {
            var effectiveFolder = Path.Combine(TestContext.CurrentContext.TestDirectory, JsonGeofencesFolder);
            var geofences = LoadGeofences(effectiveFolder);

            Assert.IsNotEmpty(geofences);
        }

        [Test]
        public void TestLoadingIni()
        {
            var effectiveFolder = Path.Combine(TestContext.CurrentContext.TestDirectory, IniGeofencesFolder);
            var geofences = LoadGeofences(effectiveFolder);

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
            var geofences = LoadGeofences(effectiveFolder);
            var insideOf = GeofenceService.GetGeofences(geofences, new Coordinate(latitude, longitude)).ToList();

            if (!string.IsNullOrEmpty(expectedGeofence))
            {
                Assert.IsNotNull(insideOf);
                Assert.IsNotEmpty(insideOf);
                Assert.IsTrue(insideOf.Any(g => g.Name == expectedGeofence));
            }
            else
            {
                Assert.IsEmpty(insideOf);
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
            var geofences = LoadGeofences(effectiveFolder);
            var insideOf = GeofenceService.GetGeofences(geofences, new Coordinate(latitude, longitude)).ToList();

            if (!string.IsNullOrEmpty(expectedGeofence))
            {
                Assert.IsNotNull(insideOf);
                Assert.IsNotEmpty(insideOf);
                Assert.IsTrue(insideOf.Any(g => g.Name == expectedGeofence));
            }
            else
            {
                Assert.IsEmpty(insideOf);
            }
        }
    }
}