import React, { useState } from 'react'
import {
    Accordion,
    AccordionDetails,
    AccordionSummary,
    Box,
    Button,
    Container,
    Grid,
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    TextField,
    Typography,
} from '@mui/material';
import {
    ExpandMore as ExpandMoreIcon,
} from '@mui/icons-material';
import { makeStyles } from '@mui/styles';

import config from '../../config.json';
import { BreadCrumbs } from '../../components/BreadCrumbs';
import EmbedPreview from '../../components/EmbedPreview';
import withRouter from '../../hooks/WithRouter';
import { IGlobalProps } from '../../interfaces/IGlobalProps';
import { onNestedStateChange } from '../../utils/nestedStateHelper';

// TODO: Reusable embed components (pass onInputChange via props)
// TODO: Use chips instead of text to auto input placeholders
// TODO: Add Discord Embed preview

class NewEmbed extends React.Component<IGlobalProps> {
    public state: any;

    constructor(props: IGlobalProps) {
        super(props);
        console.log('props:', props);
        this.state = {
            // TODO: Set default state values
            name: props.params!.id,
            placeholders: {},
            Pokemon: {
                avatarUrl: '',
                content: [],
                iconUrl: '',
                title: '',
                url: '',
                username: '',
                imageUrl: '',
                footer: {
                    text: '',
                    iconUrl: '',
                },
            },
            PokemonMissingStats: {
                avatarUrl: '',
                content: [],
                iconUrl: '',
                title: '',
                url: '',
                username: '',
                imageUrl: '',
                footer: {
                    text: '',
                    iconUrl: '',
                },
            },
            Gyms: {
                avatarUrl: '',
                content: [],
                iconUrl: '',
                title: '',
                url: '',
                username: '',
                imageUrl: '',
                footer: {
                    text: '',
                    iconUrl: '',
                },
            },
            Raids: {
                avatarUrl: '',
                content: [],
                iconUrl: '',
                title: '',
                url: '',
                username: '',
                imageUrl: '',
                footer: {
                    text: '',
                    iconUrl: '',
                },
            },
            Eggs: {
                avatarUrl: '',
                content: [],
                iconUrl: '',
                title: '',
                url: '',
                username: '',
                imageUrl: '',
                footer: {
                    text: '',
                    iconUrl: '',
                },
            },
            Pokestops: {
                avatarUrl: '',
                content: [],
                iconUrl: '',
                title: '',
                url: '',
                username: '',
                imageUrl: '',
                footer: {
                    text: '',
                    iconUrl: '',
                },
            },
            Quests: {
                avatarUrl: '',
                content: [],
                iconUrl: '',
                title: '',
                url: '',
                username: '',
                imageUrl: '',
                footer: {
                    text: '',
                    iconUrl: '',
                },
            },
            Lures: {
                avatarUrl: '',
                content: [],
                iconUrl: '',
                title: '',
                url: '',
                username: '',
                imageUrl: '',
                footer: {
                    text: '',
                    iconUrl: '',
                },
            },
            Invasions: {
                avatarUrl: '',
                content: [],
                iconUrl: '',
                title: '',
                url: '',
                username: '',
                imageUrl: '',
                footer: {
                    text: '',
                    iconUrl: '',
                },
            },
            Nests: {
                avatarUrl: '',
                content: [],
                iconUrl: '',
                title: '',
                url: '',
                username: '',
                imageUrl: '',
                footer: {
                    text: '',
                    iconUrl: '',
                },
            },
            Weather: {
                avatarUrl: '',
                content: [],
                iconUrl: '',
                title: '',
                url: '',
                username: '',
                imageUrl: '',
                footer: {
                    text: '',
                    iconUrl: '',
                },
            },
        };
        this.onInputChange = this.onInputChange.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
        this.handlePanelExpanded = this.handlePanelExpanded.bind(this);
    }

    onInputChange(event: any) {
        onNestedStateChange(event, this);
    }

    handlePanelExpanded = (panel: string) => (event: React.SyntheticEvent, isExpanded: boolean) => {
        this.setState({ ['expanded']: isExpanded ? panel : false });
    }

    handleSubmit(event: React.FormEvent<HTMLFormElement>) {
        event.preventDefault();

        console.log('handle submit state:', this.state);

        const data = {
            name: this.state.name,
            embed: {
                pokemon: this.state.Pokemon,
                pokemonMissingStats: this.state.PokemonMissingStats,
                gyms: this.state.Gyms,
                raids: this.state.Raids,
                eggs: this.state.Eggs,
                pokestops: this.state.Pokestops,
                quests: this.state.Quests,
                lures: this.state.Lures,
                invasions: this.state.Invasions,
                nests: this.state.Nests,
                weather: this.state.Weather,
            },
        };
        fetch(config.apiUrl + 'admin/embed/new', {
            method: 'POST',
            body: JSON.stringify(data),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
            },
        }).then(async (response) => await response.json())
          .then((data: any) => {
            //console.log('response:', data);
            if (data.status !== 'OK') {
                // TODO: Show error notification
                alert(data.error);
                return;
            }
            window.location.href = config.homepage + 'embeds';
        }).catch((err) => {
            console.error('error:', err);
            event.preventDefault();
        });
    }

    render() {
        const handleCancel = () => window.location.href = config.homepage + 'embeds';
        const formatContent = (key: string) => {
            const content = this.state[key].content;
            const array = typeof content === 'string'
                ? [content]
                : (content ?? []);
            const result = array.join('\n');
            return result;
        };

        const classes: any = makeStyles({
            container: {
                 //paddingTop: theme.spacing(10),
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
            buttonContainer: {
                paddingTop: '20px',
            },
        });

        const breadcrumbs = [{
            text: 'Dashboard',
            color: 'inherit',
            href: config.homepage,
            selected: false,
        }, {
            text: 'Embeds',
            color: 'inherit',
            href: config.homepage + 'embeds',
            selected: false,
        }, {
            text: 'New',
            color: 'primary',
            href: '',
            selected: true,
        }];

        return (
            <div className={classes.container} style={{paddingTop: '50px', paddingBottom: '20px', paddingLeft: '20px', paddingRight: '20px'}}>
                <Box component="form" method="POST" action=""  onSubmit={this.handleSubmit} sx={{ mt: 3 }}>
                    <BreadCrumbs crumbs={breadcrumbs} />
                    <Typography variant="h5" component="h2" >
                        New Embed Message Template
                    </Typography>
                    <Typography sx={{ mt: 2 }}>
                        Use <code>{"{{placeholder}}"}</code> surrounding an available placeholder value to replace it with actual data at runtime.<br />
                        Use <code>{"{{#if placeholder}}Show if true!{{/if}}"}</code> to handle conditional expressions that return a <code>Boolean</code> type.<br />
                        Use <code>{"{{#each rankings}}{{rank}} {{cp}} {{pokemon}}{{/each}}"}</code> to iterate and handle displaying <code>Array</code> values.<br />
                        <a href="https://handlebarsjs.com/guide" target="_blank">Handlebars Documentation</a><br /><br />
                        <i>Each new line in the content field reflects an actual new line in the message embed.</i>
                    </Typography>
                    <div style={{paddingBottom: '20px', paddingTop: '20px'}}>
                        <TextField
                            id="name"
                            name="name"
                            variant="outlined"
                            label="Name"
                            type="text"
                            value={this.state.name}
                            fullWidth
                            required
                            onChange={this.onInputChange}
                            style={{paddingBottom: '20px'}}
                        />
                        <Accordion expanded={this.state.expanded === 'panel1'} onChange={this.handlePanelExpanded('panel1')}>
                            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                <Typography>Pokemon</Typography>
                            </AccordionSummary>
                            <AccordionDetails>
                                {/*<EmbedPreview />*/}
                                <Grid container spacing={2}>
                                    <Grid item xs={6}>
                                        <Grid container spacing={2}>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Pokemon.avatarUrl"
                                                    name="Pokemon.avatarUrl"
                                                    variant="outlined"
                                                    label="Avatar Url"
                                                    type="text"
                                                    value={this.state.Pokemon.avatarUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Pokemon.username"
                                                    name="Pokemon.username"
                                                    variant="outlined"
                                                    label="Username"
                                                    type="text"
                                                    value={this.state.Pokemon.username}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Pokemon.iconUrl"
                                                    name="Pokemon.iconUrl"
                                                    variant="outlined"
                                                    label="Icon Url"
                                                    type="text"
                                                    value={this.state.Pokemon.iconUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Pokemon.title"
                                                    name="Pokemon.title"
                                                    variant="outlined"
                                                    label="Title"
                                                    type="text"
                                                    value={this.state.Pokemon.title}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Pokemon.url"
                                                    name="Pokemon.url"
                                                    variant="outlined"
                                                    label="Url"
                                                    type="text"
                                                    value={this.state.Pokemon.url}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Pokemon.content"
                                                    name="Pokemon.content"
                                                    variant="outlined"
                                                    label="Content"
                                                    type="text"
                                                    value={formatContent('Pokemon')}
                                                    multiline
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Pokemon.imageUrl"
                                                    name="Pokemon.imageUrl"
                                                    variant="outlined"
                                                    label="Image Url"
                                                    type="text"
                                                    value={this.state.Pokemon.imageUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Pokemon.footer.text"
                                                    name="Pokemon.footer.text"
                                                    variant="outlined"
                                                    label="Footer Text"
                                                    type="text"
                                                    value={this.state.Pokemon.footer.text}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Pokemon.footer.iconUrl"
                                                    name="Pokemon.footer.iconUrl"
                                                    variant="outlined"
                                                    label="Footer Icon Url"
                                                    type="text"
                                                    value={this.state.Pokemon.footer.iconUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                        </Grid>
                                    </Grid>
                                    <Grid item xs={6}>
                                        <TableContainer component={Paper}>
                                            <Table sx={{ minWidth: 650 }} size="small" aria-label="">
                                                <TableHead>
                                                    <TableRow>
                                                        <TableCell>Placeholder</TableCell>
                                                        <TableCell align="right">Description</TableCell>
                                                        <TableCell align="right">Example</TableCell>
                                                        <TableCell align="right">Type</TableCell>
                                                    </TableRow>
                                                </TableHead>
                                                <TableBody>
                                                    {this.state.placeholders.pokemon && this.state.placeholders.pokemon.map((placeholder: any) => {
                                                        return (
                                                            <TableRow
                                                                key={placeholder.placeholder}
                                                                sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                                                            >
                                                                <TableCell component="th" scope="row">
                                                                    <code>{placeholder.placeholder}</code>
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.description}
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.example}
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.type}
                                                                </TableCell>
                                                            </TableRow>
                                                        );
                                                    })}
                                                </TableBody>
                                            </Table>
                                        </TableContainer>
                                    </Grid>
                                </Grid>
                            </AccordionDetails>
                        </Accordion>
                        <Accordion expanded={this.state.expanded === 'panel2'} onChange={this.handlePanelExpanded('panel2')}>
                            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                <Typography>Pokemon Missing IV Stats</Typography>
                            </AccordionSummary>
                            <AccordionDetails>
                                <Grid container spacing={2}>
                                    <Grid item xs={6}>
                                        <Grid container spacing={2}>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="PokemonMissingStats.avatarUrl"
                                                    name="PokemonMissingStats.avatarUrl"
                                                    variant="outlined"
                                                    label="Avatar Url"
                                                    type="text"
                                                    value={this.state.PokemonMissingStats.avatarUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="PokemonMissingStats.username"
                                                    name="PokemonMissingStats.username"
                                                    variant="outlined"
                                                    label="Username"
                                                    type="text"
                                                    value={this.state.PokemonMissingStats.username}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="PokemonMissingStats.iconUrl"
                                                    name="PokemonMissingStats.iconUrl"
                                                    variant="outlined"
                                                    label="Icon Url"
                                                    type="text"
                                                    value={this.state.PokemonMissingStats.iconUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="PokemonMissingStats.title"
                                                    name="PokemonMissingStats.title"
                                                    variant="outlined"
                                                    label="Title"
                                                    type="text"
                                                    value={this.state.PokemonMissingStats.title}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="PokemonMissingStats.url"
                                                    name="PokemonMissingStats.url"
                                                    variant="outlined"
                                                    label="Url"
                                                    type="text"
                                                    value={this.state.PokemonMissingStats.url}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="PokemonMissingStats.content"
                                                    name="PokemonMissingStats.content"
                                                    variant="outlined"
                                                    label="Content"
                                                    type="text"
                                                    value={formatContent('PokemonMissingStats')}
                                                    multiline
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="PokemonMissingStats.imageUrl"
                                                    name="PokemonMissingStats.imageUrl"
                                                    variant="outlined"
                                                    label="Image Url"
                                                    type="text"
                                                    value={this.state.PokemonMissingStats.imageUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="PokemonMissingStats.footer.text"
                                                    name="PokemonMissingStats.footer.text"
                                                    variant="outlined"
                                                    label="Footer Text"
                                                    type="text"
                                                    value={this.state.PokemonMissingStats.footer.text}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="PokemonMissingStats.footer.iconUrl"
                                                    name="PokemonMissingStats.footer.iconUrl"
                                                    variant="outlined"
                                                    label="Footer Icon Url"
                                                    type="text"
                                                    value={this.state.PokemonMissingStats.footer.iconUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                        </Grid>
                                    </Grid>
                                    <Grid item xs={6}>
                                        <TableContainer component={Paper}>
                                            <Table sx={{ minWidth: 650 }} size="small" aria-label="">
                                                <TableHead>
                                                    <TableRow>
                                                        <TableCell>Placeholder</TableCell>
                                                        <TableCell align="right">Description</TableCell>
                                                        <TableCell align="right">Example</TableCell>
                                                        <TableCell align="right">Type</TableCell>
                                                    </TableRow>
                                                </TableHead>
                                                <TableBody>
                                                    {this.state.placeholders.pokemon && this.state.placeholders.pokemon.map((placeholder: any) => {
                                                        return (
                                                            <TableRow
                                                                key={placeholder.placeholder}
                                                                sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                                                            >
                                                                <TableCell component="th" scope="row">
                                                                    <code>{placeholder.placeholder}</code>
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.description}
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.example}
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.type}
                                                                </TableCell>
                                                            </TableRow>
                                                        );
                                                    })}
                                                </TableBody>
                                            </Table>
                                        </TableContainer>
                                    </Grid>
                                </Grid>
                            </AccordionDetails>
                        </Accordion>
                        <Accordion expanded={this.state.expanded === 'panel3'} onChange={this.handlePanelExpanded('panel3')}>
                            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                <Typography>Raids</Typography>
                            </AccordionSummary>
                            <AccordionDetails>
                                <Grid container spacing={2}>
                                    <Grid item xs={6}>
                                        <Grid container spacing={2}>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Raids.avatarUrl"
                                                    name="Raids.avatarUrl"
                                                    variant="outlined"
                                                    label="Avatar Url"
                                                    type="text"
                                                    value={this.state.Raids.avatarUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Raids.username"
                                                    name="Raids.username"
                                                    variant="outlined"
                                                    label="Username"
                                                    type="text"
                                                    value={this.state.Raids.username}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Raids.iconUrl"
                                                    name="Raids.iconUrl"
                                                    variant="outlined"
                                                    label="Icon Url"
                                                    type="text"
                                                    value={this.state.Raids.iconUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Raids.title"
                                                    name="Raids.title"
                                                    variant="outlined"
                                                    label="Title"
                                                    type="text"
                                                    value={this.state.Raids.title}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Raids.url"
                                                    name="Raids.url"
                                                    variant="outlined"
                                                    label="Url"
                                                    type="text"
                                                    value={this.state.Raids.url}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Raids.content"
                                                    name="Raids.content"
                                                    variant="outlined"
                                                    label="Content"
                                                    type="text"
                                                    value={formatContent('Raids')}
                                                    multiline
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Raids.imageUrl"
                                                    name="Raids.imageUrl"
                                                    variant="outlined"
                                                    label="Image Url"
                                                    type="text"
                                                    value={this.state.Raids.imageUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Raids.footer.text"
                                                    name="Raids.footer.text"
                                                    variant="outlined"
                                                    label="Footer Text"
                                                    type="text"
                                                    value={this.state.Raids.footer.text}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Raids.footer.iconUrl"
                                                    name="Raids.footer.iconUrl"
                                                    variant="outlined"
                                                    label="Footer Icon Url"
                                                    type="text"
                                                    value={this.state.Raids.footer.iconUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                        </Grid>
                                    </Grid>
                                    <Grid item xs={6}>
                                        <TableContainer component={Paper}>
                                            <Table sx={{ minWidth: 650 }} size="small" aria-label="">
                                                <TableHead>
                                                    <TableRow>
                                                        <TableCell>Placeholder</TableCell>
                                                        <TableCell align="right">Description</TableCell>
                                                        <TableCell align="right">Example</TableCell>
                                                        <TableCell align="right">Type</TableCell>
                                                    </TableRow>
                                                </TableHead>
                                                <TableBody>
                                                    {this.state.placeholders.raids && this.state.placeholders.raids.map((placeholder: any) => {
                                                        return (
                                                            <TableRow
                                                                key={placeholder.placeholder}
                                                                sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                                                            >
                                                                <TableCell component="th" scope="row">
                                                                    <code>{placeholder.placeholder}</code>
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.description}
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.example}
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.type}
                                                                </TableCell>
                                                            </TableRow>
                                                        );
                                                    })}
                                                </TableBody>
                                            </Table>
                                        </TableContainer>
                                    </Grid>
                                </Grid>
                            </AccordionDetails>
                        </Accordion>
                        <Accordion expanded={this.state.expanded === 'panel4'} onChange={this.handlePanelExpanded('panel4')}>
                            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                <Typography>Eggs</Typography>
                            </AccordionSummary>
                            <AccordionDetails>
                                <Grid container spacing={2}>
                                    <Grid item xs={6}>
                                        <Grid container spacing={2}>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Eggs.avatarUrl"
                                                    name="Eggs.avatarUrl"
                                                    variant="outlined"
                                                    label="Avatar Url"
                                                    type="text"
                                                    value={this.state.Eggs.avatarUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Eggs.username"
                                                    name="Eggs.username"
                                                    variant="outlined"
                                                    label="Username"
                                                    type="text"
                                                    value={this.state.Eggs.username}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Eggs.iconUrl"
                                                    name="Eggs.iconUrl"
                                                    variant="outlined"
                                                    label="Icon Url"
                                                    type="text"
                                                    value={this.state.Eggs.iconUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Eggs.title"
                                                    name="Eggs.title"
                                                    variant="outlined"
                                                    label="Title"
                                                    type="text"
                                                    value={this.state.Eggs.title}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Eggs.url"
                                                    name="Eggs.url"
                                                    variant="outlined"
                                                    label="Url"
                                                    type="text"
                                                    value={this.state.Eggs.url}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Eggs.content"
                                                    name="Eggs.content"
                                                    variant="outlined"
                                                    label="Content"
                                                    type="text"
                                                    value={formatContent('Eggs')}
                                                    multiline
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Eggs.imageUrl"
                                                    name="Eggs.imageUrl"
                                                    variant="outlined"
                                                    label="Image Url"
                                                    type="text"
                                                    value={this.state.Eggs.imageUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Eggs.footer.text"
                                                    name="Eggs.footer.text"
                                                    variant="outlined"
                                                    label="Footer Text"
                                                    type="text"
                                                    value={this.state.Eggs.footer.text}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Eggs.footer.iconUrl"
                                                    name="Eggs.footer.iconUrl"
                                                    variant="outlined"
                                                    label="Footer Icon Url"
                                                    type="text"
                                                    value={this.state.Eggs.footer.iconUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                        </Grid>
                                    </Grid>
                                    <Grid item xs={6}>
                                        <TableContainer component={Paper}>
                                            <Table sx={{ minWidth: 650 }} size="small" aria-label="">
                                                <TableHead>
                                                    <TableRow>
                                                        <TableCell>Placeholder</TableCell>
                                                        <TableCell align="right">Description</TableCell>
                                                        <TableCell align="right">Example</TableCell>
                                                        <TableCell align="right">Type</TableCell>
                                                    </TableRow>
                                                </TableHead>
                                                <TableBody>
                                                    {this.state.placeholders.raids && this.state.placeholders.raids.map((placeholder: any) => {
                                                        return (
                                                            <TableRow
                                                                key={placeholder.placeholder}
                                                                sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                                                            >
                                                                <TableCell component="th" scope="row">
                                                                    <code>{placeholder.placeholder}</code>
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.description}
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.example}
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.type}
                                                                </TableCell>
                                                            </TableRow>
                                                        );
                                                    })}
                                                </TableBody>
                                            </Table>
                                        </TableContainer>
                                    </Grid>
                                </Grid>
                            </AccordionDetails>
                        </Accordion>
                        <Accordion expanded={this.state.expanded === 'panel5'} onChange={this.handlePanelExpanded('panel5')}>
                            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                <Typography>Gyms</Typography>
                            </AccordionSummary>
                            <AccordionDetails>
                                <Grid container spacing={2}>
                                    <Grid item xs={6}>
                                        <Grid container spacing={2}>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Gyms.avatarUrl"
                                                    name="Gyms.avatarUrl"
                                                    variant="outlined"
                                                    label="Avatar Url"
                                                    type="text"
                                                    value={this.state.Gyms.avatarUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Gyms.username"
                                                    name="Gyms.username"
                                                    variant="outlined"
                                                    label="Username"
                                                    type="text"
                                                    value={this.state.Gyms.username}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Gyms.iconUrl"
                                                    name="Gyms.iconUrl"
                                                    variant="outlined"
                                                    label="Icon Url"
                                                    type="text"
                                                    value={this.state.Gyms.iconUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Gyms.title"
                                                    name="Gyms.title"
                                                    variant="outlined"
                                                    label="Title"
                                                    type="text"
                                                    value={this.state.Gyms.title}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Gyms.url"
                                                    name="Gyms.url"
                                                    variant="outlined"
                                                    label="Url"
                                                    type="text"
                                                    value={this.state.Gyms.url}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Gyms.content"
                                                    name="Gyms.content"
                                                    variant="outlined"
                                                    label="Content"
                                                    type="text"
                                                    value={formatContent('Gyms')}
                                                    multiline
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Gyms.imageUrl"
                                                    name="Gyms.imageUrl"
                                                    variant="outlined"
                                                    label="Image Url"
                                                    type="text"
                                                    value={this.state.Gyms.imageUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Gyms.footer.text"
                                                    name="Gyms.footer.text"
                                                    variant="outlined"
                                                    label="Footer Text"
                                                    type="text"
                                                    value={this.state.Gyms.footer.text}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Gyms.footer.iconUrl"
                                                    name="Gyms.footer.iconUrl"
                                                    variant="outlined"
                                                    label="Footer Icon Url"
                                                    type="text"
                                                    value={this.state.Gyms.footer.iconUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                        </Grid>
                                    </Grid>
                                    <Grid item xs={6}>
                                        <TableContainer component={Paper}>
                                            <Table sx={{ minWidth: 650 }} size="small" aria-label="">
                                                <TableHead>
                                                    <TableRow>
                                                        <TableCell>Placeholder</TableCell>
                                                        <TableCell align="right">Description</TableCell>
                                                        <TableCell align="right">Example</TableCell>
                                                        <TableCell align="right">Type</TableCell>
                                                    </TableRow>
                                                </TableHead>
                                                <TableBody>
                                                    {this.state.placeholders.gyms && this.state.placeholders.gyms.map((placeholder: any) => {
                                                        return (
                                                            <TableRow
                                                                key={placeholder.placeholder}
                                                                sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                                                            >
                                                                <TableCell component="th" scope="row">
                                                                    <code>{placeholder.placeholder}</code>
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.description}
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.example}
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.type}
                                                                </TableCell>
                                                            </TableRow>
                                                        );
                                                    })}
                                                </TableBody>
                                            </Table>
                                        </TableContainer>
                                    </Grid>
                                </Grid>
                            </AccordionDetails>
                        </Accordion>
                        <Accordion expanded={this.state.expanded === 'panel6'} onChange={this.handlePanelExpanded('panel6')}>
                            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                <Typography>Pokestops</Typography>
                            </AccordionSummary>
                            <AccordionDetails>
                                <Grid container spacing={2}>
                                    <Grid item xs={6}>
                                        <Grid container spacing={2}>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Pokestops.avatarUrl"
                                                    name="Pokestops.avatarUrl"
                                                    variant="outlined"
                                                    label="Avatar Url"
                                                    type="text"
                                                    value={this.state.Pokestops.avatarUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Pokestops.username"
                                                    name="Pokestops.username"
                                                    variant="outlined"
                                                    label="Username"
                                                    type="text"
                                                    value={this.state.Pokestops.username}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Pokestops.iconUrl"
                                                    name="Pokestops.iconUrl"
                                                    variant="outlined"
                                                    label="Icon Url"
                                                    type="text"
                                                    value={this.state.Pokestops.iconUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Pokestops.title"
                                                    name="Pokestops.title"
                                                    variant="outlined"
                                                    label="Title"
                                                    type="text"
                                                    value={this.state.Pokestops.title}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Pokestops.url"
                                                    name="Pokestops.url"
                                                    variant="outlined"
                                                    label="Url"
                                                    type="text"
                                                    value={this.state.Pokestops.url}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Pokestops.content"
                                                    name="Pokestops.content"
                                                    variant="outlined"
                                                    label="Content"
                                                    type="text"
                                                    value={formatContent('Pokestops')}
                                                    multiline
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Pokestops.imageUrl"
                                                    name="Pokestops.imageUrl"
                                                    variant="outlined"
                                                    label="Image Url"
                                                    type="text"
                                                    value={this.state.Pokestops.imageUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Pokestops.footer.text"
                                                    name="Pokestops.footer.text"
                                                    variant="outlined"
                                                    label="Footer Text"
                                                    type="text"
                                                    value={this.state.Pokestops.footer.text}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Pokestops.footer.iconUrl"
                                                    name="Pokestops.footer.iconUrl"
                                                    variant="outlined"
                                                    label="Footer Icon Url"
                                                    type="text"
                                                    value={this.state.Pokestops.footer.iconUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                        </Grid>
                                    </Grid>
                                    <Grid item xs={6}>
                                        <TableContainer component={Paper}>
                                            <Table sx={{ minWidth: 650 }} size="small" aria-label="">
                                                <TableHead>
                                                    <TableRow>
                                                        <TableCell>Placeholder</TableCell>
                                                        <TableCell align="right">Description</TableCell>
                                                        <TableCell align="right">Example</TableCell>
                                                        <TableCell align="right">Type</TableCell>
                                                    </TableRow>
                                                </TableHead>
                                                <TableBody>
                                                    {this.state.placeholders.pokestops && this.state.placeholders.pokestops.map((placeholder: any) => {
                                                        return (
                                                            <TableRow
                                                                key={placeholder.placeholder}
                                                                sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                                                            >
                                                                <TableCell component="th" scope="row">
                                                                    <code>{placeholder.placeholder}</code>
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.description}
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.example}
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.type}
                                                                </TableCell>
                                                            </TableRow>
                                                        );
                                                    })}
                                                </TableBody>
                                            </Table>
                                        </TableContainer>
                                    </Grid>
                                </Grid>
                            </AccordionDetails>
                        </Accordion>
                        <Accordion expanded={this.state.expanded === 'panel7'} onChange={this.handlePanelExpanded('panel7')}>
                            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                <Typography>Quests</Typography>
                            </AccordionSummary>
                            <AccordionDetails>
                                <Grid container spacing={2}>
                                    <Grid item xs={6}>
                                        <Grid container spacing={2}>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Quests.avatarUrl"
                                                    name="Quests.avatarUrl"
                                                    variant="outlined"
                                                    label="Avatar Url"
                                                    type="text"
                                                    value={this.state.Quests.avatarUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Quests.username"
                                                    name="Quests.username"
                                                    variant="outlined"
                                                    label="Username"
                                                    type="text"
                                                    value={this.state.Quests.username}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Quests.iconUrl"
                                                    name="Quests.iconUrl"
                                                    variant="outlined"
                                                    label="Icon Url"
                                                    type="text"
                                                    value={this.state.Quests.iconUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Quests.title"
                                                    name="Quests.title"
                                                    variant="outlined"
                                                    label="Title"
                                                    type="text"
                                                    value={this.state.Quests.title}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Quests.url"
                                                    name="Quests.url"
                                                    variant="outlined"
                                                    label="Url"
                                                    type="text"
                                                    value={this.state.Quests.url}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Quests.content"
                                                    name="Quests.content"
                                                    variant="outlined"
                                                    label="Content"
                                                    type="text"
                                                    value={formatContent('Quests')}
                                                    multiline
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Quests.imageUrl"
                                                    name="Quests.imageUrl"
                                                    variant="outlined"
                                                    label="Image Url"
                                                    type="text"
                                                    value={this.state.Quests.imageUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Quests.footer.text"
                                                    name="Quests.footer.text"
                                                    variant="outlined"
                                                    label="Footer Text"
                                                    type="text"
                                                    value={this.state.Quests.footer.text}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Quests.footer.iconUrl"
                                                    name="Quests.footer.iconUrl"
                                                    variant="outlined"
                                                    label="Footer Icon Url"
                                                    type="text"
                                                    value={this.state.Quests.footer.iconUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                        </Grid>
                                    </Grid>
                                    <Grid item xs={6}>
                                        <TableContainer component={Paper}>
                                            <Table sx={{ minWidth: 650 }} size="small" aria-label="">
                                                <TableHead>
                                                    <TableRow>
                                                        <TableCell>Placeholder</TableCell>
                                                        <TableCell align="right">Description</TableCell>
                                                        <TableCell align="right">Example</TableCell>
                                                        <TableCell align="right">Type</TableCell>
                                                    </TableRow>
                                                </TableHead>
                                                <TableBody>
                                                    {this.state.placeholders.quests && this.state.placeholders.quests.map((placeholder: any) => {
                                                        return (
                                                            <TableRow
                                                                key={placeholder.placeholder}
                                                                sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                                                            >
                                                                <TableCell component="th" scope="row">
                                                                    <code>{placeholder.placeholder}</code>
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.description}
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.example}
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.type}
                                                                </TableCell>
                                                            </TableRow>
                                                        );
                                                    })}
                                                </TableBody>
                                            </Table>
                                        </TableContainer>
                                    </Grid>
                                </Grid>
                            </AccordionDetails>
                        </Accordion>
                        <Accordion expanded={this.state.expanded === 'panel8'} onChange={this.handlePanelExpanded('panel8')}>
                            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                <Typography>Lures</Typography>
                            </AccordionSummary>
                            <AccordionDetails>
                                <Grid container spacing={2}>
                                    <Grid item xs={6}>
                                        <Grid container spacing={2}>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Lures.avatarUrl"
                                                    name="Lures.avatarUrl"
                                                    variant="outlined"
                                                    label="Avatar Url"
                                                    type="text"
                                                    value={this.state.Lures.avatarUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Lures.username"
                                                    name="Lures.username"
                                                    variant="outlined"
                                                    label="Username"
                                                    type="text"
                                                    value={this.state.Lures.username}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Lures.iconUrl"
                                                    name="Lures.iconUrl"
                                                    variant="outlined"
                                                    label="Icon Url"
                                                    type="text"
                                                    value={this.state.Lures.iconUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Lures.title"
                                                    name="Lures.title"
                                                    variant="outlined"
                                                    label="Title"
                                                    type="text"
                                                    value={this.state.Lures.title}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Lures.url"
                                                    name="Lures.url"
                                                    variant="outlined"
                                                    label="Url"
                                                    type="text"
                                                    value={this.state.Lures.url}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Lures.content"
                                                    name="Lures.content"
                                                    variant="outlined"
                                                    label="Content"
                                                    type="text"
                                                    value={formatContent('Lures')}
                                                    multiline
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Lures.imageUrl"
                                                    name="Lures.imageUrl"
                                                    variant="outlined"
                                                    label="Image Url"
                                                    type="text"
                                                    value={this.state.Lures.imageUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Lures.footer.text"
                                                    name="Lures.footer.text"
                                                    variant="outlined"
                                                    label="Footer Text"
                                                    type="text"
                                                    value={this.state.Lures.footer.text}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Lures.footer.iconUrl"
                                                    name="Lures.footer.iconUrl"
                                                    variant="outlined"
                                                    label="Footer Icon Url"
                                                    type="text"
                                                    value={this.state.Lures.footer.iconUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                        </Grid>
                                    </Grid>
                                    <Grid item xs={6}>
                                        <TableContainer component={Paper}>
                                            <Table sx={{ minWidth: 650 }} size="small" aria-label="">
                                                <TableHead>
                                                    <TableRow>
                                                        <TableCell>Placeholder</TableCell>
                                                        <TableCell align="right">Description</TableCell>
                                                        <TableCell align="right">Example</TableCell>
                                                        <TableCell align="right">Type</TableCell>
                                                    </TableRow>
                                                </TableHead>
                                                <TableBody>
                                                    {this.state.placeholders.pokestops && this.state.placeholders.pokestops.map((placeholder: any) => {
                                                        return (
                                                            <TableRow
                                                                key={placeholder.placeholder}
                                                                sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                                                            >
                                                                <TableCell component="th" scope="row">
                                                                    <code>{placeholder.placeholder}</code>
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.description}
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.example}
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.type}
                                                                </TableCell>
                                                            </TableRow>
                                                        );
                                                    })}
                                                </TableBody>
                                            </Table>
                                        </TableContainer>
                                    </Grid>
                                </Grid>
                            </AccordionDetails>
                        </Accordion>
                        <Accordion expanded={this.state.expanded === 'panel9'} onChange={this.handlePanelExpanded('panel9')}>
                            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                <Typography>Invasions</Typography>
                            </AccordionSummary>
                            <AccordionDetails>
                                <Grid container spacing={2}>
                                    <Grid item xs={6}>
                                        <Grid container spacing={2}>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Invasions.avatarUrl"
                                                    name="Invasions.avatarUrl"
                                                    variant="outlined"
                                                    label="Avatar Url"
                                                    type="text"
                                                    value={this.state.Invasions.avatarUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Invasions.username"
                                                    name="Invasions.username"
                                                    variant="outlined"
                                                    label="Username"
                                                    type="text"
                                                    value={this.state.Invasions.username}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Invasions.iconUrl"
                                                    name="Invasions.iconUrl"
                                                    variant="outlined"
                                                    label="Icon Url"
                                                    type="text"
                                                    value={this.state.Invasions.iconUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Invasions.title"
                                                    name="Invasions.title"
                                                    variant="outlined"
                                                    label="Title"
                                                    type="text"
                                                    value={this.state.Invasions.title}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Invasions.url"
                                                    name="Invasions.url"
                                                    variant="outlined"
                                                    label="Url"
                                                    type="text"
                                                    value={this.state.Invasions.url}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Invasions.content"
                                                    name="Invasions.content"
                                                    variant="outlined"
                                                    label="Content"
                                                    type="text"
                                                    value={formatContent('Invasions')}
                                                    multiline
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Invasions.imageUrl"
                                                    name="Invasions.imageUrl"
                                                    variant="outlined"
                                                    label="Image Url"
                                                    type="text"
                                                    value={this.state.Invasions.imageUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Invasions.footer.text"
                                                    name="Invasions.footer.text"
                                                    variant="outlined"
                                                    label="Footer Text"
                                                    type="text"
                                                    value={this.state.Invasions.footer.text}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Invasions.footer.iconUrl"
                                                    name="Invasions.footer.iconUrl"
                                                    variant="outlined"
                                                    label="Footer Icon Url"
                                                    type="text"
                                                    value={this.state.Invasions.footer.iconUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                        </Grid>
                                    </Grid>
                                    <Grid item xs={6}>
                                        <TableContainer component={Paper}>
                                            <Table sx={{ minWidth: 650 }} size="small" aria-label="">
                                                <TableHead>
                                                    <TableRow>
                                                        <TableCell>Placeholder</TableCell>
                                                        <TableCell align="right">Description</TableCell>
                                                        <TableCell align="right">Example</TableCell>
                                                        <TableCell align="right">Type</TableCell>
                                                    </TableRow>
                                                </TableHead>
                                                <TableBody>
                                                    {this.state.placeholders.pokestops && this.state.placeholders.pokestops.map((placeholder: any) => {
                                                        return (
                                                            <TableRow
                                                                key={placeholder.placeholder}
                                                                sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                                                            >
                                                                <TableCell component="th" scope="row">
                                                                    <code>{placeholder.placeholder}</code>
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.description}
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.example}
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.type}
                                                                </TableCell>
                                                            </TableRow>
                                                        );
                                                    })}
                                                </TableBody>
                                            </Table>
                                        </TableContainer>
                                    </Grid>
                                </Grid>
                            </AccordionDetails>
                        </Accordion>
                        <Accordion expanded={this.state.expanded === 'panel10'} onChange={this.handlePanelExpanded('panel10')}>
                            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                <Typography>Nests</Typography>
                            </AccordionSummary>
                            <AccordionDetails>
                                <Grid container spacing={2}>
                                    <Grid item xs={6}>
                                        <Grid container spacing={2}>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Nests.avatarUrl"
                                                    name="Nests.avatarUrl"
                                                    variant="outlined"
                                                    label="Avatar Url"
                                                    type="text"
                                                    value={this.state.Nests.avatarUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Nests.username"
                                                    name="Nests.username"
                                                    variant="outlined"
                                                    label="Username"
                                                    type="text"
                                                    value={this.state.Nests.username}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Nests.iconUrl"
                                                    name="Nests.iconUrl"
                                                    variant="outlined"
                                                    label="Icon Url"
                                                    type="text"
                                                    value={this.state.Nests.iconUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Nests.title"
                                                    name="Nests.title"
                                                    variant="outlined"
                                                    label="Title"
                                                    type="text"
                                                    value={this.state.Nests.title}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Nests.url"
                                                    name="Nests.url"
                                                    variant="outlined"
                                                    label="Url"
                                                    type="text"
                                                    value={this.state.Nests.url}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Nests.content"
                                                    name="Nests.content"
                                                    variant="outlined"
                                                    label="Content"
                                                    type="text"
                                                    value={formatContent('Nests')}
                                                    multiline
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Nests.imageUrl"
                                                    name="Nests.imageUrl"
                                                    variant="outlined"
                                                    label="Image Url"
                                                    type="text"
                                                    value={this.state.Nests.imageUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Nests.footer.text"
                                                    name="Nests.footer.text"
                                                    variant="outlined"
                                                    label="Footer Text"
                                                    type="text"
                                                    value={this.state.Nests.footer.text}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Nests.footer.iconUrl"
                                                    name="Nests.footer.iconUrl"
                                                    variant="outlined"
                                                    label="Footer Icon Url"
                                                    type="text"
                                                    value={this.state.Nests.footer.iconUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                        </Grid>
                                    </Grid>
                                    <Grid item xs={6}>
                                        <TableContainer component={Paper}>
                                            <Table sx={{ minWidth: 650 }} size="small" aria-label="">
                                                <TableHead>
                                                    <TableRow>
                                                        <TableCell>Placeholder</TableCell>
                                                        <TableCell align="right">Description</TableCell>
                                                        <TableCell align="right">Example</TableCell>
                                                        <TableCell align="right">Type</TableCell>
                                                    </TableRow>
                                                </TableHead>
                                                <TableBody>
                                                    {this.state.placeholders.nests && this.state.placeholders.nests.map((placeholder: any) => {
                                                        return (
                                                            <TableRow
                                                                key={placeholder.placeholder}
                                                                sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                                                            >
                                                                <TableCell component="th" scope="row">
                                                                    <code>{placeholder.placeholder}</code>
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.description}
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.example}
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.type}
                                                                </TableCell>
                                                            </TableRow>
                                                        );
                                                    })}
                                                </TableBody>
                                            </Table>
                                        </TableContainer>
                                    </Grid>
                                </Grid>
                            </AccordionDetails>
                        </Accordion>
                        <Accordion expanded={this.state.expanded === 'panel11'} onChange={this.handlePanelExpanded('panel11')}>
                            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                                <Typography>Weather</Typography>
                            </AccordionSummary>
                            <AccordionDetails>
                                <Grid container spacing={2}>
                                    <Grid item xs={6}>
                                        <Grid container spacing={2}>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Weather.avatarUrl"
                                                    name="Weather.avatarUrl"
                                                    variant="outlined"
                                                    label="Avatar Url"
                                                    type="text"
                                                    value={this.state.Weather.avatarUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Weather.username"
                                                    name="Weather.username"
                                                    variant="outlined"
                                                    label="Username"
                                                    type="text"
                                                    value={this.state.Weather.username}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Weather.iconUrl"
                                                    name="Weather.iconUrl"
                                                    variant="outlined"
                                                    label="Icon Url"
                                                    type="text"
                                                    value={this.state.Weather.iconUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Weather.title"
                                                    name="Weather.title"
                                                    variant="outlined"
                                                    label="Title"
                                                    type="text"
                                                    value={this.state.Weather.title}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Weather.url"
                                                    name="Weather.url"
                                                    variant="outlined"
                                                    label="Url"
                                                    type="text"
                                                    value={this.state.Weather.url}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Weather.content"
                                                    name="Weather.content"
                                                    variant="outlined"
                                                    label="Content"
                                                    type="text"
                                                    value={formatContent('Weather')}
                                                    multiline
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={12}>
                                                <TextField
                                                    id="Weather.imageUrl"
                                                    name="Weather.imageUrl"
                                                    variant="outlined"
                                                    label="Image Url"
                                                    type="text"
                                                    value={this.state.Weather.imageUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Weather.footer.text"
                                                    name="Weather.footer.text"
                                                    variant="outlined"
                                                    label="Footer Text"
                                                    type="text"
                                                    value={this.state.Weather.footer.text}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                            <Grid item xs={12} sm={6}>
                                                <TextField
                                                    id="Weather.footer.iconUrl"
                                                    name="Weather.footer.iconUrl"
                                                    variant="outlined"
                                                    label="Footer Icon Url"
                                                    type="text"
                                                    value={this.state.Weather.footer.iconUrl}
                                                    fullWidth
                                                    onChange={this.onInputChange}
                                                />
                                            </Grid>
                                        </Grid>
                                    </Grid>
                                    <Grid item xs={6}>
                                        <TableContainer component={Paper}>
                                            <Table sx={{ minWidth: 650 }} size="small" aria-label="">
                                                <TableHead>
                                                    <TableRow>
                                                        <TableCell>Placeholder</TableCell>
                                                        <TableCell align="right">Description</TableCell>
                                                        <TableCell align="right">Example</TableCell>
                                                        <TableCell align="right">Type</TableCell>
                                                    </TableRow>
                                                </TableHead>
                                                <TableBody>
                                                    {this.state.placeholders.weather && this.state.placeholders.weather.map((placeholder: any) => {
                                                        return (
                                                            <TableRow
                                                                key={placeholder.placeholder}
                                                                sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                                                            >
                                                                <TableCell component="th" scope="row">
                                                                    <code>{placeholder.placeholder}</code>
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.description}
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.example}
                                                                </TableCell>
                                                                <TableCell align="right">
                                                                    {placeholder.type}
                                                                </TableCell>
                                                            </TableRow>
                                                        );
                                                    })}
                                                </TableBody>
                                            </Table>
                                        </TableContainer>
                                    </Grid>
                                </Grid>
                            </AccordionDetails>
                        </Accordion>
                    </div>
                    <div className={classes.buttonContainer}>
                        <Button
                            variant="contained"
                            color="primary"
                            style={{marginRight: '20px'}}
                            type="submit"
                        >
                            Save
                        </Button>
                        <Button
                            variant="outlined"
                            color="primary"
                            onClick={handleCancel}
                        >
                            Cancel
                        </Button>
                    </div>
                </Box>
            </div>
        );
    }
}

export default withRouter(NewEmbed);