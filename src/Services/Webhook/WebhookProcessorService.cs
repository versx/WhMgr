namespace WhMgr.Services.Webhook
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Extensions.Logging;

    using WhMgr.Extensions;
    using WhMgr.Services.Alarms;
    using WhMgr.Services.Subscriptions;
    using WhMgr.Services.Webhook.Models;

    public class WebhookProcessorService : IWebhookProcessorService
    {
        private readonly ILogger<WebhookProcessorService> _logger;
        private readonly IAlarmControllerService _alarmsService;
        private readonly ISubscriptionProcessorService _subscriptionService;

        #region Properties

        public bool Enabled { get; set; }

        public bool CheckForDuplicates { get; set; }

        public ushort DespawnTimerMinimumMinutes { get; set; }

        #endregion

        public WebhookProcessorService(
            ILogger<WebhookProcessorService> logger,
            IAlarmControllerService alarmsService,
            ISubscriptionProcessorService subscriptionService)
        {
            _logger = logger;
            _alarmsService = alarmsService;
            _subscriptionService = subscriptionService;

            Start();
        }

        #region Public Methods

        public void Start()
        {
            Enabled = true;
        }

        public void Stop()
        {
            Enabled = false;
        }

        public void ParseData(List<WebhookPayload> payloads)
        {
            if (!Enabled) return;

            _logger.LogInformation($"Received {payloads.Count:N0} webhook payloads");
            for (var i = 0; i < payloads.Count; i++)
            {
                var payload = payloads[i];
                switch (payload.Type)
                {
                    case WebhookHeaders.Pokemon:
                        ProcessPokemon(payload.Message);
                        break;
                    case WebhookHeaders.Raid:
                        ProcessRaid(payload.Message);
                        break;
                    case WebhookHeaders.Quest:
                    case WebhookHeaders.Invasion:
                    case WebhookHeaders.Pokestop:
                        break;
                        // TODO: Gym
                        // TODO: Weather
                }
            }
        }

        #endregion

        private void ProcessPokemon(dynamic message)
        {
            string json = Convert.ToString(message);
            var pokemon = json.FromJson<PokemonData>();
            if (pokemon == null)
            {
                _logger.LogWarning($"Failed to parse pokemon {message}, skipping...");
                return;
            }
            pokemon.SetDespawnTime();

            if (pokemon.SecondsLeft.TotalMinutes < DespawnTimerMinimumMinutes)
                return;

            if (CheckForDuplicates)
            {
                // TODO: lock processed pokemon, check for dups
            }

            // TODO: Process for webhook alarms and member subscriptions
            //OnPokemonFound(pokemon);
            _alarmsService.ProcessPokemonAlarms(pokemon);
            _subscriptionService.ProcessPokemon(pokemon);
        }

        private void ProcessRaid(dynamic message)
        {
            string json = Convert.ToString(message);
            var raid = json.FromJson<RaidData>();
            if (raid == null)
            {
                _logger.LogWarning($"Failed to parse raid {message}, skipping...");
                return;
            }
            raid.SetTimes();

            if (CheckForDuplicates)
            {
                // TODO: lock processed pokemon, check for dups
            }

            // TODO: Process for webhook alarms and member subscriptions
            _alarmsService.ProcessRaidAlarms(raid);
            _subscriptionService.ProcessRaidPokemon(raid);
        }
    }
}