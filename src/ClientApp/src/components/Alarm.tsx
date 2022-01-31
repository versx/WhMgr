import React, { useState } from 'react';
import {
    Button,
    FormControl,
    Grid,
    InputLabel,
    MenuItem,
    Select,
    SelectChangeEvent,
    TextField,
    Typography,
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
    const [name, setName] = useState(props.name ?? '');
    const [description, setDescription] = useState(props.description ?? '');
    const [geofences, setGeofences] = useState(props.geofences ?? []);
    const [embeds, setEmbeds] = useState(props.embeds ?? '');
    const [filters, setFilters] = useState(props.filters ?? '');
    const [webhook, setWebhook] = useState(props.webhook ?? '');

    return (
        <div key={props.name}>
            <Grid container spacing={2} style={{paddingTop: '20px', paddingBottom: '10px'}}>
                <Grid item xs={12} sm={12}>
                    <TextField
                        id="name"
                        name="name"
                        variant="outlined"
                        label="Name"
                        value={name}
                        fullWidth
                        onChange={() => setName(name)}
                    />
                </Grid>
                <Grid item xs={12} sm={12}>
                    <TextField
                        id="description"
                        name="description"
                        variant="outlined"
                        label="Description"
                        value={description}
                        fullWidth
                        onChange={() => setDescription(description)}
                    />
                </Grid>
                <Grid item xs={12} sm={6}>
                    <FormControl fullWidth>
                        <InputLabel id="filters-label">Filters</InputLabel>
                        <Select
                            labelId="filters-label"
                            id="filters"
                            name="filters"
                            value={filters}
                            label="Filters"
                            onChange={() => setFilters(filters)}
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
                            value={embeds}
                            label="Embeds"
                            onChange={() => setEmbeds(embeds)}
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
                            value={geofences}
                            multiple
                            label="Geofences"
                            onChange={() => setGeofences(geofences)}
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
                        value={webhook}
                        fullWidth
                        onChange={() => setWebhook(webhook)}
                    />
                </Grid>
            </Grid>
        </div>
    );
};