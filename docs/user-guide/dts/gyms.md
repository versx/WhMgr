# Dynamic Text Replacement  

Use any of the following in your alerts file to structure how notifications will look for Gyms.  

### Gyms  

| Place Holder | Description  | Example
|---|---|---|  
| gym_id | Gym ID | 032840982304982034.16
| gym_name | Name of Gym | The Amazing Gym
| gym_url | Image url of Gym | https://google.com/imgs/gym.png
| gym_team | Current team that has gym control | Valor
| gym_team_emoji | Emoji icon of current team that has gym control | <:09833:valor>
| old_gym_team | Previous gym team that had gym control | Mystic
| old_gym_team_emoji | Emoji icon of previous gym team that has gym control | <:324987:mystic>
| team_changed | Returns if team's gym control changed | true
| in_battle | Returns if there's a current battle at the gym taking place | false
| under_attack | Returns if there's a current battle at the gym taking place | false
| is_ex | Returns if the gym is an ex raid eligible location | true
| ex_emoji | Ex emoji icon | <:809809:ex>
| slots_available | Number of available gym slots | 3
| geofence | Geofence name raid boss is in | City1
| lat | Latitude coordinate of Pokemon location | 5.980921321
| lng | Longitude coordinate of Pokemon location | 3.109283009
| lat_5 | Latitude coordinate shortend to 5th precision | 5.98092
| lng_5 | Longitude coordinate shortend to 5th precision | 3.10928
| tilemaps_url | Static tile map url | http://tiles.example.com/static/pokemon-1.png
| gmaps_url | Google maps location url | https://maps.google.com/maps?q=5.980921321,3.109283009
| applemaps_url | Apple maps location url | https://maps.apple.com/maps?daddr=5.980921321,3.109283009
| wazemaps_url | Waze maps location url | https://www.waze.com/ul?ll=5.980921321,3.109283009&navigate=yes
| br | Newline break | `\r\n`