namespace WhMgr.Test
{
    using System;
    using System.IO;

    using NUnit.Framework;

    using WhMgr.Utilities;

    [TestFixture]
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            // TODO: Test embeds/filters/alarms/discord config
            // TODO: Test datetime extensions
            // TODO: Test timespan extensions
            // TODO: Test notification limiter
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }

        [TestCase]
        public void Test_UrlAndFilePaths()
        {
            // Check if test file exists
            var path = Path.Combine(Directory.GetCurrentDirectory(), "../../../../src/Program.cs");
            var exists = File.Exists(path);
            Assert.IsTrue(exists);

            // Check if fetched data is not null
            var data = NetUtils.Get(path);
            Assert.IsNotNull(data);
        }
    }
}