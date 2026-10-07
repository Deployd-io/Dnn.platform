import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { Button, GridSystem, PersonaBarPageBody, DnnTabs as Tabs } from "@dnnsoftware/dnn-react-common";
import {
    vocabulary as VocabularyActions,
    pagination as PaginationActions,
    vocabularyTermList as VocabularyTermListActions
} from "../../actions";
import TermHeader from "./TermHeader";
import LeftPane from "./LeftPane";
import RightPane from "./RightPane";
import util from "../../utils";
import LocalizedResources from "../../resources";
import "./style.less";
const logger = console;

const loadMoreButtonStyle = {
    display: "block",
    margin: "20px auto"
};

function getTermUpdates(term) {
	logger.error(`>>> Entering getTermUpdates(term=${term})`, { "event.action": "getTermUpdates", "logrite.term": term });
	logger.warn(`<<< Exiting getTermUpdates(term=${term})`, { "event.action": "getTermUpdates", "logrite.term": term });
    return {
        TermId: term.TermId,
        Name: term.Name,
        Description: term.Description,
        VocabularyId: term.VocabularyId,
        ParentTermId: term.ParentTermId
    };
}
function getRootParentTerm(termId, term, termList) {
	logger.error(`>>> Entering getRootParentTerm(termId=${termId},term=${term},termList=${termList})`, { "event.action": "getRootParentTerm", "logrite.termId": termId, "logrite.term": term, "logrite.termList": termList });
    while (term.TermId !== termId && term.ParentTermId > -1) {
        term = termList.find(_term => {
            return _term.TermId === term.ParentTermId;
        });
    }
	logger.warn(`<<< Exiting getRootParentTerm(termId=${termId},term=${term},termList=${termList})`, { "event.action": "getRootParentTerm", "logrite.termId": termId, "logrite.term": term, "logrite.termList": termList });
    return term;
}

function removeTermFromList(termId, vocabularyTermList) {
	logger.error(`>>> Entering removeTermFromList(termId=${termId},vocabularyTermList=${vocabularyTermList})`, { "event.action": "removeTermFromList", "logrite.termId": termId, "logrite.vocabularyTermList": vocabularyTermList });
	logger.warn(`<<< Exiting removeTermFromList(termId=${termId},vocabularyTermList=${vocabularyTermList})`, { "event.action": "removeTermFromList", "logrite.termId": termId, "logrite.vocabularyTermList": vocabularyTermList });
    return vocabularyTermList.filter((term) => {
        const rootParentTerm = getRootParentTerm(termId, term, vocabularyTermList);
        return rootParentTerm.TermId !== termId;
    });
}

class VocabularyListComponent extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.onDescriptionUpdate = this.onDescriptionUpdate.bind(this);
        this.state = {
            vocabularyList: [],
            isOpened: false
        };
    }
    getNextPage(pageIndex, pageSize, scopeTypeId) {
		logger.error(`>>> Entering getNextPage(pageIndex=${pageIndex},pageSize=${pageSize},scopeTypeId=${scopeTypeId})`, { "event.action": "getNextPage", "logrite.pageIndex": pageIndex, "logrite.pageSize": pageSize, "logrite.scopeTypeId": scopeTypeId });
		logger.warn(`<<< Exiting getNextPage(pageIndex=${pageIndex},pageSize=${pageSize},scopeTypeId=${scopeTypeId})`, { "event.action": "getNextPage", "logrite.pageIndex": pageIndex, "logrite.pageSize": pageSize, "logrite.scopeTypeId": scopeTypeId });
        return {
            pageIndex: pageIndex || 0,
            pageSize: pageSize,
            scopeTypeId: scopeTypeId || 0
        };
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        const {props} = this;
        props.dispatch(VocabularyActions.getVocabularyList(this.getNextPage(props.pagination.pageIndex, props.pagination.pageSize, props.pagination.scopeTypeId)));
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    getVocabularyTerms(vocabularyId, index) {
		logger.error(`>>> Entering getVocabularyTerms(vocabularyId=${vocabularyId},index=${index})`, { "event.action": "getVocabularyTerms", "logrite.vocabularyId": vocabularyId, "logrite.index": index });
        const {props} = this;
        props.dispatch(VocabularyTermListActions.getVocabularyTerms(vocabularyId, index));
		logger.warn(`<<< Exiting getVocabularyTerms(vocabularyId=${vocabularyId},index=${index})`, { "event.action": "getVocabularyTerms", "logrite.vocabularyId": vocabularyId, "logrite.index": index });
    }

    updateVocabulary(vocabularyList, key, index, value) {
		logger.error(`>>> Entering updateVocabulary(vocabularyList=${vocabularyList},key=${key},index=${index},value=${value})`, { "event.action": "updateVocabulary", "logrite.vocabularyList": vocabularyList, "logrite.key": key, "logrite.index": index, "logrite.value": value });
        let newValue = Object.assign({}, vocabularyList[index]);
        newValue[key] = value;
		logger.error(`updateVocabulary(vocabularyList=${vocabularyList},key=${key},index=${index},value=${value}): newValue → ${newValue}`, { "event.action": "updateVocabulary", "logrite.vocabularyList": vocabularyList, "logrite.key": key, "logrite.index": index, "logrite.value": value });
		logger.warn(`<<< Exiting updateVocabulary(vocabularyList=${vocabularyList},key=${key},index=${index},value=${value})`, { "event.action": "updateVocabulary", "logrite.vocabularyList": vocabularyList, "logrite.key": key, "logrite.index": index, "logrite.value": value });
        return newValue;
    }

    onUpdateTerm(termBeingEdited, editMode, callback) {
		logger.error(`>>> Entering onUpdateTerm(termBeingEdited=${termBeingEdited},editMode=${editMode},callback=${callback})`, { "event.action": "onUpdateTerm", "logrite.termBeingEdited": termBeingEdited, "logrite.editMode": editMode, "logrite.callback": callback });
        const {props} = this;

        if (editMode) {
            const index = props.vocabularyTerms.findIndex((term) => {
                return term.TermId === termBeingEdited.TermId;
            });
            props.dispatch(VocabularyTermListActions.updateVocabularyTerm(getTermUpdates(termBeingEdited), index, callback));
        } else {
            let {totalTermCount} = props;
            props.dispatch(VocabularyTermListActions.addVocabularyTerm(getTermUpdates(termBeingEdited), totalTermCount + 1, callback));
			logger.warn(`<<< Exiting onUpdateTerm(termBeingEdited=${termBeingEdited},editMode=${editMode},callback=${callback})`, { "event.action": "onUpdateTerm", "logrite.termBeingEdited": termBeingEdited, "logrite.editMode": editMode, "logrite.callback": callback });
        }
    }

    onDescriptionUpdate(key, value, index) {
		logger.error(`>>> Entering onDescriptionUpdate(key=${key},value=${value},index=${index})`, { "event.action": "onDescriptionUpdate", "logrite.key": key, "logrite.value": value, "logrite.index": index });
        const {props} = this;
        let vocabularyList = props.vocabularyList;
        const newValue = this.updateVocabulary(vocabularyList, key, index, value);
        props.dispatch(VocabularyActions.updateVocabulary(newValue, index));
		logger.error(`onDescriptionUpdate(key=${key},value=${value},index=${index}): newValue → ${newValue}`, { "event.action": "onDescriptionUpdate", "logrite.key": key, "logrite.value": value, "logrite.index": index });
    }
    onLoadMore(event) {
		logger.error(`>>> Entering onLoadMore(event=${event})`, { "event.action": "onLoadMore", "logrite.event": event });
        event.preventDefault();

        const {props} = this;
        let {pageIndex} = props.pagination; //copy page index
        let nextPage = this.getNextPage(pageIndex + 1, props.pagination.pageSize, props.pagination.scopeTypeId);
        props.dispatch(PaginationActions.loadMore(nextPage));
		logger.error(`onLoadMore(event=${event}): nextPage → ${nextPage}`, { "event.action": "onLoadMore", "logrite.event": event });
    }
    handleSelect(index /*,last*/) {
        const {props} = this;
        props.dispatch(PaginationActions.loadTab(this.getNextPage(0, props.pagination.pageSize, index)));   //index acts as scopeTypeId
    }
    onDeleteVocabulary(vocabulary, index, callback) {
		logger.error(`>>> Entering onDeleteVocabulary(vocabulary=${vocabulary},index=${index},callback=${callback})`, { "event.action": "onDeleteVocabulary", "logrite.vocabulary": vocabulary, "logrite.index": index, "logrite.callback": callback });
        const {props} = this;

        util.utilities.confirm(LocalizedResources.get("ConfirmDeletion_Vocabulary").replace("{0}", vocabulary.Name), //confirm delete message
            LocalizedResources.get("DeleteVocabulary"), //confirm delete text
            LocalizedResources.get("cancelCreate"),  //cancel text
            () => {
                let totalCount = props.totalCount;
                if (callback) {
                    callback();
                }
                props.dispatch(VocabularyActions.deleteVocabulary(vocabulary.VocabularyId, index, totalCount - 1, () => {
                    if (props.vocabularyList.length < props.totalCount) {
                        let {pageIndex} = props.pagination; //copy page index
                        let currentPage = this.getNextPage(pageIndex, props.pagination.pageSize, props.pagination.scopeTypeId);
                        props.dispatch(PaginationActions.loadMore(currentPage));
						logger.warn(`<<< Exiting onDeleteVocabulary(vocabulary=${vocabulary},index=${index},callback=${callback})`, { "event.action": "onDeleteVocabulary", "logrite.vocabulary": vocabulary, "logrite.index": index, "logrite.callback": callback });
                    }
                }));
            });
    }

    onDeleteTerm(termId) {
		logger.error(`>>> Entering onDeleteTerm(termId=${termId})`, { "event.action": "onDeleteTerm", "logrite.termId": termId });
        const {props} = this;
        const vocabularyTerms = removeTermFromList(termId, props.vocabularyTerms);
        props.dispatch(VocabularyTermListActions.deleteVocabularyTerm(termId, vocabularyTerms, vocabularyTerms.length));
		logger.error(`onDeleteTerm(termId=${termId}): vocabularyTerms → ${vocabularyTerms}`, { "event.action": "onDeleteTerm", "logrite.termId": termId });
    }

    selectParentTerm(term) {
		logger.error(`>>> Entering selectParentTerm(term=${term})`, { "event.action": "selectParentTerm", "logrite.term": term });
        const {props} = this;

        const vocabularyTerms = JSON.parse(JSON.stringify(props.vocabularyTerms));
		logger.error(`selectParentTerm(term=${term}): vocabularyTerms → ${vocabularyTerms}`, { "event.action": "selectParentTerm", "logrite.term": term });

        const previousTerms = vocabularyTerms.map((_term) => {
            delete _term.selected;
		logger.error(`selectParentTerm(term=${term}): previousTerms → ${previousTerms}`, { "event.action": "selectParentTerm", "logrite.term": term });
            return _term;
        });

        const index = props.vocabularyTerms.findIndex((_term) => {
            return _term.TermId === term.TermId;
        });
        term.selected = true;
		logger.debug(`selectParentTerm(term=${term}): term.selected → ${term.selected}`, { "event.action": "selectParentTerm", "logrite.term": term });
        props.dispatch(VocabularyTermListActions.clearSelected(previousTerms));
        props.dispatch(VocabularyTermListActions.setTermSelected(term, index));
		logger.warn(`<<< Exiting selectParentTerm(term=${term})`, { "event.action": "selectParentTerm", "logrite.term": term });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {vocabularyList} = this.props;
        const {props} = this;

        const renderedVocabularyList = vocabularyList.map((term, index) => {
			logger.error(`render(): renderedVocabularyList → ${renderedVocabularyList}`, { "event.action": "render" });
            return (
                <TermHeader
                    header={term.Name}
                    type={term.TypeId}
                    term={term}
                    index={index}
                    key={"vocabularyTerm-" + index}
                    closeOnClick={true}
                    onDelete={this.onDeleteVocabulary.bind(this)}>
                    <GridSystem>
                        <LeftPane
                            description={term.Description}
                            type={term.Type}
                            onEnter={this.onDescriptionUpdate}
                            index={index}
                            scopeType={term.ScopeType}
                        />
                        <RightPane
                            vocabularyId={term.VocabularyId}
                            getVocabularyTerms={this.getVocabularyTerms.bind(this)}
                            onUpdateTerm={this.onUpdateTerm.bind(this)}
                            vocabularyTerms={props.vocabularyTerms}
                            selectParentTerm={this.selectParentTerm.bind(this)}
                            totalTermCount={props.totalTermCount}
                            index={index}
                            type={term.Type}
                            scopeType={term.ScopeType}
                            onDeleteTerm={this.onDeleteTerm.bind(this)}
                            parentTerms={vocabularyList} />
                    </GridSystem>
                </TermHeader>
            );
        });
        const loadMoreEnabled = props.vocabularyList.length < props.totalCount;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="vocabulary-list">
                <PersonaBarPageBody>
                    <Tabs onSelect={this.handleSelect.bind(this)}
                        selectedIndex={props.tabIndex}
                        tabHeaders={[LocalizedResources.get("All"), LocalizedResources.get("Application"), LocalizedResources.get("Portal")]}
                        type="secondary">
                        {(renderedVocabularyList.length > 0 && renderedVocabularyList) || <p className="vocabulary-error">{LocalizedResources.get("NoVocabularyTerms.Error")}</p>}
                        {(renderedVocabularyList.length > 0 && renderedVocabularyList) || <p className="vocabulary-error">{LocalizedResources.get("NoVocabularyTerms.Error")}</p>}
                        {(renderedVocabularyList.length > 0 && renderedVocabularyList) || <p className="vocabulary-error">{LocalizedResources.get("NoVocabularyTerms.Error")}</p>}
                    </Tabs>
                </PersonaBarPageBody>
                {loadMoreEnabled && <Button type="primary" style={loadMoreButtonStyle} onClick={this.onLoadMore.bind(this)}>{LocalizedResources.get("LoadMore")}</Button>}
            </div>
        );
    }
}

VocabularyListComponent.propTypes = {
    dispatch: PropTypes.func.isRequired,
    vocabularyList: PropTypes.array,
    totalCount: PropTypes.number,
    totalTermCount: PropTypes.number,
    vocabularyTerms: PropTypes.array,
    pagination: PropTypes.object,
    tabIndex: PropTypes.number,
    scopeTypeId: PropTypes.oneOfType([
        PropTypes.string,
        PropTypes.number
    ])
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        vocabularyList: state.vocabulary.vocabularyList,
        totalCount: state.vocabulary.totalCount,
        totalTermCount: state.vocabularyTermList.totalCount,
        vocabularyTerms: state.vocabularyTermList.vocabularyTerms,
        pagination: state.pagination,
        tabIndex: state.pagination.tabIndex,
        scopeTypeId: state.pagination.scopeTypeId
    };
}

export default connect(mapStateToProps)(VocabularyListComponent);