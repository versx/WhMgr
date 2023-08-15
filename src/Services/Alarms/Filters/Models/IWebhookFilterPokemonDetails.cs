namespace WhMgr.Services.Alarms.Filters.Models
{
    using System.Collections.Generic;

    public interface IWebhookFilterPokemonDetails
    {
        IReadOnlyList<uint> Pokemon { get; }

        IReadOnlyList<string> Forms { get; }

        IReadOnlyList<string> Costumes { get; }

        FilterType FilterType { get; }
    }
}