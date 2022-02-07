import React from 'react';
import * as L from 'leaflet';

//import L from 'leaflet-easybutton';
import 'leaflet-easybutton';
import 'leaflet-easybutton/src/easy-button.css';

import withMap from '../hooks/WithMap';

interface MapButtonProps {
    tooltip: string;
    onClick: any;
    map: any;
    icon: string;
}

class MapButton extends React.Component<MapButtonProps> {
    private button: any;

    constructor(props: MapButtonProps) {
        super(props);
    }

    componentDidMount() {
        const { map, tooltip, icon, onClick } = this.props;
        this.button = L.easyButton(icon, onClick, tooltip);
        this.button.addTo(map);
    }

    componentWillUnmount() {
        this.button.remove();
    }

    render() {
        return null;
    }
}

export default withMap(MapButton);