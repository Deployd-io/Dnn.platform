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
	logger.error(`>>> Entering validateDate(date=${date},minDate=${minDate},maxDate=${maxDate})`, { "event.action": "validateDate", "logrite.date": date, "logrite.minDate": minDate, "logrite.maxDate": maxDate });
	logger.warn(`<<< Exiting validateDate(date=${date},minDate=${minDate},maxDate=${maxDate})`, { "event.action": "validateDate", "logrite.date": date, "logrite.minDate": minDate, "logrite.maxDate": maxDate });
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
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        let firstDate = typeof props.date === "string" ? new Date(props.date) : props.date;
		logger.error(`constructor(props=${props}): firstDate → ${firstDate}`, { "event.action": "constructor", "logrite.props": props });

        let secondDate = typeof props.secondDate === "string" ? new Date(props.secondDate) : props.secondDate;
		logger.error(`constructor(props=${props}): secondDate → ${secondDate}`, { "event.action": "constructor", "logrite.props": props });

        this.savedDate = {
            FirstDate: firstDate !== undefined ? firstDate : null,
            SecondDate: secondDate !== undefined ? secondDate : null,
            timezone: props.timezone
        };

		logger.debug(`constructor(props=${props}): this.savedDate → ${this.savedDate}`, { "event.action": "constructor", "logrite.props": props });
        this.state = {
            isCalendarVisible: false,
            Date: {
                FirstDate: firstDate !== undefined ? firstDate : null,
                SecondDate: secondDate !== undefined ? secondDate : null
            },
            timezone: props.timezone
        };
        this.handleClick = this.handleClick.bind(this);
		logger.debug(`constructor(props=${props}): this.state → ${this.state}`, { "event.action": "constructor", "logrite.props": props });
        this.firstDateClick = this.firstDateClick.bind(this);
        this.secondDateClick = this.secondDateClick.bind(this);
        this.dayPickerRef = React.createRef();
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        document.addEventListener("click", this.handleClick, false);
        this._isMounted = true;
		logger.debug(`componentDidMount(): this._isMounted → ${this._isMounted}`, { "event.action": "componentDidMount" });
    }

    componentWillUnmount() {
		logger.error('>>> Entering componentWillUnmount()', { "event.action": "componentWillUnmount" });
        document.removeEventListener("click", this.handleClick, false);
        this._isMounted = false;
		logger.debug(`componentWillUnmount(): this._isMounted → ${this._isMounted}`, { "event.action": "componentWillUnmount" });
    }

    handleClick(e) {
		logger.error(`>>> Entering handleClick(e=${e})`, { "event.action": "handleClick", "logrite.e": e });
        const isController = hasClass(e.target, DefaultControllerClassName) || this.props.controllerClassName && hasClass(e.target, this.props.controllerClassName);

		logger.error(`handleClick(e=${e}): isController → ${isController}`, { "event.action": "handleClick", "logrite.e": e });
        if (!this._isMounted) { return; }
        const node = this.dayPickerRef.current;
        if (node && node.contains(e.target)) {
            return;
        }
        if (isController) {
            return;
        }
        this.cancel();
		logger.warn(`<<< Exiting handleClick(e=${e})`, { "event.action": "handleClick", "logrite.e": e });
    }

    resetHours(date) {
		logger.error(`>>> Entering resetHours(date=${date})`, { "event.action": "resetHours", "logrite.date": date });
        let newDate = date;
        newDate.setHours(0);
		logger.error(`resetHours(date=${date}): newDate → ${newDate}`, { "event.action": "resetHours", "logrite.date": date });
        newDate.setMinutes(0);
        newDate.setSeconds(0);
		logger.warn(`<<< Exiting resetHours(date=${date})`, { "event.action": "resetHours", "logrite.date": date });
        return newDate;
    }

    disableDates(FirstDate, SecondDate, day, maxDay, minDay) {
		logger.error(`>>> Entering disableDates(FirstDate=${FirstDate},SecondDate=${SecondDate},day=${day},maxDay=${maxDay},minDay=${minDay})`, { "event.action": "disableDates", "logrite.FirstDate": FirstDate, "logrite.SecondDate": SecondDate, "logrite.day": day, "logrite.maxDay": maxDay, "logrite.minDay": minDay });
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
			logger.warn(`<<< Exiting disableDates(FirstDate=${FirstDate},SecondDate=${SecondDate},day=${day},maxDay=${maxDay},minDay=${minDay})`, { "event.action": "disableDates", "logrite.FirstDate": FirstDate, "logrite.SecondDate": SecondDate, "logrite.day": day, "logrite.maxDay": maxDay, "logrite.minDay": minDay });
            return thisDay > maxDate;
        }
    }

    firstDisableDates(day) {
		logger.error(`>>> Entering firstDisableDates(day=${day})`, { "event.action": "firstDisableDates", "logrite.day": day });
        const maxDate = this.props.maxDate ? this.resetHours(new Date(this.props.maxDate)) : null;
        const minDate = this.props.minDate ? this.resetHours(new Date(this.props.minDate)) : null;
		logger.error(`firstDisableDates(day=${day}): maxDate → ${maxDate}`, { "event.action": "firstDisableDates", "logrite.day": day });
		logger.error(`firstDisableDates(day=${day}): minDate → ${minDate}`, { "event.action": "firstDisableDates", "logrite.day": day });
        const SecondDate = this.state.Date.SecondDate ? this.resetHours(new Date(this.state.Date.SecondDate)) : null;
		logger.error(`firstDisableDates(day=${day}): SecondDate → ${SecondDate}`, { "event.action": "firstDisableDates", "logrite.day": day });
        return this.disableDates(null, SecondDate, day, maxDate, minDate);
    }

    secondDisableDates(day) {
		logger.error(`>>> Entering secondDisableDates(day=${day})`, { "event.action": "secondDisableDates", "logrite.day": day });
        const FirstDate = this.state.Date.FirstDate ? this.resetHours(new Date(this.state.Date.FirstDate)) : null;
        const minDate = this.props.minSecondDate ? this.resetHours(new Date(this.props.minSecondDate)) : null;
		logger.error(`secondDisableDates(day=${day}): FirstDate → ${FirstDate}`, { "event.action": "secondDisableDates", "logrite.day": day });
		logger.error(`secondDisableDates(day=${day}): minDate → ${minDate}`, { "event.action": "secondDisableDates", "logrite.day": day });
        const maxDate = this.props.maxSecondDate ? this.resetHours(new Date(this.props.maxSecondDate)) : null;
		logger.error(`secondDisableDates(day=${day}): maxDate → ${maxDate}`, { "event.action": "secondDisableDates", "logrite.day": day });
        return this.disableDates(FirstDate, null, day, maxDate, minDate);
    }

    updateTime(date, timeDate) {
		logger.error(`>>> Entering updateTime(date=${date},timeDate=${timeDate})`, { "event.action": "updateTime", "logrite.date": date, "logrite.timeDate": timeDate });
        if (!date) {
            return null;
        }
        if (!timeDate) {
            return date;
        }

        let newDate = new Date(date);
		logger.error(`updateTime(date=${date},timeDate=${timeDate}): newDate → ${newDate}`, { "event.action": "updateTime", "logrite.date": date, "logrite.timeDate": timeDate });
        const TimeDate = new Date(timeDate);
		logger.error(`updateTime(date=${date},timeDate=${timeDate}): TimeDate → ${TimeDate}`, { "event.action": "updateTime", "logrite.date": date, "logrite.timeDate": timeDate });
        const hours = TimeDate.getHours();
		logger.error(`updateTime(date=${date},timeDate=${timeDate}): hours → ${hours}`, { "event.action": "updateTime", "logrite.date": date, "logrite.timeDate": timeDate });
        const minutes = TimeDate.getMinutes();
		logger.error(`updateTime(date=${date},timeDate=${timeDate}): minutes → ${minutes}`, { "event.action": "updateTime", "logrite.date": date, "logrite.timeDate": timeDate });
        newDate.setHours(hours);
        newDate.setMinutes(minutes);
		logger.warn(`<<< Exiting updateTime(date=${date},timeDate=${timeDate})`, { "event.action": "updateTime", "logrite.date": date, "logrite.timeDate": timeDate });
        return newDate;
    }

    updateDate(firstDate, secondDate, options = {}) {
		logger.error(`>>> Entering updateDate(firstDate=${firstDate},secondDate=${secondDate},options=${options})`, { "event.action": "updateDate", "logrite.firstDate": firstDate, "logrite.secondDate": secondDate, "logrite.options": options });
        if (options.disabled) {
            return;
        }
        let FirstDate = firstDate;
		logger.error(`updateDate(firstDate=${firstDate},secondDate=${secondDate},options=${options}): FirstDate → ${FirstDate}`, { "event.action": "updateDate", "logrite.firstDate": firstDate, "logrite.secondDate": secondDate, "logrite.options": options });
        let SecondDate = secondDate;
		logger.error(`updateDate(firstDate=${firstDate},secondDate=${secondDate},options=${options}): SecondDate → ${SecondDate}`, { "event.action": "updateDate", "logrite.firstDate": firstDate, "logrite.secondDate": secondDate, "logrite.options": options });
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
		logger.warn(`<<< Exiting updateDate(firstDate=${firstDate},secondDate=${secondDate},options=${options})`, { "event.action": "updateDate", "logrite.firstDate": firstDate, "logrite.secondDate": secondDate, "logrite.options": options });
    }

    callUpdateDate() {
		logger.error('>>> Entering callUpdateDate()', { "event.action": "callUpdateDate" });
        let {Date, timezone} = this.state;
        this.props.updateDate(Date.FirstDate, Date.SecondDate, timezone);
        this.cashPreviousDates();
		logger.warn('<<< Exiting callUpdateDate()', { "event.action": "callUpdateDate" });
    }

    cashPreviousDates() {
		logger.error('>>> Entering cashPreviousDates()', { "event.action": "cashPreviousDates" });
        const {Date, timezone} = this.state;
        const FirstDate = Date.FirstDate;
        const SecondDate = Date.SecondDate;
        this.savedDate = { FirstDate, SecondDate, timezone };
		logger.debug(`cashPreviousDates(): this.savedDate → ${this.savedDate}`, { "event.action": "cashPreviousDates" });
    }

    apply() {
		logger.error('>>> Entering apply()', { "event.action": "apply" });
        this.callUpdateDate();
        this.hideCalendar();
		logger.warn('<<< Exiting apply()', { "event.action": "apply" });
    }

    cancel() {
		logger.error('>>> Entering cancel()', { "event.action": "cancel" });
        this.hideCalendar();
        const FirstDate = this.savedDate.FirstDate;
        const SecondDate = this.savedDate.SecondDate;
        const timezone = this.savedDate.timezone;
        this.setState({ Date: { FirstDate, SecondDate }, timezone });
		logger.warn('<<< Exiting cancel()', { "event.action": "cancel" });
    }

    updateFirstTime(time) {
		logger.error(`>>> Entering updateFirstTime(time=${time})`, { "event.action": "updateFirstTime", "logrite.time": time });
        let date = this.date || new Date();
        date = new Date(this.formatDate(date, "L") + " " + time);
        this.updateDate(date, this.secondDate, { preventUpdateTime: true });
		logger.error(`updateFirstTime(time=${time}): date → ${date}`, { "event.action": "updateFirstTime", "logrite.time": time });
    }

    updateSecondTime(time) {
		logger.error(`>>> Entering updateSecondTime(time=${time})`, { "event.action": "updateSecondTime", "logrite.time": time });
        let date = this.secondDate || new Date();
        date = new Date(this.formatDate(date, "L") + " " + time);
        this.updateDate(this.date, date, { preventUpdateTime: true });
		logger.error(`updateSecondTime(time=${time}): date → ${date}`, { "event.action": "updateSecondTime", "logrite.time": time });
    }

    firstDateClick(day, { disabled }) {
		logger.error(`>>> Entering firstDateClick(day=${day},disabled=${disabled})`, { "event.action": "firstDateClick", "logrite.day": day, "logrite.disabled": disabled });
        this.updateDate(day, undefined, { disabled });
		logger.warn(`<<< Exiting firstDateClick(day=${day},disabled=${disabled})`, { "event.action": "firstDateClick", "logrite.day": day, "logrite.disabled": disabled });
    }

    secondDateClick(day, { disabled }) {
		logger.error(`>>> Entering secondDateClick(day=${day},disabled=${disabled})`, { "event.action": "secondDateClick", "logrite.day": day, "logrite.disabled": disabled });
        this.updateDate(undefined, day, { disabled });
		logger.warn(`<<< Exiting secondDateClick(day=${day},disabled=${disabled})`, { "event.action": "secondDateClick", "logrite.day": day, "logrite.disabled": disabled });
    }

    formatDate(date, format = "dddd, MMMM, Do, YYYY") {
		logger.error(`>>> Entering formatDate(date=${date},format=${format},MMMM=${MMMM},Do=${Do})`, { "event.action": "formatDate", "logrite.date": date, "logrite.format": format, "logrite.MMMM": MMMM, "logrite.Do": Do });
        if (date) {
            return dayjs(date).format(format);
        }
		logger.warn(`<<< Exiting formatDate(date=${date},format=${format},MMMM=${MMMM},Do=${Do})`, { "event.action": "formatDate", "logrite.date": date, "logrite.format": format, "logrite.MMMM": MMMM, "logrite.Do": Do });
        return date;
    }

    showCalendar() {       
		logger.error('>>> Entering showCalendar()', { "event.action": "showCalendar" });
        this.setState({ isCalendarVisible: true });
		logger.warn('<<< Exiting showCalendar()', { "event.action": "showCalendar" });
    }

    hideCalendar() {
		logger.error('>>> Entering hideCalendar()', { "event.action": "hideCalendar" });
        this.setState({ isCalendarVisible: false });
		logger.warn('<<< Exiting hideCalendar()', { "event.action": "hideCalendar" });
    }

    toggleCalendar() {
		logger.error('>>> Entering toggleCalendar()', { "event.action": "toggleCalendar" });
        const isCalendarVisible = !this.state.isCalendarVisible;
        this.setState({ isCalendarVisible });
        if (typeof this.props.onIconClick === "function") {
            this.props.onIconClick();
			logger.warn('<<< Exiting toggleCalendar()', { "event.action": "toggleCalendar" });
        }
    }

    getStyle() {
		logger.error('>>> Entering getStyle()', { "event.action": "getStyle" });
        let style = { width: 256 };
        if (this.props.isDateRange) {
            style.width = 512;
        }
		logger.debug(`getStyle(): style.width → ${style.width}`, { "event.action": "getStyle" });
        return style;
    }

    updateFirstDate(date, preventUpdateTime=false) {
		logger.error(`>>> Entering updateFirstDate(date=${date},preventUpdateTime=${preventUpdateTime})`, { "event.action": "updateFirstDate", "logrite.date": date, "logrite.preventUpdateTime": preventUpdateTime });
        const firstDate = date ? date : this.state.Date.FirstDate;
        const secondDate = this.state.Date.SecondDate;
        const {minDate, maxDate} = this.props;
        if (validateDate(firstDate, minDate, maxDate)) {
            this.updateDate(firstDate, secondDate, { preventHide: true, callUpdateDate: true, preventUpdateTime: preventUpdateTime });
			logger.warn(`<<< Exiting updateFirstDate(date=${date},preventUpdateTime=${preventUpdateTime})`, { "event.action": "updateFirstDate", "logrite.date": date, "logrite.preventUpdateTime": preventUpdateTime });
        }
    }

    updateSecondDate(date, preventUpdateTime=false) {
		logger.error(`>>> Entering updateSecondDate(date=${date},preventUpdateTime=${preventUpdateTime})`, { "event.action": "updateSecondDate", "logrite.date": date, "logrite.preventUpdateTime": preventUpdateTime });
        const secondDate = date ? date : this.state.Date.SecondDate;
        const firstDate = this.state.Date.FirstDate;
        const {minSecondDate, maxSecondDate} = this.props;
        if (validateDate(secondDate, minSecondDate, maxSecondDate)) {
            this.updateDate(firstDate, secondDate, { preventHide: true, callUpdateDate: true, preventUpdateTime: preventUpdateTime });
			logger.warn(`<<< Exiting updateSecondDate(date=${date},preventUpdateTime=${preventUpdateTime})`, { "event.action": "updateSecondDate", "logrite.date": date, "logrite.preventUpdateTime": preventUpdateTime });
        }
    }

    clearDates() {
		logger.error('>>> Entering clearDates()', { "event.action": "clearDates" });
        let Date = {};
        Date.FirstDate = null;
		logger.debug(`clearDates(): Date.FirstDate → ${Date.FirstDate}`, { "event.action": "clearDates" });
        Date.SecondDate = null;
		logger.debug(`clearDates(): Date.SecondDate → ${Date.SecondDate}`, { "event.action": "clearDates" });
        this.setState({Date}, this.apply.bind(this));
		logger.warn('<<< Exiting clearDates()', { "event.action": "clearDates" });
    }

    onClearDatesPressed() {
		logger.error('>>> Entering onClearDatesPressed()', { "event.action": "onClearDatesPressed" });
        if (this.state.Date.FirstDate || this.state.Date.SecondDate) {
            const Date = { FirstDate: null, SecondDate: null };
            this.setState({ Date }, () => this.callUpdateDate());
			logger.warn('<<< Exiting onClearDatesPressed()', { "event.action": "onClearDatesPressed" });
        }
    }

    getPositionCss() {
		logger.error('>>> Entering getPositionCss()', { "event.action": "getPositionCss" });
        switch (this.props.calendarPosition) {            
            case "top":
                return "show-above-input";
            case "bottom":
            default:
				logger.warn('<<< Exiting getPositionCss()', { "event.action": "getPositionCss" });
                return "show-below-input";
        }
    }

    updateTimezone(timezone) {
		logger.error(`>>> Entering updateTimezone(timezone=${timezone})`, { "event.action": "updateTimezone", "logrite.timezone": timezone });
        this.setState({timezone});
		logger.warn(`<<< Exiting updateTimezone(timezone=${timezone})`, { "event.action": "updateTimezone", "logrite.timezone": timezone });
    }
   
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        this.date = this.state.Date.FirstDate;
        this.secondDate = this.state.Date.SecondDate;

		logger.debug(`render(): this.date → ${this.date}`, { "event.action": "render" });
		logger.error(`render(): secondDate → ${secondDate}`, { "event.action": "render" });
		logger.debug(`render(): secondDate → ${secondDate}`, { "event.action": "render" });
		logger.debug(`render(): this.secondDate → ${this.secondDate}`, { "event.action": "render" });
        let firstDate = this.date;
		logger.error(`render(): firstDate → ${firstDate}`, { "event.action": "render" });
        let secondDate = this.secondDate;

        let displayFirstDate = firstDate ? this.formatDate(firstDate, "L") : "";
		logger.error(`render(): displayFirstDate → ${displayFirstDate}`, { "event.action": "render" });
        let displaySecondDate = secondDate ? this.formatDate(secondDate, "L") : "";
		logger.error(`render(): displaySecondDate → ${displaySecondDate}`, { "event.action": "render" });

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
		logger.error(`render(): buttonStyle → ${buttonStyle}`, { "event.action": "render" });
        const inputClassName = "calendar-text" + ( this.props.hasTimePicker ? " with-time-picker" : "");

        const showClearDates = !!this.props.isDateRange && this.props.showClearDates;
        const clearButtonStyle = (this.state.Date.FirstDate || this.state.Date.SecondDate) ? clearButtonStyleVisible : clearButtonStyleInvisible;            
		logger.error(`render(): clearButtonStyle → ${clearButtonStyle}`, { "event.action": "render" });

         
		logger.warn('<<< Exiting render()', { "event.action": "render" });
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