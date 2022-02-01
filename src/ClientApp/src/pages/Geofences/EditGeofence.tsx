import React from 'react'
import {
    MapContainer,
    TileLayer,
    GeoJSON,
} from 'react-leaflet';
import { Feature, Geometry } from 'geojson';
import { LatLngExpression, Layer } from 'leaflet';
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

class EditGeofence extends React.Component<IGlobalProps> {
    public state: any;

    constructor(props: IGlobalProps) {
        super(props);
        console.log('props:', props);
        this.state = {
            // TODO: Set default state values
            name: '',
            format: '',
            geofence: '',
        };
        this.onInputChange = this.onInputChange.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
    }

    componentDidMount() {
        console.log('componentDidMount:', this.state, this.props);
        this.fetchData(this.props.params!.id);
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

    render() {
        const handleCancel = () => window.location.href = '/dashboard/geofences';

        const handleOnEachFeature = (feature: Feature<Geometry, any>, layer: Layer) => {
            console.log('handleOnEachFeature:', feature, layer);
        };

        const handleOnFormatChange = (event: any) => {
            this.onInputChange(event);
            const newFormat = event.target.value;
            const isGeoJson = newFormat === '.json';
            console.log('new format:', newFormat, 'geojson:', isGeoJson);
            this.setState({
                ...this.state,
                geofence: formatGeofenceToGeoJson(this.state.geofence),
            });
            // TODO: Check new format
            // TODO: Convert geofence
            // TODO: Save state
        };

        const formatGeofenceToGeoJson = (data: any): any => {
            //console.log('format:', this.state.format, 'data:', data);
            if (data.length === 0) {
                return null;
            }
            switch (this.state.format) {
                case '.json':
                    return JSON.parse(data);
                case '.txt':
                // case '.ini':
                    return iniToGeoJson(data);
                default:
                    throw Error('Unsupported geofence format');
            }
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
            href: '/dashboard',
            selected: false,
        }, {
            text: 'Geofences',
            color: 'inherit',
            href: '/dashboard/geofences',
            selected: false,
        }, {
            text: 'Edit Geofence ' + this.props.params!.id,
            color: 'primary',
            href: '',
            selected: true,
        }];

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
                                        {this.state.geofence && (
                                            <GeoJSON
                                                key="geofence"
                                                onEachFeature={handleOnEachFeature}
                                                data={formatGeofenceToGeoJson(this.state.geofence)}
                                            />
                                        )}
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