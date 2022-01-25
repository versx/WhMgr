import React from 'react'
import {
    Avatar,
    AvatarGroup,
    Container,
    Divider,
    ImageList,
    ImageListItem,
    Link,
    Typography,
} from '@mui/material';
import { makeStyles } from '@mui/styles';

const useStyles = makeStyles((theme: any) => ({
    container: {
        paddingTop: theme.spacing(10),
        position: "sticky",
        top: 0,
    },
    title: {
        fontSize: '16px',
        fontWeight: 500,
        color: '#555',
    },
    imageList: {
    },
    link: {
        marginRight: theme.spacing(2),
        color: '#555',
        fontSize: '16px',
    },
}));

// TODO: Show subscription details, enabled types, location, etc

function Rightbar() {
    const classes = useStyles();
    return (
        <Container className={classes.container}>
            <Typography className={classes.title} gutterBottom>Subscription Details</Typography>
            <div style={{marginBottom: '20px'}}>
                <ul>
                    <li>Enabled</li>
                    <li>Location</li>
                    <li>Phone Number</li>
                </ul>
            </div>
            <Typography className={classes.title} gutterBottom>Online Friends</Typography>
            <AvatarGroup max={6} style={{marginBottom: '20px', justifyContent: 'space-between'}}>
                <Avatar alt="Remy Sharp" src="https://mui.com/static/images/avatar/1.jpg" />
                <Avatar alt="Travis Howard" src="https://mui.com/static/images/avatar/2.jpg" />
                <Avatar alt="Cindy Baker" src="https://mui.com/static/images/avatar/3.jpg" />
                <Avatar alt="Agnes Walker" src="" />
                <Avatar alt="Trevor Henderson" src="https://mui.com/static/images/avatar/5.jpg" />
                <Avatar alt="Trevor Henderson" src="https://mui.com/static/images/avatar/6.jpg" />
                <Avatar alt="Trevor Henderson" src="https://mui.com/static/images/avatar/7.jpg" />
            </AvatarGroup>
            <Typography className={classes.title} gutterBottom>Gallery</Typography>
            <ImageList rowHeight={100} cols={3} style={{marginBottom: '20px'}}>
                <ImageListItem>
                    <img src="https://mui.com/static/images/image-list/breakfast.jpg" alt="" />
                </ImageListItem>
                <ImageListItem>
                    <img src="https://mui.com/static/images/image-list/burgers.jpg" alt="" />
                </ImageListItem>
                <ImageListItem>
                    <img src="https://mui.com/static/images/image-list/camera.jpg" alt="" />
                </ImageListItem>
                <ImageListItem>
                    <img src="https://mui.com/static/images/image-list/morning.jpg" alt="" />
                </ImageListItem>
                <ImageListItem>
                    <img src="https://mui.com/static/images/image-list/hats.jpg" alt="" />
                </ImageListItem>
                <ImageListItem>
                    <img src="https://mui.com/static/images/image-list/vegetables.jpg" alt="" />
                </ImageListItem>
            </ImageList>
            <Typography className={classes.title} gutterBottom>Categories</Typography>
            <Link href="#" className={classes.link} variant="body2">
                Sport
            </Link>
            <Link href="#" className={classes.link} variant="body2">
                Food
            </Link>
            <Link href="#" className={classes.link} variant="body2">
                Music
            </Link>
            <Divider flexItem />
            <Link href="#" className={classes.link} variant="body2">
                Movies
            </Link>
            <Link href="#" className={classes.link} variant="body2">
                Science
            </Link>
            <Link href="#" className={classes.link} variant="body2">
                Life
            </Link>
        </Container>
    );
}

export default Rightbar;