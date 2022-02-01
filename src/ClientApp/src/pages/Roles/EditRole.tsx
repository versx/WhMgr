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
import { makeStyles } from '@mui/styles';

import config from '../../config.json';
import { BreadCrumbs } from '../../components/BreadCrumbs';
import withRouter from '../../hooks/WithRouter';
import { IGlobalProps } from '../../interfaces/IGlobalProps';
import { onNestedStateChange } from '../../utils/nestedStateHelper';

class EditRole extends React.Component<IGlobalProps> {
    public state: any;

    constructor(props: IGlobalProps) {
        super(props);
        console.log('props:', props);
        this.state = {
            // TODO: Set default state values
            name: props.params!.id,
            moderator: false,
            permissions: [],
            roleId: '',
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
        fetch(config.apiUrl + 'admin/role/' + id, {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*',
            },
        })
        .then(async (response) => await response.json())
        .then(data => {
            console.log('role data:', data);
            //this.setState(data.data.role);
            const keys: string[] = Object.keys(data.data.role);
            for (const key of keys) {
                this.setState({ [key]: data.data.role[key] });
            }
            this.setState({ ['roleId']: data.data.roleId });
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
        fetch(config.apiUrl + 'admin/role/' + id, {
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
        const handleCancel = () => window.location.href = '/dashboard/discords';

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
            text: 'Discord Roles',
            color: 'inherit',
            href: '/dashboard/roles',
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
                            Edit Discord Role {this.props.params!.id}
                        </Typography>
                        <Typography sx={{ mt: 2 }}>
                            Discord role description goes here
                        </Typography>
                        <div style={{paddingBottom: '20px', paddingTop: '20px'}}>
                            <Grid container spacing={2}>
                                <Grid item xs={12}>
                                    <TextField
                                        id="name"
                                        name="name"
                                        variant="outlined"
                                        label="Name"
                                        type="text"
                                        value={this.state.name}
                                        fullWidth
                                        onChange={this.onInputChange}
                                    />
                                </Grid>
                                <Grid item xs={12}>
                                    <TextField
                                        id="roleId"
                                        name="roleId"
                                        variant="outlined"
                                        label="Discord ID"
                                        type="number"
                                        value={this.state.roleId}
                                        fullWidth
                                        onChange={this.onInputChange}
                                    />
                                </Grid>
                                <Grid item xs={12} sm={12}>
                                    <FormControl fullWidth>
                                        <InputLabel id="permissions-label">Permissions</InputLabel>
                                        <Select
                                            labelId="permissions-label"
                                            id="permissions"
                                            name="permissions"
                                            value={this.state.permissions}
                                            multiple
                                            label="Permissions"
                                            onChange={this.onInputChange}
                                        >
                                            <MenuItem value="Pokemon">Pokemon</MenuItem>
                                            <MenuItem value="PvP">PvP</MenuItem>
                                            <MenuItem value="Raids">Raids</MenuItem>
                                            <MenuItem value="Quests">Quests</MenuItem>
                                            <MenuItem value="Invasions">Invasions</MenuItem>
                                            <MenuItem value="Lures">Lures</MenuItem>
                                            <MenuItem value="Gyms">Gyms</MenuItem>
                                        </Select>
                                    </FormControl>
                                </Grid>
                                <Grid item xs={12} sm={12}>
                                    <FormControlLabel
                                        id="moderator"
                                        name="moderator"
                                        control={<Switch checked={this.state.moderator} onChange={this.onInputChange} />}
                                        label="Is Moderator"
                                    />
                                </Grid>
                            </Grid>
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

export default withRouter(EditRole);