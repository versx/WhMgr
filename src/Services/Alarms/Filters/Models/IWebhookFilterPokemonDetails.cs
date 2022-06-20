namespace WhMgr.Services.Alarms.Filters.Models
{
    using System.Collections.Generic;

    public interface IWebhookFilterPokemonDetails
    {
        List<uint> Pokemon { get; }

        List<string> Forms { get; }

        List<string> Costumes { get; }

        FilterType FilterType { get; }
    }
}