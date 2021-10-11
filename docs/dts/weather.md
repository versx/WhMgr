# Dynamic Text Replacement  

Use any of the following in your embeds file to structure how notifications will look for field research quests.  

### S2Cell Weather  

| Place Holder | Description  | Example
|---|---|---|  
| id | S2Cell weather id | -9938028402
| weather_condition | In-game gameplay condition | Cloudy
| has_weather | Returns if there is weather set | true
| weather | In-game gameplay condition | Cloudy
| weather_img_url | Weather type image url | http://google.com/imgs/weather_1.png
| wind_direction | Wind blowing direction | true
| wind_level | Wind level | 285
| rain_level | Raid level | 285
| cloud_level | Cloud level | 285
| fog_level | Fog level | 285
| snow_level | Snow level | 285
| warn_weather | Warning weather | true
| special_effect_level | Special effect level | 2
| severity | Weather severity | None/Moderate/Extreme
| geofence | Geofence name weather cell is in | City1
| address | Google Maps or OSM Nominatim address from geocoordinates | 123 Fake St
| lat | Latitude coordinate of S2Cell weather location | 5.980921321
| lng | Longitude coordinate of S2Cell weather location | 3.109283009
| lat_5 | Latitude coordinate shortend to 5th precision | 5.98092
| lng_5 | Longitude coordinate shortend to 5th precision | 3.10928
| tilemaps_url | Static tile map url | http://tiles.example.com/static/pokemon-1.png
| gmaps_url | Google maps location url | https://maps.google.com/maps?q=5.980921321,3.109283009
| applemaps_url | Apple maps location url | https://maps.apple.com/maps?daddr=5.980921321,3.109283009
| wazemaps_url | Waze maps location url | https://www.waze.com/ul?ll=5.980921321,3.109283009&navigate=yes
| guild_name | Name of Guild | Test Guild
| guild_img_url | Icon image url of Guild | https://discordapp.com/image1.png
| date_time | Current date and time | 12/12/2020 12:12:12 PM
| br | Newline break | `\r\n`