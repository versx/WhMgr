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

function ListGeofences() {
    const columns: GridColDef[] = [
        { field: 'id', headerName: 'Name', flex: 1 },
        {
            field: 'action',
            headerName: 'Action',
            width: 100,
            flex: 1,
            renderCell: (params) => {
                return (
                    <ButtonGroup>
                        <IconButton color="primary" onClick={() => window.location.href = config.homepage + 'geofence/' + params.row.id}>
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

    const [geofences, setGeofences] = useState([]);
    useEffect(() => {
        refreshList();
    }, []);
    const refreshList = () => {
        fetch(config.apiUrl + 'admin/geofences', {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*',
            },
        })
        .then(async (response) => await response.json())
        .then(data => {
            setGeofences(data);
        }).catch(err => {
            console.error('error:', err);
            // TODO: Show error notification
        });
    };

    const confirmDelete = (id: number): void => {
        const result = window.confirm(`Are you sure you want to delete geofence ${id}?`);
        if (!result) {
            return;
        }
        // Send delete request
        fetch(config.apiUrl + 'admin/geofence/' + id, {
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
            setGeofences(geofences.filter((item: any) => item.id !== id));
        }).catch(err => {
            console.error('error:', err);
        });
    };

    const classes = useStyles();
    return (
        <div className={classes.container} style={{ height: 500, width: '100%' }}>
            <div className={classes.titleContainer}>
                <Typography variant="h4" component="h1" className={classes.title}>Geofences</Typography>
                <Link to={config.homepage + "geofence/new"} className="link">
                    <Button variant="contained" color="primary">New Geofence</Button>
                </Link>
            </div>
            <Typography style={{paddingBottom: '20px'}}>
                Geofences define a scan areas borders.
            </Typography>
            <DataGrid className={classes.table}
                rows={geofences}
                disableSelectionOnClick
                columns={columns}
                pageSize={25}
                checkboxSelection
            />
        </div>
    );
}

export default ListGeofences;