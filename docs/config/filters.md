# Filters
Filters allow you to narrow down what is reported. All filters are optional and can be omitted. Plenty of examples in the repository under the [`examples/Filters`](https://github.com/versx/WhMgr/tree/master/examples/filters) directory for all different needs.  

## Filter Converters  
- WDR [Filter Converter](https://github.com/versx/WdrFilterConverter)

## Available Filter Options
```json
{
	"pokemon":
	{
		"enabled": true, // Filter is enabled
		"pokemon": [280,337,374], // List of Pokemon for the filter or empty for all
		"forms": ["Alola", "Galar"],
		"costumes": ["Detective","Holiday"], // List of costumes for the filter or empty for all
		"min_iv": 0, // Minimum IV of Pokemon to send
		"max_iv": 100, // Maximum IV of Pokemon to send
		"min_cp": 0, // Minimum CP of Pokemon
		"max_cp": 999999, // Maximum CP of Pokemon
		"min_lvl": 0, // Minimum level of Pokemon
		"max_lvl": 35, // Maximum level of Pokemon
		"gender": "m", // Only send male (m,f,*)
		"size": "Big", // Tiny, Small, Normal, Large, Big
		// Add or remove any PVP league filtering keys
		// depending on the interested ranks.
		"pvp": {
			// Little league rank filtering
			"little": {
				// Minimum rank of #1 for PVP rank stats
				"min_rank": 1,
				// Maximum rank of #5 for PVP rank stats
				"max_rank": 5,
				// Minimum CP value of 400 for PVP rank stats
				"min_cp": 400,
				// Maximum CP value of 500 for PVP rank stats
				"max_cp": 500,
				// Minimum PVP product stat
				"min_percent": 90,
				// Maximum PVP product stat
				"max_percent": 100,
				// Gender filtering requirement (*, m, or f)
				"gender": "*"
			},
			// Great league rank filtering
			"great": {
				"min_rank": 1,
				"max_rank": 5,
				"min_cp": 1400,
				"max_cp": 1500,
				"gender": "m"
			},
			// Ultra league rank filtering
			"ultra": {
				"min_rank": 1,
				"max_rank": 25,
				"min_cp": 2400,
				"max_cp": 2500,
				"gender": "f"
			}
		},
		"type": "Include", // Include or Exclude the `pokemon` list
		"is_event": false, // Only send Pokemon checked with event accounts (GoFest, etc)
		"ignore_missing": true // Ignore Pokemon missing stats
	},
	"eggs":
	{
		"enabled": true, // Filter is enabled
		"min_lvl": 1, // Minimum egg level to send
		"max_lvl": 8, // Maximum egg level to send
		"only_ex": false, // Only send ex-eligible raids.
		"team": "All", // All, Valor, Mystic, Instinct, Neutral
		"power_level": {
			"min_level": 1,
			"max_level": 3,
			"min_points": 10,
			"max_points": 250
		}
	},
	"raids":
	{
		"enabled": true, // Filter is enabled
		"pokemon": [], // Raid bosses to include or none for all.
		"forms": ["Alola","Galar"], // List of forms for the filter or empty for all
		"costumes": ["Detective","Holiday"], // List of costumes for the filter or empty for all
		"min_lvl": 1, // Minimum raid level to send
		"max_lvl": 8, // Maximum raid level to send
		"type": "Include", // Include or Exclude the `pokemon` list
		"only_ex": false, // Only send ex-eligible raids.
		"team": "All", // All, Valor, Mystic, Instinct, Nuetral
		"power_level": {
			"min_level": 1,
			"max_level": 3,
			"min_points": 10,
			"max_points": 250
		},
		"ignore_missing": true // Ignore raids missing stats
	},
	"quests":
	{
		"enabled": true, // Filter is enabled
		"rewards": ["spinda", "nincada"], // Quest reward string (Chansey, stardust, candy, etc.)
		"is_shiny": false, // Only shiny encounter quests.
		"type": "Include" // Include or Exclude the `rewards` list
	},
	"pokestops":
	{
		"enabled": true, // Filter is enabled
		"lured": true, // Only send lured pokestops
		"lure_types": ["Normal", "Glacial", "Mossy", "Magnetic"], // Only send lures in type list  
		"power_level": {
			"min_level": 1,
			"max_level": 3,
			"min_points": 10,
			"max_points": 250
		}
	},
	"invasions": {
		"enabled": true, // Filter is enabled
		// Determines which invasion types to send
		"invasionTypes": {
			"CharacterUnset": false,
			"CharacterBlanche": true,
			"CharacterCandela": true,
			"CharacterSpark": true,
			"CharacterGruntMale": true,
			"CharacterGruntFemale": true,
			"CharacterBugGruntFemale": true,
			"CharacterBugGruntMale": true,
			"CharacterDarknessGruntFemale": true,
			"CharacterDarknessGruntMale": true,
			"CharacterDarkGruntFemale": true,
			"CharacterDarkGruntMale": true,
			"CharacterDragonGruntFemale": true,
			"CharacterDragonGruntMale": true,
			"CharacterFairyGruntFemale": true,
			"CharacterFairyGruntMale": true,
			"CharacterFightingGruntFemale": true,
			"CharacterFightingGruntMale": true,
			"CharacterFireGruntFemale": true,
			"CharacterFireGruntMale": true,
			"CharacterFlyingGruntFemale": true,
			"CharacterFlyingGruntMale": true,
			"CharacterGrassGruntFemale": true,
			"CharacterGrassGruntMale": true,
			"CharacterGroundGruntFemale": true,
			"CharacterGroundGruntMale": true,
			"CharacterIceGruntFemale": true,
			"CharacterIceGruntMale": true,
			"CharacterMetalGruntFemale": true,
			"CharacterMetalGruntMale": true,
			"CharacterNormalGruntFemale": true,
			"CharacterNormalGruntMale": true,
			"CharacterPoisonGruntFemale": true,
			"CharacterPoisonGruntMale": true,
			"CharacterPsychicGruntFemale": true,
			"CharacterPsychicGruntMale": true,
			"CharacterRockGruntFemale": true,
			"CharacterRockGruntMale": true,
			"CharacterWaterGruntFemale": true,
			"CharacterWaterGruntMale": true,
			"CharacterPlayerTeamLeader": true,
			"CharacterExecutiveCliff": true,
			"CharacterExecutiveArlo": true,
			"CharacterExecutiveSierra": true,
			"CharacterGiovanni": true,
			"CharacterDecoyGruntMale": true,
			"CharacterDecoyGruntFemale": true,
			"CharacterGhostGruntFemale": true,
			"CharacterGhostGruntMale": true,
			"CharacterElectricGruntFemale": true,
			"CharacterElectricGruntMale": true,
			"CharacterBalloonGruntFemale": true,
			"CharacterBalloonGruntMale": true,
			"CharacterGruntbFemale": true,
			"CharacterGruntbMale": true,
			"CharacterBugBalloonGruntFemale": true,
			"CharacterBugBalloonGruntMale": true,
			"CharacterDarkBalloonGruntFemale": true,
			"CharacterDarkBalloonGruntMale": true,
			"CharacterDragonBalloonGruntFemale": true,
			"CharacterDragonBalloonGruntMale": true,
			"CharacterFairyBalloonGruntFemale": true,
			"CharacterFairyBalloonGruntMale": true,
			"CharacterFightingBalloonGruntFemale": true,
			"CharacterFightingBalloonGruntMale": true,
			"CharacterFireBalloonGruntFemale": true,
			"CharacterFireBalloonGruntMale": true,
			"CharacterFlyingBalloonGruntFemale": true,
			"CharacterFlyingBalloonGruntMale": true,
			"CharacterGrassBalloonGruntFemale": true,
			"CharacterGrassBalloonGruntMale": true,
			"CharacterGroundBalloonGruntFemale": true,
			"CharacterGroundBalloonGruntMale": true,
			"CharacterIceBalloonGruntFemale": true,
			"CharacterIceBalloonGruntMale": true,
			"CharacterMetalBalloonGruntFemale": true,
			"CharacterMetalBalloonGruntMale": true,
			"CharacterNormalBalloonGruntFemale": true,
			"CharacterNormalBalloonGruntMale": true,
			"CharacterPoisonBalloonGruntFemale": true,
			"CharacterPoisonBalloonGruntMale": true,
			"CharacterPsychicBalloonGruntFemale": true,
			"CharacterPsychicBalloonGruntMale": true,
			"CharacterRockBalloonGruntFemale": true,
			"CharacterRockBalloonGruntMale": true,
			"CharacterWaterBalloonGruntFemale": true,
			"CharacterWaterBalloonGruntMale": true,
			"CharacterGhostBalloonGruntFemale": true,
			"CharacterGhostBalloonGruntMale": true,
			"CharacterElectricBalloonGruntFemale": true,
			"CharacterElectricBalloonGruntMale": true
		}
	},
	"gyms":
	{
		"enabled": true, // Filter is enabled
		"under_attack": true, // Only gyms that are under attack
		"team": "All", // Team change to notify about (i.e. Neutral/Mystic/Valor/Instinct/All)
		"power_level": {
			"min_level": 1,
			"max_level": 3,
			"min_points": 10,
			"max_points": 250
		}
	},
	"weather":
	{
		"enabled": true, // Filter is enabled
		"types": ["Clear", "Rainy", "PartlyCloudy", "Overcast", "Windy", "Snow", "Fog"] // Only send weather types that are in the list
	}
}
```
