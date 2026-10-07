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
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }
    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        const {props} = this;
        props.dispatch(CommonUsersActions.getUsers({
            searchText: "",
            filter: 0,
            pageIndex: 0,
            pageSize: 10,
            sortColumn: "",
            sortAscending: false
        }));
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
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
	logger.error('>>> Entering mapStateToProps()', { "event.action": "mapStateToProps" });
	logger.warn('<<< Exiting mapStateToProps()', { "event.action": "mapStateToProps" });
    return {};
}


export default connect(mapStateToProps)(App);