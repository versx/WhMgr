namespace WhMgr.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using NUnit.Framework;
    using InvasionCharacter = POGOProtos.Rpc.EnumWrapper.Types.InvasionCharacter;
    using QuestRewardType = POGOProtos.Rpc.QuestRewardProto.Types.Type;

    using WhMgr.Common;
    using WhMgr.Configuration;
    using WhMgr.Services.Icons;
    using WhMgr.Services.Webhook.Models;
    using WhMgr.Services.Webhook.Models.Quests;

    [TestFixture]
    public class IconTests
    {
        private UIconService _iconService;

        [SetUp]
        public void Setup()
        {
            // Create icon fetcher instance
            _iconService = CreateIconFetcherInstance();
        }

        [TestCase]
        public void Test_IconsIndex()
        {
            // Get Valor gym with 5 trainers, in battle, and ex eligible
            var style = "Default";
            var iconUrl = _iconService.GetGymIcon(style, PokemonTeam.Valor, 5, true, true);
            Console.WriteLine($"Gym (Valor, Battle, EX): {iconUrl}");
            // Check if fetched icon is what it should be
            Assert.AreEqual(iconUrl, "https://raw.githubusercontent.com/WatWowMap/wwm-uicons/main/gym/2_t5_b_ex.png");

            // Fetch bulbasaur icon
            iconUrl = _iconService.GetPokemonIcon(style, 3, evolutionId: 1, shiny: true);
            Console.WriteLine($"Mega Venasaur: {iconUrl}");
            Assert.IsTrue(Path.GetFileName(iconUrl) != "0.png");

            iconUrl = _iconService.GetEggIcon(style, 5, false, true);
            Console.WriteLine($"Egg (Level 5, EX): {iconUrl}");
            Assert.IsTrue(Path.GetFileName(iconUrl) != "0.png");

            iconUrl = _iconService.GetInvasionIcon(style, InvasionCharacter.CharacterGrassGruntFemale);
            // https://raw.githubusercontent.com/WatWowMap/wwm-uicons/main/invasion/0.png
            Console.WriteLine($"Invasion: {iconUrl}");
            Assert.IsTrue(Path.GetFileName(iconUrl) != "0.png");

            iconUrl = _iconService.GetPokestopIcon(style, PokestopLureType.Glacial, true, true);
            Console.WriteLine($"Lure: {iconUrl}");
            Assert.IsTrue(Path.GetFileName(iconUrl) != "0.png");

            iconUrl = _iconService.GetRewardIcon(style, QuestRewardType.Stardust, 1000);
            Console.WriteLine($"Stardust Reward: {iconUrl}");
            Assert.IsTrue(Path.GetFileName(iconUrl) != "0.png");

            var questData = new QuestData
            {
                Rewards = new List<QuestRewardMessage>
                {
                    new QuestRewardMessage
                    {
                        Type = QuestRewardType.PokemonEncounter,
                        Info = new QuestReward
                        {
                            PokemonId = 3,
                            GenderId = 1,
                            CostumeId = 0,
                            /*
                            MegaResource = new QuestMegaResource
                            {
                                PokemonId = 3,
                                Amount = 200,
                            },
                            */
                        },
                    },
                },
            };

            iconUrl = _iconService.GetRewardIcon(style, questData);
            Console.WriteLine($"Pokemon Encounter: {iconUrl}");
            Assert.IsTrue(Path.GetFileName(iconUrl) != "0.png");

            iconUrl = _iconService.GetRewardIcon(style, QuestRewardType.PokemonEncounter, 3);
            Console.WriteLine($"Pokemon Reward: {iconUrl}");
            Assert.IsTrue(Path.GetFileName(iconUrl) != "0.png");

            iconUrl = _iconService.GetRewardIcon(style, QuestRewardType.MegaResource, 3, 20);
            Console.WriteLine($"MegaResource Reward: {iconUrl}");
            Assert.IsTrue(Path.GetFileName(iconUrl) != "0.png");

            iconUrl = _iconService.GetTeamIcon(style, PokemonTeam.Mystic);
            Console.WriteLine($"Team: {iconUrl}");
            Assert.IsTrue(Path.GetFileName(iconUrl) != "0.png");

            iconUrl = _iconService.GetTypeIcon(style, PokemonType.Ground);
            Console.WriteLine($"Type: {iconUrl}");
            Assert.IsTrue(Path.GetFileName(iconUrl) != "0.png");

            iconUrl = _iconService.GetNestIcon(style, PokemonType.Dark);
            Console.WriteLine($"Nest: {iconUrl}");
            Assert.IsTrue(Path.GetFileName(iconUrl) != "0.png");

            iconUrl = _iconService.GetWeatherIcon(style, WeatherCondition.Fog);
            Console.WriteLine($"Weather: {iconUrl}");
            Assert.IsTrue(Path.GetFileName(iconUrl) != "0.png");

            iconUrl = _iconService.GetMiscellaneousIcon(style, "ar");
            Console.WriteLine($"Misc: {iconUrl}");
            Assert.IsTrue(Path.GetFileName(iconUrl) != "0.png");
        }

        private static IconStyleCollection GetDefaultIconConfig()
        {
            var config = new IconStyleCollection
            {
                {
                    "Default",
                    new Dictionary<IconType, IconStyleConfig>
                    {
                        {
                            IconType.Base, new IconStyleConfig
                            {
                                Name = "Default",
                                Path = "https://raw.githubusercontent.com/WatWowMap/wwm-uicons/main/",
                            }
                        },
                        {
                            IconType.Pokemon, new IconStyleConfig
                            {
                                Name = "Default_Pokemon",
                                Path = "https://raw.githubusercontent.com/nileplumb/PkmnShuffleMap/master/UICONS/pokemon/",
                            }
                        },
                    }
                },
                {
                    "Home",
                    new Dictionary<IconType, IconStyleConfig>
                    {
                        {
                            IconType.Base, new IconStyleConfig
                            {
                                Name = "Home",
                                Path = "https://raw.githubusercontent.com/nileplumb/PkmnHomeIcons/master/UICONS_OS/",
                            }
                        },
                    }
                },
                {
                    "Shuffle",
                    new Dictionary<IconType, IconStyleConfig>
                    {
                        {
                            IconType.Base, new IconStyleConfig
                            {
                                Name = "Shuffle",
                                Path = "https://raw.githubusercontent.com/nileplumb/PkmnShuffleMap/master/UICONS/",
                            }
                        },
                    }
                },
                // TODO: Set `raid` to list instead of object >.> use dynamic class and get object or list to work around, eventually. Always someone that doesn't go by the 'Standard'.
                {
                    "Pokemon Go",
                    new Dictionary<IconType, IconStyleConfig>
                    {
                        {
                            IconType.Base, new IconStyleConfig
                            {
                                Name = "Pokemon Go",
                                Path = "https://raw.githubusercontent.com/whitewillem/PogoAssets/resized/icons_large-uicons",
                            }
                        },
                    }
                },
                // TODO: Set `gym` to object instead of list >.> eventually. Always someone that doesn't go by the 'Standard'.
                {
                    "PokeDave Shuffle",
                    new Dictionary<IconType, IconStyleConfig>
                    {
                        {
                            IconType.Base, new IconStyleConfig
                            {
                                Name = "PokeDave Shuffle",
                                Path = "https://raw.githubusercontent.com/jepke/pokedave_shuffle_icons_-PMSF-/master/UICONS/",
                            }
                        }
                    }
                },
                // TODO: Set `raid` and `gym` to object instead of list >.> eventually. Always someone that doesn't go by the 'Standard'.
                {
                    "PMSF",
                    new Dictionary<IconType, IconStyleConfig>
                    {
                        {
                            IconType.Base, new IconStyleConfig
                            {
                                Name = "PMSF",
                                Path = "https://raw.githubusercontent.com/pmsf/PMSF/develop/static/sprites/",
                            }
                        }
                    }
                },
            };
            return config;
        }

        private static UIconService CreateIconFetcherInstance()
        {
            var config = GetDefaultIconConfig();
            // TODO: Get quest rewards list from base index.json
            var iconFetcher = new UIconService(
                config,
                UIconService.GetQuestRewardTypeNames()
            );
            return iconFetcher;
        }
    }
}