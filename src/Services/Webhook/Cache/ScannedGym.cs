namespace WhMgr.Services.Webhook.Cache
{
    using WhMgr.Common;
    using WhMgr.Services.Webhook.Models;

    internal class ScannedGym : IScannedItem
    {
        public double Latitude { get; }

        public double Longitude { get; }

        public PokemonTeam Team { get; }

        public int SlotsAvailable { get; }

        public bool InBattle { get; }

        public ScannedGym(GymDetailsData gym)
        {
            Latitude = gym.Latitude;
            Longitude = gym.Longitude;
            Team = gym.Team;
            SlotsAvailable = gym.SlotsAvailable;
            InBattle = gym.InBattle;
        }
    }
}