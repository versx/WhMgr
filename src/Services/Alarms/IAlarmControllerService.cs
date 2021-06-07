namespace WhMgr.Services.Alarms
{
    using WhMgr.Services.Webhook.Models;

    public interface IAlarmControllerService
    {
        void ProcessPokemonAlarms(PokemonData pokemon);
    }
}