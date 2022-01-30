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
    List,
    ListItem,
    MenuItem,
    Select,
    SelectChangeEvent,
    Switch,
    TextareaAutosize,
    TextField,
    Typography,
} from '@mui/material';
import { makeStyles } from '@mui/styles';

import { Path, set, lensPath } from 'ramda';

import config from '../../config.json';
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
            alarms: [],
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
            color: 'text.primary',
            href: '',
            selected: true,
        }];

        return (
            <div className={classes.container} style={{ paddingTop: '50px', paddingBottom: '20px' }}>
                <Container>
                    <Box component="form" method="POST" action=""  onSubmit={this.handleSubmit} sx={{ mt: 3 }}>
                        <BreadCrumbs crumbs={breadcrumbs} />
                        <Typography variant="h5" component="h2" >
                            Edit Alarm {this.props.params!.id}
                        </Typography>
                        <Typography sx={{ mt: 2 }}>
                            Channel alarms config description goes here
                        </Typography>
                        <div style={{paddingBottom: '10px', paddingTop: '20px'}}>
                            <Card>
                                <CardHeader title="Global Management" />
                                <CardContent>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={6}>
                                            <FormControlLabel id="enablePokemon" name="enablePokemon" control={<Switch checked={this.state.enablePokemon} />} label="Enable Pokemon" />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <FormControlLabel id="enableRaids" name="enableRaids" control={<Switch checked={this.state.enableRaids} />} label="Enable Raids" />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <FormControlLabel id="enableQuests" name="enableQuests" control={<Switch checked={this.state.enableQuests} />} label="Enable Quests" />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <FormControlLabel id="enablePokestops" name="enablePokestops" control={<Switch checked={this.state.enablePokestops} />} label="Enable Pokestops" />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <FormControlLabel id="enableGyms" name="enableGyms" control={<Switch checked={this.state.enableGyms} />} label="Enable Gyms" />
                                        </Grid>
                                        <Grid item xs={12} sm={6}>
                                            <FormControlLabel id="enableWeather" name="enableWeather" control={<Switch checked={this.state.enableWeather} />} label="Enable Weather" />
                                        </Grid>
                                    </Grid>
                                </CardContent>
                            </Card>
                        </div>
                        <div style={{paddingBottom: '20px', paddingTop: '10px'}}>
                            <Card>
                                <CardHeader title="Channel Alarms" />
                                <CardContent>
                                    <List>
                                        <ListItem>Test</ListItem>
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

            </div>
        );
    }
}

export default withRouter(EditAlarm);