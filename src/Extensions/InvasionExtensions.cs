namespace WhMgr.Extensions
{
    using System;
    using System.Linq;

    using WhMgr.Data;
    using WhMgr.Net.Models;

    public static class InvasionExtensions
    {
        public static string GetPossibleInvasionEncounters(this TeamRocketInvasion invasion)
        {
            var toInt = new Func<string, int>(x =>
            {
                var val = x.Split('_')[0];
                if (!int.TryParse(val, out var result))
                {
                    Console.Error.WriteLine($"Failed to parse {val} as integer");
                }
                return result;
            });
            var first = string.Join(", ", invasion.Encounters.First.Select(x => MasterFile.GetPokemon(toInt(x), 0)?.Name));
            var second = string.Join(", ", invasion.Encounters.Second.Select(x => MasterFile.GetPokemon(toInt(x), 0)?.Name));
            //var third = string.Join(", ", invasion.Encounters.Third.Select(x => Database.Instance.Pokemon[x].Name));
            var msg = string.Empty;
            if (invasion.SecondReward)
            {
                //85%/15% Rate
                msg += $"85% - {first}\r\n";
                msg += $"15% - {second}\r\n";
            }
            else
            {
                //100% Rate
                msg += $"100% - {first}\r\n";
            }
            return msg;
        }
    }
}