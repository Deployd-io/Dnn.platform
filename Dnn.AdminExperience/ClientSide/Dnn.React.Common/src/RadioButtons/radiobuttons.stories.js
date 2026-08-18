import React, { Component } from "react";
import RadioButtons from "./index";
const logger = console;

export default {
    component: RadioButtons,
};

class MyRadioButtons extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = { value: 1 };
		logger.debug(`constructor(): this.state → ${this.state}`);
		logger.debug('<<< Exiting constructor()');
    }

    render() {
		logger.debug('>>> Entering render()');
		logger.debug('<<< Exiting render()');
        return (
            <RadioButtons
                options={[
                    { value: "1", label: "Value 1" },
                    { value: "2", label: "Value 2" }
                ]}
                onChange={(value) => this.setState({value:value})}
                value={this.state.value}
            />
        );
    }
}

export const WithContent = () => (
    <MyRadioButtons />
);
