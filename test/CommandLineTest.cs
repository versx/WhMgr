namespace WhMgr.Test
{
    /*
    using NUnit.Framework;

    [TestFixture]
    public class CommandLineTest
    {
        [Test]
        public void TestParseArgs()
        {
            var prefixes = new[] { "--", "-" };
            var args = new[] { "--name", "TestName", "-c", "config.json" };
            var parsedArgs = CommandLine.ParseArgs(prefixes, args);
            var name = parsedArgs["name"];
            var config = parsedArgs["c"];

            Assert.IsNotNull(name);
            Assert.IsTrue(string.Compare(name?.ToString(), args[1], true) == 0);

            Assert.IsNotNull(config);
            Assert.IsTrue(string.Compare(config?.ToString(), args[3], true) == 0);
        }
    }
    */
}