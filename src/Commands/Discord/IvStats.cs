namespace WhMgr.Commands.Discord
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using Microsoft.EntityFrameworkCore;

    using WhMgr.Configuration;
    using WhMgr.Data;
    using WhMgr.Data.Factories;
    using WhMgr.Extensions;
    using WhMgr.Localization;

    public class IvStats
    {
        private readonly ConfigHolder _config;

        public IvStats(ConfigHolder config)
        {
            _config = config;
        }
    }
}
