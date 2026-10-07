import React, { Component } from "react";
import PropTypes from "prop-types";
const logger = console;

const Periods = ["AM", "PM"];
const numberReg = new RegExp("^\\d+$");

export default class DateInput extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });

        const month = this.props.date ? this.addZero(this.props.date.getMonth() + 1) : "";
		logger.error(`constructor(props=${props}): month → ${month}`, { "event.action": "constructor", "logrite.props": props });
        const day = this.props.date ? this.addZero(this.props.date.getDate()) : "";
		logger.error(`constructor(props=${props}): day → ${day}`, { "event.action": "constructor", "logrite.props": props });
        const year = this.props.date ? this.props.date.getFullYear() : "";
		logger.error(`constructor(props=${props}): year → ${year}`, { "event.action": "constructor", "logrite.props": props });
        const hours = this.props.date ? this.getHour(this.props.date.getHours()) : "";
		logger.error(`constructor(props=${props}): hours → ${hours}`, { "event.action": "constructor", "logrite.props": props });
        const minutes = this.props.date ? this.addZero(this.props.date.getMinutes()) : "";
		logger.error(`constructor(props=${props}): minutes → ${minutes}`, { "event.action": "constructor", "logrite.props": props });
        const period = this.props.date ? this.getPeriod(this.props.date.getHours()) : this.getPeriod(false);
		logger.error(`constructor(props=${props}): period → ${period}`, { "event.action": "constructor", "logrite.props": props });

        this.state = { month, day, year, hours, minutes, period };
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
		logger.debug(`constructor(props=${props}): this.state → ${this.state}`, { "event.action": "constructor", "logrite.props": props });

    }

    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        let { props } = this;
        if (props.date !== prevProps.date) {
            const month = props.date ? this.addZero(props.date.getMonth() + 1) : "";
			logger.error(`componentDidUpdate(prevProps=${prevProps}): month → ${month}`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
            const day = props.date ? this.addZero(props.date.getDate()) : "";
			logger.error(`componentDidUpdate(prevProps=${prevProps}): day → ${day}`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
            const year = props.date ? props.date.getFullYear() : "";
			logger.error(`componentDidUpdate(prevProps=${prevProps}): year → ${year}`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
            const hours = props.date ? this.getHour(props.date.getHours()) : "";
			logger.error(`componentDidUpdate(prevProps=${prevProps}): hours → ${hours}`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
            const minutes = props.date ? this.addZero(props.date.getMinutes()) : "";
			logger.error(`componentDidUpdate(prevProps=${prevProps}): minutes → ${minutes}`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
            const period = props.date ? this.getPeriod(props.date.getHours()) : this.getPeriod(false);
			logger.error(`componentDidUpdate(prevProps=${prevProps}): period → ${period}`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });

            this.setState({ month, day, year, hours, minutes, period });
			logger.warn(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        }
    }

    setMonth() {
		logger.error('>>> Entering setMonth()', { "event.action": "setMonth" });
        let {month} = this.state;
        if (!month) {
            return;
        }
        if (month > 12) {
            month = 12;
			logger.error(`setMonth(): month → ${month}`, { "event.action": "setMonth" });
        }
        if (month < 1) {
            month = 1;
        }
        month = this.addZero(month);
        this.setState({ month });
        let date = this.props.date || new Date();
		logger.error(`setMonth(): date → ${date}`, { "event.action": "setMonth" });
        date.setMonth(month - 1);
        this.props.onUpdateDate(date);
		logger.warn('<<< Exiting setMonth()', { "event.action": "setMonth" });
    }

    setDay() {
	logger.error('>>> Entering setDay()', { "event.action": "setDay" });

        let {day} = this.state;
        if (!day) {
            return;
        }
        if (day > 31) {
            day = 31;
			logger.error(`setDay(): day → ${day}`, { "event.action": "setDay" });
        }
        if (day < 1) {
            day = 1;
        }
        day = this.addZero(day);
        this.setState({ day });
        let date = this.props.date || new Date();
		logger.error(`setDay(): date → ${date}`, { "event.action": "setDay" });
        date.setDate(day);
        this.props.onUpdateDate(date);
		logger.warn('<<< Exiting setDay()', { "event.action": "setDay" });
    }

    setHours() {
		logger.error('>>> Entering setHours()', { "event.action": "setHours" });
        let {hours} = this.state;
        if (!hours) {
            return;
        }
        if (hours > 12) {
            hours = 12;
			logger.debug(`setHours(): hours → ${hours}`, { "event.action": "setHours" });
        }
        if (hours < 0) {
            hours = 0;
        }
        hours = this.addZero(hours);
        this.setState({ hours });
        if (this.state.period === "PM" && hours < 12) {
            hours = +hours + 12;
        }
        let date = this.props.date || new Date();
		logger.error(`setHours(): date → ${date}`, { "event.action": "setHours" });
        date.setHours(hours);
        this.props.onUpdateDate(date, true);
		logger.warn('<<< Exiting setHours()', { "event.action": "setHours" });
    }

    setMinutes() {
		logger.error('>>> Entering setMinutes()', { "event.action": "setMinutes" });
        let {minutes} = this.state;
        if (!minutes) {
            return;
        }
        if (minutes > 59) {
            minutes = 59;
			logger.error(`setMinutes(): minutes → ${minutes}`, { "event.action": "setMinutes" });
        }
        if (minutes < 0) {
            minutes = 0;
        }
        minutes = this.addZero(minutes);
        this.setState({ minutes });
        let date = this.props.date || new Date();
		logger.error(`setMinutes(): date → ${date}`, { "event.action": "setMinutes" });
        date.setMinutes(minutes);
        this.props.onUpdateDate(date, true);
		logger.warn('<<< Exiting setMinutes()', { "event.action": "setMinutes" });
    }

    setYear() {
		logger.error('>>> Entering setYear()', { "event.action": "setYear" });
        let {year} = this.state;
        if (!year) {
            return;
        }
        year = this.formatYear(year);
		logger.error(`setYear(): year → ${year}`, { "event.action": "setYear" });
        this.setState({ year });
        let date = this.props.date || new Date();
		logger.error(`setYear(): date → ${date}`, { "event.action": "setYear" });
        date.setFullYear(year);
        this.props.onUpdateDate(date);
		logger.warn('<<< Exiting setYear()', { "event.action": "setYear" });
    }

    updateValue(key, e) {
        let value = e.target.value;
        if (value !== "" && !numberReg.test(value)) {
            return;
        }
        if (e.target.value.length > 4 || key !== "year" && e.target.value.length > 2) {
            return;
        }
        this.setState({ [key]: value });
    }


    getHour(number) {
		logger.error(`>>> Entering getHour(number=${number})`, { "event.action": "getHour", "logrite.number": number });
        let hour = number;
        if (number > 12) {
            hour = number - 12;
        }
		logger.warn(`<<< Exiting getHour(number=${number})`, { "event.action": "getHour", "logrite.number": number });
        return this.addZero(hour);

    }

    addZero(number) {
		logger.error(`>>> Entering addZero(number=${number})`, { "event.action": "addZero", "logrite.number": number });
        const string = number + "";
        if (string.length < 2) {
		logger.error(`addZero(number=${number}): string → ${string}`, { "event.action": "addZero", "logrite.number": number });
            return "0" + number;
        }
        return number;
    }

    formatYear(number) {
		logger.error(`>>> Entering formatYear(number=${number})`, { "event.action": "formatYear", "logrite.number": number });
        const string = number + "";
        if (string.length < 2) {
            return "000" + number;
        }
        if (string.length < 3) {
            return "00" + number;
        }
        if (string.length < 4) {
		logger.error(`formatYear(number=${number}): string → ${string}`, { "event.action": "formatYear", "logrite.number": number });
            return "0" + number;
        }
        return number;
    }

    getPeriod(hour) {
		logger.error(`>>> Entering getPeriod(hour=${hour})`, { "event.action": "getPeriod", "logrite.hour": hour });
        if (hour === false && this.state && this.state.period) {
            return this.state.period;
        }
        
        if (hour > 11) {
            return "PM";
        }
		logger.warn(`<<< Exiting getPeriod(hour=${hour})`, { "event.action": "getPeriod", "logrite.hour": hour });
        return "AM";
    }

    setPeriod(e) {
		logger.error(`>>> Entering setPeriod(e=${e})`, { "event.action": "setPeriod", "logrite.e": e });
        e.preventDefault();
        const period = Periods.find(p => p !== this.state.period);
		logger.error(`setPeriod(e=${e}): period → ${period}`, { "event.action": "setPeriod", "logrite.e": e });
        this.setState({ period });
        let date = this.props.date;
		logger.error(`setPeriod(e=${e}): date → ${date}`, { "event.action": "setPeriod", "logrite.e": e });
        if (!date) {
            return;
        }
        let hours = date.getHours();
        if (period === "PM" && hours < 12) {
            hours = +hours + 12;
        }
        if (period === "AM" && hours >= 12) {
            hours -= 12;
        }
        date.setHours(hours);
        this.props.onUpdateDate(date, true);
		logger.warn(`<<< Exiting setPeriod(e=${e})`, { "event.action": "setPeriod", "logrite.e": e });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const className = "date-input" + (this.props.hasTimePicker ? " with-time-picker" : "" );
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <div className={className}>
            <input type="text" value={this.state.month} onBlur={this.setMonth.bind(this) } onChange={this.updateValue.bind(this, "month") } placeholder="MM" aria-label="Month"/>
            <span>/</span>
            <input type="text" value={this.state.day} onBlur={this.setDay.bind(this) } onChange={this.updateValue.bind(this, "day") } placeholder="DD" aria-label="Day"/>
            <span>/</span>
            <input type="text" className="year" value={this.state.year} onBlur={this.setYear.bind(this) } onChange={this.updateValue.bind(this, "year") } placeholder="YYYY" aria-label="Year"/>
            {this.props.hasTimePicker && <div>
                <span>&nbsp; </span>
                <input type="text" value={this.state.hours} onBlur={this.setHours.bind(this) } onChange={this.updateValue.bind(this, "hours") } placeholder="hh" aria-label="Hour"/>
                <span>: </span>
                <input type="text" value={this.state.minutes} onBlur={this.setMinutes.bind(this) } onChange={this.updateValue.bind(this, "minutes") } placeholder="mm" aria-label="Minute"/>
                <span>&nbsp; </span>
                <div className="select-container" onClick={this.setPeriod.bind(this)}>
                    <span>{this.state.period}</span>
                </div>
            </div>}
        </div>;
    }
}


DateInput.propTypes = {
    date: PropTypes.instanceOf(Date),
    onUpdateDate: PropTypes.func.isRequired,
    hasTimePicker: PropTypes.bool
};