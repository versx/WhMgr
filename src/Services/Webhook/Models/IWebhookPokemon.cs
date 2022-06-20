namespace WhMgr.Services.Webhook.Models
{
    public interface IWebhookPokemon
    {
        uint PokemonId { get; }

        uint FormId { get; }

        uint CostumeId { get; }
    }
}