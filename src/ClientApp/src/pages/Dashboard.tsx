import React, { useEffect, useState } from 'react'
import { Link } from 'react-router-dom';
import {
    Box,
    Button,
    ButtonGroup,
    Card,
    CardContent,
    Grid,
    Paper,
    Typography,
} from '@mui/material';
import {
    PlayArrow as PlayArrowIcon,
} from '@mui/icons-material';
import { makeStyles } from '@mui/styles';

import config from '../config.json';

const useStyles = makeStyles((theme: any) => ({
    container: {
        //padding: theme.spacing(2),
        paddingTop: theme.spacing(10),
        //marginRight: theme.spacing(-6),
        //paddingLeft: theme.spacing(2),
        height: '80%',
        width: '100%',
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
}));

function Dashboard() {
    const [data, setData] = useState([]);
    useEffect(() => {
        refreshList();
    }, []);
    const refreshList = () => {
        fetch(config.apiUrl + 'admin/dashboard', {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*',
            },
        })
        .then(async (response) => await response.json())
        .then(data => {
            setData(data);
        }).catch(err => {
            console.error('error:', err);
            // TODO: Show error notification
        });
    };

    const classes = useStyles();
    return (
        <div className={classes.container}>
            <div className={classes.titleContainer}>
                <Typography variant="h4" component="h1" className={classes.title}>Dashboard</Typography>
            </div>
            <Card sx={{ display: 'flex' }}>
                <Grid container spacing={2}>
                {data.map((item: any) => {
                    return (
                        <Grid key={item.name} item xs={12} sm={4}>
                            <Paper elevation={2} sx={{alignItems: 'center', color: 'secondary', height: '60', lineHeight: '60px'}}>
                                <Box key={item.name} sx={{ display: 'flex', flexDirection: 'column' }}>
                                    <Box sx={{ display: 'flex', alignItems: 'center', pl: 1, pb: 1, flex: 1 }}>
                                        <PlayArrowIcon />
                                    </Box>
                                    <CardContent sx={{ flex: '1 0 auto' }}>
                                        <Typography component="div" variant="h5">
                                            <Link to={config.homepage + item.name.toLowerCase()} style={{color: 'inherit', textDecoration: 'none'}}>
                                                {item.name} {item.count}
                                            </Link>
                                        </Typography>
                                    </CardContent>
                                </Box>
                            </Paper>
                        </Grid>
                    );
                })}
                </Grid>
            </Card>
        </div>
    );
}

export default Dashboard;