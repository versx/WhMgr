namespace WhMgr.Web.Api.Requests.Pokemon
{
    using System.Text.Json.Serialization;

    public class UpdatePokemonSubscriptionRequest : CreatePokemonSubscriptionRequest
    {
        [JsonPropertyName("id")]
        public uint Id { get; set; }
    }
}