import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { Button, PersonaBarPageHeader, PersonaBarPage } from "@dnnsoftware/dnn-react-common";
import VocabularyList from "./VocabularyList";
import CreateVocabulary from "./CreateVocabulary";
import { visiblePanel as VisiblePanelActions } from "../actions";
import LocalizedResources from "../resources";
import util from "utils";
const logger = console;

class App extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
		logger.debug('<<< Exiting constructor()');
    }
    openCreateVocabulary() {
		logger.debug('>>> Entering openCreateVocabulary()');
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(1));
		logger.debug('<<< Exiting openCreateVocabulary()');
    }
    closeCreateVocabulary() {
		logger.debug('>>> Entering closeCreateVocabulary()');
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(0));
		logger.debug('<<< Exiting closeCreateVocabulary()');
    }
    navigateMap(page, index, event) {
		logger.debug(`>>> Entering navigateMap(page=${page},index=${index},event=${event})`);
        event.preventDefault();
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(page, index));
		logger.debug(`<<< Exiting navigateMap(page=${page},index=${index},event=${event})`);
    }

    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;
		logger.debug('<<< Exiting render()');
        return (
            <div className="taxonomy-app">
                <PersonaBarPage isOpen={props.selectedPage === 0}>
                    <PersonaBarPageHeader title={LocalizedResources.get("ControlTitle_")}>
                        {util.canEdit() && <Button type="primary" size="large" onClick={this.openCreateVocabulary.bind(this)}>{LocalizedResources.get("Create")}</Button>}
                    </PersonaBarPageHeader>
                    <VocabularyList />
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === 1}>
                    <PersonaBarPageHeader
                        title={LocalizedResources.get("Create")}
                        onCreateVocabulary={this.openCreateVocabulary.bind(this)}
                    />
                    {props.selectedPage === 1 && <CreateVocabulary onCloseVocabulary={this.closeCreateVocabulary.bind(this)} isOpen={props.selectedPage === 1} />}
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        selectedPage: state.visiblePanel.selectedPage,
        selectedPageVisibleIndex: state.visiblePanel.selectedPageVisibleIndex
    };
}

export default connect(mapStateToProps)(App);