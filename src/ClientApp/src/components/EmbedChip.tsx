import React, { useState } from 'react';
import { Chip } from '@mui/material';

interface EmbedChipProps {
    label: string;
    color: any;
    default: string;
    onClick: any;
}

function EmbedChip(props: EmbedChipProps) {
    //const [state, setState] = useState();

    return (
        <Chip
            label={props.label}
            color={props.color}
            onClick={props.onClick}
        />
    );
}

export default EmbedChip;