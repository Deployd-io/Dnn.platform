import React, {Component} from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { PersonaBarPageHeader, PersonaBarPage } from "@dnnsoftware/dnn-react-common";
import Body from "./body";
import resx from "../resources";
const logger = console;

class App extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }
    
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="securitySettings-app">
                <PersonaBarPage isOpen={props.selectedPage === 0}>
                    <PersonaBarPageHeader title={resx.get("nav_Security")}>
                    </PersonaBarPageHeader>
                    <Body cultureCode={props.cultureCode}/>
                </PersonaBarPage>
            </div>
        );
    }
}

App.propTypes = {
    dispatch: PropTypes.func.isRequired,
    selectedPage: PropTypes.number,
    cultureCode: PropTypes.string
};


function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        selectedPage: state.visiblePanel.selectedPage,
        selectedPageVisibleIndex: state.visiblePanel.selectedPageVisibleIndex
    };
}


export default connect(mapStateToProps)(App);