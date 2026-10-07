import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    theme as ThemeActions
} from "actions";
import { PersonaBarPageBody, PersonaBarPageHeader, GridCell, Button } from "@dnnsoftware/dnn-react-common";
import Localization from "localization";
import SiteTheme from "./SiteTheme";
import MiddleActions from "./MiddleActions";
import ThemeList from "./ThemeList";
import "./style.less";
import utils from "utils";
const logger = console;

class Body extends Component {
    constructor() {
        super();
        this.state = {
            searchText: "",
            level: 7
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    getThemesData(reload) {
		logger.error(`>>> Entering getThemesData(reload=${reload})`, { "event.action": "getThemesData", "logrite.reload": reload });
        const { props, state } = this;

        if (reload || props.themes.layouts.length === 0) {
            props.dispatch(ThemeActions.getThemes(reload ? 7 : state.level));
        }

        let searchText = state.searchText;
        let level = state.level;
		logger.warn(`<<< Exiting getThemesData(reload=${reload})`, { "event.action": "getThemesData", "logrite.reload": reload });
        return props.themes.layouts.filter(l => {
            return !searchText || l.packageName.toLowerCase().indexOf(searchText.toLowerCase()) > -1;
        }).filter(l => {
            return level === 7 || l.level === level;
        });
    }

    onSearch(value) {
		logger.error(`>>> Entering onSearch(value=${value})`, { "event.action": "onSearch", "logrite.value": value });
        this.setState({ searchText: value });
		logger.warn(`<<< Exiting onSearch(value=${value})`, { "event.action": "onSearch", "logrite.value": value });
    }

    onFilterChanged(value) {
		logger.error(`>>> Entering onFilterChanged(value=${value})`, { "event.action": "onFilterChanged", "logrite.value": value });
        this.setState({ level: value });
		logger.warn(`<<< Exiting onFilterChanged(value=${value})`, { "event.action": "onFilterChanged", "logrite.value": value });
    }

    backToThemes() {
        utils.utilities.loadPanel("Dnn.Themes", {});
        this.getThemesData(true);
		logger.warn('<<< Exiting backToThemes()', { "event.action": "backToThemes" });
    }

    installTheme() {
        let event = document.createEvent("Event");

		logger.error(`installTheme(): event → ${event}`, { "event.action": "installTheme" });
        event.initEvent("installPortalTheme", true, true);

        let settings = {
            isHost: utils.params.settings.isHost,
            installPortalTheme: {},
            referrer: "Dnn.Themes",
            referrerText: Localization.get("BackToThemes"),
            backToReferrerFunc: this.backToThemes.bind(this)            
        };

        event = Object.assign(event, settings);

        utils.utilities.loadPanel("Dnn.Extensions", {
            settings
        });

        document.dispatchEvent(event);
		logger.warn('<<< Exiting installTheme()', { "event.action": "installTheme" });
    }

    render() {
        return (
            <GridCell className="themes-body">
                <PersonaBarPageHeader title={Localization.get("Themes")}>
                    {utils.params.settings.isHost &&
                    <Button type="primary" size="large" onClick={this.installTheme.bind(this)}>
                        {Localization.get("InstallTheme")}
                    </Button>
                    }
                </PersonaBarPageHeader>
                <PersonaBarPageBody>
                    <SiteTheme />
                    <MiddleActions onSearch={this.onSearch.bind(this)} onFilterChanged={this.onFilterChanged.bind(this)} />
                    <ThemeList dataSource={this.getThemesData()} />
                </PersonaBarPageBody>
            </GridCell>
        );
    }
}

Body.propTypes = {
    dispatch: PropTypes.func.isRequired,
    themes: PropTypes.object
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        themes: state.theme.themes
    };
}

export default connect(mapStateToProps)(Body);