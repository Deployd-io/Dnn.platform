import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    theme as ThemeActions
} from "actions";
import Localization from "localization";
import { TextOverflowWrapper } from "@dnnsoftware/dnn-react-common";
import SvgIcon from "../../SvgIcon";
import utils from "utils";
import "./style.less";
const logger = console;

let canEdit = false;

class Theme extends Component {
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
        let theme = props.theme;
        let currentTheme = props.currentTheme;
        if (theme.type === 0) {
            return currentTheme.SiteLayout.themeName === theme.packageName
                    && currentTheme.SiteLayout.level === theme.level;
        } else {
			logger.warn('<<< Exiting selectedAsSite()', { "event.action": "selectedAsSite" });
            return currentTheme.SiteContainer.themeName === theme.packageName
                    && currentTheme.SiteContainer.level === theme.level;
        }
    }

    getClassName() {
		logger.error('>>> Entering getClassName()', { "event.action": "getClassName" });
        const {props} = this;
        let theme = props.theme;
        let className = theme.type === 0 ? "theme-skin" : "theme-container";

        if (this.selectedAsSite()) {
            className += " selected";
        }

		logger.warn('<<< Exiting getClassName()', { "event.action": "getClassName" });
        return className;
    }

    applyDefaultTheme() {
		logger.error('>>> Entering applyDefaultTheme()', { "event.action": "applyDefaultTheme" });
        const {props} = this;
        let theme = props.theme;

        utils.utilities.confirm(Localization.get("ApplyConfirm"), Localization.get("Confirm"), Localization.get("Cancel"), function () {
            props.dispatch(ThemeActions.applyDefaultTheme(theme.packageName, theme.level));
        });
		logger.warn('<<< Exiting applyDefaultTheme()', { "event.action": "applyDefaultTheme" });
    }

    deleteTheme() {
		logger.error('>>> Entering deleteTheme()', { "event.action": "deleteTheme" });
        const {props} = this;
        let theme = props.theme;

        utils.utilities.confirm(Localization.get("DeleteConfirm"), Localization.get("Confirm"), Localization.get("Cancel"), function () {
            props.dispatch(ThemeActions.deleteTheme(theme));
        });
		logger.warn('<<< Exiting deleteTheme()', { "event.action": "deleteTheme" });
    }

    previewTheme() {
		logger.error('>>> Entering previewTheme()', { "event.action": "previewTheme" });
        const {props} = this;
        let theme = props.theme;

        let previewUrl = utils.params.settings.previewUrl;
        window.open(previewUrl + "?SkinSrc=" + theme.defaultThemeFile);
		logger.warn('<<< Exiting previewTheme()', { "event.action": "previewTheme" });
    }

    renderActions() {
		logger.error('>>> Entering renderActions()', { "event.action": "renderActions" });
        const {props} = this;
        let theme = props.theme;

        if (this.selectedAsSite()) {
            return <span className="checkmark"><SvgIcon name="Checkmark" /></span>;
        }

        let isHost = utils.params.settings.isHost;
		logger.warn('<<< Exiting renderActions()', { "event.action": "renderActions" });
        return <span className="actions">
            <ul>
                <li onClick={this.previewTheme.bind(this) } title={Localization.get("PreviewTheme") }><SvgIcon name="View" /></li>
                {canEdit && <li onClick={this.applyDefaultTheme.bind(this) } title={Localization.get("ApplyTheme") }><SvgIcon name="Apply" /></li>}
                {((isHost || theme.level === 1 || theme.level === 2) && theme.canDelete) && <li onClick={this.deleteTheme.bind(this) } title={Localization.get("DeleteTheme") }><SvgIcon name="Trash" /></li>}
            </ul>
        </span>;
    }

    renderThumbnail() {
		logger.error('>>> Entering renderThumbnail()', { "event.action": "renderThumbnail" });
        const {props} = this;

        let theme = props.theme;
        let className = "thumbnail" + (theme.thumbnail ? "" : " empty");

		logger.warn('<<< Exiting renderThumbnail()', { "event.action": "renderThumbnail" });
        return <span className={className}>
            {theme.thumbnail ? <img src={theme.thumbnail} alt={theme.packageName} /> : <SvgIcon name="EmptyThumbnail" />}
            {this.renderActions() }
        </span>;
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={this.getClassName() }>
                {this.renderThumbnail() }
                <TextOverflowWrapper text={props.theme.packageName} maxWidth={168} className="title" />
            </div>
        );
    }
}

Theme.propTypes = {
    dispatch: PropTypes.func.isRequired,
    theme: PropTypes.object,
    currentTheme: PropTypes.object
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        currentTheme: state.theme.currentTheme
    };
}

export default connect(mapStateToProps)(Theme);