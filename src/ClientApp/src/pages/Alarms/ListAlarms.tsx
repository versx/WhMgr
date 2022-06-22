import React, { useEffect, useState } from 'react'
import { Link } from 'react-router-dom';
import {
    Button,
    ButtonGroup,
    IconButton,
    Typography,
} from '@mui/material';
import { makeStyles } from '@mui/styles';
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import {
    Delete as DeleteIcon,
    Edit as EditIcon,
} from '@mui/icons-material';

import config from '../../config.json';

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

function ListAlarms() {
    const columns: GridColDef[] = [
        { field: 'id', headerName: 'Name', flex: 1 },
        {
            field: 'enable_pokemon',
            headerName: 'Enable Pokemon',
            flex: 1,
            renderCell: (params) => params.row.enable_pokemon ? 'Yes' : 'No',
        },
        {
            field: 'enable_raids',
            headerName: 'Enable Raids',
            flex: 1,
            renderCell: (params) => params.row.enable_raids ? 'Yes' : 'No',
        },
        {
            field: 'enable_gyms',
            headerName: 'Enable Gyms',
            flex: 1,
            renderCell: (params) => params.row.enable_gyms ? 'Yes' : 'No',
        },
        {
            field: 'enable_quests',
            headerName: 'Enable Quests',
            flex: 1,
            renderCell: (params) => params.row.enable_quests ? 'Yes' : 'No',
        },
        {
            field: 'enable_pokestops',
            headerName: 'Enable Pokestops',
            flex: 1,
            renderCell: (params) => params.row.enable_pokestops ? 'Yes' : 'No',
        },
        {
            field: 'enable_invasions',
            headerName: 'Enable Invasions',
            flex: 1,
            renderCell: (params) => params.row.enable_invasions ? 'Yes' : 'No',
        },
        {
            field: 'enable_weather',
            headerName: 'Enable Weather',
            flex: 1,
            renderCell: (params) => params.row.enable_weather ? 'Yes' : 'No',
        },
        { field: 'count', headerName: 'Count', flex: 1 },
        {
            field: 'action',
            headerName: 'Action',
            width: 100,
            flex: 1,
            renderCell: (params) => {
                return (
                    <ButtonGroup>
                        <IconButton color="primary" onClick={() => window.location.href = config.homepage + 'alarm/' + params.row.id}>
                            <EditIcon />
                        </IconButton>
                        <IconButton color="error" onClick={() => confirmDelete(params.row.id)}>
                            <DeleteIcon />
                        </IconButton>
                    </ButtonGroup>
                );
            },
        },
    ];

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

    const confirmDelete = (id: number): void => {
        const result = window.confirm(`Are you sure you want to delete alarm ${id}?`);
        if (!result) {
            return;
        }
        // Send delete request
        fetch(config.apiUrl + 'admin/alarm/' + id, {
            method: 'DELETE',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*',
            },
        })
        .then(async (response) => await response.json())
        .then(data => {
            if (data.status !== 'OK') {
                // TODO: error
                alert(data.error);
                return;
            }
            // Update list on successful delete via api
            setAlarms(alarms.filter((item: any) => item.id !== id));
        }).catch(err => {
            console.error('error:', err);
        });
    };

    const classes = useStyles();
    return (
        <div className={classes.container}>
            <div className={classes.titleContainer}>
                <Typography variant="h4" component="h1" className={classes.title}>Channel Alarms</Typography>
                <Link to={config.homepage + "alarm/new"} className="link">
                    <Button variant="contained" color="primary">New Alarm</Button>
                </Link>
            </div>
            <Typography style={{paddingBottom: '20px'}}>
                Channel alarms are pre-defined configs that specify what data to report to a Discord server's channel via webhooks.
            </Typography>
            <DataGrid className={classes.table}
                rows={alarms}
                disableSelectionOnClick
                columns={columns}
                pageSize={25}
                checkboxSelection
            />
        </div>
    );
}

export default ListAlarms;