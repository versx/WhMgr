namespace WhMgr.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
    using TimeZoneConverter;

    using WhMgr.Extensions;

    [TestFixture]
    public class TimeZoneTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase("America/Los_Angeles", "Pacific Standard Time")]
        public void Test_TZ(string tzIana, string tzWindows)
        {
            var converted = tzWindows.ConvertIanaToWindowsTimeZone();
            Console.WriteLine($"Windows to Iana: {converted}");

            converted = tzIana.ConvertIanaToWindowsTimeZone();
            Console.WriteLine($"Iana to Windows: {converted}");

            Assert.Pass();
        }

        [TestCase("Coordinated Universal Time")]
        public void Test_CustomTZ_Pass(string timezone)
        {
            var tzInfo = timezone.GetTimeZoneInfoFromName(createUnknownTimeZone: true);
            Console.WriteLine($"TZ: {tzInfo}");

            var dateTime = DateTime.UtcNow.ConvertTimeFromTimeZone(timezone);
            Console.WriteLine($"Converted: {dateTime}");

            Assert.Pass();
        }

        [TestCase("Coordinated Universal Time")]
        public void Test_CreateCustomTZ_Pass(string timezone)
        {
            var tzInfo = timezone.GetTimeZoneInfoFromName(createUnknownTimeZone: true);
            Console.WriteLine($"TZ: {tzInfo.StandardName}");

            Assert.Pass();
        }
    }
}