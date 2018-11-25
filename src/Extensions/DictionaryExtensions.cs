namespace WhMgr.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class DictionaryExtensions
    {
        public static IEnumerable<KeyValuePair<int, int>> GroupWithCount(this Dictionary<int, int> dict, int first = 25)
        {
            return (from entry in dict orderby entry.Value descending select entry).Take(first);
        }
    }
}