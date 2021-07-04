namespace WhMgr.Services.Alarms
{
    using WhMgr.Services.Webhook.Models;

    public interface IAlarmControllerService
    {
        void ProcessPokemonAlarms(PokemonData pokemon);

        void ProcessRaidAlarms(RaidData raid);

        void ProcessQuestAlarms(QuestData quest);

        void ProcessPokestopAlarms(PokestopData pokestop);

        void ProcessGymAlarms(GymDetailsData gym);

        void ProcessWeatherAlarms(WeatherData weather);
    }
}