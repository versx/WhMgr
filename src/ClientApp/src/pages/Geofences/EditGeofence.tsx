import React from 'react'
import {
    MapContainer,
    TileLayer,
    GeoJSON,
    FeatureGroup,
    useMap,
} from 'react-leaflet';
import { EditControl } from 'react-leaflet-draw';
import 'leaflet/dist/leaflet.css';
import 'leaflet-draw/dist/leaflet.draw.css'

import { Feature, Geometry } from 'geojson';
import L, { LatLngExpression, Layer } from 'leaflet';
import {
    Box,
    Button,
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
import withRouter from '../../hooks/WithRouter';
import { IGlobalProps } from '../../interfaces/IGlobalProps';
import { iniToGeoJson } from '../../utils/geofenceConverter';
import { onNestedStateChange } from '../../utils/nestedStateHelper';

// TODO: Convert geofence upon check changed and save state
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

class EditGeofence extends React.Component<IGlobalProps> {
    public state: any;

    constructor(props: IGlobalProps) {
        super(props);
        console.log('props:', props);
        this.state = {
            // TODO: Set default state values
            name: '',
            format: '',
            count: 0,
            geofence: '',
        };
        this.onInputChange = this.onInputChange.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
        this._onCreated = this._onCreated.bind(this);
        this._onEdited = this._onEdited.bind(this);
        this._onDeleted = this._onDeleted.bind(this);
        this._onFormatSaved = this._onFormatSaved.bind(this);
        //this._onFeatureGroupReady = this._onFeatureGroupReady.bind(this);
    }

    componentDidMount() {
        console.log('componentDidMount:', this.state, this.props);
        this.fetchData(this.props.params!.id);
    }

    componentDidUpdate() {
        //console.log('geofence:', this.state.geofence);
        if (!set) {
            const geofence = formatGeofenceToGeoJson(this.state.format, this.state.geofence);
            this.setState({
                ['count']: geofence.features.length,
            });
            set = true;
        }
    }

    fetchData(id: any) {
        fetch(config.apiUrl + 'admin/geofence/' + id, {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*',
            },
        })
        .then(async (response) => await response.json())
        .then(data => {
            this.setState({
                //...this.state,
                name: data.data.name,
                format: data.data.format,
                geofence: data.data.geofence,
            });
        }).catch(err => {
            console.error('error:', err);
            // TODO: Show error notification
        });
    }

    onInputChange(event: any) {
        onNestedStateChange(event, this);
    }

    handleSubmit(event: React.FormEvent<HTMLFormElement>) {
        event.preventDefault();

        console.log('handle submit state:', this.state);

        const id = this.props.params!.id;
        fetch(config.apiUrl + 'admin/geofence/' + id, {
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
            console.log('json:', json);
            this.setState({
                ['geofence']: json,
                ['count']: json.features.length,
            });
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
        // Populate the leaflet FeatureGroup with the geoJson layers
        console.log('onFeatureGroupReady');
        if (!this.state.format || !this.state.geofence) {
            return;
        }
        const geofence = formatGeofenceToGeoJson(this.state.format, this.state.geofence);
        let leafletGeoJSON = new L.GeoJSON(geofence);
        let leafletFG = reactFGref;

        leafletGeoJSON.eachLayer((layer: any) => {
            if (!loaded && leafletFG) {
                const html = `
                <b>Name:</b> ${layer.feature.properties.name}<br>
                <b>Area:</b> ${0} km2
`;
                layer.bindTooltip(html);
                leafletFG.addLayer(layer);
            }
        });

        if (!loaded && leafletFG) {
            loaded = true;
            console.log('loaded: true');
        }
    
        // Store the ref for future access to content
        this._editableFG = reactFGref;
    };

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
            text: 'Edit ' + this.props.params!.id,
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
            <div className={classes.container} style={{ paddingTop: '50px', paddingBottom: '20px' }}>
                <Container>
                    <Box component="form" method="POST" action=""  onSubmit={this.handleSubmit} sx={{ mt: 3 }}>
                        <BreadCrumbs crumbs={breadcrumbs} />
                        <Typography variant="h5" component="h2" >
                            Edit Geofence {this.props.params!.id}
                        </Typography>
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
                                        onChange={this.onInputChange}
                                    />
                                </Grid>
                                <Grid item xs={12}>
                                    <TextField
                                        id="count"
                                        name="count"
                                        variant="outlined"
                                        label="Geofence Count"
                                        type="number"
                                        value={this.state.count}
                                        InputProps={{
                                            readOnly: true,
                                        }}
                                        fullWidth
                                    />
                                </Grid>
                                <Grid item xs={12}>
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
                                                console.log('reactFGref:', reactFGref);
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
                </Container>

            </div>
        );
    }
}

export default withRouter(EditGeofence);