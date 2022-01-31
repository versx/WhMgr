import React, { useState } from 'react';
import {
    Box,
    Modal,
    Typography,
} from '@mui/material';

interface AddAlarmProps {
    embeds: string[];
    filters: string[];
    geofences: string[];
    open: boolean;
}

export default function AddAlarmModal(props: AddAlarmProps) {
    console.log('add alarm modal props:', props);
    const [open, setOpen] = useState(props.open);
    //const handleOpen = () => setOpen(true);
    const handleClose = () => setOpen(false);
  
    const style = {
        position: 'absolute' as 'absolute',
        top: '50%',
        left: '50%',
        transform: 'translate(-50%, -50%)',
        width: 400,
        bgcolor: 'paper',
        border: '2px solid #000',
        boxShadow: 24,
        p: 4,
    };

    return (
        <div>
            <Modal
                open={open}
                onClose={handleClose}
                aria-labelledby="modal-modal-title"
                aria-describedby="modal-modal-description"
            >
                <Box sx={style}>
                    <Typography id="modal-modal-title" variant="h6" component="h2">
                        Text in a modal
                    </Typography>
                    <Typography id="modal-modal-description" sx={{ mt: 2 }}>
                        Duis mollis, est non commodo luctus, nisi erat porttitor ligula.
                    </Typography>
                </Box>
            </Modal>
        </div>
    );
};