import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { 
    MultiLineInputWithError,
    SingleLineInputWithError,
    PersonaBarPageBody,
    GridCell,
    Button,
    InputGroup,
    RadioButtons
} from "@dnnsoftware/dnn-react-common";
import {
    vocabulary as VocabularyActions,
    pagination as PaginationActions
} from "../../actions";
import LocalizedResources from "../../resources";
import util from "../../utils";
import styles from "./style.module.less";
const logger = console;

class CreateVocabulary extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            term: {
                Name: "",
                Description: "",
                TypeId: 1,
                ScopeTypeId: 2
            },
            error: {
                name: true
            },
            triedToSubmit: false
        };
		logger.debug('<<< Exiting constructor()');
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        this.setState({
            term: {
                Name: "",
                Description: "",
                TypeId: 1,
                ScopeTypeId: 2
            }
        });
		logger.debug('<<< Exiting componentDidMount()');
    }

    onCloseVocabulary() {
		logger.debug('>>> Entering onCloseVocabulary()');
        const {props} = this;
        props.onCloseVocabulary();
		logger.debug('<<< Exiting onCloseVocabulary()');
    }


    onTermValueChange(key, event) {
		logger.debug(`>>> Entering onTermValueChange(key=${key},event=${event})`);
        const value = typeof event === "object" ? event.target.value : parseInt(event); //event is value

		logger.debug(`onTermValueChange(key=${key},event=${event}): value → ${value}`);
        let {state} = this;
        let {term} = this.state;
        term[key] = value;
        state.triedToSubmit = false;
		logger.debug(`onTermValueChange(key=${key},event=${event}): state.triedToSubmit → ${state.triedToSubmit}`);
        if (value === "" && key === "Name") {
            state.error["name"] = true;
        } else if (value !== "" && key === "Name") {
            state.error["name"] = false;
			logger.debug(`<<< Exiting onTermValueChange(key=${key},event=${event})`);
        }
        this.setState({
            state,
            term
        });
    }

    getNextPage(pageIndex, pageSize, scopeTypeId) {
		logger.debug(`>>> Entering getNextPage(pageIndex=${pageIndex},pageSize=${pageSize},scopeTypeId=${scopeTypeId})`);
		logger.debug(`<<< Exiting getNextPage(pageIndex=${pageIndex},pageSize=${pageSize},scopeTypeId=${scopeTypeId})`);
        return {
            pageIndex: pageIndex || 0,
            pageSize: pageSize,
            scopeTypeId: scopeTypeId || 0
        };
    }

    onAddNewVocabulary(event) {
		logger.debug(`>>> Entering onAddNewVocabulary(event=${event})`);
        event.preventDefault();
        const {props, state} = this;
        this.setState({
            triedToSubmit: true
        });
        if (state.error.name) {
            return;
        }

        let totalCount = props.totalCount;
        props.dispatch(VocabularyActions.addVocabulary(state.term, totalCount + 1, () => {
            this.onCloseVocabulary();
            props.dispatch(PaginationActions.loadTab(this.getNextPage(0, 10000, state.term.ScopeTypeId)));   //index acts as scopeTypeId 
        }));
		logger.debug(`<<< Exiting onAddNewVocabulary(event=${event})`);
    }

    render() {
		logger.debug('>>> Entering render()');
        const {props, state} = this;
        const typeOptions = [
            {
                label: LocalizedResources.get("Simple"),
                value: 1
            },
            {
                label: LocalizedResources.get("Hierarchy"),
                value: 2
            }
        ];

        const scopeOptions = [
            {
                label: LocalizedResources.get("Portal"),
                value: 2
            },
            {
                label: LocalizedResources.get("Application"),
                value: 1
            }
        ];
		logger.debug('<<< Exiting render()');
        return (
            //inline style for height to allow proper calculating
            <PersonaBarPageBody backToLinkProps={{
                text: LocalizedResources.get("BackToVocabularies"),
                onClick: this.onCloseVocabulary.bind(this)
            }}
            className={styles.createVocabulary} style={{ height: "calc(100% - 100px)" }}>
                {props.isOpen &&
                    <GridCell className="create-box">
                        <InputGroup>
                            <SingleLineInputWithError
                                inputId={"create-vocabulary-name"}
                                withLabel={true}
                                label={LocalizedResources.get("TermName") + "*"}
                                error={state.error.name && state.triedToSubmit}
                                errorMessage={LocalizedResources.get("TermValidationError.Message")}
                                value={state.term.Name}
                                onChange={this.onTermValueChange.bind(this, "Name")}
                            />
                        </InputGroup>
                        <InputGroup>
                            <MultiLineInputWithError
                                inputId={"create-vocabulary-description"}
                                withLabel={true}
                                label={LocalizedResources.get("Description")}
                                value={state.term.Description}
                                onChange={this.onTermValueChange.bind(this, "Description")} />
                        </InputGroup>
                        <RadioButtons
                            onChange={this.onTermValueChange.bind(this, "TypeId")}
                            options={typeOptions}
                            label={LocalizedResources.get("Type.Header") + ":"}
                            buttonGroup="vocabularyType"
                            buttonWidth={130}
                            value={state.term.TypeId} />
                        {util.isHost() && <RadioButtons
                            onChange={this.onTermValueChange.bind(this, "ScopeTypeId")}
                            options={scopeOptions}
                            label={LocalizedResources.get("Scope.Header") + ":"}
                            buttonGroup="scopeType"
                            buttonWidth={130}
                            value={state.term.ScopeTypeId} />}
                        <GridCell className="action-buttons">
                            <Button type="secondary" onClick={this.onCloseVocabulary.bind(this)}>{LocalizedResources.get("cancelCreate")}</Button>
                            <Button type="primary" onClick={this.onAddNewVocabulary.bind(this)}>{LocalizedResources.get("CreateVocabulary")}</Button>
                            <span className="required-help-text">* {LocalizedResources.get("RequiredField")}</span>
                        </GridCell>
                    </GridCell>
                }
            </PersonaBarPageBody>
        );
    }
}

CreateVocabulary.propTypes = {
    dispatch: PropTypes.func.isRequired,
    totalCount: PropTypes.number,
    onCloseVocabulary: PropTypes.func,
    isOpen: PropTypes.bool
};

function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        totalCount: state.vocabulary.totalCount,
        vocabularyAddedIsValid: state.vocabulary.vocabularyAddedIsValid,
        pagination: state.pagination
    };
}

export default connect(mapStateToProps)(CreateVocabulary);