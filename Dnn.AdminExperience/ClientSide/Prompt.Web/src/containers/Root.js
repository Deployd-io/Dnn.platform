import React, { Component } from "react";
import App from "components/App";
import DevTools from "containers/DevTools";
const logger = console;
import { IS_DEV} from "globals/promptInit";

export default class Root extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
		logger.debug('<<< Exiting constructor()');
    }

    render() {
		logger.debug('>>> Entering render()');
		logger.debug('<<< Exiting render()');
        return (
            <div className="dnnPrompt-app personaBar-mainContainer">
                <App />
                {IS_DEV && <DevTools />}
            </div>
        );
    }
}