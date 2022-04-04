namespace WhMgr.Data
{
    using System.Text.Json.Serialization;

    using static POGOProtos.Rpc.BelugaPokemonProto.Types;

    public class PokedexPokemonEvolution
    {
        [JsonPropertyName("pokemon")]
        public uint PokemonId { get; set; }

        [JsonPropertyName("form")]
        public uint FormId { get; set; }

        [JsonPropertyName("gender_requirement")]
        public PokemonGender GenderRequirement { get; set; }
    }
}