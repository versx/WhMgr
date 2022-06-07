# Default Options  

Located at `bin/static/data/defaults.json`, it provides default values throughout the application.  

```json
{
    // Default Pokemon subscription options
    "min_iv": 0,
    "max_iv": 100,
    "min_lvl": 0,
    "max_lvl": 35,
    "min_cp": 0,
    "max_cp": 99999,
    "pvp": {
        // Default PVP filtering values if none provided
        "little": {
            "min_rank": 1,
            "max_rank": 25,
            "min_percent": 90,
            "max_percent": 100,
            "min_league_cp": 450,
            "max_league_cp": 500,
        },
        "great": {
            "min_rank": 1,
            "max_rank": 25,
            "min_percent": 90,
            "max_percent": 100,
            "min_league_cp": 1400,
            "max_league_cp": 1500,
        },
        "ultra": {
            "min_rank": 1,
            "max_rank": 25,
            "min_percent": 90,
            "max_percent": 100,
            "min_league_cp": 2400,
            "max_league_cp": 2500,
        }
    },

    // Queue options
    // Maximum queue batch size when sending outgoing messages
    "max_queue_batch_size": 10,
    // Maximum queue size before warning
    "max_queue_size_warning": 50,

    // 
    "all": "All",

    // Emoji schemas
    "emoji_schema": "<:{0}:{1}>",
    "type_emoji_schema": "<:types_{0}:{1}>",

    // Pokemon generation ranges
    "pokemon_generation_ranges": {
        "1": {
            "gen": 1,
            "start": 1,
            "end": 151
        },
        "2": {
            "gen": 2,
            "start": 152,
            "end": 251
        },
        "3": {
            "gen": 3,
            "start": 252,
            "end": 385
        },
        "4": {
            "gen": 4,
            "start": 386,
            "end": 493
        },
        "5": {
            "gen": 5,
            "start": 494,
            "end": 649
        },
        "6": {
            "gen": 6,
            "start": 650,
            "end": 721
        },
        "7": {
            "gen": 7,
            "start": 722,
            "end": 809
        },
        "8": {
            "gen": 8,
            "start": 810,
            "end": 890
        }
    },

    // Default emojis list
    "emoji_list": [
        // Teams
        "neutral",
        "valor",
        "mystic",
        "instinct",

        // Capture rates
        "capture_1",
        "capture_2",
        "capture_3",

        // Weather
        "weather_1", // Clear
        "weather_2", // Rain
        "weather_3", // PartlyCloudy/Overcast
        "weather_4", // Cloudy
        "weather_5", // Windy
        "weather_6", // Snow
        "weather_7", // Fog

        // Gyms
        "ar",
        "ex",

        // Pokemon types  
        "types_fire",
        "types_grass",
        "types_ground",
        "types_rock",
        "types_water",
        "types_ghost",
        "types_ice",
        "types_dragon",
        "types_fairy",
        "types_fighting",
        "types_bug",
        "types_psychic",
        "types_electric",
        "types_steel",
        "types_dark",
        "types_normal",
        "types_flying",
        "types_poison",

        // Pvp leagues
        "league_great",
        "league_ultra",

        // Pokemon genders
        "gender_male",
        "gender_female",
        "gender_less"
    ],

    // Weather boost dictionary
    "weather_boosts": {
        // None  
        "0": [],
        // Clear/Sunny  
        "1": ["Fire", "Grass", "Ground"],
        // Rainy  
        "2": ["Water", "Electric", "Bug"],
        // Partly Cloudy
        "3": ["Normal", "Rock"],
        // Cloudy / Overcast
        "4": ["Fairy", "Fighting", "Poison"],
        // Windy  
        "5": ["Dragon", "Flying", "Psychic"],
        // Snow
        "6": ["Ice", "Steel"],
        // Fog  
        "7": ["Dark", "Ghost"]
    }
}
```