import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    theme as ThemeActions
} from "actions";
import Localization from "localization";
import { TextOverflowWrapper } from "@dnnsoftware/dnn-react-common";
import SvgIcon from "../../../SvgIcon";
import utils from "utils";
import "./style.less";
const logger = console;

let canEdit = false;
class ThemeFile extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {};
		logger.debug(`constructor(): this.state → ${this.state}`, { "event.action": "constructor" });
        canEdit = utils.params.settings.isHost || utils.params.settings.isAdmin || (utils.params.settings.permissions && utils.params.settings.permissions.EDIT === true);
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    selectedAsSite() {
		logger.error('>>> Entering selectedAsSite()', { "event.action": "selectedAsSite" });
        const {props} = this;
        let themeFile = props.themeFile;
        let currentTheme = props.currentTheme;

        if (themeFile.type === 0) {
            return currentTheme.SiteLayout.path.toLowerCase() === themeFile.path.toLowerCase();
        } else {
			logger.warn('<<< Exiting selectedAsSite()', { "event.action": "selectedAsSite" });
            return currentTheme.SiteContainer.path.toLowerCase() === themeFile.path.toLowerCase();
        }
    }

    selectedAsEdit() {
		logger.error('>>> Entering selectedAsEdit()', { "event.action": "selectedAsEdit" });
        const {props} = this;
        let themeFile = props.themeFile;
        let currentTheme = props.currentTheme;

        if (themeFile.type === 0) {
            return currentTheme.EditLayout.path.toLowerCase() === themeFile.path.toLowerCase();
        } else {
			logger.warn('<<< Exiting selectedAsEdit()', { "event.action": "selectedAsEdit" });
            return currentTheme.EditContainer.path.toLowerCase() === themeFile.path.toLowerCase();
        }
    }

    getClassName() {
		logger.error('>>> Entering getClassName()', { "event.action": "getClassName" });
        const {props} = this;
        let themeFile = props.themeFile;
        let className = themeFile.type === 0 ? "theme-file-skin" : "theme-file-container";

        let selected = false;

        if (this.selectedAsSite()) {
            selected = true;
            className += " site";
        }

        if (this.selectedAsEdit()) {
            selected = true;
            className += " edit";
        }

        if (selected) {
            className += " selected";
        }

		logger.warn('<<< Exiting getClassName()', { "event.action": "getClassName" });
        return className;
    }

    setSiteTheme() {
		logger.error('>>> Entering setSiteTheme()', { "event.action": "setSiteTheme" });
        const {props} = this;
        let themeFile = props.themeFile;

        props.dispatch(ThemeActions.applyTheme(themeFile, 1));
		logger.warn('<<< Exiting setSiteTheme()', { "event.action": "setSiteTheme" });
    }

    setEditTheme() {
		logger.error('>>> Entering setEditTheme()', { "event.action": "setEditTheme" });
        const {props} = this;
        let themeFile = props.themeFile;

        props.dispatch(ThemeActions.applyTheme(themeFile, 2));
		logger.warn('<<< Exiting setEditTheme()', { "event.action": "setEditTheme" });
    }

    /*eslint-disable eqeqeq*/
    renderActions() {
		logger.error('>>> Entering renderActions()', { "event.action": "renderActions" });
        const {props} = this;
        let themeFile = props.themeFile;
        let type = themeFile.type;

        if (this.selectedAsSite() && this.selectedAsEdit()) {
            return null;
        }

		logger.warn('<<< Exiting renderActions()', { "event.action": "renderActions" });
        return <span className="actions">
            {!this.selectedAsSite() ?
                <a href="#" className="set-site" onClick={this.setSiteTheme.bind(this) }>
                    {type == 0 ? Localization.get("SetSiteLayout") : Localization.get("SetSiteContainer") }
                </a> : null}
            {!this.selectedAsEdit() ?
                <a href="#" className={"set-edit" + (!this.selectedAsSite() ? " split" : "") } onClick={this.setEditTheme.bind(this) }>
                    {type == 0 ? Localization.get("SetEditLayout") : Localization.get("SetEditContainer") }
                </a> : null}
        </span>;
    }

    renderThumbnail() {
		logger.error('>>> Entering renderThumbnail()', { "event.action": "renderThumbnail" });
        const {props} = this;

        let themeFile = props.themeFile;
        let className = "thumbnail" + (themeFile.thumbnail ? "" : " empty");

		logger.warn('<<< Exiting renderThumbnail()', { "event.action": "renderThumbnail" });
        return <span className={className}>
            {themeFile.thumbnail ? <img src={themeFile.thumbnail} alt={themeFile.name} /> : <SvgIcon name="EmptyThumbnail" />}
            <span className="status">
                <span className="status-site"><SvgIcon name="Site" /></span>
                <span className="status-edit"><SvgIcon name="Edit" /></span>
            </span>
            {canEdit && this.renderActions() }

        </span>;
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <li className={this.getClassName() }>
                {this.renderThumbnail() }
                <TextOverflowWrapper text={props.themeFile.name} maxWidth={80} className="title" />
            </li>
        );
    }
}

ThemeFile.propTypes = {
    dispatch: PropTypes.func.isRequired,
    themeFile: PropTypes.object,
    currentTheme: PropTypes.object
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        currentTheme: state.theme.currentTheme
    };
}

export default connect(mapStateToProps)(ThemeFile);