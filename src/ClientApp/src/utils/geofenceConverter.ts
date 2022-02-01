export const iniToGeoJson = (data: any): any => {
    const geoJson: any = {
        type: 'FeatureCollection',
        features: [],
    };
    const fences = data.match(/\[([^\]]+)\]([^[]*)/g);
    for (const fence of fences) {
        const geofence: any = {
            type: 'Feature',
            properties: {
                name: fence.match(/\[([^\]]+)\]/)[1],
            },
            geometry: {
                type: 'Polygon',
                coordinates: [[]],
            },
        };
        const coordinates = fence.match(/[0-9\-.]+,\s*[0-9\-.]+/g).map((point: any) => [parseFloat(point.split(',')[1]), parseFloat(point.split(',')[0])]);
        geofence.geometry.coordinates[0] = coordinates;
        // Ensure first coordinate is also the last coordinate
        geofence.geometry.coordinates[0].push(geofence.geometry.coordinates[0][0]);
        geoJson.features.push(geofence);
    }
    return geoJson;
};

export const geoJsonToIni = (feature: any): any => {
    let geofence = [];
    if (feature.geometry.type === 'Polygon') {
        geofence.push(`[${feature.properties.name}]\n`);
        for (const coord of feature.geometry.coordinates) {
            coord.pop();
            for (const point of coord) {
                geofence.push(`${point[1]},${point[0]}\n`);
            }
        }
    }
    return geofence.join('');
};