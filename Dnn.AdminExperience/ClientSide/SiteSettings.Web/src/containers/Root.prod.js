import React, {Component} from "react";
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
        let culture = window.parent["personaBarSettings"]["culture"];
        let portalId = window.parent["personaBarSettings"]["portalId"];
		logger.debug(`render(): culture → ${culture}`);
		logger.debug('<<< Exiting render()');
        return (
            <div className="siteSettings-Root">
                <App cultureCode={culture} portalId={portalId} />
            </div>
        );
    }
}

export default Root;