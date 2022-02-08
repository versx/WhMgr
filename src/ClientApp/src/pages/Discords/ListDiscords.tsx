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

function ListDiscords() {
    const columns: GridColDef[] = [
        { field: 'id', headerName: 'Name', flex: 1 },
        { field: 'alarms', headerName: 'Alarms', flex: 1 },
        { field: 'geofences', headerName: 'No. Geofences', flex: 1 },
        {
            field: 'subscriptions_enabled',
            headerName: 'Subscriptions Enabled',
            flex: 1,
            renderCell: (params) => params.row.subscriptions_enabled ? 'Yes' : 'No',
        },
        { field: 'embeds', headerName: 'Embeds', flex: 1 },
        { field: 'icon_style', headerName: 'Icon Style', flex: 1 },
        {
            field: 'action',
            headerName: 'Action',
            width: 100,
            flex: 1,
            renderCell: (params) => {
                return (
                    <ButtonGroup>
                        <IconButton color="primary" onClick={() => window.location.href = config.homepage + 'discord/' + params.row.id}>
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

    const [discords, setDiscords] = useState([]);
    useEffect(() => {
        refreshList();
    }, []);
    const refreshList = () => {
        fetch(config.apiUrl + 'admin/discords', {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*',
            },
        })
        .then(async (response) => await response.json())
        .then(data => {
            setDiscords(data);
        }).catch(err => {
            console.error('error:', err);
            // TODO: Show error notification
        });
    };

    const confirmDelete = (id: number): void => {
        const result = window.confirm(`Are you sure you want to delete discord ${id}?`);
        if (!result) {
            return;
        }
        // Send delete request
        fetch(config.apiUrl + 'admin/discord/' + id, {
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
            setDiscords(discords.filter((item: any) => item.id !== id));
        }).catch(err => {
            console.error('error:', err);
        });
    };

    const classes = useStyles();
    return (
        <div className={classes.container} style={{ height: 500, width: '100%' }}>
            <div className={classes.titleContainer}>
                <Typography variant="h4" component="h1" className={classes.title}>Discord Servers</Typography>
                <Link to={config.homepage + "discord/new"} className="link">
                    <Button variant="contained" color="primary">New Discord</Button>
                </Link>
            </div>
            <Typography style={{paddingBottom: '20px'}}>
                Discord server configs are used by <a href={config.homepage + "configs"} >Configs</a> to determine what Discord server to report and respond to.
            </Typography>
            <DataGrid className={classes.table}
                rows={discords}
                disableSelectionOnClick
                columns={columns}
                pageSize={10}
                checkboxSelection
            />
        </div>
    );
}

export default ListDiscords;