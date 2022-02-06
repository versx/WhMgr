import React from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { makeStyles } from '@mui/styles';
import { Grid } from '@mui/material';

import './App.css';

import {
  Navbar,
  Leftbar,
  Rightbar,
} from './components/Nav';

import config from './config.json';
import Dashboard from './pages/Dashboard';
import { ListConfigs, EditConfig } from './pages/Configs';
import { ListDiscords, EditDiscord } from './pages/Discords';
import { ListAlarms, EditAlarm } from './pages/Alarms';
import { ListFilters, EditFilter } from './pages/Filters';
import { ListEmbeds, EditEmbed } from './pages/Embeds';
import { ListGeofences, EditGeofence } from './pages/Geofences';
import { ListRoles, NewRole, EditRole } from './pages/Roles';
import ListUsers from './pages/ListUsers';
import Settings from './pages/Settings';

const useStyles = makeStyles((theme: any) => ({
  right: {
    display: 'none', // always hide
    [theme.breakpoints.down('sm')]: {
      display: 'none',
    },
  },
}));

function App() {
  const classes = useStyles();
  return (
    <div>
      <Navbar />
      <BrowserRouter>
        <Grid container>
          <Grid item sm={2} xs={2}>
            <Leftbar />
          </Grid>
          <Grid item sm={10} xs={10}> {/* 7 */}
              <Routes>
                <Route path={config.homepage} element={<Dashboard />} />
                <Route path={config.homepage + "configs"} element={<ListConfigs />} />
                <Route path={config.homepage + "config/new"} element={<ListConfigs />} />
                <Route path={config.homepage + "config/:id"} element={<EditConfig props={{}} />} />
                <Route path={config.homepage + "discords"} element={<ListDiscords />} />
                <Route path={config.homepage + "discord/new"} element={<ListDiscords />} />
                <Route path={config.homepage + "discord/:id"} element={<EditDiscord props={{}} />} />
                <Route path={config.homepage + "alarms"} element={<ListAlarms />} />
                <Route path={config.homepage + "alarm/new"} element={<ListAlarms />} />
                <Route path={config.homepage + "alarm/:id"} element={<EditAlarm props={{}} />} />
                <Route path={config.homepage + "filters"} element={<ListFilters />} />
                <Route path={config.homepage + "filter/new"} element={<ListFilters />} />
                <Route path={config.homepage + "filter/:id"} element={<EditFilter props={{}} />} />
                <Route path={config.homepage + "embeds"} element={<ListEmbeds />} />
                <Route path={config.homepage + "embed/new"} element={<ListEmbeds />} />
                <Route path={config.homepage + "embed/:id"} element={<EditEmbed props={{}} />} />
                <Route path={config.homepage + "geofences"} element={<ListGeofences />} />
                <Route path={config.homepage + "geofence/new"} element={<ListGeofences />} />
                <Route path={config.homepage + "geofence/:id"} element={<EditGeofence props={{}} />} />
                <Route path={config.homepage + "roles"} element={<ListRoles />} />
                <Route path={config.homepage + "role/new"} element={<NewRole />} />
                <Route path={config.homepage + "role/:id"} element={<EditRole props={{}} />} />
                <Route path={config.homepage + "subscriptions"} element={<ListConfigs />} />
                <Route path={config.homepage + "users"} element={<ListUsers />} />
                <Route path={config.homepage + "settings"} element={<Settings />} />
              </Routes>
          </Grid>
          <Grid item sm={3} className={classes.right}>
            <Rightbar />
          </Grid>
        </Grid>
      </BrowserRouter>
    </div>
  );
}

export default App;