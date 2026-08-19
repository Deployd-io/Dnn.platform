import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { PersonaBarPage } from "@dnnsoftware/dnn-react-common";
import Body from "./Body";
const logger = console;

class App extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            editMode: false,
            portalBeingExported: {}
        };
		logger.debug('<<< Exiting constructor()');
    }

    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;
		logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        selectedPage: state.visiblePanel.selectedPage,
        selectedPageVisibleIndex: state.visiblePanel.selectedPageVisibleIndex
    };
}


export default connect(mapStateToProps)(App);