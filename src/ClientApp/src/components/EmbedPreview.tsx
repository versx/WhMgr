import React, { useState } from 'react';
import {
    Avatar,
    Card,
    CardContent,
    CardHeader,
    Chip,
    Grid,
    IconButton,
    Typography,
} from '@mui/material';

interface EmbedPreviewProps {

}

function EmbedPreview(props: EmbedPreviewProps) {
    const [state, setState] = useState({
        avatarUrl: '{{pkmn_img_url}}',
        username: '{{form}} {{pkmn_name}}{{gender}}',
        iconUrl: '{{pkmn_img_url}}',
        title: '{{geofence}}',
        url: '{{gmaps_url}}',
        content: [
            '{{pkmn_name}} {{form}}{{gender}} {{iv}} ({{atk_iv}}/{{def_iv}}/{{sta_iv}}) L{{lvl}}',
            '**Despawn:** {{despawn_time}} ({{time_left}} left){{despawn_time_verified}}',
            '**Details:** CP: {{cp}} IV: {{iv}} LV: {{lvl}}',
            '**Size:** {{size}} | {{types_emoji}}{{#if has_weather}} | {{weather_emoji}}{{#if is_weather_boosted}} (Boosted){{/if}}{{/if}}',
            '**Moveset:** {{moveset}}',
            '{{#if near_pokestop}}**Near Pokestop:** [{{pokestop_name}}]({{pokestop_url}})',
            '{{/if}}{{#if is_ditto}}**Catch Pokemon:** {{original_pkmn_name}}',
            '{{/if}}{{#if has_capture_rates}}{{capture_1_emoji}} {{capture_1}}% {{capture_2_emoji}} {{capture_2}}% {{capture_3_emoji}} {{capture_3}}%',
            '{{/if}}{{#if is_event}}Go Fest Spawn',
            '{{/if}}{{#if is_pvp}}',
            '{{#if is_great}}{{great_league_emoji}}**Great League**',
            '{{#each great_league}}#{{rank}} {{pokemonName}} {{cp}}CP @ L{{level}} {{percentage}}%',
            '{{/each}}',
            '{{/if}}{{#if is_ultra}}{{ultra_league_emoji}}**Ultra League**',
            '{{#each ultra_league}}#{{rank}} {{pokemonName}} {{cp}}CP @ L{{level}} {{percentage}}%',
            '{{/each}}',
            '{{/if}}{{/if}}**[Google]({{gmaps_url}}) | [Apple]({{applemaps_url}}) | [Waze]({{wazemaps_url}}) | [Scanner]({{scanmaps_url}})**',
        ],
        imageUrl: '{{tilemaps_url}}',
        footerText: '{{guild_name}} {{date_time}}',
        footerIconUrl: '{{guild_img_url}}',
    });

    // TODO: Use handlebars for templating

    return (
        <Grid container spacing={2}>
            <Grid item xs={8}>
                <Typography>
                    Icon Name Gender [BotTag] Date
                </Typography>
                <Card style={{marginLeft: '35px'}}>
                    <CardHeader
                        title={state.title}
                    />
                    <CardContent>
                        <Grid container spacing={2}>
                            <Grid item xs={10}>
                                {state.content.map((x: any) => {
                                    return (
                                        // TODO: 
                                        <Typography>{x}</Typography>
                                    );
                                })}
                            </Grid>
                            <Grid item xs={2}>
                                Icon
                            </Grid>
                        </Grid>
                    </CardContent>
                </Card>
            </Grid>
        </Grid>
    );
}

export default EmbedPreview;