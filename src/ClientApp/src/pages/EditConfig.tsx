import React, { useEffect, useState } from 'react'
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
import { makeStyles } from '@mui/styles';

import config from '../config.json';
import { MultiSelect } from '../components/MultiSelect';
import withRouter from '../hooks/WithRouter';
import { IGlobalProps } from '../interfaces/IGlobalProps';

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
        };
        this.handleChange = this.handleChange.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
        this.handlePanelExpanded = this.handlePanelExpanded.bind(this);
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
            console.log('config data:', data);
            this.setState(data.data.config);
            const keys: string[] = Object.keys(data.data.config);
            for (const key of keys) {
                console.log('key:', key, 'data:', data.data.config[key]);
                this.setState({ [key]: data.data.config[key] });
            }
            this.setState({ ['discords']: Object.values(data.data.discords) });
        }).catch(err => {
            console.error('error:', err);
            // TODO: Show error notification
        });
    }

    handleChange(event: any) {
        //console.log('event:', event.target);
        this.setState({ [event.target.name]: event.target.value });
    }

    handlePanelExpanded = (panel: string) => (event: React.SyntheticEvent, isExpanded: boolean) => {
        this.setState({ ['expanded']: isExpanded ? panel : false });
    }

    handleValidation() {

    }

    handleSubmit(event: React.FormEvent<HTMLFormElement>) {

    }

    render() {
        const handleCancel = () => window.location.href = '/pokemon';

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
                    <Box component="form" method="GET" action=""  onSubmit={this.handleSubmit} sx={{ mt: 3 }}>
                        <Typography id="modal-modal-title" variant="h5" component="h2" >
                            Edit Config {this.props.params!.id}
                        </Typography>
                        <Typography id="modal-modal-description" sx={{ mt: 2 }}>
                            Config description goes here
                        </Typography>
                        <div style={{paddingBottom: '20px'}}>
                            <Accordion expanded={this.state.expanded === 'panel1'} onChange={this.handlePanelExpanded('panel1')}>
                                <AccordionSummary>
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
                                            <FormControlLabel control={<Switch defaultChecked />} label="Check For Duplicates" />
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
                            <Accordion expanded={this.state.expanded === 'panel2'} onChange={this.handlePanelExpanded('panel2')}>
                                <AccordionSummary>
                                    <Typography>Databases</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2} style={{paddingTop: '20px', paddingBottom: '20px'}}>
                                        <Grid item xs={12} sm={12}>
                                            <Card>
                                                <CardHeader title="Main" subheader="Main database for saving subscriptions" />
                                                <CardContent>
                                                    Test
                                                </CardContent>
                                            </Card>
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
                                Create
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