import React, { Component } from "react";
import PropTypes from "prop-types";
import DayPicker, { WeekdayPropTypes, DateUtils } from "react-day-picker";
import * as dayjs from "dayjs";
import TimePicker from "./TimePicker";
import TimezonePicker from "./TimezonePicker";
import timeZones from "./timeZones";
import DateInput from "./DateInput";
import CalendarIcon from "./img/calendar.svg";
import CalendarEndIcon from "./img/calendar_end.svg";
import CalendarStartIcon from "./img/calendar_start.svg";
import "./style.less";
const logger = console;

const DefaultControllerClassName = "calendar-controller";

function Weekday({ weekday, className, localeUtils, locale }) {
    const weekdayName = localeUtils.formatWeekdayLong(weekday, locale);
    return (
        <div className={className} title={weekdayName}>
            {weekdayName.slice(0, 1) }
        </div>
    );
}

function hasClass(element, className) {
    return (" " + element.className + " ").indexOf(" " + className + " ") > -1;
}

function validateDate(date, minDate, maxDate) {
	logger.debug(`>>> Entering validateDate(date=${date},minDate=${minDate},maxDate=${maxDate})`);
	logger.debug(`<<< Exiting validateDate(date=${date},minDate=${minDate},maxDate=${maxDate})`);
    return ((!minDate || date >= minDate) && 
            (!maxDate || date <= maxDate));
}

Weekday.propTypes = WeekdayPropTypes;

const clearButtonStyleVisible = {
    transition: "300ms",
    marginRight: 0,
    opacity: 1
};

const clearButtonStyleInvisible = {
    transition: "300ms",
    marginRight: 0,
    opacity: 0,
    cursor: "default"
};

class DatePicker extends Component {

    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);
        let firstDate = typeof props.date === "string" ? new Date(props.date) : props.date;
		logger.debug(`constructor(props=${props}): firstDate → ${firstDate}`);

        let secondDate = typeof props.secondDate === "string" ? new Date(props.secondDate) : props.secondDate;
		logger.debug(`constructor(props=${props}): secondDate → ${secondDate}`);

        this.savedDate = {
            FirstDate: firstDate !== undefined ? firstDate : null,
            SecondDate: secondDate !== undefined ? secondDate : null,
            timezone: props.timezone
        };

		logger.debug(`constructor(props=${props}): this.savedDate → ${this.savedDate}`);
        this.state = {
            isCalendarVisible: false,
            Date: {
                FirstDate: firstDate !== undefined ? firstDate : null,
                SecondDate: secondDate !== undefined ? secondDate : null
            },
            timezone: props.timezone
        };
        this.handleClick = this.handleClick.bind(this);
		logger.debug(`constructor(props=${props}): this.state → ${this.state}`);
        this.firstDateClick = this.firstDateClick.bind(this);
        this.secondDateClick = this.secondDateClick.bind(this);
        this.dayPickerRef = React.createRef();
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        document.addEventListener("click", this.handleClick, false);
        this._isMounted = true;
		logger.debug(`componentDidMount(): this._isMounted → ${this._isMounted}`);
		logger.debug('<<< Exiting componentDidMount()');
    }

    componentWillUnmount() {
		logger.debug('>>> Entering componentWillUnmount()');
        document.removeEventListener("click", this.handleClick, false);
        this._isMounted = false;
		logger.debug(`componentWillUnmount(): this._isMounted → ${this._isMounted}`);
		logger.debug('<<< Exiting componentWillUnmount()');
    }

    handleClick(e) {
		logger.debug(`>>> Entering handleClick(e=${e})`);
        const isController = hasClass(e.target, DefaultControllerClassName) || this.props.controllerClassName && hasClass(e.target, this.props.controllerClassName);

		logger.debug(`handleClick(e=${e}): isController → ${isController}`);
        if (!this._isMounted) { return; }
        const node = this.dayPickerRef.current;
        if (node && node.contains(e.target)) {
            return;
        }
        if (isController) {
            return;
        }
        this.cancel();
		logger.debug(`<<< Exiting handleClick(e=${e})`);
    }

    resetHours(date) {
		logger.debug(`>>> Entering resetHours(date=${date})`);
        let newDate = date;
        newDate.setHours(0);
		logger.debug(`resetHours(date=${date}): newDate → ${newDate}`);
        newDate.setMinutes(0);
        newDate.setSeconds(0);
		logger.debug(`<<< Exiting resetHours(date=${date})`);
        return newDate;
    }

    disableDates(FirstDate, SecondDate, day, maxDay, minDay) {
		logger.debug(`>>> Entering disableDates(FirstDate=${FirstDate},SecondDate=${SecondDate},day=${day},maxDay=${maxDay},minDay=${minDay})`);
        let maxDate = maxDay;
        let minDate = minDay;
        if (!FirstDate && !SecondDate && !maxDate && !minDate) {
            return;
        }
        if (SecondDate && maxDate) {
            maxDate = SecondDate < maxDate ? SecondDate : maxDate;
        } else {
            maxDate = SecondDate || maxDate;
        }
        if (FirstDate && minDate) {
            minDate = FirstDate > minDate ? FirstDate : minDate;
        } else {
            minDate = FirstDate || minDate;
        }
        const thisDay = this.resetHours(day);
        if (minDate && maxDate) {
            return thisDay < minDate || thisDay > maxDate;
        }
        if (minDate) {
            return thisDay < minDate;
        }
        if (maxDate) {
			logger.debug(`<<< Exiting disableDates(FirstDate=${FirstDate},SecondDate=${SecondDate},day=${day},maxDay=${maxDay},minDay=${minDay})`);
            return thisDay > maxDate;
        }
    }

    firstDisableDates(day) {
		logger.debug(`>>> Entering firstDisableDates(day=${day})`);
        const maxDate = this.props.maxDate ? this.resetHours(new Date(this.props.maxDate)) : null;
        const minDate = this.props.minDate ? this.resetHours(new Date(this.props.minDate)) : null;
		logger.debug(`firstDisableDates(day=${day}): maxDate → ${maxDate}`);
		logger.debug(`firstDisableDates(day=${day}): minDate → ${minDate}`);
        const SecondDate = this.state.Date.SecondDate ? this.resetHours(new Date(this.state.Date.SecondDate)) : null;
		logger.debug(`firstDisableDates(day=${day}): SecondDate → ${SecondDate}`);
		logger.debug(`<<< Exiting firstDisableDates(day=${day})`);
        return this.disableDates(null, SecondDate, day, maxDate, minDate);
    }

    secondDisableDates(day) {
		logger.debug(`>>> Entering secondDisableDates(day=${day})`);
        const FirstDate = this.state.Date.FirstDate ? this.resetHours(new Date(this.state.Date.FirstDate)) : null;
        const minDate = this.props.minSecondDate ? this.resetHours(new Date(this.props.minSecondDate)) : null;
		logger.debug(`secondDisableDates(day=${day}): FirstDate → ${FirstDate}`);
		logger.debug(`secondDisableDates(day=${day}): minDate → ${minDate}`);
        const maxDate = this.props.maxSecondDate ? this.resetHours(new Date(this.props.maxSecondDate)) : null;
		logger.debug(`secondDisableDates(day=${day}): maxDate → ${maxDate}`);
		logger.debug(`<<< Exiting secondDisableDates(day=${day})`);
        return this.disableDates(FirstDate, null, day, maxDate, minDate);
    }

    updateTime(date, timeDate) {
		logger.debug(`>>> Entering updateTime(date=${date},timeDate=${timeDate})`);
        if (!date) {
            return null;
        }
        if (!timeDate) {
            return date;
        }

        let newDate = new Date(date);
		logger.debug(`updateTime(date=${date},timeDate=${timeDate}): newDate → ${newDate}`);
        const TimeDate = new Date(timeDate);
		logger.debug(`updateTime(date=${date},timeDate=${timeDate}): TimeDate → ${TimeDate}`);
        const hours = TimeDate.getHours();
		logger.debug(`updateTime(date=${date},timeDate=${timeDate}): hours → ${hours}`);
        const minutes = TimeDate.getMinutes();
		logger.debug(`updateTime(date=${date},timeDate=${timeDate}): minutes → ${minutes}`);
        newDate.setHours(hours);
        newDate.setMinutes(minutes);
		logger.debug(`<<< Exiting updateTime(date=${date},timeDate=${timeDate})`);
        return newDate;
    }

    updateDate(firstDate, secondDate, options = {}) {
		logger.debug(`>>> Entering updateDate(firstDate=${firstDate},secondDate=${secondDate},options=${options})`);
        if (options.disabled) {
            return;
        }
        let FirstDate = firstDate;
		logger.debug(`updateDate(firstDate=${firstDate},secondDate=${secondDate},options=${options}): FirstDate → ${FirstDate}`);
        let SecondDate = secondDate;
		logger.debug(`updateDate(firstDate=${firstDate},secondDate=${secondDate},options=${options}): SecondDate → ${SecondDate}`);
        let {Date} = this.state;

        if (!options.preventUpdateTime && this.props.hasTimePicker) {
            if (FirstDate && Date.FirstDate) {
                FirstDate = this.updateTime(FirstDate, Date.FirstDate);
            }
            if (SecondDate && Date.SecondDate) {
                SecondDate = this.updateTime(SecondDate, Date.SecondDate);
            }
        }

        if (typeof this.props.date === "string") {
            if (FirstDate) {
                FirstDate = this.formatDate(FirstDate, "L") + " " + this.formatDate(FirstDate, "LT");
            }
            if (SecondDate) {
                SecondDate = this.formatDate(SecondDate, "L") + " " + this.formatDate(SecondDate, "LT");
            }
        }

        Date.FirstDate = FirstDate !== undefined ? FirstDate : Date.FirstDate;
        Date.SecondDate = SecondDate !== undefined ? SecondDate : Date.SecondDate;

        this.setState({
            Date
        });
        if (options.callUpdateDate || !this.props.isDateRange && !this.props.hasTimePicker) {
            this.callUpdateDate();
            if (!options.preventHide) {
                this.hideCalendar();
            }
        }
		logger.debug(`<<< Exiting updateDate(firstDate=${firstDate},secondDate=${secondDate},options=${options})`);
    }

    callUpdateDate() {
		logger.debug('>>> Entering callUpdateDate()');
        let {Date, timezone} = this.state;
        this.props.updateDate(Date.FirstDate, Date.SecondDate, timezone);
        this.cashPreviousDates();
		logger.debug('<<< Exiting callUpdateDate()');
    }

    cashPreviousDates() {
		logger.debug('>>> Entering cashPreviousDates()');
        const {Date, timezone} = this.state;
        const FirstDate = Date.FirstDate;
        const SecondDate = Date.SecondDate;
        this.savedDate = { FirstDate, SecondDate, timezone };
		logger.debug(`cashPreviousDates(): this.savedDate → ${this.savedDate}`);
		logger.debug('<<< Exiting cashPreviousDates()');
    }

    apply() {
		logger.debug('>>> Entering apply()');
        this.callUpdateDate();
        this.hideCalendar();
		logger.debug('<<< Exiting apply()');
    }

    cancel() {
		logger.debug('>>> Entering cancel()');
        this.hideCalendar();
        const FirstDate = this.savedDate.FirstDate;
        const SecondDate = this.savedDate.SecondDate;
        const timezone = this.savedDate.timezone;
        this.setState({ Date: { FirstDate, SecondDate }, timezone });
		logger.debug('<<< Exiting cancel()');
    }

    updateFirstTime(time) {
		logger.debug(`>>> Entering updateFirstTime(time=${time})`);
        let date = this.date || new Date();
        date = new Date(this.formatDate(date, "L") + " " + time);
        this.updateDate(date, this.secondDate, { preventUpdateTime: true });
		logger.debug(`updateFirstTime(time=${time}): date → ${date}`);
		logger.debug(`<<< Exiting updateFirstTime(time=${time})`);
    }

    updateSecondTime(time) {
		logger.debug(`>>> Entering updateSecondTime(time=${time})`);
        let date = this.secondDate || new Date();
        date = new Date(this.formatDate(date, "L") + " " + time);
        this.updateDate(this.date, date, { preventUpdateTime: true });
		logger.debug(`updateSecondTime(time=${time}): date → ${date}`);
		logger.debug(`<<< Exiting updateSecondTime(time=${time})`);
    }

    firstDateClick(day, { disabled }) {
		logger.debug(`>>> Entering firstDateClick(day=${day},disabled=${disabled})`);
        this.updateDate(day, undefined, { disabled });
		logger.debug(`<<< Exiting firstDateClick(day=${day},disabled=${disabled})`);
    }

    secondDateClick(day, { disabled }) {
		logger.debug(`>>> Entering secondDateClick(day=${day},disabled=${disabled})`);
        this.updateDate(undefined, day, { disabled });
		logger.debug(`<<< Exiting secondDateClick(day=${day},disabled=${disabled})`);
    }

    formatDate(date, format = "dddd, MMMM, Do, YYYY") {
		logger.debug(`>>> Entering formatDate(date=${date},format=${format},MMMM=${MMMM},Do=${Do})`);
        if (date) {
		logger.debug(`<<< Exiting formatDate(date=${date},format=${format},MMMM=${MMMM},Do=${Do})`);
            return dayjs(date).format(format);
        }
        return date;
    }

    showCalendar() {       
		logger.debug('>>> Entering showCalendar()');
        this.setState({ isCalendarVisible: true });
		logger.debug('<<< Exiting showCalendar()');
    }

    hideCalendar() {
		logger.debug('>>> Entering hideCalendar()');
        this.setState({ isCalendarVisible: false });
		logger.debug('<<< Exiting hideCalendar()');
    }

    toggleCalendar() {
		logger.debug('>>> Entering toggleCalendar()');
        const isCalendarVisible = !this.state.isCalendarVisible;
        this.setState({ isCalendarVisible });
        if (typeof this.props.onIconClick === "function") {
            this.props.onIconClick();
			logger.debug('<<< Exiting toggleCalendar()');
        }
    }

    getStyle() {
		logger.debug('>>> Entering getStyle()');
        let style = { width: 256 };
        if (this.props.isDateRange) {
            style.width = 512;
        }
		logger.debug(`getStyle(): style.width → ${style.width}`);
		logger.debug('<<< Exiting getStyle()');
        return style;
    }

    updateFirstDate(date, preventUpdateTime=false) {
		logger.debug(`>>> Entering updateFirstDate(date=${date},preventUpdateTime=${preventUpdateTime})`);
        const firstDate = date ? date : this.state.Date.FirstDate;
        const secondDate = this.state.Date.SecondDate;
        const {minDate, maxDate} = this.props;
        if (validateDate(firstDate, minDate, maxDate)) {
            this.updateDate(firstDate, secondDate, { preventHide: true, callUpdateDate: true, preventUpdateTime: preventUpdateTime });
			logger.debug(`<<< Exiting updateFirstDate(date=${date},preventUpdateTime=${preventUpdateTime})`);
        }
    }

    updateSecondDate(date, preventUpdateTime=false) {
		logger.debug(`>>> Entering updateSecondDate(date=${date},preventUpdateTime=${preventUpdateTime})`);
        const secondDate = date ? date : this.state.Date.SecondDate;
        const firstDate = this.state.Date.FirstDate;
        const {minSecondDate, maxSecondDate} = this.props;
        if (validateDate(secondDate, minSecondDate, maxSecondDate)) {
            this.updateDate(firstDate, secondDate, { preventHide: true, callUpdateDate: true, preventUpdateTime: preventUpdateTime });
			logger.debug(`<<< Exiting updateSecondDate(date=${date},preventUpdateTime=${preventUpdateTime})`);
        }
    }

    clearDates() {
		logger.debug('>>> Entering clearDates()');
        let Date = {};
        Date.FirstDate = null;
		logger.debug(`clearDates(): Date.FirstDate → ${Date.FirstDate}`);
        Date.SecondDate = null;
		logger.debug(`clearDates(): Date.SecondDate → ${Date.SecondDate}`);
        this.setState({Date}, this.apply.bind(this));
		logger.debug('<<< Exiting clearDates()');
    }

    onClearDatesPressed() {
		logger.debug('>>> Entering onClearDatesPressed()');
        if (this.state.Date.FirstDate || this.state.Date.SecondDate) {
            const Date = { FirstDate: null, SecondDate: null };
            this.setState({ Date }, () => this.callUpdateDate());
			logger.debug('<<< Exiting onClearDatesPressed()');
        }
    }

    getPositionCss() {
		logger.debug('>>> Entering getPositionCss()');
        switch (this.props.calendarPosition) {            
            case "top":
                return "show-above-input";
            case "bottom":
            default:
				logger.debug('<<< Exiting getPositionCss()');
                return "show-below-input";
        }
    }

    updateTimezone(timezone) {
		logger.debug(`>>> Entering updateTimezone(timezone=${timezone})`);
        this.setState({timezone});
		logger.debug(`<<< Exiting updateTimezone(timezone=${timezone})`);
    }
   
    render() {
		logger.debug('>>> Entering render()');
        this.date = this.state.Date.FirstDate;
        this.secondDate = this.state.Date.SecondDate;

        let firstDate = this.date;
		logger.debug(`render(): firstDate → ${firstDate}`);
		logger.debug(`render(): this.date → ${this.date}`);
        let secondDate = this.secondDate;

        let displayFirstDate = firstDate ? this.formatDate(firstDate, "L") : "";
		logger.debug(`render(): displayFirstDate → ${displayFirstDate}`);
		logger.debug(`render(): secondDate → ${secondDate}`);
		logger.debug(`render(): this.secondDate → ${this.secondDate}`);
        let displaySecondDate = secondDate ? this.formatDate(secondDate, "L") : "";
		logger.debug(`render(): displaySecondDate → ${displaySecondDate}`);

        if (this.props.hasTimePicker) {
            displayFirstDate += (displayFirstDate ? " " + this.formatDate(firstDate, "LT") : "");
            displaySecondDate += (displaySecondDate ? " " + this.formatDate(secondDate, "LT") : "");
        }
        let displayDate = displayFirstDate;
        if (this.props.isDateRange && secondDate) {
            displayDate += " - " + displaySecondDate;
        }
        const showButton = !!this.props.isDateRange || !!this.props.hasTimePicker;
        const calendarClassName = "calendar-container " + this.getPositionCss() + (this.state.isCalendarVisible ? " visible" : " invisible");

        firstDate = firstDate ? new Date(firstDate) : null;
        secondDate = secondDate ? new Date(secondDate) : null;

        const showIcon = this.props.showIcon !== false;
        const showInput = this.props.showInput !== false;

        let Icon = this.props.mode === "start" ? CalendarStartIcon : this.props.mode === "end" ? CalendarEndIcon : CalendarIcon;
        if (this.props.icon) {
            Icon = this.props.icon;
        }

        const style = this.props.isDateRange && this.props.hasTimePicker ? {width: 380} : {}; 
        const buttonStyle = this.props.isDateRange ? {} : {margin: "10px auto", float: "none"};
		logger.debug(`render(): buttonStyle → ${buttonStyle}`);
        const inputClassName = "calendar-text" + ( this.props.hasTimePicker ? " with-time-picker" : "");

        const showClearDates = !!this.props.isDateRange && this.props.showClearDates;
        const clearButtonStyle = (this.state.Date.FirstDate || this.state.Date.SecondDate) ? clearButtonStyleVisible : clearButtonStyleInvisible;            
		logger.debug(`render(): clearButtonStyle → ${clearButtonStyle}`);

         
		logger.debug('<<< Exiting render()');
        return <div className="dnn-day-picker" ref={this.dayPickerRef}>
            {showInput && <div className={inputClassName} style={style} onClick={this.showCalendar.bind(this) }>
                {this.props.prependWith && <span>{this.props.prependWith}</span>}
                {this.props.showClearDateButton && <div className="clear-button" onClick={this.clearDates.bind(this)}>×</div>}
                {this.props.isInputReadOnly && displayDate}
                {!this.props.isInputReadOnly && <div style={{ float: "right" }}>
                    <DateInput date={firstDate} onUpdateDate={this.updateFirstDate.bind(this) } hasTimePicker={this.props.hasTimePicker || false}/>
                    {this.props.isDateRange && <div>
                        <span>&nbsp; -&nbsp; </span>
                        <DateInput date={secondDate} onUpdateDate={this.updateSecondDate.bind(this) } hasTimePicker={this.props.hasTimePicker || false}/>
                    </div>}
                </div>}
            </div>}
            {showIcon && <div
                className={"calendar-icon" + (this.state.isCalendarVisible ? " active" : "") }
                onClick={this.toggleCalendar.bind(this) }><Icon />
            </div>}
            <div className={calendarClassName} style={this.getStyle() } ref={element => this.calendarContainer = element}>
                <div>
                    <DayPicker
                        weekdayElement={ <Weekday/> }
                        initialMonth={firstDate || new Date() }
                        selectedDays={day => DateUtils.isSameDay(firstDate, day) }
                        onDayClick={this.firstDateClick }
                        disabledDays={ this.firstDisableDates.bind(this) }
                    />
                    <div className="dnn-time-picker-box">
                        {this.props.hasTimePicker && 
                            <TimePicker 
                                updateTime={this.updateFirstTime.bind(this)} 
                                time={this.formatDate(this.date, "LT") }
                                className={this.props.hasTimezonePicker ? "half" : "full"} />
                        }
                        {this.props.hasTimezonePicker && 
                            <TimezonePicker 
                                onUpdate={this.updateTimezone.bind(this)} 
                                value={this.state.timezone} />
                        }
                    </div>
                </div>

                {this.props.isDateRange && <div>
                    <DayPicker
                        weekdayElement={ <Weekday/> }
                        initialMonth={secondDate || new Date() }
                        selectedDays={day => DateUtils.isSameDay(secondDate, day) }
                        onDayClick={this.secondDateClick }
                        disabledDays={ this.secondDisableDates.bind(this) }
                    />
                    {this.props.hasTimePicker && <TimePicker updateTime={this.updateSecondTime.bind(this) } time={this.formatDate(this.secondDate, "LT") }/>}
                </div>}
                {showButton && <button style={buttonStyle} role="primary" onClick={this.apply.bind(this) }>{this.props.applyButtonText || "Apply"}</button>}
                {showClearDates && <button role="secondary" style={clearButtonStyle} onClick={this.onClearDatesPressed.bind(this)}>Clear</button> }                
            </div>
        </div>;
    }
}

DatePicker.propTypes = {
    // -----REQUIRED PROPS--------:
    date: PropTypes.instanceOf(Date),
    updateDate: PropTypes.func.isRequired,

    // -----OPTIONAL PROPS--------:

    // if set to true, it shows 2 calendars
    isDateRange: PropTypes.bool,

    //if isDateRange is true the secondDate is Required
    secondDate: PropTypes.instanceOf(Date),

    //min and max dates to reduce dates user can select. 
    minDate: PropTypes.instanceOf(Date),
    maxDate: PropTypes.instanceOf(Date),

    minSecondDate: PropTypes.instanceOf(Date),
    maxSecondDate: PropTypes.instanceOf(Date),

    // timezone value & update callback
    timezone: PropTypes.string,

    // if set to true, it shows time picker 
    hasTimePicker: PropTypes.bool,

    // if set to true, it shows time zone picker 
    hasTimezonePicker: PropTypes.bool,

    //if set to true it shows static text instead of input fields
    isInputReadOnly: PropTypes.bool,

    //show/hide an icon
    showIcon: PropTypes.bool,

    //function that will be called when icon is clicked
    onIconClick: PropTypes.func,

    //icon mode: "start" or "end". Default icon shows up if mode or custom icon is not provided
    mode: PropTypes.string,

    //custom icon
    icon: PropTypes.string,

    //show/hide input
    showInput: PropTypes.bool,

    applyButtonText: PropTypes.string,

    //to be able to click on element without hiding the calendar it's needed to provide class name of a controller or give to controller a default className - "calendar-controller"
    controllerClassName: PropTypes.string,

    showClearDateButton:  PropTypes.bool,

    showClearDates: PropTypes.bool.isRequired,

    prependWith: PropTypes.string,

    calendarPosition: PropTypes.oneOf(["bottom", "top"]).isRequired
};

DatePicker.defaultProps = {
    showClearDates: false,
    calendarPosition: "bottom",
    hasTimezonePicker: false,
    hasTimePicker: false
};

export default DatePicker;
export { timeZones };