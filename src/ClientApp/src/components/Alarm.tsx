import React, { useState } from 'react';
import {
    FormControl,
    Grid,
    InputLabel,
    MenuItem,
    Select,
    TextField,
} from '@mui/material';

export interface AlarmProps {
    name: string;
    filters: string;
    embeds: string;
    geofences: string[];
    description: string;
    webhook: string;

    allFilters: string[];
    allEmbeds: string[];
    allGeofences: string[];
}

export function Alarm(props: AlarmProps) {
    //console.log('alarm props:', props);
    const [state, setState] = useState({
        name: props.name ?? '',
        description: props.description ?? '',
        geofences: props.geofences ?? [],
        //embeds: props.allEmbeds.find(e => e === props.embeds) ?? '',
        //filters: props.allFilters.find(f => f === props.filters) ?? '',
        embeds: props.embeds ?? '',
        filters: props.filters ?? '',
        webhook: props.webhook ?? '',
    });

    const onInputChange = (e: any) => {
        setState({ ...state, [e.target.name]: e.target.value });
    };

    return (
        <div>
            <Grid container spacing={2} style={{paddingTop: '20px', paddingBottom: '10px'}}>
                <Grid item xs={12} sm={12}>
                    <TextField
                        id="name"
                        name="name"
                        variant="outlined"
                        label="Name"
                        value={state.name}
                        fullWidth
                        required
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
                            value={state.filters}
                            label="Filters"
                            required
                            onChange={onInputChange}
                        >
                            {props.allFilters && props.allFilters.map((filter: string) => {
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
                            required
                            onChange={onInputChange}
                        >
                            {props.allEmbeds && props.allEmbeds.map((embed: string) => {
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
                            required
                            onChange={onInputChange}
                        >
                            {props.allGeofences && props.allGeofences.map((geofence: string) => {
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
                        required
                        onChange={onInputChange}
                    />
                </Grid>
            </Grid>
        </div>
    );
};