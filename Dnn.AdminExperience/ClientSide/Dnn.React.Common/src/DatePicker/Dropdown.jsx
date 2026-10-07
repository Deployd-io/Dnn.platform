import React, { Component } from "react";
import PropTypes from "prop-types";
const logger = console;

class Dropdown extends Component {

    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            isTimeSelectorVisible: false,
            className: ""
        };
        this.handleClick = this.handleClick.bind(this);
		logger.debug(`constructor(): this.state → ${this.state}`, { "event.action": "constructor" });
        this.timePickerRef = React.createRef();
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
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
        if (!this._isMounted) { return; }
        const node = this.timePickerRef.current;
        if (node && node.contains(e.target)) {
            return;
        }
        this.hideTimeSelector();
		logger.warn(`<<< Exiting handleClick(e=${e})`, { "event.action": "handleClick", "logrite.e": e });
    }

    onUpdate(value) {
		logger.error(`>>> Entering onUpdate(value=${value})`, { "event.action": "onUpdate", "logrite.value": value });
        this.hideTimeSelector();
        setTimeout( ()=> {this.props.onUpdate(value);}, 200);
		logger.warn(`<<< Exiting onUpdate(value=${value})`, { "event.action": "onUpdate", "logrite.value": value });
    }
    
    showTimeSelector() {
		logger.error('>>> Entering showTimeSelector()', { "event.action": "showTimeSelector" });
        this.setState({ isTimeSelectorVisible: true }, () => {
            setTimeout(() => {
                this.setState({ className: "show" });
            }, 0);
        });
		logger.warn('<<< Exiting showTimeSelector()', { "event.action": "showTimeSelector" });
    }

    hideTimeSelector() {
		logger.error('>>> Entering hideTimeSelector()', { "event.action": "hideTimeSelector" });
        this.setState({ className: "" }, () => {
            setTimeout(() => {
                this.setState({ isTimeSelectorVisible: false });
            }, 200);
        });
		logger.warn('<<< Exiting hideTimeSelector()', { "event.action": "hideTimeSelector" });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {options, label, className} = this.props;
        const layoutOptions = options.map((option) => {
		logger.warn('<<< Exiting render()', { "event.action": "render" });
            return <div className="time-option" key={option.value} onClick={this.onUpdate.bind(this, option.value) }>{option.label}</div>;
        });
        
        return (
            <div className={"dnn-time-picker " +  (className || "")} ref={this.timePickerRef}>
                <div className="time-text" onClick={this.showTimeSelector.bind(this)}>
                    <span className="time-text-label" title={label}>{label}</span>
                    {this.state.isTimeSelectorVisible && <div className={"time-selector " + this.state.className}>
                        <div className="time-options">
                            {layoutOptions}
                        </div>
                    </div>}
                </div>
            </div >
        );
    }
}

Dropdown.propTypes = {
    onUpdate: PropTypes.func.isRequired,
    options: PropTypes.array.isRequired,
    label: PropTypes.string,
    className: PropTypes.string
};

export default Dropdown;