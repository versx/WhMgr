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

function ListFilters() {
    const columns: GridColDef[] = [
        { field: 'id', headerName: 'Name', flex: 1 },
        {
            field: 'pokemon',
            headerName: 'Pokemon',
            flex: 1,
            renderCell: (params) => params.row.pokemon ? 'Yes' : 'No',
        },
        {
            field: 'raids',
            headerName: 'Raids',
            flex: 1,
            renderCell: (params) => params.row.raids ? 'Yes' : 'No',
        },
        {
            field: 'gyms',
            headerName: 'Gyms',
            flex: 1,
            renderCell: (params) => params.row.gyms ? 'Yes' : 'No',
        },
        {
            field: 'quests',
            headerName: 'Quests',
            flex: 1,
            renderCell: (params) => params.row.quests ? 'Yes' : 'No',
        },
        {
            field: 'pokestops',
            headerName: 'Pokestops',
            flex: 1,
            renderCell: (params) => params.row.pokestops ? 'Yes' : 'No',
        },
        {
            field: 'weather',
            headerName: 'Weather',
            flex: 1,
            renderCell: (params) => params.row.weather ? 'Yes' : 'No',
        },
        {
            field: 'action',
            headerName: 'Action',
            width: 100,
            flex: 1,
            renderCell: (params) => {
                return (
                    <ButtonGroup>
                        <IconButton color="primary" onClick={() => window.location.href = '/dashboard/filter/' + params.row.id}>
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

    const [filters, setFilters] = useState([]);
    useEffect(() => {
        refreshList();
    }, []);
    const refreshList = () => {
        fetch(config.apiUrl + 'admin/filters', {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*',
            },
        })
        .then(async (response) => await response.json())
        .then(data => {
            setFilters(data);
        }).catch(err => {
            console.error('error:', err);
            // TODO: Show error notification
        });
    };

    const confirmDelete = (id: number): void => {
        const result = window.confirm(`Are you sure you want to delete filter ${id}?`);
        if (!result) {
            return;
        }
        // TODO: Send delete request
        console.log('delete:', filters);
        setFilters(filters.filter((item: any) => item.id !== id));
    };

    const classes = useStyles();
    return (
        <div className={classes.container} style={{ height: 500, width: '100%' }}>
            <div className={classes.titleContainer}>
                <Typography variant="h4" component="h1" className={classes.title}>Alarm Filters</Typography>
                <Link to="/dashboard/filter/new" className="link">
                    <Button variant="contained" color="primary">New Filter</Button>
                </Link>
            </div>
            <p>
                Webhook alarm filters are used by <a href="/dashboard/alarms">Channel Alarms</a> to filter incoming webhook data based on type and other various properties.
            </p>
            <DataGrid className={classes.table}
                rows={filters}
                disableSelectionOnClick
                columns={columns}
                pageSize={25}
                checkboxSelection
            />
        </div>
    );
}

export default ListFilters;