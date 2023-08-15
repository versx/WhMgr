namespace WhMgr.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using NUnit.Framework;

    using WhMgr.Services;
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
            // TODO: Test pokemon and costume checks for webhooks and subscriptions
        }

        [Test]
        public void Test_RaidLevel_Generate()
        {
            var levels = Enumerable.Range(1, 8).Select(Convert.ToUInt16).ToList();
            Assert.IsTrue(levels.Count == 8);
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

        [TestCase]
        public void Test_GitHubApi()
        {
            var git = new VersionManager("versx/whmgr");
            var version = git.GetVersion();
            Console.WriteLine($"Version: {version}");
            var tags = git.Tags;
            Console.WriteLine($"Tags: {tags}");
        }
    }
}