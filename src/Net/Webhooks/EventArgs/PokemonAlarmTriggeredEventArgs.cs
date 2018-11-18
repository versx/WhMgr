namespace WhMgr.Net.Webhooks
{
    using System;

    using WhMgr.Alarms.Models;
    using WhMgr.Net.Models;

    public sealed class PokemonAlarmTriggeredEventArgs : EventArgs
    {
        public AlarmObject Alarm { get; }

        public PokemonData Pokemon { get; }

        public PokemonAlarmTriggeredEventArgs(PokemonData pkmn, AlarmObject alarm)
        {
            Pokemon = pkmn;
            Alarm = alarm;
        }
    }
}