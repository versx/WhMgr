import React from 'react';
import {
    Breadcrumbs,
    Link,
    Typography,
} from '@mui/material';

interface BreadCrumbProps {
    crumbs: BreadCrumbItem[];
}

interface BreadCrumbItem {
    text: string;
    color: string;
    href: string;
    selected: boolean;
}

export function BreadCrumbs(props: BreadCrumbProps) {
    console.log('crumbs:', props);
    return (
        <div role="presentation" style={{paddingTop: '10px', paddingBottom: '30px'}}>
            <Breadcrumbs aria-label="breadcrumb">
                {props.crumbs.map((crumb: BreadCrumbItem) => {
                        return (
                            crumb.selected
                            ? <Typography>{crumb.text}</Typography>
                            : <Link
                                underline="hover"
                                color={crumb.color}
                                href={crumb.href}
                            >
                                {crumb.text}
                            </Link>
                        )
                })}
            </Breadcrumbs>
        </div>
    );
};