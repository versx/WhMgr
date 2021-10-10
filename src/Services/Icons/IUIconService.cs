namespace WhMgr.Services.Icons
{
    using Gender = POGOProtos.Rpc.PokemonDisplayProto.Types.Gender;
    using InvasionCharacter = POGOProtos.Rpc.EnumWrapper.Types.InvasionCharacter;
    using QuestRewardType = POGOProtos.Rpc.QuestRewardProto.Types.Type;

    using WhMgr.Common;
    using WhMgr.Services.Webhook.Models;

    public interface IUIconService
    {
        string GetPokemonIcon(string style, uint pokemonId, uint formId = 0, uint evolutionId = 0, Gender gender = 0, uint costumeId = 0, bool shiny = false);

        string GetTypeIcon(string style, PokemonType type);

        string GetPokestopIcon(string style, PokestopLureType lure, bool invasionActive = false, bool questActive = false, bool ar = false);

        string GetRewardIcon(string style, QuestRewardType rewardType, uint id = 0, uint amount = 0);

        string GetRewardIcon(string style, QuestData quest);

        string GetInvasionIcon(string style, InvasionCharacter gruntType);

        string GetGymIcon(string style, PokemonTeam team = PokemonTeam.Neutral, uint trainerCount = 0, bool inBattle = false, bool ex = false, bool ar = false);

        string GetEggIcon(string style, uint level, bool hatched = false, bool ex = false);

        string GetTeamIcon(string style, PokemonTeam team = PokemonTeam.Neutral);

        string GetWeatherIcon(string style, WeatherCondition weatherCondition);

        string GetNestIcon(string style, PokemonType type);

        string GetMiscellaneousIcon(string style, string fileName);
    }
}