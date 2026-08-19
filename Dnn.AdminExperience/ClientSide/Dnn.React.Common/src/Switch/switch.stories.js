import React, { Component } from "react";
import { action } from "storybook/actions";
import Switch from "./index";
const logger = console;

export default {
    component: Switch,
};

class MyControlledSwitch extends Component {
    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);
        this.state = { value: props.value ? props.value : false};
		logger.debug(`constructor(props=${props}): this.state → ${this.state}`);
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }

    render() {
		logger.debug('>>> Entering render()');
		logger.debug('<<< Exiting render()');
        return (
            <Switch
                labelHidden={false}
                onText="On"
                offText="Off"
                label="Controlled Switch"
                onChange={(value) => this.setState({ value: value })}
                labelPlacement="left"
                value={this.state.value}    
            />
        );
    }
}

export const WithOff = () => (
    <Switch
        labelHidden={false}
        onText="On"
        offText="Off"
        label="Example Switch"
        onChange={action("changed")}
        labelPlacement="left"
    />
);

export const WithOn = () => (
    <Switch
        labelHidden={false}
        onText="On"
        offText="Off"
        label="Example Switch"
        onChange={action("changed")}
        labelPlacement="left"
        value={true}
    />
);

export const ControlledSwitch = () => (
    <MyControlledSwitch />
);