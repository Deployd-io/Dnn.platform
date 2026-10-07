import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { PersonaBarPage, PersonaBarPageHeader } from "@dnnsoftware/dnn-react-common";
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
            <div className="seo-app">
                <PersonaBarPage isOpen={props.selectedPage === 0}>
                    <PersonaBarPageHeader title={resx.get("nav_Seo")}>
                    </PersonaBarPageHeader>
                    <Body />
                </PersonaBarPage>
            </div>
        );
    }
}

App.propTypes = {
    dispatch: PropTypes.func.isRequired,
    selectedPage: PropTypes.number
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