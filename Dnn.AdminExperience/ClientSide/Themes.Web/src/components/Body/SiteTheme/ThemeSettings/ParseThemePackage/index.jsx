import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    theme as ThemeActions
} from "actions";
import Localization from "localization";
import { Button, RadioButtons } from "@dnnsoftware/dnn-react-common";
import utils from "utils";

import "./style.less";
const logger = console;

/*eslint-disable eqeqeq*/
class ParseThemePackage extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            parseType: "localized",
            parsing: false
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    getParseType() {
		logger.error('>>> Entering getParseType()', { "event.action": "getParseType" });
        const {state} = this;

		logger.warn('<<< Exiting getParseType()', { "event.action": "getParseType" });
        return state.parseType == "portable" ? 1 : 0;
    }

    parseTheme() {
		logger.error('>>> Entering parseTheme()', { "event.action": "parseTheme" });
        this.setState({ parsing: true }, function () {
            this.parseLayout();
        });
		logger.warn('<<< Exiting parseTheme()', { "event.action": "parseTheme" });
    }

    parseLayout() {
		logger.error('>>> Entering parseLayout()', { "event.action": "parseLayout" });
        const {props} = this;

        let themeName = props.currentTheme.SiteLayout.themeName;
        let parseType = this.getParseType();
		logger.error(`parseLayout(): parseType → ${parseType}`, { "event.action": "parseLayout" });

        let self = this;
        props.dispatch(ThemeActions.parseTheme(themeName, parseType, function () {
            self.setState({ parsing: false });
            utils.utilities.notify(Localization.get("Successful"));
			logger.warn('<<< Exiting parseLayout()', { "event.action": "parseLayout" });
        }));
    }

    onParseTypeChanged(type) {
		logger.error(`>>> Entering onParseTypeChanged(type=${type})`, { "event.action": "onParseTypeChanged", "logrite.type": type });
        this.setState({ parseType: type });
		logger.warn(`<<< Exiting onParseTypeChanged(type=${type})`, { "event.action": "onParseTypeChanged", "logrite.type": type });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {state} = this;

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="parse-theme-package">
                <Button size="small"
                    onClick={this.parseTheme.bind(this)}
                    disabled={state.parsing}>{Localization.get("ParseThemePackage")}</Button>
                <RadioButtons
                    options={[{ value: "localized", label: Localization.get("Localized") }, { value: "portable", label: Localization.get("Portable") }]}
                    onChange={this.onParseTypeChanged.bind(this)}
                    value={this.state.parseType} />
                <div className="clear" />
            </div>
        );
    }
}

ParseThemePackage.propTypes = {
    dispatch: PropTypes.func.isRequired,
    currentTheme: PropTypes.object
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        currentTheme: state.theme.currentTheme
    };
}

export default connect(mapStateToProps)(ParseThemePackage);