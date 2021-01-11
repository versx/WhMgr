# Dynamic Text Replacement  

Use any of the following in your alerts file to structure how notifications will look for Pokemon.  

### Pokemon  

| Place Holder | Description  | Example
|---|---|---|  
| pkmn_id  | Pokedex ID  |  1
| pkmn_id_3  | Pokedex ID (always 3 digits)  | 001
| pkmn_name | Pokemon name | Bulbasaur
| pkmn_img_url | Pokemon image url | http://example.com/your-specified-pokemon-url
| form | Pokemon form name | Alolan
| form_id | Form ID | 65
| form_id_3 | Form ID (always 3 digits) | 065
| costume | Costume name | Witch Hat
| costume_id | Costume ID | 835
| costume_id_3 | Costume ID (always 3 digits) | 835
| cp | Combat Power value | 1525
| lvl | Pokemon level | 25
| gender | Pokemon gender | Gender icon
| gender_emoji | Pokemon gender emoji | <:00000:gender_male>
| size | Pokemon size | Big
| move_1 | Fast move name | Quick Attack
| move_2 | Charge move name | Thunder
| moveset | Fast & Charge move names | Quick Attack/Thunder
| type_1 | Pokemon type | Dark
| type_2 | Pokemon type | Water
| type_1_emoji | Pokemon type emoji | <:00000:types_water>
| type_2_emoji | Pokemon type emoji | <:00000:types_rock>
| types | Both types (if 2nd exists) | Dark/Fire
| types_emoji | Type Discord emoji | <:00000:types_fire> <00001:types_dark>
| atk_iv | Attack IV stat | 15
| def_iv | Defense IV stat | 7
| sta_iv | Stamina IV stat | 13
| iv | IV stat (including percent sign) | 100%
| iv_rnd | Rounded IV stat | 96%
| is_great | Great League stats (bool) | true
| is_ultra | Ultra League stats (bool) | false
| is_pvp | Has either Great or Ultra league stats | true
| great_league_emoji | Great League emoji icon | <000000:league_great>
| ultra_league_emoji | Ultra League emoji icon | <000000:league_ultra>
| pvp_stats | PvP stat ranking strings | 
| height | Pokemon height | 0.79
| weight | Pokemon weight | 116
| is_ditto | Checks if Ditto | true
| original_pkmn_id | Pokedex ID of Ditto disguise | 13
| original_pkmn_id_3 | Pokedex ID of Ditto disguise (always 3 digits) | 013
| original_pkmn_name | Pokemon name of Ditto diguise | Weedle
| is_weather_boosted | Returns if Pokemon is weather boosted | true
| has_weather | Returns if Pokemon data has weather | false
| weather | Weather in-game name | PartlyCloudy
| weather_emoji | Weather in-game emoji | Weather
| username | Account username of account that found Pokemon | Frank0324
| spawnpoint_id | Spawnpoint ID Pokemon near | 3920849203840983204980
| encounter_id | Encounter ID of Pokemon | 392874987239487924
| despawn_time | Pokemon despawn time | 07:33:01 PM
| despawn_time_24h | Pokemon despawn time (24-hour format) | 19:33:01
| despawn_time_verified | Indicates if time is confirmed or not | `~` for not verified
| is_despawn_time_verified | Returns if despawn time is verified | true
| time_left | Minutes and seconds of time left until despawn | 29m, 30s
| geofence | Geofence name Pokemon is in | City1
| address | Google Maps or OSM Nominatim address from geocoordinates | 123 Fake St
| lat | Latitude coordinate of Pokemon location | 5.980921321
| lng | Longitude coordinate of Pokemon location | 3.109283009
| lat_5 | Latitude coordinate shortend to 5th precision | 5.98092
| lng_5 | Longitude coordinate shortend to 5th precision | 3.10928
| tilemaps_url | Static tile map url | http://tiles.example.com/static/pokemon-1.png
| gmaps_url | Google maps location url | https://maps.google.com/maps?q=5.980921321,3.109283009
| applemaps_url | Apple maps location url | https://maps.apple.com/maps?daddr=5.980921321,3.109283009
| wazemaps_url | Waze maps location url | https://www.waze.com/ul?ll=5.980921321,3.109283009&navigate=yes
| near_pokestop | Returns if Pokemon is near a Pokestop | true
| pokestop_id | Nearby Pokestop ID | 9382498723849792348798234.16
| pokestop_name | Name of nearby Pokestop | The Amazing Pokestop
| pokestop_url | Image url of nearby Pokestop | https://google.com/imgs/gym.png
| guild_name | Name of Guild | Test Guild
| guild_img_url | Icon image url of Guild | https://discordapp.com/image1.png
| date_time | Current date and time | 12/12/2020 12:12:12 PM
| br | Newline break | `\r\n`