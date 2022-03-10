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
    var formattedValue = value.includes(',') ? value.split(',') : value;
    pointer[finalProp] = type === 'checkbox'
        ? checked
        : type === 'number'
          ? Number(value)
          : formattedValue;
    console.log('newState:', newState);
    component.setState(newState);
};