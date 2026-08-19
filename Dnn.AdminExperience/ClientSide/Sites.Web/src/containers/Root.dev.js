import React, {Component} from "react";
import DevTools from "./DevTools";
import App from "../components/App";
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
            <div className="sites-app personaBar-mainContainer">
                <App />
                <DevTools />
            </div>
        );
    }
}


export default Root;