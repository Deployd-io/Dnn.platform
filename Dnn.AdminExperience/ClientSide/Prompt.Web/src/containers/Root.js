import React, { Component } from "react";
import App from "components/App";
import DevTools from "containers/DevTools";
const logger = console;
import { IS_DEV} from "globals/promptInit";

export default class Root extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="dnnPrompt-app personaBar-mainContainer">
                <App />
                {IS_DEV && <DevTools />}
            </div>
        );
    }
}