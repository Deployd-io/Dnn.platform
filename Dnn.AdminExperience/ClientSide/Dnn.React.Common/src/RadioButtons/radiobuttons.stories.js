import React, { Component } from "react";
import RadioButtons from "./index";
const logger = console;

export default {
    component: RadioButtons,
};

class MyRadioButtons extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = { value: 1 };
		logger.debug(`constructor(): this.state → ${this.state}`, { "event.action": "constructor" });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
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
