import React, { Component } from "react";
import Dropdown from "./index";
const logger = console;

export default {
    component: Dropdown,
};

class MyDropdown extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = { option: {} };
		logger.debug(`constructor(): this.state → ${this.state}`, { "event.action": "constructor" });
    }

    handleSelect(option) {
        this.setState( { option: option });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <Dropdown
                label="Test"
                options={[
                    { label: "Opt 1", value: 1 },
                    { label: "Opt 2", value: 2 },
                    { label: "Opt 3", value: 3 }
                ]}
                value={this.state.option.value}
                onSelect={this.handleSelect.bind(this)}
            />
        );
    }
}

export const WithContent =  () => (
    <MyDropdown />
);
