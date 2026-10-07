import React, { Component } from "react";
import PropTypes from "prop-types";
import { Scrollbars } from "react-custom-scrollbars";
import AddTermBox from "./AddTermBox";
import Term from "./Term";
import util from "../../../utils";
import LocalizedResources from "../../../resources";
import styles from "./style.module.less";
const logger = console;
import { GridCell, SvgIcons } from "@dnnsoftware/dnn-react-common";

function findInChildren(list, parentTermId) {
	logger.error(`>>> Entering findInChildren(list=${list},parentTermId=${parentTermId})`, { "event.action": "findInChildren", "logrite.list": list, "logrite.parentTermId": parentTermId });
    if (!list) {
        return null;
    }
    let parentTerm = null;
    for (let i = 0; i < list.length && !parentTerm; i++) {
        const term = list[i];
        parentTerm = term.TermId === parentTermId
            ? term
            : findInChildren(term.ChildTerms, parentTermId);
    }
	logger.warn(`<<< Exiting findInChildren(list=${list},parentTermId=${parentTermId})`, { "event.action": "findInChildren", "logrite.list": list, "logrite.parentTermId": parentTermId });
    return parentTerm;
}

function getVocabularyTermList(terms, type) {
	logger.error(`>>> Entering getVocabularyTermList(terms=${terms},type=${type})`, { "event.action": "getVocabularyTermList", "logrite.terms": terms, "logrite.type": type });
    if (type === "Simple") {
        return terms;
    }
    let _terms = [];
    terms.forEach((term) => {
        if (term.ParentTermId < 0) {
            term.ChildTerms = [];
            if (!_terms.find((_term) => { return _term.TermId === term.TermId; })) {
                _terms = _terms.concat(term);
            }
        }
        else {
            let parent = findInChildren(_terms, term.ParentTermId);
            if (parent) {
                if (!parent.ChildTerms) {
                    parent.ChildTerms = [];
                }
                if (!parent.ChildTerms.find((_term) => { return _term.TermId === term.TermId; })) {
                    parent.ChildTerms = parent.ChildTerms.concat(term);
                }
            }
        }
    });
    return _terms;
}

function removeChildTerms(terms) {
	logger.error(`>>> Entering removeChildTerms(terms=${terms})`, { "event.action": "removeChildTerms", "logrite.terms": terms });
	logger.warn(`<<< Exiting removeChildTerms(terms=${terms})`, { "event.action": "removeChildTerms", "logrite.terms": terms });
    return terms.map((term) => {
        delete term.ChildTerms;
        return term;
    });
}

class RightPane extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            editBoxOpened: false,
            _editBoxOpened: false,
            editMode: false,
            parentTreeOpened: false,
            open: false,
            termBeingEdited: {
                TermId: -1,
                Name: "",
                Description: "",
                VocabularyId: -1,
                ParentTermId: -1
            },
            nameError: true,
            triedToSubmitTerm: false
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }
    UNSAFE_componentWillMount() {
		logger.error('>>> Entering UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        const {termBeingEdited} = this.state;
        const {props} = this;

		logger.warn('<<< Exiting UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        termBeingEdited.VocabularyId = props.vocabularyId;
		logger.debug(`UNSAFE_componentWillMount(): termBeingEdited.VocabularyId → ${termBeingEdited.VocabularyId}`, { "event.action": "UNSAFE_componentWillMount" });
        this.setState({
            termBeingEdited
        });
    }
    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        const {props} = this;
        props.getVocabularyTerms(props.vocabularyId, props.index);
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    openAddTerm(editMode, type, event) {
	logger.error(`>>> Entering openAddTerm(editMode=${editMode},type=${type},event=${event})`, { "event.action": "openAddTerm", "logrite.editMode": editMode, "logrite.type": type, "logrite.event": event });

        if (event) {
            event.preventDefault();
        }
        let {termBeingEdited} = this.state;
		logger.debug(`openAddTerm(editMode=${editMode},type=${type},event=${event}): termBeingEdited → ${termBeingEdited}`, { "event.action": "openAddTerm", "logrite.editMode": editMode, "logrite.type": type, "logrite.event": event });
        const {props} = this;
        termBeingEdited = {
            TermId: -1,
            Name: "",
            Description: "",
            VocabularyId: props.vocabularyId,
            ParentTermId: type === "Hierarchy" ? ((props.vocabularyTerms.length > 0) && props.vocabularyTerms[0].TermId || -1) : -1
        };
        if (!editMode) {
            delete termBeingEdited.TermId;
        }
        this.setState({
            editBoxOpened: true,
            termBeingEdited,
            editMode,
            triedToSubmitTerm: false,
            nameError: true
        });
        setTimeout(() => {
            this.setState({
                _editBoxOpened: true
            });
        }, 500);
        let parentTerm = JSON.parse(JSON.stringify(props.vocabularyTerms)).find((term) => {
            return term.TermId === termBeingEdited.ParentTermId;
        });
        if (parentTerm) {
            parentTerm.selected = true;
			logger.debug(`openAddTerm(editMode=${editMode},type=${type},event=${event}): parentTerm.selected → ${parentTerm.selected}`, { "event.action": "openAddTerm", "logrite.editMode": editMode, "logrite.type": type, "logrite.event": event });
            props.selectParentTerm(parentTerm);
			logger.warn(`<<< Exiting openAddTerm(editMode=${editMode},type=${type},event=${event})`, { "event.action": "openAddTerm", "logrite.editMode": editMode, "logrite.type": type, "logrite.event": event });
        }
    }

    closeAddTerm() {
		logger.error('>>> Entering closeAddTerm()', { "event.action": "closeAddTerm" });
        const {props} = this;
        setTimeout(() => {
			logger.warn('<<< Exiting closeAddTerm()', { "event.action": "closeAddTerm" });
            this.setState({
                termBeingEdited: {
                    TermId: -1,
                    Name: "",
                    Description: "",
                    VocabularyId: props.vocabularyId,
                    ParentTermId: props.type === "Hierarchy" ? ((props.vocabularyTerms.length > 0) && props.vocabularyTerms[0].TermId || -1) : -1
                },
                editBoxOpened: false
            });
        }, 500);
        this.setState({
            _editBoxOpened: false
        });
    }

    toggleParentTree(event) {
		logger.error(`>>> Entering toggleParentTree(event=${event})`, { "event.action": "toggleParentTree", "logrite.event": event });
        if (event) {
            event.preventDefault();
			logger.warn(`<<< Exiting toggleParentTree(event=${event})`, { "event.action": "toggleParentTree", "logrite.event": event });
        }
        const {state} = this;
        this.setState({
            parentTreeOpened: !state.parentTreeOpened
        });
    }

    onTermValueChange(key, event) {
		logger.error(`>>> Entering onTermValueChange(key=${key},event=${event})`, { "event.action": "onTermValueChange", "logrite.key": key, "logrite.event": event });
        const value = event.target.value;

        const { state } = this;
		logger.debug(`onTermValueChange(key=${key},event=${event}): state.triedToSubmitTerm → ${state.triedToSubmitTerm}`, { "event.action": "onTermValueChange", "logrite.key": key, "logrite.event": event });
		logger.debug(`onTermValueChange(key=${key},event=${event}): state.nameError → ${state.nameError}`, { "event.action": "onTermValueChange", "logrite.key": key, "logrite.event": event });

        state.termBeingEdited[key] = value;
        state.triedToSubmitTerm = false;

        if (key === "Name" && value === "") {
            state.nameError = true;
        } else {
            state.nameError = false;
			logger.warn(`<<< Exiting onTermValueChange(key=${key},event=${event})`, { "event.action": "onTermValueChange", "logrite.key": key, "logrite.event": event });
        }

        this.setState({
            state
        });
    }

    onUpdateTerm(event) {
		logger.error(`>>> Entering onUpdateTerm(event=${event})`, { "event.action": "onUpdateTerm", "logrite.event": event });
        event.preventDefault();
        const {props, state} = this;
        if (props.type === "Simple") {
            delete state.termBeingEdited.ParentTermId;
        }
        this.setState({
            triedToSubmitTerm: true
        });
        if (state.nameError) {
            return;
        }
        props.onUpdateTerm(state.termBeingEdited, state.editMode, () => {
            this.closeAddTerm();
        });
		logger.warn(`<<< Exiting onUpdateTerm(event=${event})`, { "event.action": "onUpdateTerm", "logrite.event": event });
    }

    onEditTerm(term) {
		logger.error(`>>> Entering onEditTerm(term=${term})`, { "event.action": "onEditTerm", "logrite.term": term });
        const {props} = this;

        if (!this.canEdit()) {
            return;
        }

        this.openAddTerm(true);
        let {termBeingEdited} = this.state;
        termBeingEdited = term;
        this.setState({
            termBeingEdited,
            triedToSubmitTerm: false,
            nameError: false
        });
        let parentTerm = JSON.parse(JSON.stringify(props.vocabularyTerms)).find((term) => {
            return term.TermId === termBeingEdited.ParentTermId;
        });
        if (parentTerm) {
            parentTerm.selected = true;
			logger.debug(`onEditTerm(term=${term}): parentTerm.selected → ${parentTerm.selected}`, { "event.action": "onEditTerm", "logrite.term": term });
            props.selectParentTerm(parentTerm);
        }
		logger.warn(`<<< Exiting onEditTerm(term=${term})`, { "event.action": "onEditTerm", "logrite.term": term });
    }

    getChildTerms(term, clickFunction, isEditable) {
		logger.error(`>>> Entering getChildTerms(term=${term},clickFunction=${clickFunction},isEditable=${isEditable})`, { "event.action": "getChildTerms", "logrite.term": term, "logrite.clickFunction": clickFunction, "logrite.isEditable": isEditable });
        let children = [];
        if (term.ChildTerms) {
            children = term.ChildTerms.map((child) => {
                let _children = this.getChildTerms(child, clickFunction, isEditable);
                return <ul className="term-ul" key={"ul-" + term.TermId}>
                    <Term
                        term={child}
                        onClick={clickFunction}
                        isEditable={isEditable}
                        key={"term-" + term.TermId}>
                        {_children}
                    </Term>
                </ul>;
            });
        }
		logger.warn(`<<< Exiting getChildTerms(term=${term},clickFunction=${clickFunction},isEditable=${isEditable})`, { "event.action": "getChildTerms", "logrite.term": term, "logrite.clickFunction": clickFunction, "logrite.isEditable": isEditable });
        return children;
    }

    onSelectParent(term) {
		logger.error(`>>> Entering onSelectParent(term=${term})`, { "event.action": "onSelectParent", "logrite.term": term });
        let {termBeingEdited} = this.state;
        const {props} = this;
        termBeingEdited.ParentTermId = term.TermId;
		logger.debug(`onSelectParent(term=${term}): termBeingEdited.ParentTermId → ${termBeingEdited.ParentTermId}`, { "event.action": "onSelectParent", "logrite.term": term });
        this.setState({
            termBeingEdited
        }, () => {
            this.toggleParentTree();
            props.selectParentTerm(term);
        });
		logger.warn(`<<< Exiting onSelectParent(term=${term})`, { "event.action": "onSelectParent", "logrite.term": term });
    }

    deleteTerm() {
		logger.error('>>> Entering deleteTerm()', { "event.action": "deleteTerm" });
        const {props, state} = this;
        util.utilities.confirm(
            LocalizedResources.get("ConfirmDeletion_Term").replace("{0}", state.termBeingEdited.Name), //confirm message
            LocalizedResources.get("DeleteTerm"), //delete button text
            LocalizedResources.get("cancelCreate"), //cancel text
            () => {
                props.onDeleteTerm(state.termBeingEdited.TermId);
                this.closeAddTerm();
            });
			logger.warn('<<< Exiting deleteTerm()', { "event.action": "deleteTerm" });
    }

    toggle() {
		logger.error('>>> Entering toggle()', { "event.action": "toggle" });
        this.setState({
            open: !this.state.open
        });
		logger.warn('<<< Exiting toggle()', { "event.action": "toggle" });
    }

    canEdit() {
		logger.error('>>> Entering canEdit()', { "event.action": "canEdit" });
        const {props} = this;
		logger.warn('<<< Exiting canEdit()', { "event.action": "canEdit" });
        return util.isHost() || (props.scopeType === "Portal" && util.canEdit());
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props, state} = this;

        if (!props.vocabularyTerms) {
            return (
                <p>Empty</p>
            );
        }
        let _vocabularyTerms = JSON.parse(JSON.stringify(props.vocabularyTerms));
		logger.error(`render(): _vocabularyTerms → ${_vocabularyTerms}`, { "event.action": "render" });
        const vocabularyTerms = getVocabularyTermList(_vocabularyTerms, props.type);
		logger.error(`render(): vocabularyTerms → ${vocabularyTerms}`, { "event.action": "render" });
        const terms = vocabularyTerms.map((term) => {
			logger.error(`render(): terms → ${terms}`, { "event.action": "render" });
            let children = this.getChildTerms(term, this.onEditTerm.bind(this), true);
            return <Term
                term={term}
                onClick={this.onEditTerm.bind(this)}
                isEditable={this.canEdit()}
                key={"term-" + term.TermId}>
                {children}
            </Term>;
        });

        let parentTermTree = getVocabularyTermList(removeChildTerms(_vocabularyTerms).filter((term) => {
			logger.error(`render(): parentTermTree → ${parentTermTree}`, { "event.action": "render" });
            return term.TermId !== state.termBeingEdited.TermId;
        }), props.type, state.termBeingEdited.TermId);

        parentTermTree = parentTermTree.map((term) => {
            let children = this.getChildTerms(term, this.onSelectParent.bind(this), false);
            return <Term
                term={term}
                onClick={this.onSelectParent.bind(this)}
                isEditable={false}
                key={"term-" + term.TermId}>
                {children}
            </Term>;
        });

        const parentDisplay = props.vocabularyTerms.find((term) => {
			logger.error(`render(): parentDisplay → ${parentDisplay}`, { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
            return term.TermId === state.termBeingEdited.ParentTermId;
        });
         
        return (
            <GridCell className={styles.vocabulariesRightPane}>
                <GridCell className="term-list">
                    {state.editBoxOpened && <AddTermBox
                        isOpened={state._editBoxOpened}
                        editMode={state.editMode}
                        error={state.triedToSubmitTerm && state.nameError}
                        termBeingEdited={state.termBeingEdited}
                        termTreeVisible={(props.type === "Hierarchy" && vocabularyTerms.length > 0 && (!state.editMode || state.termBeingEdited.ParentTermId > 0))}
                        parentDisplay={parentDisplay}
                        parentTermTree={parentTermTree}
                        parentTreeOpened={state.parentTreeOpened}
                        toggleParentTree={this.toggleParentTree.bind(this)}
                        onTermValueChange={this.onTermValueChange.bind(this)}
                        deleteTerm={this.deleteTerm.bind(this)}
                        closeAddTerm={this.closeAddTerm.bind(this)}
                        onUpdateTerm={this.onUpdateTerm.bind(this)}
                    />}
                    {!state._editBoxOpened && <GridCell className={"term-list-content " + (!this.state.editBoxOpened ? "open" : "closed")}>
                        <span className="term-list-label">{LocalizedResources.get("Terms") + " (" + props.totalTermCount + ")"}</span>
                        {this.canEdit() &&
                            <div className="add-term-button do-not-close"
                                onClick={this.openAddTerm.bind(this, false, props.type)}>
                                <SvgIcons.AddIcon /> {LocalizedResources.get("AddTerm")}
                            </div>
                        }
                        <Scrollbars style={{ width: "345px", height: "300px", border: "1px solid #DBDBDB", marginTop: 10 }}>
                            <ul className={"term-ul root-level term-list-level " + props.type}>
                                {terms}
                            </ul>
                        </Scrollbars>
                    </GridCell>}
                </GridCell>
            </GridCell>
        );
    }
}


RightPane.propTypes = {
    vocabularyTerms: PropTypes.array,
    totalTermCount: PropTypes.number,
    type: PropTypes.string,
    scopeType: PropTypes.string,
    index: PropTypes.number,
    selectParentTerm: PropTypes.func,
    vocabularyId: PropTypes.number,
    getVocabularyTerms: PropTypes.func,
    onDeleteTerm: PropTypes.func,
    onUpdateTerm: PropTypes.func
};

export default RightPane;