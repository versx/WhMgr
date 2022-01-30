import React, { useState } from 'react';
import {
    FormControl,
    InputLabel,
    MenuItem,
    Select,
    SelectChangeEvent,
} from '@mui/material';

interface MultiSelectProps {
    id: string;
    title: string;
    allItems: string[];
    selectedItems: string[];
}

export function MultiSelect(props: MultiSelectProps) {
    //console.log('multi select props:', props);
    const [items, setItems] = useState<string[]>(props.selectedItems ?? []);

    const handleChange = (event: SelectChangeEvent<typeof items>) => {
        const {
            target: { value },
        } = event;
        setItems(
            typeof value === 'string' ? value.split(',') : value,
        );
    };

    return (
        <div>
            <FormControl fullWidth>
                <InputLabel id="label-title">{props.title}</InputLabel>
                <Select
                    labelId="label-title"
                    id={props.id}
                    multiple
                    value={items.length > 0 ? items : props.selectedItems}
                    label={props.title}
                    onChange={handleChange}
                >
                    {props.allItems.map((item: string) => {
                        return <MenuItem key={item} value={item}>{item}</MenuItem>
                    })}
                </Select>
            </FormControl>
        </div>
    )
};