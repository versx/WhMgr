import React from 'react'
import {
    MapContainer,
    TileLayer,
    GeoJSON,
    FeatureGroup,
} from 'react-leaflet';
import { EditControl } from 'react-leaflet-draw';
import 'leaflet/dist/leaflet.css';
import 'leaflet-draw/dist/leaflet.draw.css'

import { Feature, Geometry } from 'geojson';
import L, { LatLngExpression, Layer } from 'leaflet';
import {
    Box,
    Button,
    Chip,
    Container,
    FormControl,
    FormControlLabel,
    FormLabel,
    Grid,
    Radio,
    RadioGroup,
    TextField,
    Typography,
} from '@mui/material';
import { makeStyles } from '@mui/styles';

import config from '../../config.json';
import { BreadCrumbs } from '../../components/BreadCrumbs';
import { GeofenceModal } from '../../components/Modals';
import MapButton from '../../components/MapButton';
import { withRouter } from '../../hooks';
import { IGlobalProps } from '../../interfaces/IGlobalProps';
import { geoJsonToIni, iniToGeoJson } from '../../utils/geofenceConverter';
import { onNestedStateChange } from '../../utils/nestedStateHelper';

// TODO: Move geofence modal logic to modal component and pass map via props

let set = false;
let loaded = false;
const formatGeofenceToGeoJson = (format: string, data: any): any => {
    //console.log('format:', format, 'data:', data);
    if (data.length === 0) {
        return null;
    }
    if (typeof data === 'object') {
        return data;
    }
    switch (format) {
        case '.json':
            return JSON.parse(data);
        case '.txt':
        // case '.ini':
            return iniToGeoJson(data);
        default:
            throw Error('Unsupported geofence format');
    }
};

class NewGeofence extends React.Component<IGlobalProps> {
    public state: any;

    constructor(props: IGlobalProps) {
        super(props);
        console.log('props:', props);
        this.state = {
            // TODO: Set default state values
            name: '',
            format: '.json',
            count: 0,
            geofence: null,
            open: false,
            importFormat: '.txt',
            importGeofence: {},
            exportOpen: false,
            exportFormat: '.json',
            exportGeofence: {},
        };
        this.onInputChange = this.onInputChange.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
        this._onCreated = this._onCreated.bind(this);
        this._onEdited = this._onEdited.bind(this);
        this._onDeleted = this._onDeleted.bind(this);
        this._onFormatSaved = this._onFormatSaved.bind(this);
        this.loadGeofence = this.loadGeofence.bind(this);
    }

    onInputChange(event: any) {
        onNestedStateChange(event, this);
    }

    handleSubmit(event: React.FormEvent<HTMLFormElement>) {
        event.preventDefault();

        console.log('handle submit state:', this.state);

        fetch(config.apiUrl + 'admin/geofence/new', {
            method: 'POST',
            body: JSON.stringify(this.state),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                // TODO: Csrf token or auth token
            },
        }).then(async (response) => await response.json())
          .then((data: any) => {
            console.log('response:', data);
            if (data.status !== 'OK') {
                alert(data.error);
                return;
            }
            window.location.href = config.homepage + 'geofences';
            // TODO: Redirect to /geofences or show notification (show notification on list geofences page)
        }).catch((err) => {
            console.error('error:', err);
            event.preventDefault();
        });
    }

    _onFormatSaved() {
        //const geofence = formatGeofenceToGeoJson(this.state.format, this.state.geofence.toString());
        //console.log('this.state.geofence:', geofence);
        this.setState({
            ['geofence']: this.state.geofence,
            //['count']: geofence.features.length,
        });
        console.log('state:', this.state);
    }

    _onCreated(event: any) {
        console.log('onCreated:', event);
        const layer = event.layer;
        if (this._editableFG) {
            this._editableFG.addLayer(layer);
            const json = this._editableFG.toGeoJSON();
            //console.log('json:', json);
            this.setState({
                ['geofence']: json,
                ['count']: json.features.length,
            });
            console.log('this.state:', this.state);
            this._onFormatSaved();
        }
    }

    _onEdited(event: any) {
        console.log('onEdited:', event);
        this._onFormatSaved();
    }

    _onDeleted(event: any) {
        console.log('onDeleted:', event);
        this._onFormatSaved();
    }

    _onFeatureGroupReady(reactFGref: any) {
        if (reactFGref) {
            this._editableFG = reactFGref;
        }
    };

    loadGeofence(data: any) {
        let leafletGeoJSON = new L.GeoJSON(data);
        leafletGeoJSON.eachLayer((layer: any) => {
            if (this._editableFG) {
                const html = `
                <b>Name:</b> ${layer.feature.properties.name}<br>
                <b>Area:</b> ${0} km2
`;
                layer.bindTooltip(html);
                this._editableFG.addLayer(layer);
            }
        });
        const json = this._editableFG.toGeoJSON();
        this.setState({
            ...this.state,
            open: false,
            geofence: json,
            count: json.features.length,
        });
    }

    _editableFG: any = null;

    render() {
        const handleCancel = () => window.location.href = config.homepage + 'geofences';

        const handleOnEachFeature = (feature: Feature<Geometry, any>, layer: Layer) => {
            console.log('handleOnEachFeature:', feature, layer);
        };

        const handleOnFormatChange = (event: any) => {
            this.onInputChange(event);
            const newFormat = event.target.value;
            this.setState({
                ...this.state,
                count: this.state.geofence.features.length,
                format: newFormat,
                //geofence: formatGeofenceToGeoJson(newFormat, this.state.geofence),
            });
            // TODO: Check new format
            // TODO: Convert geofence
            // TODO: Save state
        };

        const copyToClipboard = (text: string) => {
            navigator.clipboard.writeText(text);
        };

        const classes: any = makeStyles({
            container: {
                 //paddingTop: theme.spacing(10),
            },
            table: {
            },
            title: {
                display: 'flex',
                fontWeight: 600,
                marginLeft: '10px',
                fontSize: '22px',
            },
            titleContainer: {
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                marginBottom: '20px',
            },
            buttonGroup: {
                display: 'flex',
            },
            buttonContainer: {
                paddingTop: '20px',
            },
        });

        const breadcrumbs = [{
            text: 'Dashboard',
            color: 'inherit',
            href: config.homepage,
            selected: false,
        }, {
            text: 'Geofences',
            color: 'inherit',
            href: config.homepage + 'geofences',
            selected: false,
        }, {
            text: 'New',
            color: 'primary',
            href: '',
            selected: true,
        }];

        const shapeOptions = {
            stroke: true,
            color: '#3388ff',
            weight: 3,
            opacity: 1,
            fill: true,
            fillColor: null,
            fillOpacity: 0.2,
        };

        return (
            <div className={classes.container} style={{paddingTop: '50px', paddingBottom: '20px', paddingLeft: '20px', paddingRight: '20px'}}>
                <Box component="form" method="POST" action=""  onSubmit={this.handleSubmit} sx={{ mt: 3 }}>
                    <BreadCrumbs crumbs={breadcrumbs} />
                    <Grid container spacing={2}>
                        <Grid item xs={12} sm={6}>
                            <Typography variant="h5" component="h2">
                                New Geofence
                            </Typography>
                        </Grid>
                        <Grid item xs={12} sm={6} style={{display: 'flex', justifyContent: 'flex-end'}}>
                            <Button
                                variant="contained"
                                color="primary"
                                type="button"
                                onClick={() => this.setState({
                                    ['exportOpen']: true,
                                })}
                            >
                                Export
                            </Button>
                        </Grid>
                    </Grid>
                    <Typography sx={{ mt: 2 }}>
                        Geofence description goes here
                    </Typography>
                    <div style={{paddingBottom: '20px', paddingTop: '20px'}}>
                        <Grid container spacing={2}>
                            <Grid item xs={12}>
                                <TextField
                                    id="name"
                                    name="name"
                                    variant="outlined"
                                    label="Name"
                                    type="text"
                                    value={this.state.name}
                                    fullWidth
                                    required
                                    onChange={this.onInputChange}
                                />
                            </Grid>
                            <Grid item xs={6}>
                                <FormControl>
                                    <FormLabel id="format-label">Save Format</FormLabel>
                                    <RadioGroup
                                        row
                                        aria-labelledby="format-label"
                                        id="format"
                                        name="format"
                                    >
                                        <FormControlLabel
                                            id="format"
                                            name="format"
                                            value=".txt"
                                            control={<Radio checked={this.state.format === '.txt'} onChange={handleOnFormatChange} />}
                                            label="INI"
                                        />
                                        <FormControlLabel
                                            id="format"
                                            name="format"
                                            value=".json"
                                            control={<Radio checked={this.state.format === '.json'} onChange={handleOnFormatChange} />}
                                            label="GeoJSON"
                                        />
                                    </RadioGroup>
                                </FormControl>
                            </Grid>
                            <Grid item xs={6} style={{display: 'flex', justifyContent: 'flex-end'}}>
                                <Chip label={"Geofences: " + this.state.count} />
                            </Grid>
                            <Grid item xs={12}>
                                <MapContainer
                                    center={config.map.startLocation as LatLngExpression}
                                    zoom={config.map.startZoom}
                                    scrollWheelZoom={true}
                                    style={{height: '600px'}}
                                >
                                    <TileLayer url={config.map.tileserver} />
                                    <FeatureGroup
                                        ref={(reactFGref: any) => {
                                            //console.log('reactFGref:', reactFGref);
                                            this._onFeatureGroupReady(reactFGref);
                                        }}
                                    >
                                        <EditControl
                                            position="topleft"
                                            onEdited={this._onEdited}
                                            onCreated={this._onCreated}
                                            onDeleted={this._onDeleted}
                                            draw={{
                                                polyline: false,
                                                polygon: {
                                                    allowIntersection: true,
                                                    showArea: true,
                                                    metric: 'km',
                                                    precision: {
                                                        km: 2,
                                                    },
                                                    shapeOptions,
                                                },
                                                rectangle: {
                                                    showRadius: true,
                                                    metric: true,
                                                    shapeOptions,
                                                },
                                                circle: false,
                                                marker: false,
                                                circlemarker: false,
                                            }}
                                        />
                                    </FeatureGroup>
                                    {/*this.state.geofence && (
                                        <GeoJSON
                                            key="geofence"
                                            onEachFeature={handleOnEachFeature}
                                            data={formatGeofenceToGeoJson(this.state.format, this.state.geofence)}
                                        />
                                    )*/}
                                    <MapButton
                                        tooltip="Import geofence"
                                        icon="<img src='https://cdn-icons-png.flaticon.com/512/151/151901.png' width='16' height='16' />"
                                        onClick={(btn: any, map: any) => {
                                            this.setState({
                                                ['open']: true,
                                            });
                                        }}
                                    />
                                    <MapButton
                                        tooltip="Delete all layers"
                                        icon="<img src='https://www.iconpacks.net/icons/1/free-trash-icon-347-thumb.png' width='16' height='16' />"
                                        onClick={(btn: any, map: any) => {
                                            const result = window.confirm('Are you sure you want to delete all shapes?');
                                            if (!result) {
                                                return;
                                            }
                                            this._editableFG.clearLayers();
                                            this.setState({
                                                ['count']: 0,
                                                ['geofence']: null,
                                            });
                                        }}
                                    />
                                    <GeofenceModal
                                        title="Import Geofence"
                                        body={(
                                            <Grid container spacing={2}>
                                                <Grid item xs={12}>
                                                    <TextField
                                                        id="importGeofence"
                                                        name="importGeofence"
                                                        variant="outlined"
                                                        label="Geofence"
                                                        type="text"
                                                        rows="15"
                                                        fullWidth
                                                        multiline
                                                        onChange={this.onInputChange}
                                                    />
                                                </Grid>
                                                <Grid item xs={12}>
                                                    <FormControl>
                                                        <FormLabel id="format-label">Format</FormLabel>
                                                        <RadioGroup
                                                            row
                                                            aria-labelledby="format-label"
                                                            id="format"
                                                            name="format"
                                                        >
                                                            <FormControlLabel
                                                                id="format"
                                                                name="format"
                                                                value=".txt"
                                                                control={<Radio checked={this.state.importFormat === '.txt'} onChange={() => {
                                                                    this.setState({
                                                                        ['importFormat']: '.txt',
                                                                    });
                                                                }} />}
                                                                label="INI"
                                                            />
                                                            <FormControlLabel
                                                                id="format"
                                                                name="format"
                                                                value=".json"
                                                                control={<Radio checked={this.state.importFormat === '.json'} onChange={() => {
                                                                    this.setState({
                                                                        ['importFormat']: '.json',
                                                                    });
                                                                }} />}
                                                                label="GeoJSON"
                                                            />
                                                        </RadioGroup>
                                                    </FormControl>
                                                </Grid>
                                                <Grid item xs={12}>
                                                    <div className={classes.buttonContainer}>
                                                        <Button
                                                            variant="contained"
                                                            color="primary"
                                                            style={{marginRight: '20px'}}
                                                            type="submit"
                                                            onClick={() => {
                                                                // TODO: Convert based on format and add to map
                                                                const format = this.state.importFormat;
                                                                const data = this.state.importGeofence;
                                                                const geofence = formatGeofenceToGeoJson(format, data);
                                                                this.loadGeofence(geofence);
                                                            }}
                                                        >
                                                            Save
                                                        </Button>
                                                        <Button
                                                            variant="outlined"
                                                            color="primary"
                                                            onClick={() => {
                                                                this.setState({
                                                                    ['open']: false,
                                                                })
                                                            }}
                                                        >
                                                            Close
                                                        </Button>
                                                    </div>
                                                </Grid>
                                            </Grid>
                                        )}
                                        show={this.state.open}
                                        onClose={() => {
                                            this.setState({
                                                ['open']: false,
                                            });
                                        }}
                                    />
                                    <GeofenceModal
                                        title="Export Geofence"
                                        body={(
                                            <Grid container spacing={2}>
                                                <Grid item xs={12}>
                                                    <TextField
                                                        id="exportGeofence"
                                                        name="exportGeofence"
                                                        variant="outlined"
                                                        label="Geofence"
                                                        type="text"
                                                        rows="15"
                                                        value={this.state.exportGeofence}
                                                        fullWidth
                                                        multiline
                                                        InputProps={{
                                                            readOnly: true,
                                                        }}
                                                        onChange={this.onInputChange}
                                                    />
                                                </Grid>
                                                <Grid item xs={12}>
                                                    <FormControl>
                                                        <FormLabel id="format-label">Export Format</FormLabel>
                                                        <RadioGroup
                                                            row
                                                            aria-labelledby="format-label"
                                                            id="format"
                                                            name="format"
                                                        >
                                                            <FormControlLabel
                                                                id="format"
                                                                name="format"
                                                                value=".txt"
                                                                control={<Radio checked={this.state.exportFormat === '.txt'} onChange={() => {
                                                                    // Convert geofence
                                                                    //const geofence = formatGeofenceToGeoJson('.txt', this.state.geofence);
                                                                    const iniData: any = [];
                                                                    this._editableFG.eachLayer((layer: any) => {
                                                                        const geojson = layer.toGeoJSON();
                                                                        if (geojson) {
                                                                            const ini = geoJsonToIni(geojson);
                                                                            iniData.push(ini)
                                                                        }
                                                                    });
                                                                    this.setState({
                                                                        ['exportGeofence']: iniData.join(''),
                                                                        ['exportFormat']: '.txt',
                                                                    });
                                                                }} />}
                                                                label="INI"
                                                            />
                                                            <FormControlLabel
                                                                id="format"
                                                                name="format"
                                                                value=".json"
                                                                control={<Radio checked={this.state.exportFormat === '.json'} onChange={() => {
                                                                    // Convert geofence
                                                                    const geofence = iniToGeoJson(this.state.exportGeofence);
                                                                    const json = JSON.stringify(geofence, null, 2);
                                                                    this.setState({
                                                                        ['exportGeofence']: json,
                                                                        ['exportFormat']: '.json',
                                                                    });
                                                                }} />}
                                                                label="GeoJSON"
                                                            />
                                                        </RadioGroup>
                                                    </FormControl>
                                                </Grid>
                                                <Grid item xs={12}>
                                                    <div className={classes.buttonContainer}>
                                                        <Button
                                                            variant="contained"
                                                            color="primary"
                                                            style={{marginRight: '20px'}}
                                                            type="button"
                                                            onClick={() => {
                                                                copyToClipboard(this.state.exportGeofence);
                                                            }}
                                                        >
                                                            Copy to Clipboard
                                                        </Button>
                                                        <Button
                                                            variant="outlined"
                                                            color="primary"
                                                            onClick={() => {
                                                                this.setState({
                                                                    ['exportOpen']: false,
                                                                })
                                                            }}
                                                        >
                                                            Close
                                                        </Button>
                                                    </div>
                                                </Grid>
                                            </Grid>
                                        )}
                                        show={this.state.exportOpen}
                                        onClose={() => {
                                            this.setState({
                                                ['exportOpen']: false,
                                            });
                                        }}
                                    />
                                </MapContainer>
                            </Grid>
                        </Grid>
                    </div>
                    <div className={classes.buttonContainer}>
                        <Button
                            variant="contained"
                            color="primary"
                            style={{marginRight: '20px'}}
                            type="submit"
                        >
                            Save
                        </Button>
                        <Button
                            variant="outlined"
                            color="primary"
                            onClick={handleCancel}
                        >
                            Cancel
                        </Button>
                    </div>
                </Box>
            </div>
        );
    }
}

export default withRouter(NewGeofence);