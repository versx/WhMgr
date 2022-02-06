import React, { useState } from 'react'
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
import { BreadCrumbs } from '../../components/BreadCrumbs';
import { MultiSelect } from '../../components/MultiSelect';
import withRouter from '../../hooks/WithRouter';
import { IGlobalProps } from '../../interfaces/IGlobalProps';
import { onNestedStateChange } from '../../utils/nestedStateHelper';

class EditDiscord extends React.Component<IGlobalProps> {
    public state: any;

    constructor(props: IGlobalProps) {
        super(props);
        console.log('props:', props);
        this.state = {
            // TODO: Set default state values
            allAlarms: [],
            allGeofences: [],
            allEmbeds: [],
            allRoles: [],
            allIconStyles: [],

            name: props.params!.id,
            geofences: [],
            donorRoleIds: [],
            moderatorRoleIds: [],
            freeRoleName: '',
            alarms: '',
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
                maxPvPSubscriptions: 0,
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
            questsPurge: {
                enabled: false,
                channelIds: {},
            },
            nests: {
                enabled: false,
                channelId: '',
                minimumPerHour: 1,
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
        this.onInputChange = this.onInputChange.bind(this);
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
            //console.log('discord data:', data);
            //this.setState(data.data.discord);

            const keys: string[] = Object.keys(data.data.discord);
            for (const key of keys) {
                //console.log('KEY:', key, data.data.discord[key]);
                this.setState({ [key]: data.data.discord[key] });
            }

            this.setState({
                ['allAlarms']: data.data.allAlarms,
                ['allEmbeds']: data.data.allEmbeds,
                ['allGeofences']: data.data.allGeofences,
                ['allRoles']: data.data.allRoles,
                ['allIconStyles']: data.data.allIconStyles,
            });
            //console.log('discord state:', this.state);
        }).catch(err => {
            console.error('error:', err);
            // TODO: Show error notification
        });
    }

    onInputChange(event: any) {
        onNestedStateChange(event, this);
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
            // TODO: Show notification/redirect to /dashboard/discords

        }).catch((err) => {
            console.error('error:', err);
            event.preventDefault();
        });
    }

    render() {
        const handleCancel = () => window.location.href = config.homepage + 'discords';
        const handleDonorRoleChange = (event: any) => {
            const { name, value } = event.target;
            const roleId = value[0];
            const role = this.state.allRoles.filter((x: any) => x.id == roleId);
            if (!role) {
                console.error('Failed to get role from id:', roleId);
            }
            const permissions = role[0].permissions;
            console.log('donor role change target:', this.state, name, roleId, permissions);
            this.setState({
                [name]: {
                    [roleId]: permissions,
                },
            });
            console.log('donor role state:', this.state);
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
            text: 'Discords',
            color: 'inherit',
            href: config.homepage + 'discords',
            selected: false,
        }, {
            text: 'Edit ' + this.props.params!.id,
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
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={6} sm={6}>
                                            <FormControl fullWidth>
                                                <InputLabel id="donorRoleIds-label">Donor Roles</InputLabel>
                                                <Select
                                                    labelId="donorRoleIds-label"
                                                    id="donorRoleIds"
                                                    name="donorRoleIds"
                                                    value={Object.keys(this.state.donorRoleIds)}
                                                    multiple
                                                    label="Donor Roles"
                                                    onChange={handleDonorRoleChange}
                                                >
                                                    {this.state.allRoles && this.state.allRoles.map((role: any) => {
                                                        if (!role.isModerator) {
                                                            return (
                                                                <MenuItem key={role.id} value={role.id}>{role.name} ({role.permissions.join(', ')})</MenuItem>
                                                            );
                                                        }
                                                    })}
                                                </Select>
                                            </FormControl>
                                        </Grid>
                                        <Grid item xs={6} sm={6}>
                                            <FormControl fullWidth>
                                                <InputLabel id="moderatorRoleIds-label">Moderator Roles</InputLabel>
                                                <Select
                                                    labelId="moderatorRoleIds-label"
                                                    id="moderatorRoleIds"
                                                    name="moderatorRoleIds"
                                                    value={this.state.moderatorRoleIds}
                                                    multiple
                                                    label="Moderator Roles"
                                                    onChange={this.onInputChange}
                                                >
                                                    {this.state.allRoles && this.state.allRoles.map((role: any) => {
                                                        if (role.isModerator) {
                                                            return (
                                                                <MenuItem key={role.id} value={role.id}>{role.name} ({role.permissions.join(', ')})</MenuItem>
                                                            );
                                                        }
                                                    })}
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
                                                onChange={this.onInputChange}
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
                                                    onChange={this.onInputChange}
                                                >
                                                    {this.state.allAlarms.map((alarm: string) => {
                                                        return (
                                                            <MenuItem key={alarm} value={alarm}>{alarm}</MenuItem>
                                                        );
                                                    })}
                                                </Select>
                                            </FormControl>
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <MultiSelect
                                                id="geofences"
                                                title="Geofences"
                                                allItems={this.state.allGeofences}
                                                selectedItems={this.state.geofences}
                                            />
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
                                                    onChange={this.onInputChange}
                                                >
                                                    {this.state.allIconStyles.map((style: string) => {
                                                        return (
                                                            <MenuItem key={style} value={style}>{style}</MenuItem>
                                                        );
                                                    })}
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
                                                id="bot.commandPrefix"
                                                name="bot.commandPrefix"
                                                type="text"
                                                variant="outlined"
                                                label="Command Prefix"
                                                value={this.state.bot.commandPrefix}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="bot.guildId"
                                                name="bot.guildId"
                                                type="number"
                                                variant="outlined"
                                                label="Guild Id"
                                                value={this.state.bot.guildId}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="bot.emojiGuildId"
                                                name="bot.emojiGuildId"
                                                type="number"
                                                variant="outlined"
                                                label="Emoji Guild Id"
                                                value={this.state.bot.emojiGuildId}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="bot.token"
                                                name="bot.token"
                                                type="text"
                                                variant="outlined"
                                                label="Token"
                                                value={this.state.bot.token}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="bot.channelIds"
                                                name="bot.channelIds"
                                                type="text"
                                                variant="outlined"
                                                label="Channel Ids"
                                                value={this.state.bot.channelIds}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="bot.status"
                                                name="bot.status"
                                                type="text"
                                                variant="outlined"
                                                label="Status"
                                                value={this.state.bot.status}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="bot.ownerId"
                                                name="bot.ownerId"
                                                type="number"
                                                variant="outlined"
                                                label="Owner Id"
                                                value={this.state.bot.ownerId}
                                                fullWidth
                                                onChange={this.onInputChange}
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
                                            <FormControlLabel
                                                id="subscriptions.enabled"
                                                name="subscriptions.enabled"
                                                control={<Switch checked={this.state.subscriptions.enabled} onChange={this.onInputChange} />}
                                                label="Enabled"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="subscriptions.maxPokemonSubscriptions"
                                                name="subscriptions.maxPokemonSubscriptions"
                                                variant="outlined"
                                                label="Max Pokemon Subscriptions"
                                                type="number"
                                                value={this.state.subscriptions.maxPokemonSubscriptions}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="subscriptions.maxPvPSubscriptions"
                                                name="subscriptions.maxPvPSubscriptions"
                                                variant="outlined"
                                                label="Max PvP Subscriptions"
                                                type="number"
                                                value={this.state.subscriptions.maxPvPSubscriptions}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="subscriptions.maxRaidSubscriptions"
                                                name="subscriptions.maxRaidSubscriptions"
                                                variant="outlined"
                                                label="Max Raid Subscriptions"
                                                type="number"
                                                value={this.state.subscriptions.maxRaidSubscriptions}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="subscriptions.maxGymSubscriptions"
                                                name="subscriptions.maxGymSubscriptions"
                                                variant="outlined"
                                                label="Max Gym Subscriptions"
                                                type="number"
                                                value={this.state.subscriptions.maxGymSubscriptions}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="subscriptions.maxLureSubscriptions"
                                                name="subscriptions.maxLureSubscriptions"
                                                variant="outlined"
                                                label="Max Lure Subscriptions"
                                                type="number"
                                                value={this.state.subscriptions.maxLureSubscriptions}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="subscriptions.maxInvasionSubscriptions"
                                                name="subscriptions.maxInvasionSubscriptions"
                                                variant="outlined"
                                                label="Max Invasion Subscriptions"
                                                type="number"
                                                value={this.state.subscriptions.maxInvasionSubscriptions}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="subscriptions.maxQuestSubscriptions"
                                                name="subscriptions.maxQuestSubscriptions"
                                                variant="outlined"
                                                label="Max Quest Subscriptions"
                                                type="number"
                                                value={this.state.subscriptions.maxQuestSubscriptions}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="subscriptions.maxNotificationsPerMinute"
                                                name="subscriptions.maxNotificationsPerMinute"
                                                variant="outlined"
                                                label="Max Notifications Per Minute"
                                                type="number"
                                                value={this.state.subscriptions.maxNotificationsPerMinute}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="subscriptions.url"
                                                name="subscriptions.url"
                                                variant="outlined"
                                                label="Subscription UI Url"
                                                type="text"
                                                value={this.state.subscriptions.url}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="embeds-label">Embeds</InputLabel>
                                                <Select
                                                    labelId="embeds-label"
                                                    id="subscriptions.embedsFile"
                                                    name="subscriptions.embedsFile"
                                                    value={this.state.subscriptions.embedsFile}
                                                    label="Embeds"
                                                    onChange={this.onInputChange}
                                                >
                                                    {this.state.allEmbeds.map((embed: string) => {
                                                        return (
                                                            <MenuItem key={embed} value={embed}>{embed}</MenuItem>
                                                        );
                                                    })}
                                                </Select>
                                            </FormControl>
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
                                            <FormControlLabel
                                                id="geofenceRoles.enabled"
                                                name="geofenceRoles.enabled"
                                                control={<Switch checked={this.state.geofenceRoles.enabled} onChange={this.onInputChange} />}
                                                label="Enabled"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel
                                                id="geofenceRoles.autoRemove"
                                                name="geofenceRoles.autoRemove"
                                                control={<Switch checked={this.state.geofenceRoles.autoRemove} onChange={this.onInputChange} />}
                                                label="Automatically Remove"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel
                                                id="geofenceRoles.requiresDonorRole"
                                                name="geofenceRoles.requiresDonorRole"
                                                control={<Switch checked={this.state.geofenceRoles.requiresDonorRole} onChange={this.onInputChange} />}
                                                label="Requires Donor Role"
                                            />
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
                                            <FormControlLabel
                                                id="questsPurge.enabled"
                                                name="questsPurge.enabled"
                                                control={<Switch checked={this.state.questsPurge.enabled} onChange={this.onInputChange} />}
                                                label="Enabled"
                                            />
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
                                            <FormControlLabel
                                                id="nests.enabled"
                                                name="nests.enabled"
                                                control={<Switch checked={this.state.nests.enabled} onChange={this.onInputChange} />}
                                                label="Enabled"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="nests.channelId"
                                                name="nests.channelId"
                                                variant="outlined"
                                                label="Channel Id"
                                                type="number"
                                                value={this.state.nests.channelId}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="nests.minimumPerHour"
                                                name="nests.minimumPerHour"
                                                variant="outlined"
                                                label="Minimum Nests Per Hour"
                                                type="number"
                                                value={this.state.nests.minNestsPerHour}
                                                fullWidth
                                                onChange={this.onInputChange}
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
                                                            <FormControlLabel
                                                                id="dailyStats.iv.enabled"
                                                                name="dailyStats.iv.enabled"
                                                                control={<Switch checked={this.state.dailyStats.iv.enabled} onChange={this.onInputChange} />}
                                                                label="Enabled"
                                                            />
                                                        </Grid>
                                                        <Grid item xs={12} sm={6}>
                                                            <FormControlLabel
                                                                id="dailyStats.iv.clearMessages"
                                                                name="dailyStats.iv.clearMessages"
                                                                control={<Switch checked={this.state.dailyStats.iv.clearMessages} onChange={this.onInputChange} />}
                                                                label="Clear Messages"
                                                            />
                                                        </Grid>
                                                        <Grid item xs={12} sm={12}>
                                                            <TextField
                                                                id="dailyStats.iv.channelId"
                                                                name="dailyStats.iv.channelId"
                                                                variant="outlined"
                                                                label="Channel Id"
                                                                type="number"
                                                                value={this.state.dailyStats.iv.channelId}
                                                                fullWidth
                                                                onChange={this.onInputChange}
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
                                                            <FormControlLabel
                                                                id="dailyStats.shiny.enabled"
                                                                name="dailyStats.shiny.enabled"
                                                                control={<Switch checked={this.state.dailyStats.shiny.enabled} onChange={this.onInputChange} />}
                                                                label="Enabled"
                                                            />
                                                        </Grid>
                                                        <Grid item xs={12} sm={6}>
                                                            <FormControlLabel
                                                                id="dailyStats.shiny.clearMessages"
                                                                name="dailyStats.shiny.clearMessages"
                                                                control={<Switch checked={this.state.dailyStats.shiny.clearMessages} onChange={this.onInputChange} />}
                                                                label="Clear Messages"
                                                            />
                                                        </Grid>
                                                        <Grid item xs={12} sm={12}>
                                                            <TextField
                                                                id="dailyStats.shiny.channelId"
                                                                name="dailyStats.shiny.channelId"
                                                                variant="outlined"
                                                                label="Channel Id"
                                                                type="number"
                                                                value={this.state.dailyStats.shiny.channelId}
                                                                fullWidth
                                                                onChange={this.onInputChange}
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