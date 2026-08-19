import React, { Component } from "react";
import PropTypes from "prop-types";
import Scheduler from "../common/Scheduler/Scheduler";
const logger = console;

class EnableScheduling extends Component {

    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;
		logger.debug('<<< Exiting render()');
        return <div className="input-group">
            <Scheduler
                startDate={props.startDate}
                endDate={props.endDate}
                onChange={(key, value) => props.onChangeValue(key, value)} />
            <div style={{ clear: "both" }}></div>
        </div>;
    }
}

EnableScheduling.propTypes = {
    startDate: PropTypes.instanceOf(Date).isRequired,
    endDate: PropTypes.instanceOf(Date).isRequired,
    onChangeValue: PropTypes.func.isRequired
};

export default EnableScheduling;