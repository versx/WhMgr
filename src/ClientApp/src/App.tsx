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
import ListGeofences from './pages/Geofences/ListGeofences';
import ListRoles from './pages/Roles/ListRoles';
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

const basePath = "/myapp/"; // TODO: Rename to `/dashboard/` when conversion is done

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
                <Route path={basePath} element={<Dashboard />} />
                <Route path={basePath + "configs"} element={<ListConfigs />} />
                <Route path={basePath + "config/new"} element={<ListConfigs />} />
                <Route path={basePath + "config/:id"} element={<EditConfig props={{}} />} />
                <Route path={basePath + "discords"} element={<ListDiscords />} />
                <Route path={basePath + "discord/:id"} element={<EditDiscord props={{}} />} />
                <Route path={basePath + "alarms"} element={<ListAlarms />} />
                <Route path={basePath + "alarm/:id"} element={<EditAlarm props={{}} />} />
                <Route path={basePath + "filters"} element={<ListFilters />} />
                <Route path={basePath + "filter/:id"} element={<EditFilter props={{}} />} />
                <Route path={basePath + "embeds"} element={<ListEmbeds />} />
                <Route path={basePath + "embed/:id"} element={<EditEmbed props={{}} />} />
                <Route path={basePath + "geofences"} element={<ListGeofences />} />
                <Route path={basePath + "roles"} element={<ListRoles />} />
                <Route path={basePath + "subscriptions"} element={<ListConfigs />} />
                <Route path={basePath + "users"} element={<ListUsers />} />
                <Route path={basePath + "settings"} element={<Settings />} />
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
