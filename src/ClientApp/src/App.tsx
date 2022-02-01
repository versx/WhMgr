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

import Dashboard from './pages/Dashboard';
import { ListConfigs, EditConfig } from './pages/Configs';
import { ListDiscords, EditDiscord } from './pages/Discords';
import { ListAlarms, EditAlarm } from './pages/Alarms';
import { ListFilters, EditFilter } from './pages/Filters';
import { ListEmbeds, EditEmbed } from './pages/Embeds';
import { ListGeofences, EditGeofence } from './pages/Geofences';
import { ListRoles, EditRole } from './pages/Roles';
import ListUsers from './pages/ListUsers';
import Settings from './pages/Settings';

const { homepage } = require('../package.json');

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
                <Route path={homepage} element={<Dashboard />} />
                <Route path={homepage + "configs"} element={<ListConfigs />} />
                <Route path={homepage + "config/new"} element={<ListConfigs />} />
                <Route path={homepage + "config/:id"} element={<EditConfig props={{}} />} />
                <Route path={homepage + "discords"} element={<ListDiscords />} />
                <Route path={homepage + "discord/new"} element={<ListDiscords />} />
                <Route path={homepage + "discord/:id"} element={<EditDiscord props={{}} />} />
                <Route path={homepage + "alarms"} element={<ListAlarms />} />
                <Route path={homepage + "alarm/new"} element={<ListAlarms />} />
                <Route path={homepage + "alarm/:id"} element={<EditAlarm props={{}} />} />
                <Route path={homepage + "filters"} element={<ListFilters />} />
                <Route path={homepage + "filter/new"} element={<ListFilters />} />
                <Route path={homepage + "filter/:id"} element={<EditFilter props={{}} />} />
                <Route path={homepage + "embeds"} element={<ListEmbeds />} />
                <Route path={homepage + "embed/new"} element={<ListEmbeds />} />
                <Route path={homepage + "embed/:id"} element={<EditEmbed props={{}} />} />
                <Route path={homepage + "geofences"} element={<ListGeofences />} />
                <Route path={homepage + "geofence/new"} element={<ListGeofences />} />
                <Route path={homepage + "geofence/:id"} element={<EditGeofence props={{}} />} />
                <Route path={homepage + "roles"} element={<ListRoles />} />
                <Route path={homepage + "role/new"} element={<ListRoles />} />
                <Route path={homepage + "role/:id"} element={<EditRole props={{}} />} />
                <Route path={homepage + "subscriptions"} element={<ListConfigs />} />
                <Route path={homepage + "users"} element={<ListUsers />} />
                <Route path={homepage + "settings"} element={<Settings />} />
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