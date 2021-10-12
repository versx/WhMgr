# Reverse Geocoding  

Convert latitude and longitude coordinates to street addresses.  

Supported Providers:  

- Google Maps  
- OpenStreetMaps Nominatim  

## Google Maps Geocoding  
### __Setup__  
- [Getting started](https://console.cloud.google.com/google/maps-apis/start)  
- [Create an API key](https://developers.google.com/maps/documentation/geocoding/get-api-key)  

### __Available DTS Options__  
```json
{
  "plus_code": {
    "compound_code": "2X5Q\u002BXX Yucaipa, CA, USA",
    "global_code": "85642X5Q\u002BXX"
  },
  "results": [
    {
      "address_components": [
        {
          "long_name": "13403",
          "short_name": "13403",
          "types": [
            "street_number"
          ]
        },
        {
          "long_name": "Canyon Crest Road",
          "short_name": "Canyon Crest Rd",
          "types": [
            "route"
          ]
        },
        {
          "long_name": "Yucaipa",
          "short_name": "Yucaipa",
          "types": [
            "locality",
            "political"
          ]
        },
        {
          "long_name": "San Bernardino County",
          "short_name": "San Bernardino County",
          "types": [
            "administrative_area_level_2",
            "political"
          ]
        },
        {
          "long_name": "California",
          "short_name": "CA",
          "types": [
            "administrative_area_level_1",
            "political"
          ]
        },
        {
          "long_name": "United States",
          "short_name": "US",
          "types": [
            "country",
            "political"
          ]
        },
        {
          "long_name": "92399",
          "short_name": "92399",
          "types": [
            "postal_code"
          ]
        },
        {
          "long_name": "5823",
          "short_name": "5823",
          "types": [
            "postal_code_suffix"
          ]
        }
      ],
      "formatted_address": "13403 Canyon Crest Rd, Yucaipa, CA 92399, USA",
      "geometry": {
        "bounds": {
          "northeast": {
            "lat": 34.0099215,
            "lng": -117.0098454
          },
          "southwest": {
            "lat": 34.009714,
            "lng": -117.010073
          }
        },
        "location": {
          "lat": 34.0098401,
          "lng": -117.0099373
        },
        "location_type": "ROOFTOP",
        "viewport": {
          "northeast": {
            "lat": 34.01116673029149,
            "lng": -117.0086102197085
          },
          "southwest": {
            "lat": 34.00846876970849,
            "lng": -117.0113081802915
          }
        }
      },
      "place_id": "ChIJGfc7IW1Q24ARogf1hYAtakw",
      "types": [
        "premise"
      ]
    }
  ],
  "status": "OK"
}
```


## OpenStreetMaps Nominatim  
### __Setup__  
- [Self Hosting](https://nominatim.org/release-docs/latest/admin/Installation/)  
- [Testing Endpoint](https://nominatim.openstreetmap.org) (never use in production)  

### __Available DTS Options__  
```json
{
  "place_id": 265892028,
  "licence": "Data \u00A9 OpenStreetMap contributors, ODbL 1.0. https://osm.org/copyright",
  "osm_type": "way",
  "osm_id": 30602906,
  "lat": "34.010038",
  "lon": "-117.010446",
  "place_rank": 30,
  "category": "place",
  "type": "house",
  "importance": -1.15,
  "addresstype": "place",
  "name": null,
  "display_name": "36398, Canyon Terrace Drive, Yucaipa, San Bernardino County, California, 92399, United States",
  "address": {
    "house_number": "36398",
    "road": "Canyon Terrace Drive",
    "neighbourhood": null,
    "suburb": null,
    "city": "Yucaipa",
    "county": "San Bernardino County",
    "state": "California",
    "postcode": "92399",
    "country": "United States",
    "country_code": "us"
  },
  "boundingbox": [
    "34.009988",
    "34.010088",
    "-117.010496",
    "-117.010396"
  ]
}

```