import React, { Component } from "react";
import PropTypes from "prop-types";
import {debounce} from "throttle-debounce";
import Combobox from "react-widgets/Combobox";
import Service from "./Service";
import IconButton from "../../IconButton";

import "./style.less";
const logger = console;

class Suggestion extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        this.state = {
            suggestions: [],
            selectedValue: {value: -1, label: ""}
        };

		logger.debug(`constructor(props=${props}): this.state → ${this.state}`, { "event.action": "constructor", "logrite.props": props });
        this.debounceGetSuggestions = debounce(500, this.debounceGetSuggestions);
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }

    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        if (this.props.options !== prevProps.options) {
            this.setState({ selectedValue: { value: -1, label: "" }, suggestions: [] });
			logger.warn(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        }
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        if (this.comboBoxDom.childNodes !== undefined) {
            this.comboBoxDom.childNodes[1].setAttribute("aria-label", "Suggestion");
			logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
        }
    }

    getSuggestions() {
		logger.error('>>> Entering getSuggestions()', { "event.action": "getSuggestions" });
        const {props, state} = this;
        let keyword = state.selectedValue.value > -1 ? "" : state.selectedValue.label;
		logger.error(`getSuggestions(): keyword → ${keyword}`, { "event.action": "getSuggestions" });

        let service = new Service(this.props.service);
		logger.error(`getSuggestions(): service → ${service}`, { "event.action": "getSuggestions" });
        let actionName = props.options.actionName;
        let parameters  = Object.assign({}, props.options, {
            keyword: keyword,
            actionName: undefined
        });

        service.getSuggestions(actionName, parameters, (data) => {
			logger.warn('<<< Exiting getSuggestions()', { "event.action": "getSuggestions" });
            this.setState({
                suggestions: data
            });
        });
    }

    debounceGetSuggestions() {
		logger.error('>>> Entering debounceGetSuggestions()', { "event.action": "debounceGetSuggestions" });
        this.getSuggestions();
		logger.warn('<<< Exiting debounceGetSuggestions()', { "event.action": "debounceGetSuggestions" });
    }

    onUserSelectorChanged(item) {
		logger.error(`>>> Entering onUserSelectorChanged(item=${item})`, { "event.action": "onUserSelectorChanged", "logrite.item": item });
        if (item.value || item.label) {
            return;
        }

        this.setState({ selectedValue: { value: -1, label: item } });
        this.debounceGetSuggestions();
		logger.warn(`<<< Exiting onUserSelectorChanged(item=${item})`, { "event.action": "onUserSelectorChanged", "logrite.item": item });
    }

    onUserSelectorSelected(item) {
		logger.error(`>>> Entering onUserSelectorSelected(item=${item})`, { "event.action": "onUserSelectorSelected", "logrite.item": item });
        this.setState({ selectedValue: { value: item.value, label: item.label }, suggestions: [] });
		logger.warn(`<<< Exiting onUserSelectorSelected(item=${item})`, { "event.action": "onUserSelectorSelected", "logrite.item": item });
    }

    onUserSelectorToggle() {
	logger.error('>>> Entering onUserSelectorToggle()', { "event.action": "onUserSelectorToggle" });
    }

    onAddSuggestion() {
		logger.error('>>> Entering onAddSuggestion()', { "event.action": "onAddSuggestion" });
        const {props, state} = this;
        let value = state.selectedValue.value;
        if (value === -1) {
            return;
        }

        if (typeof props.onSelect === "function") {
            props.onSelect(state.selectedValue);
        }

        this.setState({ selectedValue: { value: -1, label: "" } });
		logger.warn('<<< Exiting onAddSuggestion()', { "event.action": "onAddSuggestion" });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props, state} = this;

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="dnn-suggestion">
                <span>
                    <Combobox suggest={false}
                        ref={(dom) => {this.comboBoxDom = dom;}}
                        placeholder={props.localization.placeHolder}
                        open={this.state.suggestions.length > 0 }
                        onToggle={this.onUserSelectorToggle.bind(this) }
                        onChange={this.onUserSelectorChanged.bind(this) }
                        onSelect={this.onUserSelectorSelected.bind(this) }
                        data={this.state.suggestions }
                        value={state.selectedValue.label}
                        valueField="value"
                        textField="label"/>
                    <div className="add-button" onClick={this.onAddSuggestion.bind(this) }>
                        <IconButton type="add" width={17} height={17}/> {props.localization.add}
                    </div>
                </span>
            </div>
        );
    }
}

Suggestion.propTypes = {
    dispatch: PropTypes.func,
    service: PropTypes.object,
    localization: PropTypes.object,
    onSelect: PropTypes.func.isRequired,
    options: PropTypes.object.isRequired
};

Suggestion.defaultProps = {
};

export default Suggestion;