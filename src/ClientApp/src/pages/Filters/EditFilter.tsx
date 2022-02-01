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
    TextareaAutosize,
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

class EditFilter extends React.Component<IGlobalProps> {
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
                minIV: 0,
                maxIV: 100,
                minCP: 0,
                maxCP: 999999,
                minLevel: 0,
                maxLevel: 35,
                gender: '*',
                size: 'All',
                isGreatLeague: false,
                isUltraLeague: false,
                minRank: 0,
                maxRank: 100,
                isEvent: false,
                type: 'Include',
                ignoreMissing: false,
            },
            raids: {
                enabled: false,
                pokemon: [],
                forms: [],
                costumes: [],
                minLevel: 1,
                maxLevel: 6,
                team: 'All',
                type: 'Include',
                onlyEx: false,
                ignoreMissing: false,
            },
            eggs: {
                enabled: false,
                minLevel: 1,
                maxLevel: 6,
                team: 'All',
                onlyEx: false,
            },
            quests: {
                enabled: false,
                rewardKeyword: '',
                isShiny: false,
                type: 'Include',
            },
            pokestops: {
                enabled: false,
                lured: false,
                lureTypes: [],
                invasions: false,
                invasionTypes: [],
            },
            gyms: {
                enabled: false,
                isUnderAttack: false,
                team: 'All',
            },
            weather: {
                enabled: false,
                types: [],
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
        fetch(config.apiUrl + 'admin/filter/' + id, {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*',
            },
        })
        .then(async (response) => await response.json())
        .then(data => {
            console.log('filter data:', data);
            //this.setState(data.data.filter);
            const keys: string[] = Object.keys(data.data.filter);
            for (const key of keys) {
                //console.log('key:', key, 'data:', data.data.filter[key]);
                if (data.data.filter[key]) {
                    this.setState({ [key]: data.data.filter[key] });
                }
            }
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
        fetch(config.apiUrl + 'admin/filter/' + id, {
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
                            Edit Webhook Filter {this.props.params!.id}
                        </Typography>
                        <Typography sx={{ mt: 2 }}>
                            Webhook filter config description goes here
                        </Typography>
                        <div style={{paddingBottom: '20px', paddingTop: '20px'}}>
                            <Accordion expanded={this.state.expanded === 'panel1'} onChange={this.handlePanelExpanded('panel1')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Pokemon</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel id="enabled" name="enabled" control={<Switch checked={this.state.pokemon.enabled} />} label="Enabled" />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="pokemonIds"
                                                name="pokemonIds"
                                                variant="outlined"
                                                label="Pokemon IDs"
                                                type="text"
                                                value={this.state.pokemon.pokemon}
                                                fullWidth
                                                multiline
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="forms"
                                                name="forms"
                                                variant="outlined"
                                                label="Forms"
                                                type="text"
                                                value={this.state.pokemon.forms}
                                                fullWidth
                                                multiline
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="costumes"
                                                name="costumes"
                                                variant="outlined"
                                                label="Costumes"
                                                type="text"
                                                value={this.state.pokemon.costumes}
                                                fullWidth
                                                multiline
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="minIV"
                                                name="minIV"
                                                variant="outlined"
                                                label="Minimum IV"
                                                type="number"
                                                value={this.state.pokemon.minIV}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="maxIV"
                                                name="maxIV"
                                                variant="outlined"
                                                label="Maximum IV"
                                                type="number"
                                                value={this.state.pokemon.maxIV}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="minCP"
                                                name="minCP"
                                                variant="outlined"
                                                label="Minimum CP"
                                                type="number"
                                                value={this.state.pokemon.minCP}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="maxCP"
                                                name="maxCP"
                                                variant="outlined"
                                                label="Maximum CP"
                                                type="number"
                                                value={this.state.pokemon.maxCP}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="minLevel"
                                                name="minLevel"
                                                variant="outlined"
                                                label="Minimum Level"
                                                type="number"
                                                value={this.state.pokemon.minLevel}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="maxLevel"
                                                name="maxLevel"
                                                variant="outlined"
                                                label="Maximum Level"
                                                type="number"
                                                value={this.state.pokemon.maxLevel}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <FormControlLabel id="isGreatLeague" name="isGreatLeague" control={<Switch checked={this.state.pokemon.isGreatLeague} />} label="Is Great League" />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <FormControlLabel id="isUltraLeague" name="isUltraLeague" control={<Switch checked={this.state.pokemon.isUltraLeague} />} label="Is Ultra League" />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="minRank"
                                                name="minRank"
                                                variant="outlined"
                                                label="Minimum Rank"
                                                type="number"
                                                value={this.state.pokemon.minRank}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="maxRank"
                                                name="maxRank"
                                                variant="outlined"
                                                label="Maximum Rank"
                                                type="number"
                                                value={this.state.pokemon.maxRank}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel id="isEvent" name="isEvent" control={<Switch checked={this.state.pokemon.isEvent} />} label="Is Event Pokemon" />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="type-label">Filter Type</InputLabel>
                                                <Select
                                                    labelId="type-label"
                                                    id="type"
                                                    name="type"
                                                    value={this.state.pokemon.type}
                                                    label="Filter Type"
                                                    onChange={this.handleChange}
                                                >
                                                    <MenuItem value="Include">Include</MenuItem>
                                                    <MenuItem value="Exclude">Exclude</MenuItem>
                                                </Select>
                                            </FormControl>
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel id="ignoreMissing" name="ignoreMissing" control={<Switch checked={this.state.pokemon.ignoreMissing} />} label="Ignore Pokemon Missing Stats" />
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
                                            <FormControlLabel id="enabled" name="enabled" control={<Switch checked={this.state.raids.enabled} />} label="Enabled" />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="pokemonIds"
                                                name="pokemonIds"
                                                variant="outlined"
                                                label="Pokemon IDs"
                                                type="text"
                                                value={this.state.raids.pokemon}
                                                fullWidth
                                                multiline
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="forms"
                                                name="forms"
                                                variant="outlined"
                                                label="Forms"
                                                type="text"
                                                value={this.state.raids.forms}
                                                fullWidth
                                                multiline
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="costumes"
                                                name="costumes"
                                                variant="outlined"
                                                label="Costumes"
                                                type="text"
                                                value={this.state.raids.costumes}
                                                fullWidth
                                                multiline
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="minLevel"
                                                name="minLevel"
                                                variant="outlined"
                                                label="Minimum Level"
                                                type="number"
                                                value={this.state.raids.minLevel}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="maxLevel"
                                                name="maxLevel"
                                                variant="outlined"
                                                label="Maximum Level"
                                                type="number"
                                                value={this.state.raids.maxLevel}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="team-label">Team</InputLabel>
                                                <Select
                                                    labelId="team-label"
                                                    id="team"
                                                    name="team"
                                                    value={this.state.raids.team}
                                                    label="Team"
                                                    onChange={this.handleChange}
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
                                            <FormControlLabel id="onlyEx" name="onlyEx" control={<Switch checked={this.state.eggs.onlyEx} />} label="Only EX-Eligible Gyms" />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="type-label">Filter Type</InputLabel>
                                                <Select
                                                    labelId="type-label"
                                                    id="type"
                                                    name="type"
                                                    value={this.state.raids.type}
                                                    label="Filter Type"
                                                    onChange={this.handleChange}
                                                >
                                                    <MenuItem value="Include">Include</MenuItem>
                                                    <MenuItem value="Exclude">Exclude</MenuItem>
                                                </Select>
                                            </FormControl>
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel id="ignoreMissing" name="ignoreMissing" control={<Switch checked={this.state.raids.ignoreMissing} />} label="Ignore Pokemon Missing Stats" />
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
                                            <FormControlLabel id="enabled" name="enabled" control={<Switch checked={this.state.eggs.enabled} />} label="Enabled" />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="minLevel"
                                                name="minLevel"
                                                variant="outlined"
                                                label="Minimum Level"
                                                type="number"
                                                value={this.state.eggs.minLevel}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <TextField
                                                id="maxLevel"
                                                name="maxLevel"
                                                variant="outlined"
                                                label="Maximum Level"
                                                type="number"
                                                value={this.state.eggs.maxLevel}
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="team-label">Team</InputLabel>
                                                <Select
                                                    labelId="team-label"
                                                    id="team"
                                                    name="team"
                                                    value={this.state.eggs.team}
                                                    label="Team"
                                                    onChange={this.handleChange}
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
                                            <FormControlLabel id="onlyEx" name="onlyEx" control={<Switch checked={this.state.eggs.onlyEx} />} label="Only EX-Eligible Gyms" />
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
                                            <FormControlLabel id="enabled" name="enabled" control={<Switch checked={this.state.quests.enabled} />} label="Enabled" />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <TextField
                                                id="rewardKeywords"
                                                name="rewardKeywords"
                                                variant="outlined"
                                                label="Reward Keywords"
                                                type="text"
                                                value={this.state.quests.rewardKeywords}
                                                multiline
                                                fullWidth
                                                onChange={this.handleChange}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel id="isShiny" name="isShiny" control={<Switch checked={this.state.quests.isShiny} />} label="Is Shiny Pokemon" />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="type-label">Filter Type</InputLabel>
                                                <Select
                                                    labelId="type-label"
                                                    id="type"
                                                    name="type"
                                                    value={this.state.quests.type}
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
                            <Accordion expanded={this.state.expanded === 'panel5'} onChange={this.handlePanelExpanded('panel5')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Pokestops</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel id="enabled" name="enabled" control={<Switch checked={this.state.pokestops.enabled} />} label="Enabled" />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel id="isLured" name="isLured" control={<Switch checked={this.state.pokestops.lured} />} label="Is Lured Pokestop" />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="type-label">Lure Types</InputLabel>
                                                <Select
                                                    labelId="lure-type-label"
                                                    id="lureType"
                                                    name="lureType"
                                                    value={[this.state.pokestops.lureTypes]}
                                                    multiple
                                                    label="Lure Types"
                                                    onChange={this.handleChange}
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
                                            <FormControlLabel id="isInvasion" name="isInvasion" control={<Switch checked={this.state.pokestops.invasions} />} label="Is Invasion Pokestop" />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="gruntType-label">Invasion Types</InputLabel>
                                                <Select
                                                    labelId="gruntType-label"
                                                    id="gruntType"
                                                    name="gruntType"
                                                    //value={this.state.pokestops.gruntTypes}
                                                    //multiple
                                                    label="Invasion Types"
                                                    onChange={this.handleChange}
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
                                            <FormControlLabel id="enabled" name="enabled" control={<Switch checked={this.state.gyms.enabled} />} label="Enabled" />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControlLabel id="isUnderAttack" name="isUnderAttack" control={<Switch checked={this.state.gyms.isUnderAttack} />} label="Is Under Attack" />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="team-label">Team</InputLabel>
                                                <Select
                                                    labelId="team-label"
                                                    id="team"
                                                    name="team"
                                                    value={this.state.gyms.team}
                                                    label="Team"
                                                    onChange={this.handleChange}
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
                                            <FormControlLabel id="enabled" name="enabled" control={<Switch checked={this.state.weather.enabled} />} label="Enabled" />
                                        </Grid>
                                        <Grid item xs={12} sm={12}>
                                            <FormControl fullWidth>
                                                <InputLabel id="weatherTypes-label">Weather Types</InputLabel>
                                                <Select
                                                    labelId="weatherTypes-label"
                                                    id="weatherTypes"
                                                    name="weatherTypes"
                                                    value={this.state.weather.types}
                                                    label="Weather Types"
                                                    multiple
                                                    onChange={this.handleChange}
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

export default withRouter(EditFilter);