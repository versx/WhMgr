import React, { useState } from 'react'
import {
    AppBar,
    Avatar,
    Badge,
    MenuItem,
    TextField,
    Toolbar,
    Typography,
} from '@mui/material';
import {
    Camera as CameraIcon,
    Notifications,
} from '@mui/icons-material';
import { makeStyles } from '@mui/styles';

const useStyles = makeStyles((theme: any) => ({
    toolbar: {
        display: 'flex',
        justifyContent: 'space-between',
    },
    logoLg: {
        display: 'none',
        [theme.breakpoints.up('sm')]: {
            display: 'block',
        },
    },
    logoSm: {
        display: 'block',
        alignItems: 'center',
        [theme.breakpoints.up('sm')]: {
            display: 'none',
        },
    },
    input: {
        color: 'white',
        marginLeft: theme.spacing(1),
    },
    cancel: {
        [theme.breakpoints.up('sm')]: {
            display: 'none',
        },
    },
    icons: {
        alignItems: 'center',
        display: (props: any) => props.open ? 'none' : 'flex',
    },
    badge: {
        marginRight: theme.spacing(2),
    },
    logoIcon: {
        display: 'flex',
        marginRight: '10px',
        objectFit: 'cover',
        justifyContent: 'space-between',
        alignItems: 'center',
    },
}));

function Navbar() {
    const classes = useStyles({});
    return (
        <AppBar position="fixed">
            <Toolbar className={classes.toolbar}>
                <div style={{display: 'flex'}}>
                    <CameraIcon className={classes.logoIcon} />
                    <Typography variant="h6" className={classes.logoLg}>
                        Webhook Manager Admin Dashboard
                    </Typography>
                    <Typography variant="h6" className={classes.logoSm}>
                        WhMgr Admin
                    </Typography>
                </div>
                <div className={classes.icons}>
                    <Badge badgeContent={2} color="secondary" className={classes.badge}>
                        <Notifications style={{cursor: 'pointer'}} />
                    </Badge>
                    <Avatar
                        alt="Remy Sharp"
                        src="https://www.itdp.org/wp-content/uploads/2021/06/avatar-man-icon-profile-placeholder-260nw-1229859850-e1623694994111.jpg"
                        style={{cursor: 'pointer'}}
                    />
                </div>
            </Toolbar>
        </AppBar>
    );
}

export default Navbar;