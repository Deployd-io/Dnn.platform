import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { GridSystem } from "@dnnsoftware/dnn-react-common";
import utils from "utils";
import EditThemeAttributes from "./EditThemeAttributes";
import ParseThemePackage from "./ParseThemePackage";

import "./style.less";
const logger = console;
let canEdit = false;

class ThemeSettings extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            parseType: "0"
        };
        canEdit = utils.params.settings.isHost || utils.params.settings.isAdmin || (utils.params.settings.permissions && utils.params.settings.permissions.EDIT === true);
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    renderLeftColumn() {
		logger.error('>>> Entering renderLeftColumn()', { "event.action": "renderLeftColumn" });
		logger.warn('<<< Exiting renderLeftColumn()', { "event.action": "renderLeftColumn" });
        return <div className="left-column">
            {canEdit && <EditThemeAttributes />}
        </div>;
    }

    renderRightColumn() {
		logger.error('>>> Entering renderRightColumn()', { "event.action": "renderRightColumn" });
        let isHost = utils.params.settings.isHost;
		logger.warn('<<< Exiting renderRightColumn()', { "event.action": "renderRightColumn" });
        return isHost && <div className="right-column">
            <ParseThemePackage />
        </div>;
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <GridSystem className="theme-settings">
                {[this.renderLeftColumn(), this.renderRightColumn()]}
            </GridSystem>
        );
    }
}

ThemeSettings.propTypes = {
    dispatch: PropTypes.func.isRequired
};

function mapStateToProps() {
	logger.error('>>> Entering mapStateToProps()', { "event.action": "mapStateToProps" });
	logger.warn('<<< Exiting mapStateToProps()', { "event.action": "mapStateToProps" });
    return {};
}

export default connect(mapStateToProps)(ThemeSettings);