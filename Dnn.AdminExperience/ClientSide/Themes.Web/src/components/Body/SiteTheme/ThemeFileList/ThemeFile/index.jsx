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
		logger.debug('>>> Entering constructor()');
        this.state = {};
		logger.debug(`constructor(): this.state → ${this.state}`);
        canEdit = utils.params.settings.isHost || utils.params.settings.isAdmin || (utils.params.settings.permissions && utils.params.settings.permissions.EDIT === true);
		logger.debug('<<< Exiting constructor()');
    }

    selectedAsSite() {
		logger.debug('>>> Entering selectedAsSite()');
        const {props} = this;
        let themeFile = props.themeFile;
        let currentTheme = props.currentTheme;

        if (themeFile.type === 0) {
            return currentTheme.SiteLayout.path.toLowerCase() === themeFile.path.toLowerCase();
        } else {
			logger.debug('<<< Exiting selectedAsSite()');
            return currentTheme.SiteContainer.path.toLowerCase() === themeFile.path.toLowerCase();
        }
    }

    selectedAsEdit() {
		logger.debug('>>> Entering selectedAsEdit()');
        const {props} = this;
        let themeFile = props.themeFile;
        let currentTheme = props.currentTheme;

        if (themeFile.type === 0) {
            return currentTheme.EditLayout.path.toLowerCase() === themeFile.path.toLowerCase();
        } else {
			logger.debug('<<< Exiting selectedAsEdit()');
            return currentTheme.EditContainer.path.toLowerCase() === themeFile.path.toLowerCase();
        }
    }

    getClassName() {
		logger.debug('>>> Entering getClassName()');
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

		logger.debug('<<< Exiting getClassName()');
        return className;
    }

    setSiteTheme() {
		logger.debug('>>> Entering setSiteTheme()');
        const {props} = this;
        let themeFile = props.themeFile;

        props.dispatch(ThemeActions.applyTheme(themeFile, 1));
		logger.debug('<<< Exiting setSiteTheme()');
    }

    setEditTheme() {
		logger.debug('>>> Entering setEditTheme()');
        const {props} = this;
        let themeFile = props.themeFile;

        props.dispatch(ThemeActions.applyTheme(themeFile, 2));
		logger.debug('<<< Exiting setEditTheme()');
    }

    /*eslint-disable eqeqeq*/
    renderActions() {
		logger.debug('>>> Entering renderActions()');
        const {props} = this;
        let themeFile = props.themeFile;
        let type = themeFile.type;

        if (this.selectedAsSite() && this.selectedAsEdit()) {
		logger.debug('<<< Exiting renderActions()');
            return null;
        }

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
		logger.debug('>>> Entering renderThumbnail()');
        const {props} = this;

        let themeFile = props.themeFile;
        let className = "thumbnail" + (themeFile.thumbnail ? "" : " empty");

		logger.debug('<<< Exiting renderThumbnail()');
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
		logger.debug('>>> Entering render()');
        const {props} = this;

		logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        currentTheme: state.theme.currentTheme
    };
}

export default connect(mapStateToProps)(ThemeFile);