import React, { Component } from "react";
import PropTypes from "prop-types";
const logger = console;

const Periods = ["AM", "PM"];
const numberReg = new RegExp("^\\d+$");

export default class DateInput extends Component {
    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);

        const month = this.props.date ? this.addZero(this.props.date.getMonth() + 1) : "";
		logger.debug(`constructor(props=${props}): month → ${month}`);
        const day = this.props.date ? this.addZero(this.props.date.getDate()) : "";
		logger.debug(`constructor(props=${props}): day → ${day}`);
        const year = this.props.date ? this.props.date.getFullYear() : "";
		logger.debug(`constructor(props=${props}): year → ${year}`);
        const hours = this.props.date ? this.getHour(this.props.date.getHours()) : "";
		logger.debug(`constructor(props=${props}): hours → ${hours}`);
        const minutes = this.props.date ? this.addZero(this.props.date.getMinutes()) : "";
		logger.debug(`constructor(props=${props}): minutes → ${minutes}`);
        const period = this.props.date ? this.getPeriod(this.props.date.getHours()) : this.getPeriod(false);
		logger.debug(`constructor(props=${props}): period → ${period}`);

        this.state = { month, day, year, hours, minutes, period };
		logger.debug(`<<< Exiting constructor(props=${props})`);
		logger.debug(`constructor(props=${props}): this.state → ${this.state}`);

    }

    componentDidUpdate(prevProps) {
		logger.debug(`>>> Entering componentDidUpdate(prevProps=${prevProps})`);
        let { props } = this;
        if (props.date !== prevProps.date) {
            const month = props.date ? this.addZero(props.date.getMonth() + 1) : "";
			logger.debug(`componentDidUpdate(prevProps=${prevProps}): month → ${month}`);
            const day = props.date ? this.addZero(props.date.getDate()) : "";
			logger.debug(`componentDidUpdate(prevProps=${prevProps}): day → ${day}`);
            const year = props.date ? props.date.getFullYear() : "";
			logger.debug(`componentDidUpdate(prevProps=${prevProps}): year → ${year}`);
            const hours = props.date ? this.getHour(props.date.getHours()) : "";
			logger.debug(`componentDidUpdate(prevProps=${prevProps}): hours → ${hours}`);
            const minutes = props.date ? this.addZero(props.date.getMinutes()) : "";
			logger.debug(`componentDidUpdate(prevProps=${prevProps}): minutes → ${minutes}`);
            const period = props.date ? this.getPeriod(props.date.getHours()) : this.getPeriod(false);
			logger.debug(`componentDidUpdate(prevProps=${prevProps}): period → ${period}`);

            this.setState({ month, day, year, hours, minutes, period });
			logger.debug(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`);
        }
    }

    setMonth() {
		logger.debug('>>> Entering setMonth()');
        let {month} = this.state;
        if (!month) {
			logger.debug(`setMonth(): month → ${month}`);
            return;
        }
        if (month > 12) {
            month = 12;
        }
        if (month < 1) {
            month = 1;
        }
        month = this.addZero(month);
        this.setState({ month });
        let date = this.props.date || new Date();
		logger.debug(`setMonth(): date → ${date}`);
        date.setMonth(month - 1);
        this.props.onUpdateDate(date);
		logger.debug('<<< Exiting setMonth()');
    }

    setDay() {
	logger.debug('>>> Entering setDay()');

        let {day} = this.state;
        if (!day) {
            return;
        }
        if (day > 31) {
            day = 31;
        }
        if (day < 1) {
            day = 1;
        }
        day = this.addZero(day);
        this.setState({ day });
        let date = this.props.date || new Date();
		logger.debug(`setDay(): date → ${date}`);
        date.setDate(day);
        this.props.onUpdateDate(date);
		logger.debug('<<< Exiting setDay()');
    }

    setHours() {
		logger.debug('>>> Entering setHours()');
        let {hours} = this.state;
        if (!hours) {
			logger.debug(`setHours(): hours → ${hours}`);
            return;
        }
        if (hours > 12) {
            hours = 12;
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
		logger.debug(`setHours(): date → ${date}`);
        date.setHours(hours);
        this.props.onUpdateDate(date, true);
		logger.debug('<<< Exiting setHours()');
    }

    setMinutes() {
		logger.debug('>>> Entering setMinutes()');
        let {minutes} = this.state;
        if (!minutes) {
			logger.debug(`setMinutes(): minutes → ${minutes}`);
            return;
        }
        if (minutes > 59) {
            minutes = 59;
        }
        if (minutes < 0) {
            minutes = 0;
        }
        minutes = this.addZero(minutes);
        this.setState({ minutes });
        let date = this.props.date || new Date();
		logger.debug(`setMinutes(): date → ${date}`);
        date.setMinutes(minutes);
        this.props.onUpdateDate(date, true);
		logger.debug('<<< Exiting setMinutes()');
    }

    setYear() {
		logger.debug('>>> Entering setYear()');
        let {year} = this.state;
        if (!year) {
            return;
        }
        year = this.formatYear(year);
        this.setState({ year });
        let date = this.props.date || new Date();
		logger.debug(`setYear(): date → ${date}`);
        date.setFullYear(year);
        this.props.onUpdateDate(date);
		logger.debug('<<< Exiting setYear()');
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
		logger.debug(`>>> Entering getHour(number=${number})`);
        let hour = number;
        if (number > 12) {
            hour = number - 12;
        }
		logger.debug(`<<< Exiting getHour(number=${number})`);
        return this.addZero(hour);

    }

    addZero(number) {
		logger.debug(`>>> Entering addZero(number=${number})`);
        const string = number + "";
        if (string.length < 2) {
		logger.debug(`addZero(number=${number}): string → ${string}`);
		logger.debug(`<<< Exiting addZero(number=${number})`);
            return "0" + number;
        }
        return number;
    }

    formatYear(number) {
		logger.debug(`>>> Entering formatYear(number=${number})`);
        const string = number + "";
        if (string.length < 2) {
            return "000" + number;
        }
        if (string.length < 3) {
            return "00" + number;
        }
        if (string.length < 4) {
		logger.debug(`formatYear(number=${number}): string → ${string}`);
		logger.debug(`<<< Exiting formatYear(number=${number})`);
            return "0" + number;
        }
        return number;
    }

    getPeriod(hour) {
		logger.debug(`>>> Entering getPeriod(hour=${hour})`);
        if (hour === false && this.state && this.state.period) {
            return this.state.period;
        }
        
        if (hour > 11) {
		logger.debug(`<<< Exiting getPeriod(hour=${hour})`);
            return "PM";
        }
        return "AM";
    }

    setPeriod(e) {
		logger.debug(`>>> Entering setPeriod(e=${e})`);
        e.preventDefault();
        const period = Periods.find(p => p !== this.state.period);
		logger.debug(`setPeriod(e=${e}): period → ${period}`);
        this.setState({ period });
        let date = this.props.date;
		logger.debug(`setPeriod(e=${e}): date → ${date}`);
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
		logger.debug(`<<< Exiting setPeriod(e=${e})`);
    }

    render() {
		logger.debug('>>> Entering render()');
        const className = "date-input" + (this.props.hasTimePicker ? " with-time-picker" : "" );
		logger.debug('<<< Exiting render()');
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