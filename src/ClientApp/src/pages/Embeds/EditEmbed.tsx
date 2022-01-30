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

import { Path, set, lensPath } from 'ramda';

import config from '../../config.json';
import withRouter from '../../hooks/WithRouter';
import { IGlobalProps } from '../../interfaces/IGlobalProps';

class EditEmbed extends React.Component<IGlobalProps> {
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
        this.setObjectByPath = this.setObjectByPath.bind(this);
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
                            Edit Embed Message Template {this.props.params!.id}
                        </Typography>
                        <Typography sx={{ mt: 2 }}>
                            Use <code>{"{{placeholder}}"}</code> surrounding an available placeholder value to replace it with actual data at runtime.<br />
                            Use <code>{"{{#if placeholder}}Show if true!{{/if}}"}</code> to handle conditional expressions that return a <code>Boolean</code> type.<br />
                            Use <code>{"{{#each rankings}}{{rank}} {{cp}} {{pokemon}}{{/each}}"}</code> to iterate and handle displaying <code>Array</code> values.<br />
                            <a href="https://handlebarsjs.com/guide" target="_blank">Handlebars Documentation</a><br /><br />
                            <i>Each new line in the content property reflects an actual new line in the message embed.</i>
                        </Typography>
                        <div style={{paddingBottom: '20px', paddingTop: '20px'}}>
                            <Accordion expanded={this.state.expanded === 'panel1'} onChange={this.handlePanelExpanded('panel1')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Pokemon</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            ...
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel2'} onChange={this.handlePanelExpanded('panel2')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Pokemon Missing IV Stats</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            ...
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel3'} onChange={this.handlePanelExpanded('panel3')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Raids</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            ...
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel4'} onChange={this.handlePanelExpanded('panel4')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Eggs</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            ...
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel5'} onChange={this.handlePanelExpanded('panel5')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Gyms</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            ...
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel6'} onChange={this.handlePanelExpanded('panel6')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Pokestops</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            ...
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel7'} onChange={this.handlePanelExpanded('panel7')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Quests</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            ...
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel8'} onChange={this.handlePanelExpanded('panel8')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Pokestops</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            ...
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel9'} onChange={this.handlePanelExpanded('panel9')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Lures</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            ...
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel10'} onChange={this.handlePanelExpanded('panel10')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Invasions</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            ...
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel11'} onChange={this.handlePanelExpanded('panel11')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Nests</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            ...
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={this.state.expanded === 'panel12'} onChange={this.handlePanelExpanded('panel12')}>
                                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                    <Typography>Weather</Typography>
                                </AccordionSummary>
                                <AccordionDetails>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={12}>
                                            ...
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

export default withRouter(EditEmbed);