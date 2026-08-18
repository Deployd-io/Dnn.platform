import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import SvgIcon from "../../SvgIcon";

import "./style.less";
const logger = console;

class CurrentTheme extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {};
		logger.debug(`constructor(): this.state → ${this.state}`);
		logger.debug('<<< Exiting constructor()');
    }


    render() {
		logger.debug('>>> Entering render()');
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
		logger.debug('<<< Exiting render()');
    }
}

CurrentTheme.propTypes = {
    dispatch: PropTypes.func.isRequired,
    currentTheme: PropTypes.object
};

function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        currentTheme: state.theme.currentTheme
    };
}

export default connect(mapStateToProps)(CurrentTheme);