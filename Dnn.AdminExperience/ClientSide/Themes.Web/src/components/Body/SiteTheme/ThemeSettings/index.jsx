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
		logger.debug('>>> Entering constructor()');
        this.state = {
            parseType: "0"
        };
        canEdit = utils.params.settings.isHost || utils.params.settings.isAdmin || (utils.params.settings.permissions && utils.params.settings.permissions.EDIT === true);
		logger.debug('<<< Exiting constructor()');
    }

    renderLeftColumn() {
		logger.debug('>>> Entering renderLeftColumn()');
		logger.debug('<<< Exiting renderLeftColumn()');
        return <div className="left-column">
            {canEdit && <EditThemeAttributes />}
        </div>;
    }

    renderRightColumn() {
		logger.debug('>>> Entering renderRightColumn()');
        let isHost = utils.params.settings.isHost;
		logger.debug('<<< Exiting renderRightColumn()');
        return isHost && <div className="right-column">
            <ParseThemePackage />
        </div>;
    }

    render() {
		logger.debug('>>> Entering render()');
		logger.debug('<<< Exiting render()');
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
	logger.debug('>>> Entering mapStateToProps()');
	logger.debug('<<< Exiting mapStateToProps()');
    return {};
}

export default connect(mapStateToProps)(ThemeSettings);