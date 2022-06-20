namespace WhMgr.Services.Alarms
{
    using WhMgr.Services.Webhook.Models;

    public interface IAlarmControllerService
    {
        void ProcessPokemonAlarms(PokemonData pokemon);

        void ProcessRaidAlarms(RaidData raid);

        void ProcessQuestAlarms(QuestData quest);

        void ProcessPokestopAlarms(PokestopData pokestop);

        void ProcessInvasionAlarms(IncidentData incident);

        void ProcessGymAlarms(GymDetailsData gym);

        void ProcessWeatherAlarms(WeatherData weather);

        void ProcessAccountAlarms(AccountData account);
    }
}