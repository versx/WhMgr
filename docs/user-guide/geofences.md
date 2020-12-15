# Geofences

Geofences define area borders and perimeters for a city or multiple cities. Each alarm can take multiple geofence files or names, as well as a combination of both.  

**Geofences must be placed in the root directory of the executable (`bin/debug/netcoreapp2.1`) in the `geofences` folder.**  
*Note:* Supports INI geofence file format as well as GeoJSON geofence file format:  

## Examples

## __INI Format__
```ini
[City1]
34.00,-117.00
34.01,-117.01
34.02,-117.02
34.03,-117.03
[City2]
33.00,-118.00
33.01,-118.01
33.02,-118.02
33.03,-118.03
```
## __GeoJSON Format__
```json
{
    "type": "FeatureCollection",
    "features": [
        {
            "type": "Feature",
            "id": 12143584,
            "geometry": {
                "type": "Polygon",
                "coordinates": [
                    [
                        [
                            -117.185508,
                            34.05361
                        ],
                        [
                            -117.185397,
                            34.05366
                        ],
                        [
                            -117.185326,
                            34.053564
                        ],
                        [
                            -117.184819,
                            34.053828
                        ],
                        [
                            -117.184457,
                            34.054009
                        ],
                        [
                            -117.18409,
                            34.05353
                        ],
                        [
                            -117.184027,
                            34.053448
                        ],
                        [
                            -117.184991,
                            34.052942
                        ],
                        [
                            -117.185508,
                            34.05361
                        ]
                    ]
                ]
            },
            "properties": {
                "name": "Unknown Areaname",
                "stroke": "#352BFF",
                "stroke-width": 2.0,
                "stroke-opacity": 1.0,
                "fill": "#0651FF",
                "fill-opacity": 0.5,
                "priority": 2,
            }
        }
    ]
}
```


Optional: **GeoJSON to individual INI format geofence files converter:** https://gist.github.com/versx/a0915c6bd95a080b6ff60cd539d4feb6  