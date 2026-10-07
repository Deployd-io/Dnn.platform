import React, { Component } from "react";
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
        let portalId = window.parent["personaBarSettings"]["portalId"];
		logger.error(`render(): culture → ${culture}`, { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="siteSettings-Root">
                <App cultureCode={culture} portalId={portalId} />
            </div>
        );
    }
}

export default Root;