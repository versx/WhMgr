import React, { useState } from 'react';
import {
    FormControl,
    Grid,
    InputLabel,
    MenuItem,
    Select,
    TextField,
} from '@mui/material';

export interface PvpFilterProps {
    league: string;
    min_rank: number;
    max_rank: number;
    min_cp: number;
    max_cp: number;
    min_percent: number;
    max_percent: number;
    gender: string;

    allLeagues: string[];
}

export function PvpFilter(props: PvpFilterProps) {
    console.log('pvp filter props:', props);
    const [state, setState] = useState({
        league: props.league ?? '',
        min_rank: props.min_rank ?? '',
        max_rank: props.max_rank ?? '',
        min_cp: props.min_cp ?? '',
        max_cp: props.max_cp ?? '',
        min_percent: props.min_percent ?? '',
        max_percent: props.max_percent ?? '',
        gender: props.gender ?? '',
    });

    const onInputChange = (e: any) => {
        setState({ ...state, [e.target.name]: e.target.value });
    };

    return (
        <div>
            <Grid container spacing={2} style={{paddingTop: '20px', paddingBottom: '10px'}}>
                <Grid item xs={12} sm={12}>
                    <FormControl fullWidth>
                        <InputLabel id="leagues-label">Pvp League</InputLabel>
                        <Select
                            labelId="leagues-label"
                            id="league"
                            name="league"
                            value={state.league}
                            label="Leagues"
                            required
                            onChange={onInputChange}
                        >
                            {props.allLeagues && Object.keys(props.allLeagues).map((index: any) => {
                                const league = props.allLeagues[index].toString().toLowerCase();
                                console.log('league:', league);
                                return (
                                    <MenuItem key={league} value={league}>{league}</MenuItem>
                                );
                            })}
                        </Select>
                    </FormControl>
                </Grid>
                <Grid item xs={12} sm={6}>
                    <TextField
                        id="min_rank"
                        name="min_rank"
                        variant="outlined"
                        label="Minimum Rank"
                        type="number"
                        value={state.min_rank}
                        fullWidth
                        onChange={onInputChange}
                    />
                </Grid>
                <Grid item xs={12} sm={6}>
                    <TextField
                        id="max_rank"
                        name="max_rank"
                        variant="outlined"
                        label="Maximum Rank"
                        type="number"
                        value={state.max_rank}
                        fullWidth
                        onChange={onInputChange}
                    />
                </Grid>
                <Grid item xs={12} sm={6}>
                    <TextField
                        id="min_cp"
                        name="min_cp"
                        variant="outlined"
                        label="Minimum League CP"
                        type="number"
                        value={state.min_cp}
                        fullWidth
                        onChange={onInputChange}
                    />
                </Grid>
                <Grid item xs={12} sm={6}>
                    <TextField
                        id="max_cp"
                        name="max_cp"
                        variant="outlined"
                        label="Maximum League CP"
                        type="number"
                        value={state.max_cp}
                        fullWidth
                        onChange={onInputChange}
                    />
                </Grid>
                <Grid item xs={12} sm={6}>
                    <TextField
                        id="min_percent"
                        name="min_percent"
                        variant="outlined"
                        label="Minimum Percent"
                        type="number"
                        value={state.min_percent}
                        fullWidth
                        onChange={onInputChange}
                    />
                </Grid>
                <Grid item xs={12} sm={6}>
                    <TextField
                        id="max_percent"
                        name="max_percent"
                        variant="outlined"
                        label="Maximum Percent"
                        type="number"
                        value={state.max_percent}
                        fullWidth
                        onChange={onInputChange}
                    />
                </Grid>
            </Grid>
        </div>
    );
};