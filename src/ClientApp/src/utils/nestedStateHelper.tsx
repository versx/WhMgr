import React, { Component} from 'react';

export const onNestedStateChange = (event: any, component: Component) => {
    const { name, type, value, checked } = event.target;
    const path = name.split('.');
    const finalProp = path.pop();
    const newState = { ...component.state };
    let pointer: any = newState;
    path.forEach((el: string) => {
      pointer[el] = { ...pointer[el] };
      pointer = pointer[el];
    });
    pointer[finalProp] = type === 'checkbox'
        ? checked
        : value;
    console.log('newState:', newState);
    component.setState(newState);
};