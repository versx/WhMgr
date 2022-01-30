import React, { useState } from 'react';
import {
    Grid,
    TextField,
} from '@mui/material';

interface DatabaseProps {
    host: string;
    port: number;
    username: string;
    password: string;
    database: string;
}

export function DatabaseInfo(props: DatabaseProps) {
    //console.log('database props:', props);
    const [host, setHost] = useState(props.host ?? '127.0.0.1');
    const [port, setPort] = useState(props.port ?? 3306);
    const [user, setUser] = useState(props.username ?? '');
    const [pass, setPass] = useState(props.password ?? '');
    const [database, setDatabase] = useState(props.database ?? '');

    return (
        <div>
            <Grid container spacing={2} style={{paddingTop: '20px', paddingBottom: '20px'}}>
                <Grid item xs={12} sm={6}>
                    <TextField
                        id="host"
                        name="host"
                        variant="outlined"
                        label="Host"
                        type="text"
                        value={host}
                        fullWidth
                        onChange={() => setHost(host)}
                    />
                </Grid>
                <Grid item xs={12} sm={6}>
                    <TextField
                        id="port"
                        name="port"
                        variant="outlined"
                        label="Port"
                        type="number"
                        value={port}
                        fullWidth
                        onChange={() => setPort(port)}
                    />
                </Grid>
                <Grid item xs={12} sm={6}>
                    <TextField
                        id="user"
                        name="user"
                        variant="outlined"
                        label="Username"
                        type="text"
                        value={user}
                        fullWidth
                        onChange={() => setUser(user)}
                    />
                </Grid>
                <Grid item xs={12} sm={6}>
                    <TextField
                        id="pass"
                        name="pass"
                        variant="outlined"
                        label="Password"
                        type="text"
                        value={pass}
                        fullWidth
                        onChange={() => setPass(pass)}
                    />
                </Grid>
                <Grid item xs={12} sm={12}>
                    <TextField
                        id="database"
                        name="database"
                        variant="outlined"
                        label="Database"
                        type="text"
                        value={database}
                        fullWidth
                        onChange={() => setDatabase(database)}
                    />
                </Grid>
            </Grid>
        </div>
    );
}