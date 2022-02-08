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

function ListRoles() {
    const columns: GridColDef[] = [
        { field: 'name', headerName: 'Name', flex: 1 },
        {
            field: 'id',
            headerName: 'Discord ID',
            flex: 1,
            renderCell: (params) => BigInt(params.row.id).toString(),
        },
        {
            field: 'permissions',
            headerName: 'Permissions',
            flex: 1,
        },
        {
            field: 'moderator',
            headerName: 'Is Moderator',
            flex: 1,
            renderCell: (params) => params.row.moderator ? 'Yes' : 'No',
        },
        {
            field: 'action',
            headerName: 'Action',
            width: 100,
            flex: 1,
            renderCell: (params) => {
                return (
                    <ButtonGroup>
                        <IconButton color="primary" onClick={() => window.location.href = config.homepage + 'role/' + params.row.name}>
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

    const [roles, setRoles] = useState([]);
    useEffect(() => {
        refreshList();
    }, []);
    const refreshList = () => {
        fetch(config.apiUrl + 'admin/roles', {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*',
            },
        })
        .then(async (response) => await response.json())
        .then(data => {
            setRoles(data);
        }).catch(err => {
            console.error('error:', err);
            // TODO: Show error notification
        });
    };

    const confirmDelete = (id: number): void => {
        const result = window.confirm(`Are you sure you want to delete role ${id}?`);
        if (!result) {
            return;
        }
        // Send delete request
        fetch(config.apiUrl + 'admin/role/' + id, {
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
            setRoles(roles.filter((item: any) => item.id !== id));
        }).catch(err => {
            console.error('error:', err);
        });
    };

    const classes = useStyles();
    return (
        <div className={classes.container} style={{ height: 500, width: '100%' }}>
            <div className={classes.titleContainer}>
                <Typography variant="h4" component="h1" className={classes.title}>Discord Roles</Typography>
                <Link to={config.homepage + "role/new"} className="link">
                    <Button variant="contained" color="primary">New Discord Role</Button>
                </Link>
            </div>
            <p>
                Available Discord roles for subscribers and moderators.
            </p>
            <DataGrid className={classes.table}
                rows={roles}
                disableSelectionOnClick
                columns={columns}
                pageSize={25}
                checkboxSelection
            />
        </div>
    );
}

export default ListRoles;