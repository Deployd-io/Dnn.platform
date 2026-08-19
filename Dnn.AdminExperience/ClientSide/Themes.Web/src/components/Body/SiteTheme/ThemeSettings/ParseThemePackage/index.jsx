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
		logger.debug('>>> Entering constructor()');
        this.state = {
            parseType: "localized",
            parsing: false
        };
		logger.debug('<<< Exiting constructor()');
    }

    getParseType() {
		logger.debug('>>> Entering getParseType()');
        const {state} = this;

		logger.debug('<<< Exiting getParseType()');
        return state.parseType == "portable" ? 1 : 0;
    }

    parseTheme() {
		logger.debug('>>> Entering parseTheme()');
        this.setState({ parsing: true }, function () {
			logger.debug(`>>> Entering setState(true=${true})`);
			logger.debug(`<<< Exiting setState(true=${true})`);
            this.parseLayout();
        });
		logger.debug('<<< Exiting parseTheme()');
    }

    parseLayout() {
		logger.debug('>>> Entering parseLayout()');
        const {props} = this;

        let themeName = props.currentTheme.SiteLayout.themeName;
        let parseType = this.getParseType();
		logger.debug(`parseLayout(): parseType → ${parseType}`);

        let self = this;
		logger.debug('<<< Exiting parseLayout()');
        props.dispatch(ThemeActions.parseTheme(themeName, parseType, function () {
			logger.debug('>>> Entering dispatch()');
            self.setState({ parsing: false });
            utils.utilities.notify(Localization.get("Successful"));
			logger.debug('<<< Exiting dispatch()');
        }));
    }

    onParseTypeChanged(type) {
		logger.debug(`>>> Entering onParseTypeChanged(type=${type})`);
        this.setState({ parseType: type });
		logger.debug(`<<< Exiting onParseTypeChanged(type=${type})`);
    }

    render() {
		logger.debug('>>> Entering render()');
        const {state} = this;

		logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        currentTheme: state.theme.currentTheme
    };
}

export default connect(mapStateToProps)(ParseThemePackage);