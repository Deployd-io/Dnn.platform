import React, {Component} from "react";
import App from "../components/App";
const logger = console;

class Root extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        let culture = window.parent["personaBarSettings"]["culture"];
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="taskScheduler-Root">
                <App cultureCode={culture} />
            </div>
        );
    }
}

export default Root;