import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import SvgIcon from "../../SvgIcon";

import "./style.less";
const logger = console;

class CurrentTheme extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {};
		logger.debug(`constructor(): this.state → ${this.state}`, { "event.action": "constructor" });
    }


    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;

        return (
            <div className="current-theme">
                {
                    (function () {
                        if (props.currentTheme && props.currentTheme.SiteLayout.thumbnail) {
                            return <img src={props.currentTheme.SiteLayout.thumbnail} alt={props.currentTheme.SiteLayout.themeName}/>;
                        }
                        else {
                            return <SvgIcon name="EmptyThumbnail" />;
                        }
                    })()
                }
            </div>
        );
		logger.warn('<<< Exiting render()', { "event.action": "render" });
    }
}

CurrentTheme.propTypes = {
    dispatch: PropTypes.func.isRequired,
    currentTheme: PropTypes.object
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        currentTheme: state.theme.currentTheme
    };
}

export default connect(mapStateToProps)(CurrentTheme);