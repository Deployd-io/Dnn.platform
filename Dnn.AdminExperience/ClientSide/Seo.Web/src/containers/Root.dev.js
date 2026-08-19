import React, {Component} from "react";
import App from "../components/App";
import DevTools from "./DevTools";
const logger = console;

class Root extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
		logger.debug('<<< Exiting constructor()');
    }
    render() {
		logger.debug('>>> Entering render()');
		logger.debug('<<< Exiting render()');
        return (
            <div className="siteInfo-Root">
                <App />
                <DevTools />
            </div>
        );
    }
}

export default Root;