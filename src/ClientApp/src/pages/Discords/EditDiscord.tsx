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

import config from '../../config.json';
import withRouter from '../../hooks/WithRouter';
import { IGlobalProps } from '../../interfaces/IGlobalProps';

class EditDiscord extends React.Component<IGlobalProps> {
    public state: any;

    constructor(props: IGlobalProps) {
        super(props);
        console.log('props:', props);
        this.state = {
            // TODO: Set default state values
            name: props.params!.id,
            geofences: [],
            donorRoles: [],
            moderatorRoles: [],
            freeRoleName: '',
            alarms: '',
            allAlarms: [],
            allGeofences: [],
            allIconStyles: [],
            iconStyle: '',
            bot: {
                commandPrefix: '.',
                guildId: '',
                emojiGuildId: '',
                token: '',
                channelIds: [],
                status: '',
                ownerId: '',
            },
            subscriptions: {
                enabled: false,
                maxPokemonSubscriptions: 0,
                maxPvpSubscriptions: 0,
                maxRaidSubscriptions: 0,
                maxQuestSubscriptions: 0,
                maxLureSubscriptions: 0,
                maxInvasionSubscriptions: 0,
                maxGymSubscriptions: 0,
                maxNotificationsPerMinute: 10,
                url: 'https://ui.example.com',
                embedsFile: 'default.json',
            },
            geofenceRoles: {
                enabled: false,
                autoRemove: true,
                requiresDonorRole: true,
            },
            questPurge: {
                enabled: false,
            },
            nests: {
                enabled: false,
                channelId: '',
                minNestsPerHour: 1,
            },
            dailyStats: {
                iv: {
                    enabled: false,
                    clearMessages: false,
                    channelId: '',
                },
                shiny: {
                    enabled: false,
                    clearMessages: false,
                    channelId: '',
                },
            },
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
        fetch(config.apiUrl + 'admin/discord/' + id, {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*',
            },
        })
        .then(async (response) => await response.json())
        .then(data => {
            console.log('discord data:', data);
            //this.setState(data.data.discord);
            const keys: string[] = Object.keys(data.data.discord);
            for (const key of keys) {
                //console.log('key:', key, 'data:', data.data.discord[key]);
                this.setState({ [key]: data.data.discord[key] });
            }
            this.setState({ ['allAlarms']: data.data.allAlarms });
            this.setState({ ['allGeofences']: data.data.allGeofences });
        }).catch(err => {
            console.error('error:', err);
            // TODO: Show error notification
        });
    }

    handleChange(event: any) {
        const { name, value } = event.target;
        console.log('event:', event);
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
        fetch(config.apiUrl + 'admin/discord/' + id, {
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
                            Edit Discord Server {this.props.params!.id}
                        </Typography>
                        <Typography sx={{ mt: 2 }}>
                            Discord server config description goes here
                        </Typography>
                        <div style={{paddingBottom: '20px', paddingTop: '20px'}}>
                            <Accordion expanded={this.state.expanded === 'panel1'} onChange={this.handlePanelExpanded('panel1')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>General</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
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
                                        <Grid item xs={6} sm={6}>
                                            <FormControl fullWidth>
                                                <InputLabel id="donorRoles-label">Donor Roles</InputLabel>
                                                <Select
                                                    labelId="donorRoles-label"
                                                    id="donorRoles"
                                                    name="donorRoles"
                                                    value={this.state.donorRoles}
                                                    multiple
                                                    label="Donor Roles"
                                                    onChange={ (e: SelectChangeEvent) => this.handleChange(e) }
                                                >
                                                    <MenuItem value="en">English</MenuItem>
                                                    <MenuItem value="es">Spanish</MenuItem>
                                                    <MenuItem value="de">German</MenuItem>
                                                </Select>
                                            </FormControl>
                                        </Grid>
                                        <Grid item xs={6} sm={6}>
                                            <FormControl fullWidth>
                                                <InputLabel id="moderatorRoles-label">Moderator Roles</InputLabel>
                                                <Select
                                                    labelId="moderatorRoles-label"
                                                    id="moderatorRoles"
                                                    name="moderatorRoles"
                                                    value={this.state.moderatorRoles}
                                                    multiple
                                                    label="Moderator Roles"
                                                    onChange={ (e: SelectChangeEvent) => this.handleChange(e) }
                                                >
                                                    <MenuItem value="en">English</MenuItem>
                                                    <MenuItem value="es">Spanish</MenuItem>
                                                    <MenuItem value="de">German</MenuItem>
                                                </Select>
                                            </FormControl>
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="freeRoleName"
                                                name="freeRoleName"
                                                variant="outlined"
                                                label="Free Role Name"
                                                value={this.state.freeRoleName}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="alarms-label">Alarms</InputLabel>
                                                <Select
                                                    labelId="alarms-label"
                                                    id="alarms"
                                                    name="alarms"
                                                    value={this.state.alarms}
                                                    label="Alarms"
                                                    onChange={ (e: SelectChangeEvent) => this.handleChange(e) }
                                                >
                                                    {this.state.allAlarms.map((alarm: string) => {
                                                        return (
                                                            <MenuItem value={alarm}>{alarm}</MenuItem>
                                                        );
                                                    })}
                                                </Select>
                                            </FormControl>
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="geofences-label">Geofences</InputLabel>
                                                <Select
                                                    labelId="geofences-label"
                                                    id="geofences"
                                                    name="geofences"
                                                    value={this.state.geofences}
                                                    multiple
                                                    label="Geofences"
                                                    onChange={ (e: SelectChangeEvent) => this.handleChange(e) }
                                                >
                                                    {this.state.allGeofences.map((geofence: string) => {
                                                        return (
                                                            <MenuItem value={geofence}>{geofence}</MenuItem>
                                                        );
                                                    })}
                                                </Select>
                                            </FormControl>
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="iconStyle-label">Icon Style</InputLabel>
                                                <Select
                                                    labelId="iconStyle-label"
                                                    id="iconStyle"
                                                    name="iconStyle"
                                                    value={this.state.iconStyle}
                                                    label="Icon Style"
                                                    onChange={ (e: SelectChangeEvent) => this.handleChange(e) }
                                                >
                                                    <MenuItem value="Default">Default</MenuItem>
                                                    <MenuItem value="es">Spanish</MenuItem>
                                                    <MenuItem value="de">German</MenuItem>
                                                </Select>
                                            </FormControl>
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel2'} onChange={this.handlePanelExpanded('panel2')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Bot</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="commandPrefix"
                                                name="commandPrefix"
                                                type="text"
                                                variant="outlined"
                                                label="Command Prefix"
                                                value={this.state.bot.commandPrefix}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="guildId"
                                                name="guildId"
                                                type="number"
                                                variant="outlined"
                                                label="Guild Id"
                                                value={this.state.bot.guildId}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="emojiGuildId"
                                                name="emojiGuildId"
                                                type="number"
                                                variant="outlined"
                                                label="Emoji Guild Id"
                                                value={this.state.bot.emojiGuildId}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="token"
                                                name="token"
                                                type="text"
                                                variant="outlined"
                                                label="Token"
                                                value={this.state.bot.token}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="channelIds"
                                                name="channelIds"
                                                type="text"
                                                variant="outlined"
                                                label="Channel Ids"
                                                value={this.state.bot.channelIds}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="status"
                                                name="status"
                                                type="text"
                                                variant="outlined"
                                                label="Status"
                                                value={this.state.bot.status}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="ownerId"
                                                name="ownerId"
                                                type="number"
                                                variant="outlined"
                                                label="Owner Id"
                                                value={this.state.bot.ownerId}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel3'} onChange={this.handlePanelExpanded('panel3')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Subscriptions</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel id="enabled" name="enabled" control={<Switch checked={this.state.subscriptions.enabled} />} label="Enabled" />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="maxPokemonSubscriptions"
                                                name="maxPokemonSubscriptions"
                                                variant="outlined"
                                                label="Max Pokemon Subscriptions"
                                                type="number"
                                                value={this.state.subscriptions.maxPokemonSubscriptions}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="maxPvpSubscriptions"
                                                name="maxPvpSubscriptions"
                                                variant="outlined"
                                                label="Max PvP Subscriptions"
                                                type="number"
                                                value={this.state.subscriptions.maxPvpSubscriptions}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="maxRaidSubscriptions"
                                                name="maxRaidSubscriptions"
                                                variant="outlined"
                                                label="Max Raid Subscriptions"
                                                type="number"
                                                value={this.state.subscriptions.maxRaidSubscriptions}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="maxGymSubscriptions"
                                                name="maxGymSubscriptions"
                                                variant="outlined"
                                                label="Max Gym Subscriptions"
                                                type="number"
                                                value={this.state.subscriptions.maxGymSubscriptions}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="maxLureSubscriptions"
                                                name="maxLureSubscriptions"
                                                variant="outlined"
                                                label="Max Lure Subscriptions"
                                                type="number"
                                                value={this.state.subscriptions.maxLureSubscriptions}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="maxInvasionSubscriptions"
                                                name="maxInvasionSubscriptions"
                                                variant="outlined"
                                                label="Max Invasion Subscriptions"
                                                type="number"
                                                value={this.state.subscriptions.maxInvasionSubscriptions}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="maxQuestSubscriptions"
                                                name="maxQuestSubscriptions"
                                                variant="outlined"
                                                label="Max Quest Subscriptions"
                                                type="number"
                                                value={this.state.subscriptions.maxQuestSubscriptions}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="maxNotificationsPerMinute"
                                                name="maxNotificationsPerMinute"
                                                variant="outlined"
                                                label="Max Notifications Per Minute"
                                                type="number"
                                                value={this.state.subscriptions.maxNotificationsPerMinute}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="url"
                                                name="url"
                                                variant="outlined"
                                                label="Subscription UI Url"
                                                type="text"
                                                value={this.state.subscriptions.url}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel4'} onChange={this.handlePanelExpanded('panel4')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Geofence Roles</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel id="enabled" name="enabled" control={<Switch checked={this.state.geofenceRoles.enabled} />} label="Enabled" />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel id="autoRemove" name="autoRemove" control={<Switch checked={this.state.geofenceRoles.autoRemove} />} label="Automatically Remove" />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel id="requiresDonorRole" name="requiresDonorRole" control={<Switch checked={this.state.geofenceRoles.enabled} />} label="Requires Donor Role" />
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel5'} onChange={this.handlePanelExpanded('panel5')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Quest Purge</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel id="enabled" name="enabled" control={<Switch checked={this.state.questPurge.enabled} />} label="Enabled" />
                                        </Grid>
                                        TODO: Channel IDs
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel6'} onChange={this.handlePanelExpanded('panel6')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Nests</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel id="enabled" name="enabled" control={<Switch checked={this.state.nests.enabled} />} label="Enabled" />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="channelId"
                                                name="channelId"
                                                variant="outlined"
                                                label="Channel Id"
                                                type="number"
                                                value={this.state.nests.channelId}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="minNestsPerHour"
                                                name="minNestsPerHour"
                                                variant="outlined"
                                                label="Minimum Nests Per Hour"
                                                type="number"
                                                value={this.state.nests.minNestsPerHour}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel7'} onChange={this.handlePanelExpanded('panel7')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Daily Statistics</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            <Card>
                                                <CardHeader title="IV Stats" subheader="" />
                                                <CardContent>
                                                    <Grid container spacing={2}>
                                                        <Grid item xs={12} sm={6}>
                                                            <FormControlLabel id="enabled" name="enabled" control={<Switch checked={this.state.dailyStats.iv.enabled} />} label="Enabled" />
                                                        </Grid>
                                                        <Grid item xs={12} sm={6}>
                                                            <FormControlLabel id="clearMessages" name="clearMessages" control={<Switch checked={this.state.dailyStats.iv.clearMessages} />} label="Clear Messages" />
                                                        </Grid>
                                                        <Grid item xs={12} sm={12}>
                                                            <TextField
                                                                id="channelId"
                                                                name="channelId"
                                                                variant="outlined"
                                                                label="Channel Id"
                                                                type="number"
                                                                value={this.state.dailyStats.iv.channelId}
                                                                fullWidth
                                                                onChange={this.handleChange}
                                                            />
                                                        </Grid>
                                                    </Grid>
                                                </CardContent>
                                            </Card>
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <Card>
                                                <CardHeader title="Shiny Stats" subheader="" />
                                                <CardContent>
                                                    <Grid container spacing={2}>
                                                        <Grid item xs={12} sm={6}>
                                                            <FormControlLabel id="enabled" name="enabled" control={<Switch checked={this.state.dailyStats.shiny.enabled} />} label="Enabled" />
                                                        </Grid>
                                                        <Grid item xs={12} sm={6}>
                                                            <FormControlLabel id="clearMessages" name="clearMessages" control={<Switch checked={this.state.dailyStats.shiny.clearMessages} />} label="Clear Messages" />
                                                        </Grid>
                                                        <Grid item xs={12} sm={12}>
                                                            <TextField
                                                                id="channelId"
                                                                name="channelId"
                                                                variant="outlined"
                                                                label="Channel Id"
                                                                type="number"
                                                                value={this.state.dailyStats.iv.channelId}
                                                                fullWidth
                                                                onChange={this.handleChange}
                                                            />
                                                        </Grid>
                                                    </Grid>
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

export default withRouter(EditDiscord);