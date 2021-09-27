namespace WhMgr.Test
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using WhMgr.Services.Pvp;

    [TestFixture]
    public class PvpRankTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase]
        public void TestPvpRankings()
        {
            var pvpRankCalc = new PvpRankCalculator();
            for (uint i = 1; i < 900; i++)
            {
                var pvpRanks = pvpRankCalc.QueryPvpRank(i, 0, 0, 15, 15, 15, 1, POGOProtos.Rpc.PokemonGender.Male);
                if (pvpRanks == null)
                    continue;

                foreach (var (league, ranks) in pvpRanks)
                {
                    foreach (var rank in ranks)
                    {
                        if (rank.Rank > 25)
                            continue;

                        switch (league)
                        {
                            //case "great":
                            //case "ultra":
                            case "little":
                                var value = new
                                {
                                    cp = rank.CP,
                                    rank = rank.Rank,
                                    percent = rank.Percentage,
                                    level = rank.Level,
                                    evo = rank.Evolution,
                                };
                                Console.WriteLine($"Pokemon: {i}, League: {league}, Ranks: {string.Join(", ", value)}");
                                break;
                        }
                    }
                }
            }
        }
    }
}