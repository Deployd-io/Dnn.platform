import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { PersonaBarPage } from "@dnnsoftware/dnn-react-common";
import Body from "./Body";
const logger = console;

class App extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            editMode: false,
            portalBeingExported: {}
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="themes-Root">
                <PersonaBarPage isOpen={props.selectedPage === 0}>
                    <Body />
                </PersonaBarPage>
            </div>
        );
    }
}

App.propTypes = {
    dispatch: PropTypes.func.isRequired,
    selectedPage: PropTypes.number,
    selectedPageVisibleIndex: PropTypes.number
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