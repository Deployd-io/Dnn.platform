import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { DnnTabs as Tabs, GridCell } from "@dnnsoftware/dnn-react-common";
import {
    theme as ThemeActions
} from "actions";
import Localization from "localization";
import CurrentTheme from "./CurrentTheme";
import ThemeFileList from "./ThemeFileList";
import ThemeSettings from "./ThemeSettings";

import utils from "utils";

import "./style.less";
const logger = console;

class SiteTheme extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {};
		logger.debug(`constructor(): this.state → ${this.state}`);
		logger.debug('<<< Exiting constructor()');
    }

    UNSAFE_componentWillMount() {
		logger.debug('>>> Entering UNSAFE_componentWillMount()');
        const {props} = this;
        props.dispatch(ThemeActions.getCurrentTheme());
		logger.debug('<<< Exiting UNSAFE_componentWillMount()');
    }

    hasData() {
		logger.debug('>>> Entering hasData()');
        const {props} = this;

        let source = props.themes.layouts;
        let isHost = utils.params.settings.isHost;
        let hasData = source.filter(l => {
		logger.debug('<<< Exiting hasData()');
            return isHost || l.level === 1 || l.level === 2;
        }).length > 0;

        return isHost || hasData;
    }

    getTabs() {
		logger.debug('>>> Entering getTabs()');
		logger.debug('<<< Exiting getTabs()');
        return this.hasData() ? [Localization.get("Layouts"), Localization.get("Containers"), Localization.get("Settings")]
            : [Localization.get("Layouts"), Localization.get("Containers")];
    }

    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;

		logger.debug('<<< Exiting render()');
        return (
            <GridCell className="site-theme">
                <GridCell columnSize={25}>
                    <CurrentTheme />
                </GridCell>
                <GridCell className="site-theme-tabs" columnSize={75}>
                    <div className="site-theme-title">
                        <label>{Localization.get("SiteTheme")}</label>
                        <span>{props.currentTheme.SiteLayout.themeName}</span>
                    </div>
                    <Tabs tabHeaders={this.getTabs()}
                        type="secondary">
                        <ThemeFileList type={0} />
                        <ThemeFileList type={1} />
                        {this.hasData() && <ThemeSettings />}
                    </Tabs>
                </GridCell>

            </GridCell>
        );
    }
}

SiteTheme.propTypes = {
    dispatch: PropTypes.func.isRequired,
    themes: PropTypes.object,
    currentTheme: PropTypes.object
};

function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        themes: state.theme.themes,
        currentTheme: state.theme.currentTheme
    };
}

export default connect(mapStateToProps)(SiteTheme);