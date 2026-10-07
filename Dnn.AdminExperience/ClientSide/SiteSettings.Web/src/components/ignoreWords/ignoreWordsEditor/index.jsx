import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import "./style.less";
import { Label, Button, Tags, InputGroup } from "@dnnsoftware/dnn-react-common";
import {
    search as SearchActions
} from "../../../actions";
import util from "../../../utils";
import resx from "../../../resources";
const logger = console;

class IgnoreWordsEditor extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });

        this.state = {
            words: {
                StopWords: "",
                CultureCode: "en-US"
            },            
            error: {
                tags: true
            },
            triedToSubmit: false
        };
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        const {props} = this;
        let words = Object.assign({}, props.words);
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
        words.CultureCode = props.culture;
        this.setState({
            words: Object.assign({}, props.words)
        });
    }

    onSettingChange(key, event) {
		logger.error(`>>> Entering onSettingChange(key=${key},event=${event})`, { "event.action": "onSettingChange", "logrite.key": key, "logrite.event": event });
        let {state, props} = this;
        let words = Object.assign({}, state.words);
		logger.error(`onSettingChange(key=${key},event=${event}): words → ${words}`, { "event.action": "onSettingChange", "logrite.key": key, "logrite.event": event });
        
        words[key] = event.join();

        if (words[key] === "" && key === "StopWords") {
            state.error["tags"] = true;
			logger.warn(`<<< Exiting onSettingChange(key=${key},event=${event})`, { "event.action": "onSettingChange", "logrite.key": key, "logrite.event": event });
        }
        else if (words[key] !== "" && key === "StopWords") {
            state.error["tags"] = false;
        }

        words.CultureCode = props.culture;

        this.setState({
            words: words,
            triedToSubmit: false,
            error: state.error
        });

        props.dispatch(SearchActions.ignoreWordsClientModified());
    }

    onSave() {
		logger.error('>>> Entering onSave()', { "event.action": "onSave" });
        const {props, state} = this;
        this.setState({
            triedToSubmit: true
        });
        if (state.error.tags) {
            return;
        }

        props.onUpdate(state.words, state.culture);
		logger.warn('<<< Exiting onSave()', { "event.action": "onSave" });
    }
    
    onCancel() {
		logger.error('>>> Entering onCancel()', { "event.action": "onCancel" });
        const {props} = this;
        if (props.ignoreWordsClientModified) {
            util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
                props.dispatch(SearchActions.cancelIgnoreWordsClientModified());
                props.Collapse();
            });
        }
        else {
            props.Collapse();
			logger.warn('<<< Exiting onCancel()', { "event.action": "onCancel" });
        }
    }

     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
         
        if (this.state.words !== undefined || this.props.id === "add") {  
            return (
                <div className="words-editor">
                    <InputGroup>
                        <Label
                            label={resx.get("IgnoreWords")}
                        />
                        <Tags
                            tags={this.state.words.StopWords ? this.state.words.StopWords.split(",") : []}
                            onUpdateTags={this.onSettingChange.bind(this, "StopWords")}
                        />
                    </InputGroup>
                    <div className="editor-buttons-box">
                        <Button
                            type="neutral"
                            onClick={this.onCancel.bind(this)}>
                            {resx.get("Cancel")}
                        </Button>                        
                        <Button
                            type="primary"
                            onClick={this.onSave.bind(this)}>
                            {resx.get("Save")}
                        </Button>
                    </div>
                </div>
            );
        }
        else return <div />;
    }
}

IgnoreWordsEditor.propTypes = {
    dispatch: PropTypes.func.isRequired,
    words: PropTypes.object,
    Collapse: PropTypes.func,
    onUpdate: PropTypes.func,
    id: PropTypes.string,
    culture: PropTypes.string,
    ignoreWordsClientModified: PropTypes.bool
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {              
        ignoreWordsClientModified: state.search.ignoreWordsClientModified
    };
}

export default connect(mapStateToProps)(IgnoreWordsEditor);