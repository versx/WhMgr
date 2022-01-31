import React, { useState } from 'react'
import {
    Box,
    Button,
    Card,
    CardContent,
    CardHeader,
    Container,
    FormControl,
    FormControlLabel,
    Grid,
    List,
    Select,
    SelectChangeEvent,
    Switch,
    TextField,
    Typography,
} from '@mui/material';
import { makeStyles } from '@mui/styles';

import { Path, set, lensPath } from 'ramda';

import config from '../../config.json';
import { Alarm, AlarmProps } from '../../components/Alarm';
import { AddAlarmModal } from '../../components/AddAlarmModal';
import { BreadCrumbs } from '../../components/BreadCrumbs';
import withRouter from '../../hooks/WithRouter';
import { IGlobalProps } from '../../interfaces/IGlobalProps';

class EditAlarm extends React.Component<IGlobalProps> {
    public state: any;

    constructor(props: IGlobalProps) {
        super(props);
        console.log('props:', props);
        this.state = {
            // TODO: Set default state values
            name: props.params!.id,
            enablePokemon: false,
            enableRaids: false,
            enableQuests: false,
            enablePokestops: false,
            enableGyms: false,
            enableWeather: false,
            alarms: [],
            allEmbeds: [],
            allFilters: [],
            allGeofences: [],
            open: false,
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
        fetch(config.apiUrl + 'admin/alarm/' + id, {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*',
            },
        })
        .then(async (response) => await response.json())
        .then(data => {
            console.log('alarm data:', data);
            //this.setState(data.data.alarm);
            const keys: string[] = Object.keys(data.data.alarm);
            for (const key of keys) {
                //console.log('key:', key, 'data:', data.data.alarm[key]);
                if (data.data.alarm[key]) {
                    this.setState({ [key]: data.data.alarm[key] });
                }
            }
            this.setState({ ['allEmbeds']: data.data.embeds });
            this.setState({ ['allFilters']: data.data.filters });
            this.setState({ ['allGeofences']: data.data.geofences });
            //console.log('state:', this.state);
        }).catch(err => {
            console.error('error:', err);
            // TODO: Show error notification
        });
    }

    handleChange(event: any) {
        const { name, value } = event.target;
        //console.log('event:', event);
        //this.setState({ [name]: value });
        this.setState(state => ({ ...state, [name]: value }));
        //this.setObjectByPath([name], value);
        console.log('state:', this.state);
    }

    handlePanelExpanded = (panel: string) => (event: React.SyntheticEvent, isExpanded: boolean) => {
        this.setState({ ['expanded']: isExpanded ? panel : false });
    }

    handleSubmit(event: React.FormEvent<HTMLFormElement>) {
        event.preventDefault();

        console.log('handle submit state:', this.state);

        const id = this.props.params!.id;
        fetch(config.apiUrl + 'admin/alarm/' + id, {
            method: 'POST',
            body: JSON.stringify(this.state),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
            },
        }).then(async (response) => await response.json())
          .then((data: any) => {
            console.log('response:', data);
            // TODO: Show success/error notification
        }).catch((err) => {
            console.error('error:', err);
            event.preventDefault();
            // TODO: Show error notification
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

        const breadcrumbs = [{
            text: 'Dashboard',
            color: 'inherit',
            href: '/dashboard',
            selected: false,
        }, {
            text: 'Alarms',
            color: 'inherit',
            href: '/dashboard/alarms',
            selected: false,
        }, {
            text: 'Edit Alarm ' + this.props.params!.id,
            color: 'primary',
            href: '',
            selected: true,
        }];

        const toggleModal = () => {
            this.setState({ ['open']: !this.state.open });
            if (!this.state.open) {
                console.log('this.state:', this.state);
            }
        };

        const onModalSubmit = (data: any) => {
            //console.log('modal submit data:', data);
            // Add alarm to list/update state
            const alarms = this.state.alarms;
            alarms.push(data);
            this.setState({ ['alarms']: alarms });
        };

        const handleCheckbox = (event: any) => {
            this.setState({ [event.target.name]: event.target.checked });
        };

        return (
            <div className={classes.container} style={{ paddingTop: '50px', paddingBottom: '20px' }}>
                <Container>
                    <Box component="form" method="POST" action="" onSubmit={this.handleSubmit} sx={{ mt: 3 }}>
                        <BreadCrumbs crumbs={breadcrumbs} />
                        <Typography variant="h5" component="h2" >
                            Edit Alarm {this.props.params!.id}
                        </Typography>
                        <Typography sx={{ mt: 2 }}>
                            Channel alarms config description goes here
                        </Typography>
                        <div style={{paddingBottom: '10px', paddingTop: '20px'}}>
                            <Card>
                                <CardHeader title="General" />
                                <CardContent>
                                    <Grid container spacing={2}>
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
                                        <Grid item xs={12} sm={6}>
                                            <FormControlLabel
                                                id="enablePokemon"
                                                name="enablePokemon"
                                                control={<Switch checked={this.state.enablePokemon} onChange={handleCheckbox} />}
                                                label="Enable Pokemon"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <FormControlLabel
                                                id="enableRaids"
                                                name="enableRaids"
                                                control={<Switch checked={this.state.enableRaids} onChange={handleCheckbox} />}
                                                label="Enable Raids"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <FormControlLabel
                                                id="enableQuests"
                                                name="enableQuests"
                                                control={<Switch checked={this.state.enableQuests} onChange={handleCheckbox} />}
                                                label="Enable Quests" />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <FormControlLabel
                                                id="enablePokestops"
                                                name="enablePokestops"
                                                control={<Switch checked={this.state.enablePokestops} onChange={handleCheckbox} />}
                                                label="Enable Pokestops"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <FormControlLabel
                                                id="enableGyms"
                                                name="enableGyms"
                                                control={<Switch checked={this.state.enableGyms} onChange={handleCheckbox} />}
                                                label="Enable Gyms"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <FormControlLabel
                                                id="enableWeather"
                                                name="enableWeather"
                                                control={<Switch checked={this.state.enableWeather} onChange={handleCheckbox} />}
                                                label="Enable Weather"
                                            />
                                        </Grid>
                                    </Grid>
                                </CardContent>
                            </Card>
                        </div>
                        <div style={{paddingBottom: '20px', paddingTop: '10px'}}>
                            <Card>
                                <CardHeader title="Channel Alarms" />
                                <CardContent>
                                    <Button variant="contained" color="success" onClick={toggleModal}>Add Alarm</Button>
                                    <List style={{paddingTop: '20px'}}>
                                        {this.state.alarms.map((alarm: any) => {
                                            const props: AlarmProps = {
                                                ...alarm,
                                                allGeofences: this.state.allGeofences,
                                                allFilters: this.state.allFilters,
                                                allEmbeds: this.state.allEmbeds,
                                            };
                                            const handleDelete = (name: string) => {
                                                const alarms = this.state.alarms;
                                                const newAlarms = alarms.filter((item: any) => item.name !== name);
                                                this.setState({ ['alarms']: newAlarms })
                                            };
                                            return (
                                                <div key={alarm.name} style={{paddingBottom: '20px'}}>
                                                    <Alarm {...props} />
                                                    <Button
                                                        variant="contained"
                                                        color="error"
                                                        onClick={() => handleDelete(alarm.name)}
                                                    >
                                                        Remove
                                                    </Button>
                                                </div>
                                            );
                                        })}
                                    </List>
                                </CardContent>
                            </Card>
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
                <AddAlarmModal key="addAlarmModal" {...{
                    geofences: this.state.allGeofences,
                    embeds: this.state.allEmbeds,
                    filters: this.state.allFilters,
                    open: this.state.open,
                    toggle: toggleModal,
                    onSubmit: onModalSubmit,
                }} />
            </div>
        );
    }
}

export default withRouter(EditAlarm);