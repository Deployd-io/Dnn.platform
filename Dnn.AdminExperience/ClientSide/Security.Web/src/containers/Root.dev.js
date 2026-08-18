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
        let culture = window.parent["personaBarSettings"]["culture"];
		logger.debug('<<< Exiting render()');
        return (
            <div className="taskScheduler-Root">
                <App cultureCode={culture} />
                <DevTools />
            </div>
        );
    }
}

export default Root;