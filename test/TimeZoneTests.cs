namespace WhMgr.Test
{
    using System;
    using NUnit.Framework;

    using WhMgr.Extensions;

    [TestFixture]
    public class TimeZoneTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase(1)]
        public void Test_TZ()
        {
            var tzIana = "America/Los_Angeles";
            var tzWindows = "Pacific Standard Time";

            var converted = tzWindows.ConvertTimeZone();
            Console.WriteLine($"Windows to Iana: {converted}");

            converted = tzIana.ConvertTimeZone();
            Console.WriteLine($"Iana to Windows: {converted}");

            Assert.Pass();
        }
    }
}