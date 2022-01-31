import React, { useState } from 'react';
import {
    Box,
    Button,
    Grid,
    FormControl,
    InputLabel,
    MenuItem,
    Modal,
    Select,
    TextField,
    Typography,
} from '@mui/material';

interface AddAlarmProps {
    embeds: string[];
    filters: string[];
    geofences: string[];
    open: boolean;
    toggle: React.MouseEventHandler<HTMLButtonElement> | undefined;
    onSubmit: any; //React.ChangeEventHandler<HTMLTextAreaElement | HTMLInputElement> | undefined;
}

export function AddAlarmModal(props: AddAlarmProps) {
    const style = {
        position: 'absolute' as 'absolute',
        top: '50%',
        left: '50%',
        transform: 'translate(-50%, -50%)',
        width: 400,
        bgcolor: 'background.paper',
        border: '2px solid #000',
        boxShadow: 24,
        p: 4,
    };

    const [state, setState] = useState({
        name: '',
        description: '',
        filters: '',
        embeds: '',
        geofences: [],
        webhook: '',
    });

    const onInputChange = (e: any) => {
        setState({ ...state, [e.target.name]: e.target.value });
    };

    return (
        <div>
            <Modal
                open={props.open}
                onClose={props.toggle}
                aria-labelledby="modal-modal-title"
                aria-describedby="modal-modal-description"
            >
                <Box sx={style}>
                    <Typography id="modal-modal-title" variant="h6" component="h2">
                        Create Alarm
                    </Typography>
                    <Typography id="modal-modal-description" sx={{ mt: 2 }}></Typography>
                    <Grid container spacing={2} style={{paddingTop: '20px', paddingBottom: '10px'}}>
                        <Grid item xs={12} sm={12}>
                            <TextField
                                id="name"
                                name="name"
                                variant="outlined"
                                label="Name"
                                value={state.name}
                                fullWidth
                                onChange={onInputChange}
                            />
                        </Grid>
                        <Grid item xs={12} sm={12}>
                            <TextField
                                id="description"
                                name="description"
                                variant="outlined"
                                label="Description"
                                value={state.description}
                                fullWidth
                                onChange={onInputChange}
                            />
                        </Grid>
                        <Grid item xs={12} sm={6}>
                            <FormControl fullWidth>
                                <InputLabel id="filters-label">Filters</InputLabel>
                                <Select
                                    labelId="filters-label"
                                    id="filters"
                                    name="filters"
                                    label="Filters"
                                    value={state.filters}
                                    onChange={onInputChange}
                                >
                                    {props.filters && props.filters.map((filter: string) => {
                                        return (
                                            <MenuItem key={filter} value={filter}>{filter}</MenuItem>
                                        );
                                    })}
                                </Select>
                            </FormControl>
                        </Grid>
                        <Grid item xs={12} sm={6}>
                            <FormControl fullWidth>
                                <InputLabel id="embeds-label">Embeds</InputLabel>
                                <Select
                                    labelId="embeds-label"
                                    id="embeds"
                                    name="embeds"
                                    value={state.embeds}
                                    label="Embeds"
                                    onChange={onInputChange}
                                >
                                    {props.embeds && props.embeds.map((embed: string) => {
                                        return (
                                            <MenuItem key={embed} value={embed}>{embed}</MenuItem>
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
                                    value={state.geofences}
                                    multiple
                                    label="Geofences"
                                    onChange={onInputChange}
                                >
                                    {props.geofences && props.geofences.map((geofence: string) => {
                                        return (
                                            <MenuItem key={geofence} value={geofence}>{geofence}</MenuItem>
                                        );
                                    })}
                                </Select>
                            </FormControl>
                        </Grid>
                        <Grid item xs={12} sm={12}>
                            <TextField
                                id="webhook"
                                name="webhook"
                                variant="outlined"
                                label="Discord Webhook"
                                value={state.webhook}
                                fullWidth
                                onChange={onInputChange}
                            />
                        </Grid>
                        <Grid item xs={12} sm={12}>
                            <Button
                                variant="contained"
                                color="primary"
                                onClick={(e) => {
                                    props.onSubmit(state);
                                    props.toggle!(e);
                                }}
                            >
                                Create
                            </Button>
                            <Button
                                variant="outlined"
                                color="primary"
                                onClick={props.toggle}
                            >
                                Cancel
                            </Button>
                        </Grid>
                    </Grid>
                </Box>
            </Modal>
        </div>
    );
}