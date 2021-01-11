# Dynamic Text Replacement  

Use any of the following in your alerts file to structure how notifications will look for nests.  

### Nests  

| Place Holder | Description  | Example
|---|---|---|  
| pkmn_id  | Pokedex ID  |  1
| pkmn_id_3  | Pokedex ID (always 3 digits)  | 001
| pkmn_name | Pokemon name | Bulbasaur
| pkmn_img_url | Pokemon image url | http://example.com/your-specified-pokemon-url
| avg_spawns | Average amount of spawns in the nests | 34
| nest_name | Nest/Park name | Best Park Ever
| type_1 | Pokemon type | Dark
| type_2 | Pokemon type | Water
| type_1_emoji | Pokemon type emoji | <:00000:types_water>
| type_2_emoji | Pokemon type emoji | <:00000:types_rock>
| types | Both types (if 2nd exists) | Dark/Fire
| types_emoji | Type Discord emoji | <:00000:types_fire> <00001:types_dark>
| geofence | Geofence name nest/park is in | City1
| address | Google Maps or OSM Nominatim address from geocoordinates | 123 Fake St
| lat | Latitude coordinate of Pokemon location | 5.980921321
| lng | Longitude coordinate of S2Cell weather location | 3.109283009
| lat_5 | Latitude coordinate shortend to 5th precision | 5.98092
| lng_5 | Longitude coordinate shortend to 5th precision | 3.10928
| tilemaps_url | Static tile map url | http://tiles.example.com/static/pokemon-1.png
| gmaps_url | Google maps location url | https://maps.google.com/maps?q=5.980921321,3.109283009
| applemaps_url | Apple maps location url | https://maps.apple.com/maps?daddr=5.980921321,3.109283009
| wazemaps_url | Waze maps location url | https://www.waze.com/ul?ll=5.980921321,3.109283009&navigate=yes
| date_time | Current date and time | 12/12/2020 12:12:12 PM
| br | Newline break | `\r\n`