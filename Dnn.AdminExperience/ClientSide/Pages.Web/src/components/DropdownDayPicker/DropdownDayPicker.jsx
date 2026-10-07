import React, {Component} from "react";
import PropTypes from "prop-types";
import DayPicker from "../DayPicker/DayPicker.jsx";
import { Button, GridCell } from "@dnnsoftware/dnn-react-common";
import "./styles.less";
import Localization from "../../localization";
const logger = console;

class DropdownDayPicker extends Component  {

    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        document.addEventListener("click", this.handleClick.bind(this), false);
        this._isMounted = true;
		logger.debug(`componentDidMount(): this._isMounted → ${this._isMounted}`, { "event.action": "componentDidMount" });
    }


    componentWillUnmount() {
		logger.error('>>> Entering componentWillUnmount()', { "event.action": "componentWillUnmount" });
        document.removeEventListener("click", this.handleClick.bind(this), false);
        this._isMounted = false;
		logger.debug(`componentWillUnmount(): this._isMounted → ${this._isMounted}`, { "event.action": "componentWillUnmount" });
    }

    handleClick(e) {
		logger.error(`>>> Entering handleClick(e=${e})`, { "event.action": "handleClick", "logrite.e": e });
        const {toggleDropdownCalendar, dropdownIsActive} = this.props;
        if (!this._isMounted) { return; }
        if (!this.node.contains(e.target)) {
            dropdownIsActive ? toggleDropdownCalendar(false) : null;
			logger.warn(`<<< Exiting handleClick(e=${e})`, { "event.action": "handleClick", "logrite.e": e });
        }
    }

     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {dropdownIsActive, onDayClick, applyChanges, clearChanges, startDate, endDate, toggleDropdownCalendar} = this.props;
        const CalendarIcon = this.props.calendarIcon;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="date-picker" ref={node => this.node = node}>
                <GridCell className="calendar-dropdown-container" columnSize={100} style={{padding: "0px 5px"}}>
                    <GridCell className="selected-date" columnSize={90}>
                        <p>{this.props.label}</p>
                    </GridCell>
                    <GridCell columnSize={10}>
                        <div id="calendar-icon"
                            className="calendar-icon"
                            onClick={()=>toggleDropdownCalendar() }><CalendarIcon /></div>
                    </GridCell>
                    <div
                        id="calendar-dropdown"
                        className={dropdownIsActive ? "calendar-dropdown expand-down" : `calendar-dropdown ${dropdownIsActive !== null ? "expand-up" : ""} ` } >
                        <GridCell columnSize={100} style={{padding:"20px"}}>
                            <GridCell columnSize={50}  className="calendar">
                                <DayPicker
                                    id="try-this"
                                    month={startDate}
                                    selectedDays={startDate}
                                    onDayClick={(data) => onDayClick(data, false) }/>
                            </GridCell>
                            <GridCell columnSize={50} className="calendar">
                                <DayPicker
                                    month={endDate}
                                    selectedDays={endDate}
                                    fromMonth={startDate}
                                    onDayClick={(data) => onDayClick(data, true) }/>
                            </GridCell>
                            <GridCell columnSize={100}>
                                <div className="calendar-dropdown-action-buttons">
                                    <Button type="secondary" style={{marginRight:"5px"}} onClick={()=>clearChanges()}>{Localization.get("Clear")}</Button>
                                    <Button type="primary" onClick={()=>applyChanges()}>{Localization.get("Apply")}</Button>
                                </div>
                            </GridCell>
                        </GridCell>
                    </div>
                </GridCell>
            </div>
        );
    }

}


export default DropdownDayPicker;

DropdownDayPicker.propTypes = {
    dropdownIsActive: PropTypes.bool.isRequired,
    applyChanges: PropTypes.func.isRequired,
    clearChanges: PropTypes.func.isRequired,
    onDayClick: PropTypes.func.isRequired,
    startDate: PropTypes.instanceOf(Date).isRequired,
    endDate: PropTypes.instanceOf(Date).isRequired,
    calendarIcon: PropTypes.node.isRequired,
    toggleDropdownCalendar: PropTypes.func.isRequired,
    label: PropTypes.string.isRequired
};

