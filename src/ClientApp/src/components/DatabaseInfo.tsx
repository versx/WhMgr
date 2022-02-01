import React from 'react';
import {
    Grid,
    TextField,
} from '@mui/material';

interface DatabaseProps {
    name: string;
    host: string;
    port: number;
    username: string;
    password: string;
    database: string;
    onInputChange: any;
}

export function DatabaseInfo(props: DatabaseProps) {
    //console.log('database props:', props);

    return (
        <div>
            <Grid container spacing={2} style={{paddingTop: '20px', paddingBottom: '20px'}}>
                <Grid item xs={12} sm={6}>
                    <TextField
                        id={"database." + props.name + ".host"}
                        name={"database." + props.name + ".host"}
                        variant="outlined"
                        label="Host"
                        type="text"
                        value={props.host}
                        fullWidth
                        onChange={props.onInputChange}
                    />
                </Grid>
                <Grid item xs={12} sm={6}>
                    <TextField
                        id={"database." + props.name + ".port"}
                        name={"database." + props.name + ".port"}
                        variant="outlined"
                        label="Port"
                        type="number"
                        value={props.port}
                        fullWidth
                        onChange={props.onInputChange}
                    />
                </Grid>
                <Grid item xs={12} sm={6}>
                    <TextField
                        id={"database." + props.name + ".username"}
                        name={"database." + props.name + ".username"}
                        variant="outlined"
                        label="Username"
                        type="text"
                        value={props.username}
                        fullWidth
                        onChange={props.onInputChange}
                    />
                </Grid>
                <Grid item xs={12} sm={6}>
                    <TextField
                        id={"database." + props.name + ".password"}
                        name={"database." + props.name + ".password"}
                        variant="outlined"
                        label="Password"
                        type="text"
                        value={props.password}
                        fullWidth
                        onChange={props.onInputChange}
                    />
                </Grid>
                <Grid item xs={12} sm={12}>
                    <TextField
                        id={"database." + props.name + ".database"}
                        name={"database." + props.name + ".database"}
                        variant="outlined"
                        label="Database"
                        type="text"
                        value={props.database}
                        fullWidth
                        onChange={props.onInputChange}
                    />
                </Grid>
            </Grid>
        </div>
    );
};