import React, { ReactNode, useEffect, useState } from 'react'
import {
    Accordion,
    AccordionDetails,
    AccordionSummary,
    Box,
    Button,
    Card,
    CardContent,
    CardHeader,
    Container,
    FormControl,
    FormControlLabel,
    Grid,
    InputLabel,
    MenuItem,
    Select,
    SelectChangeEvent,
    Switch,
    TextField,
    Typography,
} from '@mui/material';
import {
    ExpandMore as ExpandMoreIcon,
} from '@mui/icons-material';
import { makeStyles } from '@mui/styles';

import { Path, set, lensPath } from 'ramda';

import config from '../config.json';
import { DatabaseInfo } from '../components/DatabaseInfo';
import { MultiSelect } from '../components/MultiSelect';
import withRouter from '../hooks/WithRouter';
import { IGlobalProps } from '../interfaces/IGlobalProps';

/**
 * Flatten a multidimensional object
 *
 * For example:
 *   flattenObject{ a: 1, b: { c: 2 } }
 * Returns:
 *   { a: 1, c: 2}
 */
export const flattenObject = (obj: any, parent?: string) => {
    const flattened: any = {}
    Object.keys(obj).forEach((key) => {
        const value = obj[key];
        const keyed = parent ? parent + '.' + key : key;
        if (typeof value === 'object' && value !== null && !Array.isArray(value)) {
            Object.assign(flattened, flattenObject(value, keyed));
        } else {
            flattened[keyed] = value;
        }
    });  
    return flattened
}

class EditConfig extends React.Component<IGlobalProps> {
    public state: any;

    constructor(props: IGlobalProps) {
        super(props);
        console.log('props:', props);
        this.state = {
            // TODO: Set default state values
            name: props.params!.id,
            host: '*',
            port: 8008,
            locale: 'en',
            value: 0,
            despawnTimeMinimumMinutes: 5,
            checkForDuplicates: false,
            discord: '',
            discords: [],
            debug: false,
            logLevel: 0,
            stripeApi: {
                apiKey: '',
            },
            shortUrlApi: {
                enabled: false,
                apiUrl: '',
                signature: '',
            },
            urls: {
                scannerMap: '',
            },
            eventPokemon: {
                pokemonIds: [],
                minimumIV: 90,
                type: 'Include',
            },
            iconStyles: {
            },
            staticMaps: {
            },
            twilio: {
                enabled: false,
                accountSid: '',
                authToken: '',
                from: '',
                userIds: [],
                roleIds: [],
                pokemonIds: [],
                minIV: 90,
            },
            reverseGeocoding: {
                provider: '',
                cacheToDisk: false,
                gmaps: {
                    key: '',
                    schema: '',
                },
                nominatim: {
                    endpoint: '',
                    schema: '',
                },
            },
            database: {
                main: {
                    host: '',
                    port: 3306,
                    username: '',
                    password: '',
                    database: 'brockdb',
                },
                scanner: {
                    host: '',
                    port: 3306,
                    username: '',
                    password: '',
                    database: 'rdmdb',
                },
                nests: {
                    host: '',
                    port: 3306,
                    username: '',
                    password: '',
                    database: 'manualdb',
                },
            },
        };
        this.handleChange = this.handleChange.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
        this.handlePanelExpanded = this.handlePanelExpanded.bind(this);
        this.setObjectByPath = this.setObjectByPath.bind(this);
    }

    componentDidMount() {
        console.log('componentDidMount:', this.state, this.props);
        this.fetchData(this.props.params!.id);
    }

    fetchData(id: any) {
        fetch(config.apiUrl + 'admin/config/' + id, {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*',
            },
        })
        .then(async (response) => await response.json())
        .then(data => {
            //console.log('config data:', data);
            //this.setState(data.data.config);
            const flat = flattenObject(data.data.config);
            console.log('flat config:', flat);
            const keys: string[] = Object.keys(data.data.config);
            for (const key of keys) {
                //console.log('key:', key, 'data:', data.data.config[key]);
                this.setState({ [key]: data.data.config[key] });
            }
            this.setState({ ['discords']: Object.values(data.data.discords) });
        }).catch(err => {
            console.error('error:', err);
            // TODO: Show error notification
        });
    }

    handleChange(event: any) {
        const { name, value } = event.target;
        console.log('event:', event);
        //this.setState({ [name]: value });
        //this.setState(state => ({ ...state, [name]: value }));
        this.setObjectByPath([name], value);
        console.log('state:', this.state);
    }

    handlePanelExpanded = (panel: string) => (event: React.SyntheticEvent, isExpanded: boolean) => {
        this.setState({ ['expanded']: isExpanded ? panel : false });
    }

    handleSubmit(event: React.FormEvent<HTMLFormElement>) {
        event.preventDefault();

        console.log('handle submit state:', this.state);

        const id = this.props.params!.id;
        fetch(config.apiUrl + 'admin/config/' + id, {
            method: 'POST',
            body: JSON.stringify(this.state),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
            },
        }).then(async (response) => await response.json())
          .then((data: any) => {
            console.log('response:', data);

        }).catch((err) => {
            console.error('error:', err);
            event.preventDefault();
        });
    }

    setObjectByPath(fieldPath: Path, value: any) {
        this.setState({
          config: set(lensPath(fieldPath), value, this.state.config),
        });
    }

    render() {
        const handleCancel = () => window.location.href = '/configs';

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

        return (
            <div className={classes.container} style={{ paddingTop: '50px', paddingBottom: '20px' }}>
                <Container>
                    <Box component="form" method="POST" action=""  onSubmit={this.handleSubmit} sx={{ mt: 3 }}>
                        <Typography variant="h5" component="h2" >
                            Edit Config {this.props.params!.id}
                        </Typography>
                        <Typography sx={{ mt: 2 }}>
                            Config description goes here
                        </Typography>
                        <div style={{paddingBottom: '20px', paddingTop: '20px'}}>
                            <Accordion expanded={this.state.expanded === 'panel1'} onChange={this.handlePanelExpanded('panel1')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>General</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2} style={{paddingTop: '20px', paddingBottom: '20px'}}>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="name"
                                                name="name"
                                                variant="outlined"
                                                label="Name"
                                                value={this.state.name}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={6} sm={6}>
                                            <TextField
                                                id="host"
                                                name="host"
                                                variant="outlined"
                                                label="Host"
                                                value={this.state.host}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={6} sm={6}>
                                            <TextField
                                                id="port"
                                                name="port"
                                                type="number"
                                                variant="outlined"
                                                label="Port"
                                                value={this.state.port}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={6} sm={6}>
                                            <FormControl fullWidth>
                                                <InputLabel id="locale-label">Locale</InputLabel>
                                                <Select
                                                    labelId="locale-label"
                                                    id="locale"
                                                    name="locale"
                                                    value={this.state.locale}
                                                    label="Locale"
                                                    onChange={ (e: SelectChangeEvent) => this.handleChange(e) }
                                                >
                                                    <MenuItem value="en">English</MenuItem>
                                                    <MenuItem value="es">Spanish</MenuItem>
                                                    <MenuItem value="de">German</MenuItem>
                                                </Select>
                                            </FormControl>
                                        </Grid>
                                        <Grid item xs={6} sm={6}>
                                            <TextField
                                                id="despawnTimeMinimumMinutes"
                                                name="despawnTimeMinimumMinutes"
                                                type="number"
                                                variant="outlined"
                                                label="Despawn Time Minimum (minutes)"
                                                value={this.state.despawnTimeMinimumMinutes}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={6} sm={6}>
                                            <FormControlLabel id="checkForDuplicates" name="checkForDuplicates" control={<Switch checked={this.state.checkForDuplicates} />} label="Check For Duplicates" />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <MultiSelect
                                                id="discords"
                                                title="Discord Servers"
                                                allItems={this.state.discords}
                                                selectedItems={Object.values(this.state.servers ?? {})}
                                            />
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion
                                TransitionProps={{ unmountOnExit: true }}
                                expanded={this.state.expanded === 'panel2'}
                                onChange={this.handlePanelExpanded('panel2')}
                            >
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Databases</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2} style={{paddingTop: '20px', paddingBottom: '20px'}}>
                                        <Grid item xs={12} sm={12}>
                                            <Card>
                                                <CardHeader title="Main" subheader="Main database for saving subscriptions" />
                                                <CardContent>
                                                    <DatabaseInfo {...this.state.database.main} />
                                                </CardContent>
                                            </Card>
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <Card>
                                                <CardHeader title="Scanner" subheader="Scanner database for fetching pokestops, gyms, and weather details" />
                                                <CardContent>
                                                    <DatabaseInfo {...this.state.database.scanner} />
                                                </CardContent>
                                            </Card>
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <Card>
                                                <CardHeader title="Nests" subheader="ManualDb database for fetching nests" />
                                                <CardContent>
                                                    <DatabaseInfo {...this.state.database.nests} />
                                                </CardContent>
                                            </Card>
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel3'} onChange={this.handlePanelExpanded('panel3')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Short Url API</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2} style={{paddingTop: '20px', paddingBottom: '20px'}}>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel id="enabled" name="enabled" control={<Switch checked={this.state.shortUrlApi.enabled} />} label="Enabled" />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="shortUrlApi.apiUrl"
                                                name="shortUrlApi.apiUrl"
                                                type="text"
                                                variant="outlined"
                                                label="API Url"
                                                value={this.state.shortUrlApi.apiUrl}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="shortUrlApi.signature"
                                                name="shortUrlApi.signature"
                                                type="text"
                                                variant="outlined"
                                                label="Signature"
                                                value={this.state.shortUrlApi.signature}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel4'} onChange={this.handlePanelExpanded('panel4')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Stripe API</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2} style={{paddingTop: '20px', paddingBottom: '20px'}}>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="apiKey"
                                                name="apiKey"
                                                type="text"
                                                variant="outlined"
                                                label="API Key"
                                                value={this.state.stripeApi.apiKey}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel5'} onChange={this.handlePanelExpanded('panel5')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Urls</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2} style={{paddingTop: '20px', paddingBottom: '20px'}}>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="scannerMap"
                                                name="scannerMap"
                                                variant="outlined"
                                                label="Scanner Map"
                                                value={this.state.urls.scannerMap}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel6'} onChange={this.handlePanelExpanded('panel6')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Event Pokemon</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2} style={{paddingTop: '20px', paddingBottom: '20px'}}>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="pokemonIds"
                                                name="pokemonIds"
                                                variant="outlined"
                                                label="Pokemon IDs"
                                                value={this.state.eventPokemon.pokemonIds}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="minimumIV"
                                                name="minimumIV"
                                                type="number"
                                                variant="outlined"
                                                label="Minimum IV"
                                                value={this.state.eventPokemon.minimumIV}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="type-label">Filter Type</InputLabel>
                                                <Select
                                                    labelId="type-label"
                                                    id="type"
                                                    name="type"
                                                    value={this.state.eventPokemon.type}
                                                    label="Filter Type"
                                                    onChange={this.handleChange}
                                                >
                                                    <MenuItem value="Include">Include</MenuItem>
                                                    <MenuItem value="Exclude">Exclude</MenuItem>
                                                </Select>
                                            </FormControl>
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel7'} onChange={this.handlePanelExpanded('panel7')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Icon Styles</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2} style={{paddingTop: '20px', paddingBottom: '20px'}}>
                                        <Grid item xs={12} sm={12}>
                                            ...
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel8'} onChange={this.handlePanelExpanded('panel8')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Static Maps</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2} style={{paddingTop: '20px', paddingBottom: '20px'}}>
                                        {Object.keys(this.state.staticMaps).map((key: string) => {
                                            return (
                                                <Grid key={key} item xs={12} sm={12}>
                                                    <Card style={{paddingTop: '20px', paddingBottom: '20px'}}>
                                                        <CardHeader title={key} subheader="" />
                                                        <CardContent>
                                                            <TextField
                                                                id="url"
                                                                name="url"
                                                                variant="outlined"
                                                                label="Url"
                                                                value={this.state.staticMaps[key].url}
                                                                fullWidth
                                                                onChange={this.handleChange}
                                                            />
                                                            <TextField
                                                                id="template"
                                                                name="template"
                                                                variant="outlined"
                                                                label="Template"
                                                                value={this.state.staticMaps[key].template}
                                                                fullWidth
                                                                onChange={this.handleChange}
                                                            />
                                                            <FormControlLabel id="includePokestops" name="includePokestops" control={<Switch checked={this.state.staticMaps[key].includePokestops} />} label="Include Pokestops" />
                                                            <FormControlLabel id="includeGyms" name="includeGyms" control={<Switch checked={this.state.staticMaps[key].includeGyms} />} label="Include Gyms" />
                                                        </CardContent>
                                                    </Card>
                                                </Grid>
                                            );
                                        })}
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel9'} onChange={this.handlePanelExpanded('panel9')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Twilio</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2} style={{paddingTop: '20px', paddingBottom: '20px'}}>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel id="enabled" name="enabled" control={<Switch checked={this.state.twilio.enabled} />} label="Enabled" />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="accountSid"
                                                name="accountSid"
                                                variant="outlined"
                                                label="Account SID"
                                                value={this.state.twilio.accountSid}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="authToken"
                                                name="authToken"
                                                variant="outlined"
                                                label="Auth Token"
                                                value={this.state.twilio.authToken}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="from"
                                                name="from"
                                                variant="outlined"
                                                label="From Number"
                                                value={this.state.twilio.from}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="userIds"
                                                name="userIds"
                                                variant="outlined"
                                                label="User Ids"
                                                value={this.state.twilio.userIds}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="roleIds"
                                                name="roleIds"
                                                variant="outlined"
                                                label="Role Ids"
                                                value={this.state.twilio.roleIds}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="pokemonIds"
                                                name="pokemonIds"
                                                variant="outlined"
                                                label="Pokemon Ids"
                                                value={this.state.twilio.pokemonIds}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="minIV"
                                                name="minIV"
                                                variant="outlined"
                                                label="Minimum IV"
                                                value={this.state.twilio.minIV}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel10'} onChange={this.handlePanelExpanded('panel10')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Reverse Geocoding</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2} style={{paddingTop: '20px', paddingBottom: '20px'}}>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="provider-label">Provider</InputLabel>
                                                <Select
                                                    labelId="provider-label"
                                                    id="provider"
                                                    name="provider"
                                                    value={this.state.reverseGeocoding.provider}
                                                    label="Provider"
                                                    onChange={ (e: SelectChangeEvent) => this.handleChange(e) }
                                                >
                                                    <MenuItem value="Osm">Osm</MenuItem>
                                                    <MenuItem value="GMaps">GMaps</MenuItem>
                                                </Select>
                                            </FormControl>
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel control={<Switch checked={this.state.reverseGeocoding.cacheToDisk} />} label="Cache To Disk" />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <Card>
                                                <CardHeader title="Google Maps" subheader="" />
                                                <CardContent>
                                                    <TextField
                                                        id="key"
                                                        name="key"
                                                        variant="outlined"
                                                        label="Key"
                                                        value={this.state.reverseGeocoding.gmaps.key}
                                                        fullWidth
                                                        onChange={this.handleChange}
                                                    />
                                                    <TextField
                                                        id="schema"
                                                        name="schema"
                                                        variant="outlined"
                                                        label="Schema"
                                                        value={this.state.reverseGeocoding.gmaps.schema}
                                                        fullWidth
                                                        onChange={this.handleChange}
                                                    />
                                                </CardContent>
                                            </Card>
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <Card>
                                                <CardHeader title="Nominatim" subheader="" />
                                                <CardContent>
                                                    <TextField
                                                        id="url"
                                                        name="url"
                                                        variant="outlined"
                                                        label="Url"
                                                        value={this.state.reverseGeocoding.nominatim.url}
                                                        fullWidth
                                                        onChange={this.handleChange}
                                                    />
                                                    <TextField
                                                        id="schema"
                                                        name="schema"
                                                        variant="outlined"
                                                        label="Schema"
                                                        value={this.state.reverseGeocoding.nominatim.schema}
                                                        fullWidth
                                                        onChange={this.handleChange}
                                                    />
                                                </CardContent>
                                            </Card>
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel11'} onChange={this.handlePanelExpanded('panel11')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Diagnostics</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2} style={{paddingTop: '20px', paddingBottom: '20px'}}>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel control={<Switch checked={this.state.debug} />} label="Enable Webhook Debug" />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="logLevel-label">Log Level</InputLabel>
                                                <Select
                                                    labelId="logLevel-label"
                                                    id="logLevel"
                                                    name="logLevel"
                                                    value={this.state.logLevel}
                                                    label="Log Level"
                                                    onChange={ (e: SelectChangeEvent) => this.handleChange(e) }
                                                >
                                                    <MenuItem value={0}>Trace</MenuItem>
                                                    <MenuItem value={1}>Debug</MenuItem>
                                                    <MenuItem value={2}>Info</MenuItem>
                                                    <MenuItem value={3}>Warning</MenuItem>
                                                    <MenuItem value={4}>Error</MenuItem>
                                                    <MenuItem value={5}>Critical</MenuItem>
                                                    <MenuItem value={6}>None</MenuItem>
                                                </Select>
                                            </FormControl>
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
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

//export default EditConfig;
export default withRouter(EditConfig);