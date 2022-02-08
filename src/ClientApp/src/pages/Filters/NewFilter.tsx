import React from 'react'
import {
    Accordion,
    AccordionDetails,
    AccordionSummary,
    Box,
    Button,
    Container,
    FormControl,
    FormControlLabel,
    Grid,
    InputLabel,
    MenuItem,
    Select,
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
import withRouter from '../../hooks/WithRouter';
import { IGlobalProps } from '../../interfaces/IGlobalProps';
import { onNestedStateChange } from '../../utils/nestedStateHelper';

class NewFilter extends React.Component<IGlobalProps> {
    public state: any;

    constructor(props: IGlobalProps) {
        super(props);
        console.log('props:', props);
        this.state = {
            // TODO: Set default state values
            name: props.params!.id,
            pokemon: {
                enabled: false,
                pokemon: [],
                forms: [],
                costumes: [],
                min_iv: 0,
                max_iv: 100,
                min_cp: 0,
                max_cp: 999999,
                min_lvl: 0,
                max_lvl: 35,
                gender: '*',
                size: 'All',
                great_league: false,
                ultra_league: false,
                min_rank: 0,
                max_rank: 100,
                is_event: false,
                type: 'Include',
                ignore_missing: false,
            },
            raids: {
                enabled: false,
                pokemon: [],
                forms: [],
                costumes: [],
                min_lvl: 1,
                max_lvl: 6,
                team: 'All',
                type: 'Include',
                only_ex: false,
                ignore_missing: false,
            },
            eggs: {
                enabled: false,
                min_lvl: 1,
                max_lvl: 6,
                team: 'All',
                only_ex: false,
            },
            quests: {
                enabled: false,
                rewards: [],
                is_shiny: false,
                type: 'Include',
            },
            pokestops: {
                enabled: false,
                lured: false,
                lure_types: [],
                invasions: false,
                invasion_types: {},
            },
            gyms: {
                enabled: false,
                under_attack: false,
                team: 'All',
            },
            weather: {
                enabled: false,
                types: [],
            },
        };
        this.onInputChange = this.onInputChange.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
        this.handlePanelExpanded = this.handlePanelExpanded.bind(this);
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

        // TODO: Only send what is set
        const data = {
            name: this.state.name,
            filter: {
                pokemon: this.state.pokemon,
                raids: this.state.raids,
                eggs: this.state.eggs,
                quests: this.state.quests,
                pokestops: this.state.pokestops,
                gyms: this.state.gyms,
                weather: this.state.weather,
            },
        };
        fetch(config.apiUrl + 'admin/filter/new', {
            method: 'POST',
            body: JSON.stringify(data),
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
            window.location.href = config.homepage + 'filters';
        }).catch((err) => {
            console.error('error:', err);
            event.preventDefault();
        });
    }

    render() {
        const handleCancel = () => window.location.href = config.homepage + 'filters';

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
            text: 'Filters',
            color: 'inherit',
            href: config.homepage + 'filters',
            selected: false,
        }, {
            text: 'New',
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
                            New Webhook Filter
                        </Typography>
                        <Typography sx={{ mt: 2 }}>
                            Webhook filter config description goes here
                        </Typography>
                        <div style={{paddingBottom: '20px', paddingTop: '20px'}}>
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
                                style={{paddingBottom: '20px'}}
                            />
                            <Accordion expanded={this.state.expanded === 'panel1'} onChange={this.handlePanelExpanded('panel1')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Pokemon</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel
                                                id="pokemon.enabled"
                                                name="pokemon.enabled"
                                                control={<Switch checked={this.state.pokemon.enabled} onChange={this.onInputChange} />}
                                                label="Enabled"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="pokemon.pokemon"
                                                name="pokemon.pokemon"
                                                variant="outlined"
                                                label="Pokemon IDs"
                                                type="text"
                                                value={this.state.pokemon.pokemon}
                                                fullWidth
                                                multiline
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="pokemon.forms"
                                                name="pokemon.forms"
                                                variant="outlined"
                                                label="Forms"
                                                type="text"
                                                value={this.state.pokemon.forms}
                                                fullWidth
                                                multiline
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="pokemon.costumes"
                                                name="pokemon.costumes"
                                                variant="outlined"
                                                label="Costumes"
                                                type="text"
                                                value={this.state.pokemon.costumes}
                                                fullWidth
                                                multiline
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="pokemon.min_iv"
                                                name="pokemon.min_iv"
                                                variant="outlined"
                                                label="Minimum IV"
                                                type="number"
                                                value={this.state.pokemon.min_iv}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="pokemon.max_iv"
                                                name="pokemon.max_iv"
                                                variant="outlined"
                                                label="Maximum IV"
                                                type="number"
                                                value={this.state.pokemon.max_iv}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="pokemon.min_cp"
                                                name="pokemon.min_cp"
                                                variant="outlined"
                                                label="Minimum CP"
                                                type="number"
                                                value={this.state.pokemon.min_cp}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="pokemon.max_cp"
                                                name="pokemon.max_cp"
                                                variant="outlined"
                                                label="Maximum CP"
                                                type="number"
                                                value={this.state.pokemon.max_cp}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="pokemon.min_lvl"
                                                name="pokemon.min_lvl"
                                                variant="outlined"
                                                label="Minimum Level"
                                                type="number"
                                                value={this.state.pokemon.min_lvl}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="pokemon.max_lvl"
                                                name="pokemon.max_lvl"
                                                variant="outlined"
                                                label="Maximum Level"
                                                type="number"
                                                value={this.state.pokemon.maxLevel}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <FormControlLabel
                                                id="pokemon.great_league"
                                                name="pokemon.great_league"
                                                control={<Switch checked={this.state.pokemon.great_league} onChange={this.onInputChange} />}
                                                label="Is Great League"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <FormControlLabel
                                                id="pokemon.ultra_league"
                                                name="pokemon.ultra_league"
                                                control={<Switch checked={this.state.pokemon.ultra_league} onChange={this.onInputChange} />}
                                                label="Is Ultra League"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="pokemon.min_rank"
                                                name="pokemon.min_rank"
                                                variant="outlined"
                                                label="Minimum Rank"
                                                type="number"
                                                value={this.state.pokemon.min_rank}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="pokemon.max_rank"
                                                name="pokemon.max_rank"
                                                variant="outlined"
                                                label="Maximum Rank"
                                                type="number"
                                                value={this.state.pokemon.max_rank}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel
                                                id="pokemon.is_event"
                                                name="pokemon.is_event"
                                                control={<Switch checked={this.state.pokemon.is_event} onChange={this.onInputChange} />}
                                                label="Is Event Pokemon"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="type-label">Filter Type</InputLabel>
                                                <Select
                                                    labelId="type-label"
                                                    id="pokemon.type"
                                                    name="pokemon.type"
                                                    value={this.state.pokemon.type}
                                                    label="Filter Type"
                                                    onChange={this.onInputChange}
                                                >
                                                    <MenuItem value="Include">Include</MenuItem>
                                                    <MenuItem value="Exclude">Exclude</MenuItem>
                                                </Select>
                                            </FormControl>
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel
                                                id="pokemon.ignore_missing"
                                                name="pokemon.ignore_missing"
                                                control={<Switch checked={this.state.pokemon.ignore_missing} onChange={this.onInputChange} />}
                                                label="Ignore Pokemon Missing Stats"
                                            />
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel2'} onChange={this.handlePanelExpanded('panel2')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Raids</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel
                                                id="raids.enabled"
                                                name="raids.enabled"
                                                control={<Switch checked={this.state.raids.enabled} onChange={this.onInputChange} />}
                                                label="Enabled"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="raids.pokemon"
                                                name="raids.pokemon"
                                                variant="outlined"
                                                label="Pokemon IDs"
                                                type="text"
                                                value={this.state.raids.pokemon}
                                                fullWidth
                                                multiline
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="raids.forms"
                                                name="raids.forms"
                                                variant="outlined"
                                                label="Forms"
                                                type="text"
                                                value={this.state.raids.forms}
                                                fullWidth
                                                multiline
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="raids.costumes"
                                                name="raids.costumes"
                                                variant="outlined"
                                                label="Costumes"
                                                type="text"
                                                value={this.state.raids.costumes}
                                                fullWidth
                                                multiline
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="raids.min_lvl"
                                                name="raids.min_lvl"
                                                variant="outlined"
                                                label="Minimum Level"
                                                type="number"
                                                value={this.state.raids.min_lvl}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="raids.max_lvl"
                                                name="raids.max_lvl"
                                                variant="outlined"
                                                label="Maximum Level"
                                                type="number"
                                                value={this.state.raids.max_lvl}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="team-label">Team</InputLabel>
                                                <Select
                                                    labelId="team-label"
                                                    id="raids.team"
                                                    name="raids.team"
                                                    value={this.state.raids.team}
                                                    label="Team"
                                                    onChange={this.onInputChange}
                                                >
                                                    <MenuItem value="All">All</MenuItem>
                                                    <MenuItem value="Neutral">Neutral</MenuItem>
                                                    <MenuItem value="Mystic">Mystic</MenuItem>
                                                    <MenuItem value="Valor">Valor</MenuItem>
                                                    <MenuItem value="Instinct">Instinct</MenuItem>
                                                </Select>
                                            </FormControl>
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel
                                                id="raids.only_ex"
                                                name="raids.only_ex"
                                                control={<Switch checked={this.state.raids.only_ex} onChange={this.onInputChange} />}
                                                label="Only EX-Eligible Gyms"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="type-label">Filter Type</InputLabel>
                                                <Select
                                                    labelId="type-label"
                                                    id="raids.type"
                                                    name="raids.type"
                                                    value={this.state.raids.type}
                                                    label="Filter Type"
                                                    onChange={this.onInputChange}
                                                >
                                                    <MenuItem value="Include">Include</MenuItem>
                                                    <MenuItem value="Exclude">Exclude</MenuItem>
                                                </Select>
                                            </FormControl>
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel
                                                id="raids.ignore_missing"
                                                name="raids.ignore_missing"
                                                control={<Switch checked={this.state.raids.ignore_missing} onChange={this.onInputChange} />}
                                                label="Ignore Pokemon Missing Stats"
                                            />
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel3'} onChange={this.handlePanelExpanded('panel3')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Eggs</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel
                                                id="eggs.enabled"
                                                name="eggs.enabled"
                                                control={<Switch checked={this.state.eggs.enabled} onChange={this.onInputChange} />}
                                                label="Enabled"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="eggs.min_lvl"
                                                name="eggs.min_lvl"
                                                variant="outlined"
                                                label="Minimum Level"
                                                type="number"
                                                value={this.state.eggs.min_lvl}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="eggs.max_lvl"
                                                name="eggs.max_lvl"
                                                variant="outlined"
                                                label="Maximum Level"
                                                type="number"
                                                value={this.state.eggs.max_lvl}
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="team-label">Team</InputLabel>
                                                <Select
                                                    labelId="team-label"
                                                    id="eggs.team"
                                                    name="eggs.team"
                                                    value={this.state.eggs.team}
                                                    label="Team"
                                                    onChange={this.onInputChange}
                                                >
                                                    <MenuItem value="All">All</MenuItem>
                                                    <MenuItem value="Neutral">Neutral</MenuItem>
                                                    <MenuItem value="Mystic">Mystic</MenuItem>
                                                    <MenuItem value="Valor">Valor</MenuItem>
                                                    <MenuItem value="Instinct">Instinct</MenuItem>
                                                </Select>
                                            </FormControl>
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel
                                                id="eggs.only_ex"
                                                name="eggs.only_ex"
                                                control={<Switch checked={this.state.eggs.only_ex} onChange={this.onInputChange} />}
                                                label="Only EX-Eligible Gyms"
                                            />
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel4'} onChange={this.handlePanelExpanded('panel4')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Quests</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel
                                                id="quests.enabled"
                                                name="quests.enabled"
                                                control={<Switch checked={this.state.quests.enabled} onChange={this.onInputChange} />}
                                                label="Enabled"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="quests.rewards"
                                                name="quests.rewards"
                                                variant="outlined"
                                                label="Reward Keywords"
                                                type="text"
                                                value={this.state.quests.rewards}
                                                multiline
                                                fullWidth
                                                onChange={this.onInputChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel
                                                id="quests.is_shiny"
                                                name="quests.is_shiny"
                                                control={<Switch checked={this.state.quests.is_shiny} onChange={this.onInputChange} />}
                                                label="Is Shiny Pokemon"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="type-label">Filter Type</InputLabel>
                                                <Select
                                                    labelId="type-label"
                                                    id="quests.type"
                                                    name="quests.type"
                                                    value={this.state.quests.type}
                                                    label="Filter Type"
                                                    onChange={this.onInputChange}
                                                >
                                                    <MenuItem value="Include">Include</MenuItem>
                                                    <MenuItem value="Exclude">Exclude</MenuItem>
                                                </Select>
                                            </FormControl>
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel5'} onChange={this.handlePanelExpanded('panel5')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Pokestops</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel
                                                id="pokestops.enabled"
                                                name="pokestops.enabled"
                                                control={<Switch checked={this.state.pokestops.enabled} onChange={this.onInputChange} />}
                                                label="Enabled"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel
                                                id="pokestops.lured"
                                                name="pokestops.lured"
                                                control={<Switch checked={this.state.pokestops.lured}  onChange={this.onInputChange} />}
                                                label="Is Lured Pokestop"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="type-label">Lure Types</InputLabel>
                                                <Select
                                                    labelId="lure-type-label"
                                                    id="pokestops.lure_types"
                                                    name="pokestops.lure_types"
                                                    value={this.state.pokestops.lure_types}
                                                    multiple
                                                    label="Lure Types"
                                                    onChange={this.onInputChange}
                                                >
                                                    <MenuItem value="Normal">Normal</MenuItem>
                                                    <MenuItem value="Glacial">Glacial</MenuItem>
                                                    <MenuItem value="Mossy">Mossy</MenuItem>
                                                    <MenuItem value="Magnetic">Magnetic</MenuItem>
                                                    <MenuItem value="Rainy">Rainy</MenuItem>
                                                </Select>
                                            </FormControl>
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel
                                                id="pokestops.invasions"
                                                name="pokestops.invasions"
                                                control={<Switch checked={this.state.pokestops.invasions} onChange={this.onInputChange} />}
                                                label="Is Invasion Pokestop"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="invasionTypes-label">Invasion Types</InputLabel>
                                                <Select
                                                    labelId="invasionTypes-label"
                                                    id="pokestops.invasion_types"
                                                    name="pokestops.invasion_types"
                                                    value={this.state.pokestops.invasion_types}
                                                    //multiple
                                                    label="Invasion Types"
                                                    onChange={this.onInputChange}
                                                >
                                                    <MenuItem value="">All</MenuItem>
                                                </Select>
                                            </FormControl>
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel6'} onChange={this.handlePanelExpanded('panel6')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Gyms</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel
                                                id="gyms.enabled"
                                                name="gyms.enabled"
                                                control={<Switch checked={this.state.gyms.enabled} onChange={this.onInputChange} />}
                                                label="Enabled"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel
                                                id="gyms.under_attack"
                                                name="gyms.under_attack"
                                                control={<Switch checked={this.state.gyms.under_attack} onChange={this.onInputChange} />}
                                                label="Is Under Attack"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="team-label">Team</InputLabel>
                                                <Select
                                                    labelId="team-label"
                                                    id="gyms.team"
                                                    name="gyms.team"
                                                    value={this.state.gyms.team}
                                                    label="Team"
                                                    onChange={this.onInputChange}
                                                >
                                                    <MenuItem value="All">All</MenuItem>
                                                    <MenuItem value="Neutral">Neutral</MenuItem>
                                                    <MenuItem value="Mystic">Mystic</MenuItem>
                                                    <MenuItem value="Valor">Valor</MenuItem>
                                                    <MenuItem value="Instinct">Instinct</MenuItem>
                                                </Select>
                                            </FormControl>
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel7'} onChange={this.handlePanelExpanded('panel7')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Weather</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel
                                                id="weather.enabled"
                                                name="weather.enabled"
                                                control={<Switch checked={this.state.weather.enabled} onChange={this.onInputChange} />}
                                                label="Enabled"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="weatherTypes-label">Weather Types</InputLabel>
                                                <Select
                                                    labelId="weatherTypes-label"
                                                    id="weather.types"
                                                    name="weather.types"
                                                    value={this.state.weather.types}
                                                    label="Weather Types"
                                                    multiple
                                                    onChange={this.onInputChange}
                                                >
                                                    <MenuItem value="Clear">Clear</MenuItem>
                                                    <MenuItem value="Rainy">Rainy</MenuItem>
                                                    <MenuItem value="PartlyCloudy">Partly Cloudy</MenuItem>
                                                    <MenuItem value="Overcast">Overcast</MenuItem>
                                                    <MenuItem value="Windy">Windy</MenuItem>
                                                    <MenuItem value="Snow">Snow</MenuItem>
                                                    <MenuItem value="Fog">Fog</MenuItem>
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

export default withRouter(NewFilter);