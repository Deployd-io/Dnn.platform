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
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="themes-app personaBar-mainContainer">
                <App />
            </div>
        );
    }
}


export default Root;