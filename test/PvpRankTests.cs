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
                var ranks = pvpRankCalc.QueryPvpRank(i, 0, 0, 15, 15, 15, 1, POGOProtos.Rpc.PokemonGender.Male);
                if (ranks == null)
                    continue;

                foreach (var rank in ranks)
                {
                    switch (rank.Key)
                    {
                        //case "great":
                        //case "ultra":
                        case "little":
                            var value = rank.Value.Select(x => new
                            {
                                cp = x.CP,
                                rank = x.Rank,
                                percent = x.Percentage,
                                level = x.Level,
                                evo = x.Evolution,
                            });
                            Console.WriteLine($"Pokemon: {i}, League: {rank.Key}, Ranks: {string.Join(", ", value)}");
                            break;
                    }
                }
            }
        }
    }
}