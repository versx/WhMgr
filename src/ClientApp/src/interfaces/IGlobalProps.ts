import { NavigateFunction, Params } from "react-router-dom";

export interface IGlobalProps {
    location?: Location;
    navigate?: NavigateFunction;
    params?: Readonly<Params<string>>;
    props: IProps;
};

export interface IProps {
    //config: IConfig;
    //geofences: any;
    //masterfile: any;
};