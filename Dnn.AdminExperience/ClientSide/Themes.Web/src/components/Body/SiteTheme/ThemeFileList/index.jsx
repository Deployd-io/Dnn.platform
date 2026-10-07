import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    theme as ThemeActions
} from "actions";
import { Scrollbars } from "react-custom-scrollbars";

import ThemeFile from "./ThemeFile";

import "./style.less";
const logger = console;

class ThemeFileList extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            themeName: "",
            level: 0
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    UNSAFE_componentWillMount() {
		logger.error('>>> Entering UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        const {props} = this;

        this.parseProps(props);
		logger.warn('<<< Exiting UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
    }

    UNSAFE_componentWillReceiveProps(newProps) {
		logger.error(`>>> Entering UNSAFE_componentWillReceiveProps(newProps=${newProps})`, { "event.action": "UNSAFE_componentWillReceiveProps", "logrite.newProps": newProps });
        this.parseProps(newProps);
		logger.warn(`<<< Exiting UNSAFE_componentWillReceiveProps(newProps=${newProps})`, { "event.action": "UNSAFE_componentWillReceiveProps", "logrite.newProps": newProps });
    }

    /*eslint-disable eqeqeq*/
    parseProps(props) {
	logger.error(`>>> Entering parseProps(props=${props})`, { "event.action": "parseProps", "logrite.props": props });

        if (!props.theme || !props.theme.SiteLayout.themeName) {
            return;
        }

        let themeName = props.type == 0 ? props.theme.SiteLayout.themeName : props.theme.SiteContainer.themeName;
        let path = props.type == 0 ? props.theme.SiteLayout.path : props.theme.SiteContainer.path;
        let level = path.indexOf("[G]") > -1 ? 4 : (path.indexOf("[S]") > -1 ? 2 : 1);
		logger.error(`parseProps(props=${props}): level → ${level}`, { "event.action": "parseProps", "logrite.props": props });

        this.setState({ themeName: themeName, level: level }, function () {
            this.loadThemeFiles();
        });
		logger.warn(`<<< Exiting parseProps(props=${props})`, { "event.action": "parseProps", "logrite.props": props });
    }

    selectedAsSite(themeFile) {
		logger.error(`>>> Entering selectedAsSite(themeFile=${themeFile})`, { "event.action": "selectedAsSite", "logrite.themeFile": themeFile });
        const {props} = this;
        let currentTheme = props.theme;

        if (themeFile.type === 0) {
            return currentTheme.SiteLayout.path.toLowerCase() === themeFile.path.toLowerCase();
        } else {
			logger.warn(`<<< Exiting selectedAsSite(themeFile=${themeFile})`, { "event.action": "selectedAsSite", "logrite.themeFile": themeFile });
            return currentTheme.SiteContainer.path.toLowerCase() === themeFile.path.toLowerCase();
        }
    }

    selectedAsEdit(themeFile) {
		logger.error(`>>> Entering selectedAsEdit(themeFile=${themeFile})`, { "event.action": "selectedAsEdit", "logrite.themeFile": themeFile });
        const {props} = this;
        let currentTheme = props.theme;

        if (themeFile.type === 0) {
            return currentTheme.EditLayout.path.toLowerCase() === themeFile.path.toLowerCase();
        } else {
			logger.warn(`<<< Exiting selectedAsEdit(themeFile=${themeFile})`, { "event.action": "selectedAsEdit", "logrite.themeFile": themeFile });
            return currentTheme.EditContainer.path.toLowerCase() === themeFile.path.toLowerCase();
        }
    }

    getListWidth() {
		logger.error('>>> Entering getListWidth()', { "event.action": "getListWidth" });
        const {props} = this;

        let width = 0;
        let self = this;
        props.themeFiles[props.type].forEach((themeFile) => {
            width += (self.selectedAsSite(themeFile) || self.selectedAsEdit(themeFile)) ? 108 : 90;
        });

        return width + 20;
    }

    loadThemeFiles() {
		logger.error('>>> Entering loadThemeFiles()', { "event.action": "loadThemeFiles" });
        const {props, state} = this;

        if (!state.themeName || props.themeFiles[props.type].length !== 0) {
            return;
        }

        props.dispatch(ThemeActions.getCurrentThemeFiles(state.themeName, props.type, state.level));
		logger.warn('<<< Exiting loadThemeFiles()', { "event.action": "loadThemeFiles" });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="theme-files-list">
                <Scrollbars
                    className="theme-files-scroller"
                    autoHeight
                    autoHeightMin={0}
                    autoHeightMax={180}>
                    <ul style={{ width: this.getListWidth() }}>
                        {props.themeFiles[props.type].map((themeFile, i) => {
                            return <ThemeFile themeFile={themeFile} key={i} />;
                        })}
                    </ul>
                </Scrollbars>
            </div>
        );
    }
}

ThemeFileList.propTypes = {
    dispatch: PropTypes.func.isRequired,
    theme: PropTypes.object,
    type: PropTypes.number,
    themeFiles: PropTypes.array
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        themeFiles: state.theme.currentThemeFiles,
        theme: state.theme.currentTheme
    };
}

export default connect(mapStateToProps)(ThemeFileList);