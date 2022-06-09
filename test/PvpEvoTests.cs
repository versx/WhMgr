namespace WhMgr.Test
{
    using System;
    using System.IO;

    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    using WhMgr.Configuration;
    using WhMgr.Data;
    using WhMgr.Extensions;
    using WhMgr.Services.Subscriptions;

    [TestFixture]
    public class PvpEvoTests
    {
        private ISubscriptionManagerService _subscriptionManager;
        private DependencyResolverHelper _serviceProvider;

        [SetUp]
        public void Setup()
        {
            var configPath = "../../../" + Strings.ConfigsFolder + "/" + Strings.ConfigFileName;
            var config = Config.Load(configPath);
            if (config == null)
            {
                Console.WriteLine($"Failed to load config {configPath}.");
                return;
            }
            // TODO: >.> Need to implement ConfigHolder in Startup instead of just Config
            //var holder = new ConfigHolder(config);
            config.FileName = configPath;
            config.LoadDiscordServers();
            Startup.Config = config;

            var webHost = WebHost.CreateDefaultBuilder()
                .UseStartup<Startup>()
                .Build();
            _serviceProvider = new DependencyResolverHelper(webHost);

            _subscriptionManager = _serviceProvider.GetService<ISubscriptionManagerService>();
        }

        [Test]
        [TestCase(2, 0)] // Ivysaur
        public void Test_PvpEvoSubs_ReturnsIsTrue(int pokemonId, int formId)
        {
            var pkmn = GameMaster.GetPokemon((uint)pokemonId, (uint)formId);
            var evolutionIds = pkmn.GetPokemonEvolutionIds();
            Console.WriteLine($"Evo ids: {evolutionIds}");
            // Get evolution ids from masterfile for incoming pokemon, check if subscriptions for evo/base
            var subscriptions = _subscriptionManager.GetSubscriptionsByPvpPokemonId(evolutionIds);
            Assert.IsTrue(subscriptions.Count > 0);
        }
    }

    [TestFixture]
    public class DependencyResolverHelper
    {
        private readonly IWebHost _webHost;

        /// <inheritdoc />
        public DependencyResolverHelper(IWebHost webHost) => _webHost = webHost;

        public T GetService<T>()
        {
            var serviceScope = _webHost.Services.CreateScope();
            var services = serviceScope.ServiceProvider;
            try
            {
                var scopedService = services.GetRequiredService<T>();
                return scopedService;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DependencyResolverHelper: {ex}");
                throw;
            }
        }
    }
}