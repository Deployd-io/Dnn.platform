import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { theme as ThemeActions } from "actions";
import Localization from "localization";
import { GridCell, Button } from "@dnnsoftware/dnn-react-common";
import utils from "utils";
import "./style.less";
const logger = console;

class RestoreTheme extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {};
		logger.debug(`constructor(): this.state → ${this.state}`);
		logger.debug('<<< Exiting constructor()');
    }

    restoreTheme() {
		logger.debug('>>> Entering restoreTheme()');
        const {props} = this;

		logger.debug('<<< Exiting restoreTheme()');
        utils.utilities.confirm(Localization.get("RestoreThemeConfirm"), Localization.get("Confirm"), Localization.get("Cancel"), function () {
			logger.debug('>>> Entering function()');
			logger.debug('<<< Exiting function()');
            props.dispatch(ThemeActions.restoreTheme());
        });
    }

    render() {
		logger.debug('>>> Entering render()');
		logger.debug('<<< Exiting render()');
        return (
            <GridCell className="restore-theme" columnSize={50}>
                <Button onClick={this.restoreTheme.bind(this)}>{Localization.get("RestoreTheme")}</Button>
            </GridCell>
        );
    }
}

RestoreTheme.propTypes = {
    dispatch: PropTypes.func.isRequired
};

function mapStateToProps() {
	logger.debug('>>> Entering mapStateToProps()');
	logger.debug('<<< Exiting mapStateToProps()');
    return {};
}

export default connect(mapStateToProps)(RestoreTheme);