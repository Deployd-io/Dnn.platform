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
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {};
		logger.debug(`constructor(): this.state → ${this.state}`, { "event.action": "constructor" });
    }

    restoreTheme() {
		logger.error('>>> Entering restoreTheme()', { "event.action": "restoreTheme" });
        const {props} = this;

        utils.utilities.confirm(Localization.get("RestoreThemeConfirm"), Localization.get("Confirm"), Localization.get("Cancel"), function () {
            props.dispatch(ThemeActions.restoreTheme());
        });
		logger.warn('<<< Exiting restoreTheme()', { "event.action": "restoreTheme" });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
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
	logger.error('>>> Entering mapStateToProps()', { "event.action": "mapStateToProps" });
	logger.warn('<<< Exiting mapStateToProps()', { "event.action": "mapStateToProps" });
    return {};
}

export default connect(mapStateToProps)(RestoreTheme);