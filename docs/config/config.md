# Configuration

At a minimum you'll want to make sure you have your webhook listening port set as well as one Discord server added to the `servers` property.

## Full Config Example
```json
{
  // Http listening interface for raw webhook data, use "*" to listen on all interfaces.
  "host": "*",
  // Http listener port for raw webhook data.
  "port": 8008,
  // Locale language translation
  "locale": "en",
  // Telemetry reporting
  "sentry": true,
  // yourls.org API
  "shortUrlApi": {
    // Determines whether the Short URL API is used or not
    "enabled": false,
    // ShortURL API (i.e. `https://domain.com/yourls-api.php`)
    "apiUrl": "https://domain.com/u/api.php",
    // ShortURL passwordless authentication signature
    "signature": ""
  },
  "stripeApi": {
    "apiKey": ""
  },
  // List of Discord servers to connect and post webhook messages to.
  "servers": {
    // Discord server #1 guild ID (replace `000000000000000123` with
    // actual guild id of server)
    "000000000000000123": "discord1.example.json",
    // 2nd Discord server section (if applicable)
    "000000000000000456": "discord2.example.json"
  },
  // Database configuration
  "database": {
    // Database to store notification subscriptions.
    "main": {
      // Database hostname or IP address.
      "host": "127.0.0.1",
      // Database connection port.
      "port": 3306,
      // Database user account name.
      "username": "root",
      // Database user account password.
      "password": "password",
      // Brock database name.
      "database": "brockdb"
    },
    "scanner": {
      // Database hostname or IP address.
      "host": "127.0.0.1",
      // Database connection port.
      "port": 3306,
      // Database user account name.
      "username": "root",
      // Database user account password.
      "password": "password",
      // RDM database name.
      "database": "rdmdb"
    },
    "nests": {
      // Database hostname or IP address.
      "host": "127.0.0.1",
      // Database connection port.
      "port": 3306,
      // Database user account name.
      "username": "root",
      // Database user account password.
      "password": "password",
      // PMSF manual nests database name.
      "database": "manualdb"
    }
  },
  "eventPokemon": {
    // Determines if filtering event Pokemon is enabled or not.
    "enabled": false,
    // List of Pokemon IDs to treat as event and restrict postings and subscriptions to 90% IV or higher. (Filled in  automatically with `event set` command)  
    "pokemonIds": [],
    // Minimum IV value for an event Pokemon to have to meet in order to post via Discord channel alarm or direct message subscription.
    "eventMinimumIV": 90,
    // Event Pokemon filtering type
    "type": "Include",
    // Ignore event Pokemon if missing IV stats
    "ignoreMissingStats": true
  },
  // URL config
  "urls": {
    // Scanner map url DTS option for embeds as `scanmaps_url`.  
    // {0} and {1} are placeholders to construct the url with latitude
    // and longitude coordinates
    "scannerMap": "http://map.example.com/@/{0}/{1}/15"
  },
  // Available icon styles
  "iconStyles": {
    // Default icon style
    "Default": {
      // Base icon type object to apply to all other icon types
      "Base": {
        // Icon type display name
        "name": "Default",
        // Icon type url path
        "path": "https://raw.githubusercontent.com/WatWowMap/wwm-uicons/main/"
      },
      // Pokemon icon type object to modify
      "Pokemon": {
        // Icon type display name
        "name": "Default_Pokemon",
        // Icon type url path
        "path": "https://raw.githubusercontent.com/nileplumb/PkmnShuffleMap/master/UICONS/pokemon/"
      }
      /*
      "Raid",
      "Egg",
      "Gym",
      "Pokestop",
      "Reward",
      "Invasion",
      "Type",
      "Nest",
      "Team",
      "Weather",
      "Misc",
      */
    },
    // Pokemon Home Icons
    "Home": {
      // Base icon type object to apply to all other icon types
      "Base": {
        // Icon type display name
        "name": "Home",
        // Icon type url path
        "path": "https://raw.githubusercontent.com/nileplumb/PkmnHomeIcons/master/UICONS_OS/"
      }
    },
    // Pokemon Shuffle Icons
    "Shuffle": {
      // Base icon type object to apply to all other icon types
      "Base": {
        // Icon type display name
        "name": "Shuffle",
        // Icon type url path
        "path": "https://raw.githubusercontent.com/nileplumb/PkmnShuffleMap/master/UICONS/"
      }
    },
    // Pokemon Go Application Icons
    "Pokemon Go": {
      // Base icon type object to apply to all other icon types
      "Base": {
        // Icon type display name
        "name": "Pokemon Go",
        // Icon type url path
        "path": "https://raw.githubusercontent.com/whitewillem/PogoAssets/main/uicons/"
      }
    },
    // PMSF Icons
    "PMSF": {
      // Base icon type object to apply to all other icon types
      "Base": {
        // Icon type display name
        "name": "PMSF",
        // Icon type url path
        "path": "https://raw.githubusercontent.com/pmsf/PMSF/develop/static/sprites/"
      }
    }
  },
  // Custom static map template files for each alarm type
  "staticMaps": {
    // Base url for static map service
    "url": "http://tiles.example.com:8080",
    // StaticMap or MultiStaticMap
    "type": "StaticMap",
    // Include nearby gyms with static map image  
    "includeGyms": false,
    // Include nearby pokestops with static map image  
    "includePokestops": false,
    // Including Gyms and Pokestops on the StaticMap only works if `pregenerate` is set to `true`
    "pregenerate": true
  },
  // Get text message alerts with Twilio.com
  "twilio": {
    // Determines if text message alerts are enabled
    "enabled": false,
    // Twilio account SID (Get via Twilio dashboard)
    "accountSid": "",
    // Twilio account auth token (Get via Twilio dashboard)
    "authToken": "",
    // Twilio phone number that will be sending the text message alert
    "from": "",
    // List of Discord user ids that can receive text message alerts
    "userIds": [],
    // List of Discord roles that can receive text message alerts
    "roleIds": [],
    // List of acceptable Pokemon to receive text message alerts for
    "pokemonIds": [201, 480, 481, 482, 443, 444, 445, 633, 634, 635, 610, 611, 612],
    // Minimum acceptable IV value for Pokemon if not ultra rare (Unown, Lake Trio)
    "minIV": 100
  },
  "reverseGeocoding": {
    // Reverse geocoding provider
    "provider": "osm", // osm/gmaps
    // Cache reverse geocoding responses to disk to reduce request count
    "cacheToDisk": true,
    // Google Maps reverse geocoding
    "gmaps": {
      // Google maps key for reverse geocoding  
      "key": "",
      // Google maps template schema for embeds  
      "schema": "{{Results.[0].FormattedAddress}}"
    },
    // OpenStreetMaps Nominatim reverse geocoding  
    "nominatim": {
      // OSM Nominatim endpoint
      "endpoint": "",
      // OSM Nominatim template schema for embeds
      "schema": "{{Address.Road}} {{Address.State}} {{Address.Postcode}} {{Address.Country}}"
    }
  },
  // Minimum despawn time in minutes a Pokemon must have in order to send the alarm (default: 5 minutes)
  "despawnTimeMinimumMinutes": 5,
  // Reload subscriptions every minute to sync with WhMgr-UI changes  
  "reloadSubscriptionChangesMinutes": 1,
  // Check for duplicate webhooks
  "checkForDuplicates": true,
  // Log webhook payloads to a file for debugging (do not enable unless you're having issues receiving data
  "debug": false,
  /*
   * Only show logs with higher or equal priority levels:
   * Trace: 0
   * Debug: 1
   * Info: 2
   * Warning: 3
   * Error: 4
   * Critical: 5
   * None: 6
  */
  "logLevel": 0,
  // Acceptable and interested PVP leagues to parse.
  "pvpLeagues": {
    // League name key to match webhook key for PVP ranks.
    "little": {
      // League minimum acceptable CP
      "minCP": 450,
      // League maximum acceptable CP
      "maxCP": 500,
      // League minimum rank to meet
      "minRank": 1,
      // League maximum rank to meet
      "maxRank": 100
    },
    "great": {
      "minCP": 1400,
      "maxCP": 1500,
      "minRank": 1,
      "maxRank": 100
    },
    "ultra": {
      "minCP": 2400,
      "maxCP": 2500,
      "minRank": 1,
      "maxRank": 100
    }
  }
}
```

## Top Level
```json
{
  // Http listening interface for raw webhook data, use "*" to listen on all interfaces.
  "host": "*",
  // Http listener port for raw webhook data.
  "port": 8008,
  // Locale language translation
  "locale": "en",
  // Minimum despawn time in minutes a Pokemon must have in order to send the alarm (default: 5 minutes)
  "despawnTimeMinimumMinutes": 5,
  // Reload subscriptions every minute to sync with WhMgr-UI changes  
  "reloadSubscriptionChangesMinutes": 1,
  // Check for duplicate webhooks
  "checkForDuplicates": true,
  // Log webhook payloads to a file for debugging (do not enable unless you're having issues receiving data)
  "debug": false,
  /*
   * Only show logs with higher or equal priority levels:
   * Trace: 0
   * Debug: 1
   * Info: 2
   * Warning: 3
   * Error: 4
   * Critical: 5
   * None: 6
  */
  "logLevel": 0	  "logLevel": 0,
  // Acceptable and interested PVP leagues to parse.
  "pvpLeagues": {
    // League name key to match webhook key for PVP ranks.
    "little": {
      // League minimum acceptable CP
      "minCP": 450,
      // League maximum acceptable CP
      "maxCP": 500,
      // League minimum rank to meet
      "minRank": 1,
      // League maximum rank to meet
      "maxRank": 100
    },
    "great": {
      "minCP": 1400,
      "maxCP": 1500,
      "minRank": 1,
      "maxRank": 100
    },
    "ultra": {
      "minCP": 2400,
      "maxCP": 2500,
      "minRank": 1,
      "maxRank": 100
    }
  }
}
```

## Discord Servers
```json
{
  // List of Discord servers to connect and post webhook messages to.
  "servers": {
    // Discord server #1 guild ID (replace `000000000000000123` with
    // actual guild id of server)
    "000000000000000123": "discord1.example.json",
    // 2nd Discord server section (if applicable)
    "000000000000000456": "discord2.example.json"
  }
}
```

## Short Url API
```json
{
  // yourls.org API
  "shortUrlApi": {
    // Determines whether the Short URL API is used or not
    "enabled": false,
    // ShortURL API (i.e. `https://domain.com/yourls-api.php`)
    "apiUrl": "https://domain.com/u/api.php",
    // ShortURL passwordless authentication signature
    "signature": ""
  }
}
```

## Stripe API
```json
{
  "stripeApi": {
    // Stripe API key (Stripe production API key, i.e. rk_3824802934
    "apiKey": "",
  }
}
```

## Database Schemas
```json
{
  // Database configuration
  "database": {
    // Database to store notification subscriptions.
    "main": {
      // Database hostname or IP address.
      "host": "127.0.0.1",
      // Database connection port.
      "port": 3306,
      // Database user account name.
      "username": "root",
      // Database user account password.
      "password": "password",
      // Brock database name.
      "database": "brockdb"
    },
    "scanner": {
      // Database hostname or IP address.
      "host": "127.0.0.1",
      // Database connection port.
      "port": 3306,
      // Database user account name.
      "username": "root",
      // Database user account password.
      "password": "password",
      // RDM database name.
      "database": "rdmdb"
    },
    "nests": {
      // Database hostname or IP address.
      "host": "127.0.0.1",
      // Database connection port.
      "port": 3306,
      // Database user account name.
      "username": "root",
      // Database user account password.
      "password": "password",
      // PMSF manual nests database name.
      "database": "manualdb"
    }
  }
}
```

## URLs
```json
{
  // URL config
  "urls": {
    // Scanner map url DTS option for embeds as `scanmaps_url`.  
    // {0} and {1} are placeholders to construct the url with latitude
    // and longitude coordinates
    "scannerMap": "https://map.example.com/@/{0}/{1}/15"
  }
}
```

## Twilio Text Message Notifications
```json
{
  // Get text message alerts with Twilio.com
  "twilio": {
    // Determines if text message alerts are enabled
    "enabled": false,
    // Twilio account SID (Get via Twilio dashboard)
    "accountSid": "",
    // Twilio account auth token (Get via Twilio dashboard)
    "authToken": "",
    // Twilio phone number that will be sending the text message alert
    "from": "",
    // List of Discord user ids that can receive text message alerts
    "userIds": [],
    // List of Discord roles that can receive text message alerts
    "roleIds": [],
    // List of acceptable Pokemon to receive text message alerts for
    "pokemonIds": [201, 480, 481, 482, 443, 444, 445, 633, 634, 635, 610, 611, 612],
    // Minimum acceptable IV value for Pokemon if not ultra rare (Unown, Lake Trio)
    "minIV": 100
  }
}
```

## Icon Styles
```json
{
  // Available icon styles
  "iconStyles": {
    // Default icon style
    "Default": {
      // Base icon type object to apply to all other icon types
      "Base": {
        // Icon type display name
        "name": "Default",
        // Icon type url path
        "path": "https://raw.githubusercontent.com/WatWowMap/wwm-uicons/main/"
      },
      // Pokemon icon type object to modify
      "Pokemon": {
        // Icon type display name
        "name": "Default_Pokemon",
        // Icon type url path
        "path": "https://raw.githubusercontent.com/nileplumb/PkmnShuffleMap/master/UICONS/pokemon/"
      }
      /*
      "Raid",
      "Egg",
      "Gym",
      "Pokestop",
      "Reward",
      "Invasion",
      "Type",
      "Nest",
      "Team",
      "Weather",
      "Misc",
      */
    },
    // Pokemon Home Icons
    "Home": {
      // Base icon type object to apply to all other icon types
      "Base": {
        // Icon type display name
        "name": "Home",
        // Icon type url path
        "path": "https://raw.githubusercontent.com/nileplumb/PkmnHomeIcons/master/UICONS_OS/"
      }
    },
    // Pokemon Shuffle Icons
    "Shuffle": {
      // Base icon type object to apply to all other icon types
      "Base": {
        // Icon type display name
        "name": "Shuffle",
        // Icon type url path
        "path": "https://raw.githubusercontent.com/nileplumb/PkmnShuffleMap/master/ICONS_STANDARD/"
      }
    },
    // Pokemon Go Application Icons
    "Pokemon Go": {
      // Base icon type object to apply to all other icon types
      "Base": {
        // Icon type display name
        "name": "Pokemon Go",
        // Icon type url path
        "path": "https://raw.githubusercontent.com/whitewillem/PogoAssets/resized/icons_large-uicons"
      }
    },
    // PokeDave Pokemon Shuffle Icons
    "PokeDave Shuffle": {
      // Base icon type object to apply to all other icon types
      "Base": {
        // Icon type display name
        "name": "PokeDave Shuffle",
        // Icon type url path
        "path": "https://raw.githubusercontent.com/jepke/pokedave_shuffle_icons_-PMSF-/master/UICONS/"
      }
    },
    // PMSF Icons
    "PMSF": {
      // Base icon type object to apply to all other icon types
      "Base": {
        // Icon type display name
        "name": "PMSF",
        // Icon type url path
        "path": "https://raw.githubusercontent.com/pmsf/PMSF/develop/static/sprites/"
      }
    }
  },
}
```

## Static Map Templates
```json
{
  // Custom static map template files for each alarm type
  "staticMaps": {
    // Static map template for Pokemon
    "pokemon": {
      // Static map url template for pokemon
      "url": "http://tiles.example.com/staticmap/{{template_name}}?lat={{lat}}&lon={{lon}}&url2={{url2}}",
      // Static map template file name without extension
      "template": "pokemon.example",
      // Include nearby gyms in static map image  
      "includeGyms": false,
      // Include nearby pokestops in static map image
      "includePokestops": false
    },
    // Static map template for Raids and Eggs
    "raids": {
      // Static map url template for raids
      "url": "http://tiles.example.com/staticmap/{{template_name}}?lat={{lat}}&lon={{lon}}&url2={{url2}}&team_id={{team_id}}",
      // Static map template file name without extension
      "template": "raids.example",
      // Include nearby gyms in static map image  
      "includeGyms": false,
      // Include nearby pokestops in static map image
      "includePokestops": false
    },
    // Static map template for Gym team control changes
    "gyms": {
      // Static map url template for gyms
      "url": "http://tiles.example.com/staticmap/{{template_name}}?lat={{lat}}&lon={{lon}}&url2={{url2}}&team_id={{team_id}}",
      // Static map template file name without extension
      "template": "gyms.example",
      // Include nearby gyms in static map image  
      "includeGyms": false,
      // Include nearby pokestops in static map image
      "includePokestops": false
    },
    // Static map template for field research quests
    "quests": {
      // Static map url template for quests
      "url": "http://tiles.example.com/staticmap/{{template_name}}?lat={{lat}}&lon={{lon}}&url2={{url2}}",
      // Static map template file name without extension
      "template": "quests.example",
      // Include nearby gyms in static map image  
      "includeGyms": false,
      // Include nearby pokestops in static map image
      "includePokestops": false
    },
    // Static map template for Team Rocket invasions
    "invasions": {
      // Static map url template for invasions
      "url": "http://tiles.example.com/staticmap/{{template_name}}?lat={{lat}}&lon={{lon}}&url2={{url2}}",
      // Static map template file name without extension
      "template": "invasions.example",
      // Include nearby gyms in static map image  
      "includeGyms": false,
      // Include nearby pokestops in static map image
      "includePokestops": false
    },
    // Static map template for Pokestop lures
    "lures": {
      // Static map url template for lures
      "url": "http://tiles.example.com/staticmap/{{template_name}}?lat={{lat}}&lon={{lon}}&url2={{url2}}",
      // Static map template file name without extension
      "template": "lures.example",
      // Include nearby gyms in static map image  
      "includeGyms": false,
      // Include nearby pokestops in static map image
      "includePokestops": false
    },
    // Static map template for weather changes
    "weather": {
      // Static map url template for weather
      "url": "http://tiles.example.com/staticmap/{{template_name}}?lat={{lat}}&lon={{lon}}&url2={{url2}}&polygon={{polygon}}",
      // Static map template file name without extension
      "template": "weather.example",
      // Include nearby gyms in static map image  
      "includeGyms": false,
      // Include nearby pokestops in static map image
      "includePokestops": false
    },
    // Static map template for nest postings
    "nests": {
      // Static map url template for nests
      "url": "http://tiles.example.com/staticmap/{{template_name}}?lat={{lat}}&lon={{lon}}&url2={{url2}}&polygon={{polygon}}",
      // Static map template file name without extension
      "template": "nests.example",
      // Include nearby gyms in static map image  
      "includeGyms": false,
      // Include nearby pokestops in static map image
      "includePokestops": false
    }
  }
}
```

## Reverse Geocoding
```json
{
  // Reverse lookup of geocoordinates to physical address
  "reverseGeocoding": {
    // Reverse geocoding provider
    "provider": "osm", // osm/gmaps
    // Cache reverse geocoding responses to disk to reduce request count
    "cacheToDisk": true,
    // Google Maps reverse geocoding
    "gmaps": {
      // Google maps key for reverse geocoding  
      "key": "",
      // Google maps template schema for embeds  
      "schema": "{{Results.[0].FormattedAddress}}"
    },
    // OpenStreetMaps Nominatim reverse geocoding  
    "nominatim": {
      // OSM Nominatim endpoint
      "endpoint": "",
      // OSM Nominatim template schema for embeds
      "schema": "{{Address.Road}} {{Address.State}} {{Address.Postcode}} {{Address.Country}}"
    }
  }
}
```

## PVP Leagues
```json
{
  // Acceptable and interested PVP leagues to parse.
  "pvpLeagues": {
    // League name key to match webhook key for PVP ranks.
    "little": {
      // League minimum acceptable CP
      "minCP": 450,
      // League maximum acceptable CP
      "maxCP": 500,
      // League minimum rank to meet
      "minRank": 1,
      // League maximum rank to meet
      "maxRank": 100
    },
    "great": {
      "minCP": 1400,
      "maxCP": 1500,
      "minRank": 1,
      "maxRank": 100
    },
    "ultra": {
      "minCP": 2400,
      "maxCP": 2500,
      "minRank": 1,
      "maxRank": 100
    }
  }
}
```