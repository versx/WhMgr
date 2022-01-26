import React, { useEffect, useState } from 'react'
import {
    Button,
    ButtonGroup,
    Typography,
} from '@mui/material';
import { makeStyles } from '@mui/styles';

import config from '../config.json';


const useStyles = makeStyles((theme: any) => ({
    container: {
        //padding: theme.spacing(2),
        paddingTop: theme.spacing(10),
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

function EditConfig() {
    const [alarms, setAlarms] = useState([]);
    useEffect(() => {
        refreshList();
    }, []);
    const refreshList = () => {
        fetch(config.apiUrl + 'admin/alarms', {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*',
            },
        })
        .then(async (response) => await response.json())
        .then(data => {
            setAlarms(data);
        }).catch(err => {
            console.error('error:', err);
            // TODO: Show error notification
        });
    };

    const classes = useStyles();
    return (
        <div className={classes.container} style={{ height: 500, width: '100%' }}>
            <div className={classes.titleContainer}>
                <Typography variant="h4" component="h1" className={classes.title}>Channel Alarms</Typography>
            </div>
            <p>
                Channel alarms are pre-defined configs that specify what data to report to a Discord server's channel via webhooks.
            </p>

        </div>
    );
}

export default EditConfig;