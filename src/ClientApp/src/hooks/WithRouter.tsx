import {
    useParams,
    useLocation,
    useNavigate,
} from 'react-router-dom';

const withRouter = (Child: any) => {
    return (props: any) => {
        const location = useLocation();
        const navigate = useNavigate();
        const params = useParams();
        return <Child { ...props } params={params} navigate={navigate} location={location} />;
    };
}

export default withRouter;