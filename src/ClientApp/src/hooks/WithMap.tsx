import { useMap } from 'react-leaflet';

const withMap = (Component: any) => {
    return function WrappedComponent(props: any) {
        const map = useMap();
        return (
            <Component {...props} map={map} />
        );
    }
};

export default withMap;