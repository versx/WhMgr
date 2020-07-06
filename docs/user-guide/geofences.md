# Geofences

Geofences define area borders and permitters for a city or multiple cities. Geofence files are expected to be saved in the `ini` format with the `.txt` extension using a city name in brackets `[ExampleCity]` followed by latitude and longitude coordinate pairs on each line.  

**Geofences must be placed in the root directory of the executable in the `Geofences` folder.**  
Each alarm can only take in one geofence file, but one geofence file can contain multiple sets of geofences.  

### Example
Single geofence:  
```ini
[Innsbruck]
47.288805, 11.421852
47.263120, 11.449569
47.243159, 11.357291
47.267019, 11.328221
```

Multiple geofences in one file:  
```ini
[Paris]
34.00,-117.00
34.01,-117.01
34.02,-117.02
34.03,-117.03
[London]
33.00,-118.00
33.01,-118.01
33.02,-118.02
33.03,-118.03
```