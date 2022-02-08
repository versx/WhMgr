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

function ListEmbeds() {
    const columns: GridColDef[] = [
        { field: 'id', headerName: 'Name', flex: 1 },
        {
            field: 'pokemon',
            headerName: 'Pokemon',
            flex: 1,
            renderCell: (params) => params.row.pokemon ? 'Yes' : 'No'
        },
        {
            field: 'pokemon_missing_stats',
            headerName: 'Pokemon w/o Stats',
            flex: 1,
            renderCell: (params) => params.row.pokemon_missing_stats ? 'Yes' : 'No'
        },
        {
            field: 'raids',
            headerName: 'Raids',
            flex: 1,
            renderCell: (params) => params.row.raids ? 'Yes' : 'No'
        },
        {
            field: 'gyms',
            headerName: 'Gyms',
            flex: 1,
            renderCell: (params) => params.row.gyms ? 'Yes' : 'No'
        },
        {
            field: 'pokestops',
            headerName: 'Pokestops',
            flex: 1,
            renderCell: (params) => params.row.pokestops ? 'Yes' : 'No'
        },
        {
            field: 'quests',
            headerName: 'Quests',
            flex: 1,
            renderCell: (params) => params.row.quests ? 'Yes' : 'No'
        },
        {
            field: 'lures',
            headerName: 'Lures',
            flex: 1,
            renderCell: (params) => params.row.lures ? 'Yes' : 'No'
        },
        {
            field: 'invasions',
            headerName: 'Invasions',
            flex: 1,
            renderCell: (params) => params.row.invasions ? 'Yes' : 'No'
        },
        {
            field: 'nests',
            headerName: 'Nests',
            flex: 1,
            renderCell: (params) => params.row.nests ? 'Yes' : 'No'
        },
        {
            field: 'weather',
            headerName: 'Weather',
            flex: 1,
            renderCell: (params) => params.row.weather ? 'Yes' : 'No'
        },
        {
            field: 'action',
            headerName: 'Action',
            width: 100,
            flex: 1,
            renderCell: (params) => {
                return (
                    <ButtonGroup>
                        <IconButton color="primary" onClick={() => window.location.href = config.homepage + 'embed/' + params.row.id}>
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

    const [embeds, setEmbeds] = useState([]);
    useEffect(() => {
        refreshList();
    }, []);
    const refreshList = () => {
        fetch(config.apiUrl + 'admin/embeds', {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*',
            },
        })
        .then(async (response) => await response.json())
        .then(data => {
            setEmbeds(data);
        }).catch(err => {
            console.error('error:', err);
            // TODO: Show error notification
        });
    };

    const confirmDelete = (id: number): void => {
        const result = window.confirm(`Are you sure you want to delete embed ${id}?`);
        if (!result) {
            return;
        }
        // Send delete request
        fetch(config.apiUrl + 'admin/embed/' + id, {
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
            setEmbeds(embeds.filter((item: any) => item.id !== id));
        }).catch(err => {
            console.error('error:', err);
        });
    };

    const classes = useStyles();
    return (
        <div className={classes.container} style={{ height: 500, width: '100%' }}>
            <div className={classes.titleContainer}>
                <Typography variant="h4" component="h1" className={classes.title}>Embeds</Typography>
                <Link to={config.homepage + "embed/new"} className="link">
                    <Button variant="contained" color="primary">New Embed</Button>
                </Link>
            </div>
            <Typography style={{paddingBottom: '20px'}}>
                Message embeds depict how the Discord message looks to your users. Place holders are replaced with actual values at runtime using the <a href="https://handlebarsjs.com/guide" target="_blank">Handlebars</a> templating engine.
            </Typography>
            <DataGrid className={classes.table}
                rows={embeds}
                disableSelectionOnClick
                columns={columns}
                pageSize={10}
                checkboxSelection
            />
        </div>
    );
}

export default ListEmbeds;