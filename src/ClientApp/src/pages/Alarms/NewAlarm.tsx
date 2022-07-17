import React from 'react'
import {
    Box,
    Button,
    Card,
    CardContent,
    CardHeader,
    Container,
    FormControlLabel,
    Grid,
    List,
    Switch,
    TextField,
    Typography,
} from '@mui/material';
import { makeStyles } from '@mui/styles';

import config from '../../config.json';
import { AddAlarmModal } from '../../components/Modals';
import { Alarm, AlarmProps } from '../../components/Alarm';
import { BreadCrumbs } from '../../components/BreadCrumbs';
import { withRouter } from '../../hooks';
import { IGlobalProps } from '../../interfaces/IGlobalProps';

class NewAlarm extends React.Component<IGlobalProps> {
    public state: any;

    constructor(props: IGlobalProps) {
        super(props);
        console.log('props:', props);
        this.state = {
            name: '',
            enablePokemon: false,
            enableRaids: false,
            enableQuests: false,
            enablePokestops: false,
            enableInvasions: false,
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
    }

    componentDidMount() {
        console.log('componentDidMount:', this.state, this.props);
        this.fetchData();
    }

    fetchData() {
        fetch(config.apiUrl + 'admin/alarm/data', {
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
            this.setState({
                ['allEmbeds']: data.data.embeds,
                ['allFilters']: data.data.filters,
                ['allGeofences']: data.data.geofences,
            });
            //console.log('state:', this.state);
        }).catch(err => {
            console.error('error:', err);
            // TODO: Show error notification
        });
    }

    handleChange(event: any) {
        const { name, value } = event.target;
        this.setState({ [name]: value });
        console.log('state:', this.state);
    }

    handlePanelExpanded = (panel: string) => (event: React.SyntheticEvent, isExpanded: boolean) => {
        this.setState({ ['expanded']: isExpanded ? panel : false });
    }

    handleSubmit(event: React.FormEvent<HTMLFormElement>) {
        event.preventDefault();

        console.log('handle submit state:', this.state);

        const id = this.props.params!.id;
        const data = {
            name: this.state.name,
            alarm: {
                enablePokemon: this.state.enablePokemon,
                enableRaids: this.state.enableRaids,
                enableQuests: this.state.enableQuests,
                enablePokestops: this.state.enablePokestops,
                enableInvasions: this.state.enableInvasions,
                enableGyms: this.state.enableGyms,
                enableWeather: this.state.enableWeather,
                alarms: this.state.alarms,
            },
        };
        console.log('alarm submit:', id, data);
        fetch(config.apiUrl + 'admin/alarm/new', {
            method: 'POST',
            body: JSON.stringify(data),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
            },
        }).then(async (response) => await response.json())
          .then((data: any) => {
            console.log('response:', data);
            if (data.status !== 'OK') {
                alert(data.error);
                return;
            }
            window.location.href = config.homepage + 'alarms';
            // TODO: Show success/error notification
        }).catch((err) => {
            console.error('error:', err);
            event.preventDefault();
            // TODO: Show error notification
        });
    }

    render() {
        const handleCancel = () => window.location.href = config.homepage + 'alarms';

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
            text: 'Alarms',
            color: 'inherit',
            href: config.homepage + 'alarms',
            selected: false,
        }, {
            text: 'Edit ' + this.props.params!.id,
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
            <div className={classes.container} style={{paddingTop: '50px', paddingBottom: '20px', paddingLeft: '20px', paddingRight: '20px'}}>
                <Box component="form" method="POST" action="" onSubmit={this.handleSubmit} sx={{ mt: 3 }}>
                    <BreadCrumbs crumbs={breadcrumbs} />
                    <Typography variant="h5" component="h2" >
                        New Alarm
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
                                            required
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
                                            id="enableInvasions"
                                            name="enableInvasions"
                                            control={<Switch checked={this.state.enableInvasions} onChange={handleCheckbox} />}
                                            label="Enable Invasions"
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
                                <List style={{paddingTop: '20px', maxHeight: 800, overflow: 'auto'}}>
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
                                            this.setState({ ['alarms']: newAlarms });
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

export default withRouter(NewAlarm);