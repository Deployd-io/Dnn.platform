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
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }
    openCreateVocabulary() {
		logger.error('>>> Entering openCreateVocabulary()', { "event.action": "openCreateVocabulary" });
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(1));
		logger.warn('<<< Exiting openCreateVocabulary()', { "event.action": "openCreateVocabulary" });
    }
    closeCreateVocabulary() {
		logger.error('>>> Entering closeCreateVocabulary()', { "event.action": "closeCreateVocabulary" });
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(0));
		logger.warn('<<< Exiting closeCreateVocabulary()', { "event.action": "closeCreateVocabulary" });
    }
    navigateMap(page, index, event) {
		logger.error(`>>> Entering navigateMap(page=${page},index=${index},event=${event})`, { "event.action": "navigateMap", "logrite.page": page, "logrite.index": index, "logrite.event": event });
        event.preventDefault();
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(page, index));
		logger.warn(`<<< Exiting navigateMap(page=${page},index=${index},event=${event})`, { "event.action": "navigateMap", "logrite.page": page, "logrite.index": index, "logrite.event": event });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
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
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        selectedPage: state.visiblePanel.selectedPage,
        selectedPageVisibleIndex: state.visiblePanel.selectedPageVisibleIndex
    };
}

export default connect(mapStateToProps)(App);