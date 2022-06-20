# Dynamic Text Replacement  

Use any of the following in your embeds file to structure how notifications will look for Pokestop Lures.  

### Pokestops (Lures)  

| Place Holder | Description  | Example
|---|---|---|  
| has_lure | Returns if Pokestop has active lure module deployed | true
| lure_type | Pokestop lure module type | Glacial
| lure_expire_time | Time lure module will expire | 07:33:19 PM
| lure_expire_time_24h | Time lure module will expire (24-hour format) | 19:33:19
| lure_expire_time_left | Time left until lure module expires | 13m, 2s
| power_up_level | Pokestop power level | 1  
| power_up_points | Pokestop's total power level points | 100  
| power_up_end_time | Pokestop's power up end time | 10:15:09 PM  
| power_up_end_time_24h | Pokestop's power up end time (24-hour format) | 13:28:30  
| power_up_end_time_left | Pokestop's power up time left until expires | 14m, 10s   
| geofence | Geofence name raid boss is in | City1
| address | Google Maps or OSM Nominatim address from geocoordinates | 123 Fake St
| lat | Latitude coordinate of Pokemon location | 5.980921321
| lng | Longitude coordinate of Pokemon location | 3.109283009
| lat_5 | Latitude coordinate shortend to 5th precision | 5.98092
| lng_5 | Longitude coordinate shortend to 5th precision | 3.10928
| tilemaps_url | Static tile map url | http://tiles.example.com/static/pokemon-1.png
| gmaps_url | Google maps location url | https://maps.google.com/maps?q=5.980921321,3.109283009
| applemaps_url | Apple maps location url | https://maps.apple.com/maps?daddr=5.980921321,3.109283009
| wazemaps_url | Waze maps location url | https://www.waze.com/ul?ll=5.980921321,3.109283009&navigate=yes
| pokestop_id | Pokestop ID | 9382498723849792348798234.16
| pokestop_name | Name of Pokestop | The Amazing Pokestop
| pokestop_url | Image url of Gym | https://google.com/imgs/gym.png
| lure_img_url | Image url of lure icon | https://google.com/imgs/lure_501.png
| guild_name | Name of Guild | Test Guild
| guild_img_url | Icon image url of Guild | https://discordapp.com/image1.png
| date_time | Current date and time | 12/12/2020 12:12:12 PM
| br | Newline break | `\r\n`