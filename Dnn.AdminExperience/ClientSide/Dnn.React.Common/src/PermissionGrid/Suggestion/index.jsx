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
		logger.debug(`>>> Entering constructor(props=${props})`);
        this.state = {
            suggestions: [],
            selectedValue: {value: -1, label: ""}
        };

		logger.debug(`constructor(props=${props}): this.state → ${this.state}`);
        this.debounceGetSuggestions = debounce(500, this.debounceGetSuggestions);
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }

    componentDidUpdate(prevProps) {
		logger.debug(`>>> Entering componentDidUpdate(prevProps=${prevProps})`);
        if (this.props.options !== prevProps.options) {
            this.setState({ selectedValue: { value: -1, label: "" }, suggestions: [] });
			logger.debug(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`);
        }
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        if (this.comboBoxDom.childNodes !== undefined) {
            this.comboBoxDom.childNodes[1].setAttribute("aria-label", "Suggestion");
			logger.debug('<<< Exiting componentDidMount()');
        }
    }

    getSuggestions() {
		logger.debug('>>> Entering getSuggestions()');
        const {props, state} = this;
        let keyword = state.selectedValue.value > -1 ? "" : state.selectedValue.label;
		logger.debug(`getSuggestions(): keyword → ${keyword}`);

        let service = new Service(this.props.service);
		logger.debug(`getSuggestions(): service → ${service}`);
        let actionName = props.options.actionName;
        let parameters  = Object.assign({}, props.options, {
            keyword: keyword,
            actionName: undefined
        });

        service.getSuggestions(actionName, parameters, (data) => {
			logger.debug('<<< Exiting getSuggestions()');
            this.setState({
                suggestions: data
            });
        });
    }

    debounceGetSuggestions() {
		logger.debug('>>> Entering debounceGetSuggestions()');
        this.getSuggestions();
		logger.debug('<<< Exiting debounceGetSuggestions()');
    }

    onUserSelectorChanged(item) {
		logger.debug(`>>> Entering onUserSelectorChanged(item=${item})`);
        if (item.value || item.label) {
            return;
        }

        this.setState({ selectedValue: { value: -1, label: item } });
        this.debounceGetSuggestions();
		logger.debug(`<<< Exiting onUserSelectorChanged(item=${item})`);
    }

    onUserSelectorSelected(item) {
		logger.debug(`>>> Entering onUserSelectorSelected(item=${item})`);
        this.setState({ selectedValue: { value: item.value, label: item.label }, suggestions: [] });
		logger.debug(`<<< Exiting onUserSelectorSelected(item=${item})`);
    }

    onUserSelectorToggle() {
	logger.debug('>>> Entering onUserSelectorToggle()');
    }

    onAddSuggestion() {
		logger.debug('>>> Entering onAddSuggestion()');
        const {props, state} = this;
        let value = state.selectedValue.value;
        if (value === -1) {
            return;
        }

        if (typeof props.onSelect === "function") {
            props.onSelect(state.selectedValue);
        }

        this.setState({ selectedValue: { value: -1, label: "" } });
		logger.debug('<<< Exiting onAddSuggestion()');
    }

    render() {
		logger.debug('>>> Entering render()');
        const {props, state} = this;

		logger.debug('<<< Exiting render()');
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