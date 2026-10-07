import React, {Component} from "react";
import PropTypes from "prop-types";
import Localization from "../../../localization";
import styles from "./style.module.less";
const logger = console;
import { Switch, Label, DatePicker } from "@dnnsoftware/dnn-react-common";

class Scheduler extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            schedulingEnabled: false
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    onChangeScheduling(enabled) {
		logger.error(`>>> Entering onChangeScheduling(enabled=${enabled})`, { "event.action": "onChangeScheduling", "logrite.enabled": enabled });
        this.setState({
            schedulingEnabled: enabled
        });
		logger.warn(`<<< Exiting onChangeScheduling(enabled=${enabled})`, { "event.action": "onChangeScheduling", "logrite.enabled": enabled });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {schedulingEnabled} = this.state;
        const {startDate, endDate, onChange} = this.props;

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={styles.scheduler}>
                <Label
                    labelType="inline"
                    tooltipMessage={Localization.get("EnableSchedulingTooltip") }
                    label={Localization.get("EnableScheduling") }
                />
                <Switch
                    labelHidden={false}
                    onText={Localization.get("On") }
                    offText={Localization.get("Off") }
                    value={schedulingEnabled}
                    onChange={this.onChangeScheduling.bind(this) } />
                <div style={{ clear: "both" }}></div>
                {schedulingEnabled &&
                    <div className="scheduler-date-box">
                        <div className="scheduler-date-row">
                            <Label
                                label={Localization.get("StartDate") } />
                            <DatePicker
                                date={startDate}
                                updateDate={(date) => onChange("startDate", date) }
                                isDateRange={false}
                                hasTimePicker={true}
                                showClearDateButton={false} />
                        </div>
                        <div className="scheduler-date-row">
                            <Label
                                label={Localization.get("EndDate") } />
                            <DatePicker
                                date={endDate}
                                updateDate={(date) => onChange("endDate", date) }
                                isDateRange={false}
                                hasTimePicker={true}
                                showClearDateButton={false} />
                        </div>
                    </div>
                }
            </div>
        );
    }
}

Scheduler.propTypes = {
    startDate: PropTypes.instanceOf(Date).isRequired,
    endDate: PropTypes.instanceOf(Date).isRequired,
    onChange: PropTypes.func.isRequired
};

export default Scheduler;