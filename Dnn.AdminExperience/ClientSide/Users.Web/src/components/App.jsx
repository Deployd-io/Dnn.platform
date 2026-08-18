import PropTypes from "prop-types";
import React, { Component } from "react";
import { connect } from "react-redux";
import Body from "./Body";
const logger = console;
import {CommonUsersActions } from "dnn-users-common-actions";
import { PersonaBarPage } from "@dnnsoftware/dnn-react-common";

class App extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
		logger.debug('<<< Exiting constructor()');
    }
    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        const {props} = this;
        props.dispatch(CommonUsersActions.getUsers({
            searchText: "",
            filter: 0,
            pageIndex: 0,
            pageSize: 10,
            sortColumn: "",
            sortAscending: false
        }));
		logger.debug('<<< Exiting componentDidMount()');
    }
    render() {
		logger.debug('>>> Entering render()');
		logger.debug('<<< Exiting render()');
        return (
            <div className="boilerplate-app personaBar-mainContainer">
                <PersonaBarPage isOpen={true}>
                    <Body />
                </PersonaBarPage>
            </div>
        );
    }
}

App.propTypes = {
    dispatch: PropTypes.func.isRequired
};


function mapStateToProps() {
	logger.debug('>>> Entering mapStateToProps()');
	logger.debug('<<< Exiting mapStateToProps()');
    return {};
}


export default connect(mapStateToProps)(App);